using System.Collections.Generic;
using UnityEngine;

public class SendInput : MonoBehaviour {
    public static SendInput Instance;

    private bool up;
    private bool down;
    private bool left;
    private bool right;
    private bool jumping;

    public static bool DoSampleInputs = true;

    private static readonly List<PlayerInput> playerInputs = new();
    public static int lastSentTick { get; private set; }

    public PlayerInput[] inputHistory = new PlayerInput[1024];

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        GetInput();

        if (Input.GetKeyDown(KeyCode.F)) {
            Debug.Log("Spiking CPU...");
            float endTime = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < endTime) {
                float dummy = Mathf.Sin(Time.realtimeSinceStartup);
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

    public PlayerInput SampleInputs(int tick) {
        PlayerInput input = new PlayerInput {
            currentTick = tick,
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

    public static void SendPlayerInputs() {
        if (PlayerMovement.Instance == null) return;

        const int redundancy = 1;
        int bufferSize = NetworkSettings.inputBufferSize;

        int lastCompletedTick = TickTimer.tick - 1;
        if (lastCompletedTick < 0) return;

        int firstUnsents = lastSentTick + 1;

        playerInputs.Clear();

        for (int t = firstUnsents; t <= lastCompletedTick; t++) {
            PlayerInput input = Instance.inputHistory[t % bufferSize];
            if (input != null && input.currentTick == t)
                playerInputs.Add(input);
        }

        for (int i = 0; i < redundancy; i++) {
            int commandTick = lastCompletedTick - i;
            if (commandTick < 0) break;
            if (commandTick >= firstUnsents) continue;

            PlayerInput input = Instance.inputHistory[commandTick % bufferSize];
            if (input != null && input.currentTick == commandTick)
                playerInputs.Add(input);
        }

        if (playerInputs.Count == 0) return;

        playerInputs.Sort((a, b) => a.currentTick.CompareTo(b.currentTick));
        ClientSend.PlayerInput(playerInputs);

        lastSentTick = lastCompletedTick;
    }
}