using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour {
    private Player player;
    private PlayerInputBuffer playerInputBuffer;

    private void Awake() {
        player = GetComponent<Player>();

        playerInputBuffer = new PlayerInputBuffer();
        playerInputBuffer.Initialize(NetworkSettings.inputBufferSize);
    }

    public void ProcessInput(uint currentTick) {
        var currentInput = playerInputBuffer.GetInput(currentTick);

        if (currentInput != null) player.movement.SetInput(currentInput.x(), currentInput.y(), currentInput.jumping);

        player.movement.AdvanceLogic();
    }

    public void AddInputs(List<PlayerInput> inputs) {
        if (inputs == null || inputs.Count == 0) return;

        try {
            foreach (var input in inputs) playerInputBuffer.AddInputToQueue(input);
        }
        catch (Exception ex) {
            ThreadManager.ExecuteOnMainThread(() => { Debug.Log("error adding input: " + ex.Message); });
        }
    }
}