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
    
    public static double GetTime() => stopwatch.Elapsed.TotalSeconds;

    private void Start() {
        currentTime = GetTime();
        accumulator = 0;
        tick = 0;
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

        if (PlayerMovement.Instance != null && tick - 1 > SendInput.lastSentTick)
            SendInput.SendPlayerInputs();
    }

    private static void Tick() {
        if (PlayerMovement.Instance == null) return;

        PlayerInput input = SendInput.Instance.SampleInputs(tick);
        PlayerPrediction.Instance.PredictState(input);

        Physics.Simulate(NetworkSettings.tickTime);
    }
}