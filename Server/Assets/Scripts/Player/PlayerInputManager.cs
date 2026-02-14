using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInputManager : MonoBehaviour {
    private Player player;
    public InputBuffer inputBuffer;

    private void Awake() {
        player = GetComponent<Player>();

        inputBuffer = new InputBuffer();
        inputBuffer.Initialize(NetworkSettings.inputBufferSize);
    }

    public void ProcessInput(int currentTick) {
        PlayerInput currentInput = inputBuffer.GetInput(currentTick);

        double currentTime = NetworkManager.tickManager.GetTime();

        if (currentInput != null) {
            player.movement.SetInput(currentInput.x(), currentInput.y(), currentInput.jumping);
        }

        player.movement.AdvanceLogic();
    }

    public void AddInputs(List<PlayerInput> inputs, double arrivalTime) {
        if (inputs == null || inputs.Count == 0) return;

        try {
            // use the newest tick in the batch for queue/offset math
            int newestTick = inputs[0].currentTick;
            for (int i = 1; i < inputs.Count; i++) {
                int t = inputs[i].currentTick;
                if (t > newestTick) newestTick = t;
            }

            player.syncManager.CheckOffset(newestTick, arrivalTime);

            foreach (PlayerInput input in inputs) {
                inputBuffer.AddInputToQueue(input);
            }
        }
        catch (Exception ex) {
            ThreadManager.ExecuteOnMainThread(() => { Debug.Log("Error adding input: " + ex.Message); });
        }
    }
}