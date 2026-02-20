using UnityEngine;

public class ClientHandle : MonoBehaviour {
    public static float bytesReceived;
    public static float packetsReceived;

    public static void Welcome(Packet _packet) {
        var myId = _packet.ReadByte();
        var tick = _packet.ReadUInt();

        Client.Instance.myId = myId;
        Client.isConnected = true;

        TickTimer.tick = tick + 10;

        ClientSend.WelcomeReceived();

        ClientManager.Instance.ClientConnected();

        Client.Instance.udp.Connect();
    }

    public static void SpawnPlayer(Packet _packet) {
        var id = _packet.ReadByte();
        var username = _packet.ReadString();
        var position = _packet.ReadVector3();
        var rotation = _packet.ReadQuaternion();

        GameManager.Instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerDisconnected(Packet _packet) {
        var id = _packet.ReadByte();

        if (GameManager.players[id] != null) {
            if (id == Client.Instance.myId) {
                Client.Instance.Disconnect();
                Debug.Log("disconnected by the server owo");
            }
            else {
                if (GameManager.players.TryGetValue(id, out var player)) {
                    Debug.Log($"Player{id} disconnected.");
                    Destroy(player.gameObject);
                    GameManager.players.Remove(id);
                }
            }
        }
    }

    public static void WorldSnapshot(Packet _packet) {
        var tick = _packet.ReadUInt();

        var worldSnapshot = new WorldSnapshot();

        worldSnapshot.serverTick = tick;

        var playerStateCount = _packet.ReadInt();

        for (var i = 0; i < playerStateCount; i++) {
            var playerState = new PlayerState {
                id = _packet.ReadByte(),
                position = _packet.ReadVector3(),
                velocity = _packet.ReadVector3()
            };

            worldSnapshot.playerStates.Add(playerState);
        }

        WorldSnapshotManager.Instance.ProcessWorldSnapshot(worldSnapshot);
    }
}