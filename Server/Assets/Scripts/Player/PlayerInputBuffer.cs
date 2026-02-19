using UnityEngine;

public class PlayerInputBuffer {
    private PlayerInput[] inputBuffer;
    private int inputBufferSize;
    
    private int lastProcessedTick = -1;
    private PlayerInput lastReturnedInput = new();

    public void Initialize(int bufferSize) {
        inputBufferSize = bufferSize;
        inputBuffer = new PlayerInput[bufferSize];
    }

    public PlayerInput GetInput(int tick) {
        int index = tick % inputBufferSize;
        var input = inputBuffer[index];

        if (input != null && input.currentTick == tick && tick > lastProcessedTick) {
            lastProcessedTick = tick;
            lastReturnedInput = input;
            return input;
        }

        if (input == null) {
            // Debug.Log("cq starved");
        }

        return lastReturnedInput;
    }

    public void AddInputToQueue(PlayerInput input) {
        if (input.currentTick < lastProcessedTick) return;

        int index = input.currentTick % inputBufferSize;
        inputBuffer[index] = input;
    }
}