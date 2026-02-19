using System;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInputManager : MonoBehaviour {
    private Player player;
    private PlayerInputBuffer playerInputBuffer;

    private void Awake() {
        player = GetComponent<Player>();

        playerInputBuffer = new PlayerInputBuffer();
        playerInputBuffer.Initialize(NetworkSettings.inputBufferSize);
    }

    public void ProcessInput(int currentTick) {
        PlayerInput currentInput = playerInputBuffer.GetInput(currentTick);

        if (currentInput != null) {
            player.movement.SetInput(currentInput.x(), currentInput.y(), currentInput.jumping);
        }

        player.movement.AdvanceLogic();
    }

    public void AddInputs(List<PlayerInput> inputs) {
        if (inputs == null || inputs.Count == 0) return;

        try {
            foreach (PlayerInput input in inputs) {
                playerInputBuffer.AddInputToQueue(input);
            }
        }
        catch (Exception ex) {
            ThreadManager.ExecuteOnMainThread(() => { Debug.Log("Error adding input: " + ex.Message); });
        }
    }
}