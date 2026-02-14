using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TickTimer : MonoBehaviour {
    private static float timeScale = 1f;
    public static int tick;

    private static readonly Stopwatch stopwatch = Stopwatch.StartNew();
    private static double accumulator;
    private static double currentTime;

    private static readonly List<PlayerInput> playerInputs = new();
    private static int lastSentTick;

    public static double GetTime() => stopwatch.Elapsed.TotalSeconds;

    private void Start() {
        currentTime = GetTime();
        accumulator = 0;
        tick = 0;
        lastSentTick = -1;
    }

    private void Update() {
        double newTime = GetTime();
        double frameTime = (newTime - currentTime) * timeScale;
        currentTime = newTime;

        double tickInterval = NetworkSettings.tickTime;
        accumulator += frameTime;

        while (accumulator >= tickInterval) {
            accumulator -= tickInterval;
            Tick();
            tick++;
        }

        if (PlayerMovement.Instance != null && tick - 1 > lastSentTick)
            SendPlayerInputs();
    }

    private static void Tick() {
        if (PlayerMovement.Instance == null) return;

        PlayerInput input = SendInput.Instance.SampleInputs(tick);
        PlayerPrediction.Instance.PredictState(input);
        Physics.Simulate(NetworkSettings.tickTime);
    }

    private static void SendPlayerInputs() {
        if (PlayerMovement.Instance == null) return;

        const int redundancy = 2;
        int bufferSize = NetworkSettings.inputBufferSize;

        int lastCompletedTick = tick - 1;
        if (lastCompletedTick < 0) return;

        int firstUnsents = lastSentTick + 1;

        playerInputs.Clear();

        // send all unsent ticks so catch-up frames don't drop inputs
        for (int t = firstUnsents; t <= lastCompletedTick; t++) {
            PlayerInput input = SendInput.Instance.inputHistory[t % bufferSize];
            if (input != null && input.currentTick == t)
                playerInputs.Add(input);
        }

        // small history tail for loss recovery without duplicating already-sent ticks
        for (int i = 0; i < redundancy; i++) {
            int commandTick = lastCompletedTick - i;
            if (commandTick < 0) break;
            if (commandTick >= firstUnsents) continue;

            PlayerInput input = SendInput.Instance.inputHistory[commandTick % bufferSize];
            if (input != null && input.currentTick == commandTick)
                playerInputs.Add(input);
        }

        if (playerInputs.Count == 0) return;

        playerInputs.Sort((a, b) => a.currentTick.CompareTo(b.currentTick));
        ClientSend.PlayerInput(playerInputs);

        lastSentTick = lastCompletedTick;
    }

    // catch up to the server based on the timing difference data we receive
    public static void AdjustSpeed(double timeDelta) {
        const double deadZone = 0.005;

        const float dampingUp = 0.2f;
        const float dampingDown = 0.05f;

        float correction = Math.Abs(timeDelta) < deadZone
            ? 0f
            : (float)(timeDelta * 5);

        float targetScale = Mathf.Clamp(1f + correction, 0.97f, 1.03f);

        float damping = targetScale > timeScale ? dampingUp : dampingDown;
        timeScale = Mathf.Lerp(timeScale, targetScale, damping);

        ThreadManager.ExecuteOnMainThread(() => { NetworkDebug.Instance.SetGameSpeed(timeScale); });
    }
}