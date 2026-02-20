using UnityEngine;

public class WorldSnapshotManager : MonoBehaviour {
    public static WorldSnapshotManager Instance;

    private void Awake() {
        Instance = this;
    }

    private WorldSnapshot GetWorldSnapshot(uint tick) {
        var worldSnapshot = new WorldSnapshot();

        worldSnapshot.serverTick = tick;

        foreach (var client in Server.clients.Values)
            if (client.player != null)
                worldSnapshot.playerStates.Add(client.player.stateManager.GetState());

        return worldSnapshot;
    }

    public void SendWorldSnapshotToClients(uint tick) {
        foreach (var client in Server.clients.Values)
            if (client.player != null) {
                var snapshot = GetWorldSnapshot(tick);

                ServerSend.WorldSnapshot(client.player.id, snapshot);
            }
    }

    public void SendWorldSnapshotToClient(uint latestInputTick, byte id) {
        var snapshot = GetWorldSnapshot(latestInputTick);

        ServerSend.WorldSnapshot(id, snapshot);
    }
}