using UnityEngine;

public class PlayerPrediction : MonoBehaviour {
    private const float positionErrorThreshold = 0.000000001f;
    public static PlayerPrediction Instance;

    public GameObject visualPlayerObj;
    private readonly bool[] hasPositionHistory = new bool[NetworkSettings.inputBufferSize];

    private readonly Vector3[] positionHistory = new Vector3[NetworkSettings.inputBufferSize];

    private void Awake() {
        Instance = this;
    }

    private void Update() {
        if (PlayerMovement.Instance == null || visualPlayerObj == null) return;

        visualPlayerObj.transform.position = PlayerMovement.Instance.transform.position;
    }

    public void PredictState(PlayerInput input) {
        var i = input.currentTick % NetworkSettings.inputBufferSize;

        // Debug.Log($"Performing prediction on tick: {input.currentTick} with x:{input.x()} y:{input.y()}");

        PlayerMovement.Instance.SetInput(input.x(), input.y(), input.jumping);
        PlayerMovement.Instance.AdvanceLogic();

        positionHistory[i] = PlayerMovement.Instance.transform.position;
        hasPositionHistory[i] = true;

        visualPlayerObj.transform.position = PlayerMovement.Instance.transform.position;
    }

    public void CompareServerState(PlayerState playerState, uint tick) {
        // if (PlayerMovement.Instance == null || tick > NetworkManager.inputTick) return;

        var index = tick % NetworkSettings.inputBufferSize;

        if (!hasPositionHistory[index]) return;

        var prePosition = positionHistory[index];

        var errorSqrMag = (playerState.position - prePosition).sqrMagnitude;
        if (errorSqrMag > positionErrorThreshold)
            // Debug.Log($"Desync by {errorSqrMag}");
            SynchronizeMovement(playerState, tick);
    }

    private void SynchronizeMovement(PlayerState playerState, uint tick) {
        var serverPosition = playerState.position;
        PlayerMovement.Instance.transform.position = serverPosition;
        PlayerMovement.Instance.rb.velocity = playerState.velocity;

        var lastSimulatedTick = TickTimer.tick - 1;
        for (var i = tick + 1; i <= lastSimulatedTick; i++) {
            var cacheIndex = i % NetworkSettings.inputBufferSize;
            var input = SendInput.Instance.inputHistory[cacheIndex];
            if (input == null || input.currentTick != i) {
                Debug.Log("fuck");
                break;
            }

            PlayerMovement.Instance.SetInput(input.x(), input.y(), input.jumping);
            PlayerMovement.Instance.AdvanceLogic();
            Physics.Simulate(NetworkSettings.tickTime);
            positionHistory[cacheIndex] = PlayerMovement.Instance.transform.position;
            hasPositionHistory[cacheIndex] = true;
        }
    }
}