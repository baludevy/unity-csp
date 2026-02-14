using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour {
    public static float bytesReceived;
    public static float packetsReceived;

    public static void Welcome(Packet _packet) {
        byte myId = _packet.ReadByte();
        int tick = _packet.ReadInt();
        float time = _packet.ReadFloat();

        Client.Instance.myId = myId;
        Client.isConnected = true;

        TickTimer.tick = tick;

        ClientSend.WelcomeReceived();

        ClientManager.Instance.ClientConnected();

        Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet _packet) {
        byte id = _packet.ReadByte();
        string username = _packet.ReadString();
        Vector3 position = _packet.ReadVector3();
        Quaternion rotation = _packet.ReadQuaternion();

        GameManager.Instance.SpawnPlayer(id, username, position, rotation);
    }

    public static void PlayerDisconnected(Packet _packet) {
        byte id = _packet.ReadByte();

        if (GameManager.players[id] != null) {
            if (id == Client.Instance.myId) {
                Client.Instance.Disconnect();
                Debug.Log("Disconnected by the server.");
            }
            else {
                if (GameManager.players.TryGetValue(id, out PlayerManager player)) {
                    Debug.Log($"Player{id} disconnected.");
                    Destroy(player.gameObject);
                    GameManager.players.Remove(id);
                }
            }
        }
    }

    public static void WorldSnapshot(Packet _packet) {
        int tick = _packet.ReadInt();
        float timeDelta = _packet.ReadFloat();

        WorldSnapshot worldSnapshot = new WorldSnapshot();

        worldSnapshot.tick = tick;
        worldSnapshot.timeDelta = timeDelta;

        int playerStateCount = _packet.ReadInt();

        for (int i = 0; i < playerStateCount; i++) {
            PlayerState playerState = new PlayerState() {
                id = _packet.ReadByte(),
                position = _packet.ReadVector3(),
                velocity = _packet.ReadVector3()
            };

            worldSnapshot.playerStates.Add(playerState);
        }

        WorldSnapshotManager.Instance.ProcessWorldSnapshot(worldSnapshot);
    }
}