public abstract class ServerSend {
    private static void SendTCPData(byte toClient, Packet packet) {
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendTCPDataToAll(Packet packet) {
        packet.WriteLength();
        for (byte i = 1; i <= Server.MaxPlayers; i++) {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTCPDataToAllExcept(byte exceptClient, Packet packet) {
        packet.WriteLength();
        for (byte i = 1; i <= Server.MaxPlayers; i++) {
            if (i != exceptClient) {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    private static void SendUDPData(byte toClient, Packet packet) {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void SendUDPDataToAll(Packet packet) {
        packet.WriteLength();
        for (byte i = 1; i <= Server.MaxPlayers; i++) {
            Server.clients[i].udp.SendData(packet);
        }
    }

    private static void SendUDPDataToAllExcept(int exceptClient, Packet packet) {
        packet.WriteLength();
        for (byte i = 1; i <= Server.MaxPlayers; i++) {
            if (i != exceptClient) {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    #region Packets

    public static void Welcome(byte toClient) {
        using Packet _packet = new Packet((byte)ServerPackets.welcome);

        _packet.Write(toClient);
        _packet.Write(NetworkManager.tickManager.tick);
        _packet.Write(0f);

        SendTCPData(toClient, _packet);
    }

    public static void SpawnPlayer(byte toClient, Player player) {
        using Packet _packet = new Packet((byte)ServerPackets.spawnPlayer);
        _packet.Write(player.id);
        _packet.Write(player.username);
        _packet.Write(player.transform.position);
        _packet.Write(player.transform.rotation);

        SendTCPData(toClient, _packet);
    }

    public static void PlayerDisconnected(Player player) {
        using Packet _packet = new Packet((byte)ServerPackets.playerDisconnected);
        _packet.Write(player.id);

        SendTCPDataToAll(_packet);
    }

    public static void WorldSnapshot(byte toClient, WorldSnapshot worldSnapshot) {
        using Packet _packet = new Packet((byte)ServerPackets.worldState);
        _packet.Write(worldSnapshot.tick);
        _packet.Write(worldSnapshot.timeDelta);

        _packet.Write(worldSnapshot.playerStates.Count);
        foreach (PlayerState playerState in worldSnapshot.playerStates) {
            _packet.Write(playerState.id);
            _packet.Write(playerState.position);
            _packet.Write(playerState.velocity);
        }

        SendUDPData(toClient, _packet);
    }

    #endregion
}