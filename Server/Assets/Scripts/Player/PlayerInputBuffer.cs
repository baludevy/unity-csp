public class PlayerInputBuffer {
    private PlayerInput[] inputBuffer;
    private uint inputBufferSize;

    private uint lastProcessedTick;
    private PlayerInput lastReturnedInput = new();

    public void Initialize(uint bufferSize) {
        inputBufferSize = bufferSize;
        inputBuffer = new PlayerInput[bufferSize];
    }

    public PlayerInput GetInput(uint tick) {
        var index = tick % inputBufferSize;
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


        var index = input.currentTick % inputBufferSize;
        inputBuffer[index] = input;
    }
}