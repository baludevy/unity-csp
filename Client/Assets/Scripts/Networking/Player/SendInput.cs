using System;
using System.Collections.Generic;
using UnityEngine;

public class SendInput : MonoBehaviour {
    public static SendInput Instance;

    private bool up;
    private bool down;
    private bool left;
    private bool right;
    private bool jumping;

    public static bool sampleInputs;
    public static bool sendInputs;

    public PlayerInput[] inputHistory = new PlayerInput[1024];

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        sampleInputs = true;
        sendInputs = true;
    }

    private void Update() {
        GetInput();


        if (Input.GetKeyDown(KeyCode.F)) {
            // Press F to spike
            Debug.Log("Spiking CPU...");
            float endTime = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < endTime) {
                // Busy loop: maxes CPU
                float dummy = Mathf.Sin(Time.realtimeSinceStartup);
            }
        }
    }

    private void GetInput() {
        if (!sampleInputs) {
            up = false;
            down = false;
            left = false;
            right = false;

            jumping = false;
            return;
        }

        up = Input.GetKey(KeyCode.W);
        down = Input.GetKey(KeyCode.S);
        left = Input.GetKey(KeyCode.A);
        right = Input.GetKey(KeyCode.D);
        jumping = Input.GetButton("Jump");
    }

    public PlayerInput SampleInputs(int tick) {
        PlayerInput input = new PlayerInput() {
            currentTick = tick,
            lastRenderedTick = NetworkManager.serverTick,
            up = up,
            down = down,
            left = left,
            right = right,
            jumping = jumping,
        };

        int i = input.currentTick % NetworkSettings.inputBufferSize;
        inputHistory[i] = input;

        return input;
    }
}