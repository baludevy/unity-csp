using System.Collections.Generic;
using UnityEngine;

public class SendInput : MonoBehaviour {
    public static SendInput Instance;

    public static bool DoSampleInputs = true;

    private static readonly List<PlayerInput> playerInputs = new();
    private bool down;

    public PlayerInput[] inputHistory = new PlayerInput[1024];
    private bool jumping;
    private bool left;
    private bool right;

    private bool up;
    public static uint lastSentTick { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        GetInput();

        if (Input.GetKeyDown(KeyCode.F)) {
            Debug.Log("Spiking CPU...");
            var endTime = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < endTime) {
                var dummy = Mathf.Sin(Time.realtimeSinceStartup);
            }
        }
    }

    private void GetInput() {
        if (!DoSampleInputs) {
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

    public PlayerInput SampleInputs(uint tick) {
        var input = new PlayerInput {
            currentTick = tick,
            up = up,
            down = down,
            left = left,
            right = right,
            jumping = jumping
        };

        var i = input.currentTick % NetworkSettings.inputBufferSize;
        inputHistory[i] = input;

        return input;
    }

    public static void SendPlayerInputs() {
        if (PlayerMovement.Instance == null) return;
        if (Instance == null) return;

        var bufferSize = NetworkSettings.inputBufferSize;

        if (TickTimer.tick == 0) return;
        var lastCompletedTick = TickTimer.tick - 1;

        var firstUnsents = lastSentTick + 1;

        playerInputs.Clear();

        if (firstUnsents <= lastCompletedTick)
            for (var t = firstUnsents; t <= lastCompletedTick; t++) {
                var input = Instance.inputHistory[t % bufferSize];
                if (input != null && input.currentTick == t)
                    playerInputs.Add(input);
            }

        for (byte i = 0; i < 2; i++) {
            var inputTick = lastCompletedTick - i;

            if (inputTick >= firstUnsents) continue;

            var input = Instance.inputHistory[inputTick % bufferSize];
            if (input != null && input.currentTick == inputTick)
                playerInputs.Add(input);
        }

        if (playerInputs.Count == 0) return;

        playerInputs.Sort((a, b) => a.currentTick.CompareTo(b.currentTick));
        ClientSend.PlayerInput(playerInputs);

        lastSentTick = lastCompletedTick;
    }
}