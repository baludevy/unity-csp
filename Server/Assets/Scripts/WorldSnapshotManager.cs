using UnityEngine;


public class WorldSnapshotManager : MonoBehaviour {
    public static WorldSnapshotManager Instance;

    private void Awake() {
        Instance = this;
    }

    private WorldSnapshot GetWorldSnapshot(int tick) {
        WorldSnapshot worldSnapshot = new WorldSnapshot();

        worldSnapshot.tick = tick;

        foreach (Client client in Server.clients.Values) {
            if (client.player != null) {
                worldSnapshot.playerStates.Add(client.player.stateManager.GetState());
            }
        }

        return worldSnapshot;
    }

    public void SendWorldSnapshotToClients(int tick) {
        foreach (Client client in Server.clients.Values) {
            if (client.player != null) {
                WorldSnapshot snapshot = GetWorldSnapshot(tick);

                snapshot.timeDelta = (float)client.player.syncManager.timeDelta;

                ServerSend.WorldSnapshot(client.player.id, snapshot);
            }
        }
    }

    public void SendWorldSnapshotToClient(int latestInputTick, float timeDelta, byte id) {
        WorldSnapshot snapshot = GetWorldSnapshot(latestInputTick);

        ServerSend.WorldSnapshot(id, snapshot);
    }
}