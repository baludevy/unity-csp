using UnityEngine;

public class WorldSnapshotManager : MonoBehaviour {
    public static WorldSnapshotManager Instance;

    private void Awake() {
        Instance = this;
    }

    public void ProcessWorldSnapshot(WorldSnapshot snapshot) {
        if (snapshot.tick < NetworkManager.serverTick) return;
        NetworkManager.serverTick = snapshot.tick;

        TickTimer.AdjustSpeed(snapshot.timeDelta);

        ThreadManager.ExecuteOnMainThread(() => {
            NetworkManager.serverTick = snapshot.tick;

            foreach (PlayerState playerState in snapshot.playerStates) {
                if (playerState.id == Client.Instance.myId && SendInput.Instance != null) {
                    PlayerPrediction.Instance.CompareServerState(playerState, snapshot.tick);
                }
                else if (playerState.id != Client.Instance.myId) {
                    GameManager.players[playerState.id].transform.position = playerState.position;
                }
            }
        });
    }
}