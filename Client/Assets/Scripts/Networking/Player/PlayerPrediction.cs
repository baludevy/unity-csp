using UnityEngine;

public class PlayerPrediction : MonoBehaviour {
    public static PlayerPrediction Instance;

    private Vector3[] positionHistory = new Vector3[NetworkSettings.inputBufferSize];
    private bool[] hasPositionHistory = new bool[NetworkSettings.inputBufferSize];

    private const float positionErrorThreshold = 0.000000001f;

    public GameObject visualPlayerObj;

    private void Awake() {
        Instance = this;
    }

    public void PredictState(PlayerInput input) {
        int i = input.currentTick % NetworkSettings.inputBufferSize;

        // Debug.Log($"Performing prediction on tick: {input.currentTick} with x:{input.x()} y:{input.y()}");

        PlayerMovement.Instance.SetInput(input.x(), input.y(), input.jumping);
        PlayerMovement.Instance.AdvanceLogic();

        positionHistory[i] = PlayerMovement.Instance.transform.position;
        hasPositionHistory[i] = true;

        visualPlayerObj.transform.position = PlayerMovement.Instance.transform.position;
    }

    private void Update() {
        if (PlayerMovement.Instance == null || visualPlayerObj == null) return;

        visualPlayerObj.transform.position = PlayerMovement.Instance.transform.position;
    }

    public void CompareServerState(PlayerState playerState, int tick) {
        // if (PlayerMovement.Instance == null || tick > NetworkManager.inputTick) return;

        int index = tick % NetworkSettings.inputBufferSize;

        if (!hasPositionHistory[index]) return;

        Vector3 prePosition = positionHistory[index];

        float errorSqrMag = (playerState.position - prePosition).sqrMagnitude;
        if (errorSqrMag > positionErrorThreshold) {
            // Debug.Log($"Desync by {errorSqrMag}");
            SynchronizeMovement(playerState, tick);
        }
    }

    private void SynchronizeMovement(PlayerState playerState, int tick) {
        Vector3 serverPosition = playerState.position;
        PlayerMovement.Instance.transform.position = serverPosition;
        PlayerMovement.Instance.rb.velocity = playerState.velocity;

        int lastSimulatedTick = TickTimer.tick - 1;
        for (int i = tick + 1; i <= lastSimulatedTick; i++) {
            int cacheIndex = i % NetworkSettings.inputBufferSize;
            PlayerInput input = SendInput.Instance.inputHistory[cacheIndex];
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