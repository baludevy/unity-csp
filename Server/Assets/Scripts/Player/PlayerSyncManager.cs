using System;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSyncManager : MonoBehaviour {
    #region inspector tweaks

    [Header("dynamic buffer settings")] [SerializeField]
    private int minBufferTicks = 0; // ← aggressive low-latency default

    [SerializeField] private int maxBufferTicks = 6;
    [Range(0f, 4f)] [SerializeField] private double stdDevMultiplier = 1.75; // ← tighter safety margin

    [Tooltip("lerp factor when *increasing* buffer (0-1)")] [Range(0f, 1f)] [SerializeField]
    private float adaptRateUp = 0.5f;

    [Tooltip("lerp factor when *decreasing* buffer (0-1)")] [Range(0f, 1f)] [SerializeField]
    private float adaptRateDown = 0.25f; // ← faster shrink

    [Tooltip("seconds to hold enlarged buffer before allowing shrink")] [SerializeField]
    private float bufferHoldSeconds = 0.25f; // ← quicker reaction to improvements

    #endregion

    private double currentBufferTicks;
    public double timeDelta { get; private set; }

    private const int jitterSampleCount = 32;
    private readonly double[] jitterSamples = new double[jitterSampleCount];
    private int jitterIndex;
    private int jitterTotalSamples;

    private int lastReceivedTick = -1;
    private double lastArrivalTime = -1;

    private double lastIncreaseTime;

    private void Awake() {
        currentBufferTicks = minBufferTicks;
        lastIncreaseTime = 0;
    }

    public void CheckOffset(int receivedTick, double arrivalTime) {
        int serverTick = NetworkManager.tickManager.tick;
        double tickTime = NetworkSettings.tickTime;

        int queueDepth = receivedTick - serverTick;

        // measure jitter from inter-arrival timing
        if (lastReceivedTick >= 0 && lastArrivalTime >= 0) {
            int tickDelta = receivedTick - lastReceivedTick;
            if (tickDelta > 0) {
                double expectedDt = tickDelta * tickTime;
                double actualDt = arrivalTime - lastArrivalTime;
                double jitter = actualDt - expectedDt;

                jitterSamples[jitterIndex] = jitter;
                jitterIndex = (jitterIndex + 1) % jitterSampleCount;
                if (jitterTotalSamples < jitterSampleCount) jitterTotalSamples++;
            }
        }

        lastReceivedTick = receivedTick;
        lastArrivalTime = arrivalTime;

        // calculate stdDev
        double mean = 0;
        for (int i = 0; i < jitterTotalSamples; ++i) mean += jitterSamples[i];
        if (jitterTotalSamples > 0) mean /= jitterTotalSamples;

        double variance = 0;
        for (int i = 0; i < jitterTotalSamples; ++i) {
            double diff = jitterSamples[i] - mean;
            variance += diff * diff;
        }

        double stdDev = jitterTotalSamples > 0 ? Math.Sqrt(variance / jitterTotalSamples) : 0;

        int jitterTicks = Mathf.CeilToInt((float)((stdDevMultiplier * stdDev) / tickTime));

        int desiredTicksInt = Mathf.Clamp(jitterTicks, minBufferTicks, maxBufferTicks);

        bool wantsGrow = desiredTicksInt > currentBufferTicks + 0.1f;
        bool wantsShrink = desiredTicksInt < currentBufferTicks - 0.1f;

        if (wantsGrow) {
            currentBufferTicks = Mathf.Lerp((float)currentBufferTicks, desiredTicksInt, adaptRateUp);
            lastIncreaseTime = arrivalTime;
        }
        else if (wantsShrink && arrivalTime - lastIncreaseTime >= bufferHoldSeconds) {
            currentBufferTicks = Mathf.Lerp((float)currentBufferTicks, desiredTicksInt, adaptRateDown);
        }

        double tickProgress = (arrivalTime - NetworkManager.tickManager.lastTickTime) / tickTime;
        tickProgress = Mathf.Clamp01((float)tickProgress);

        double adjustedQueue = queueDepth - tickProgress;

        double desiredQueue = currentBufferTicks + 0.5; // aim for middle of the tick window

        timeDelta = (desiredQueue - adjustedQueue) * tickTime;

        Debug.Log(
            $"recv:{receivedTick} srv:{serverTick} " +
            $"queue:{queueDepth}t want:{currentBufferTicks:F2}t " +
            $"Δ:{timeDelta * 1000:F1}ms σ:{stdDev * 1000:F1}ms");
    }
}