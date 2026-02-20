using System.Diagnostics;
using UnityEngine;

public class TickTimer : MonoBehaviour {
    private static readonly float timeScale = 1f;
    public static uint tick;

    private static readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private static double accumulator;
    private static double currentTime;

    private void Start() {
        currentTime = GetTime();
        accumulator = 0;
        tick = 0;
    }

    private void Update() {
        var newTime = GetTime();
        var frameTime = (newTime - currentTime) * timeScale;
        currentTime = newTime;

        double tickInterval = NetworkSettings.tickTime;
        accumulator += frameTime;

        while (accumulator >= tickInterval) {
            accumulator -= tickInterval;
            Tick();
            tick++;
        }

        if (PlayerMovement.Instance != null && tick - 1 > SendInput.lastSentTick)
            SendInput.SendPlayerInputs();
    }

    public static double GetTime() {
        return stopwatch.Elapsed.TotalSeconds;
    }

    private static void Tick() {
        if (PlayerMovement.Instance == null) return;

        var input = SendInput.Instance.SampleInputs(tick);
        PlayerPrediction.Instance.PredictState(input);

        Physics.Simulate(NetworkSettings.tickTime);
    }
}