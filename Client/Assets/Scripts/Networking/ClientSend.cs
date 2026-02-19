using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour {
    public static int packetsSent;
    public static int bytesSent;

    private static void SendTCPData(Packet packet) {
        bytesSent += packet.Length();
        packetsSent++;
        packet.WriteLength();

        Client.Instance.tcp.SendData(packet);
    }

    private static void SendUDPData(Packet packet) {
        bytesSent += packet.Length();
        packet.WriteLength();
        packetsSent++;

        Client.Instance.udp.SendData(packet);
    }

    #region Packets

    public static void WelcomeReceived() {
        using Packet _packet = new Packet((byte)ClientPackets.welcomeReceived);
        _packet.Write(Client.Instance.myId);

        _packet.Write(NetworkUIManager.Instance.usernameField.text != ""
            ? NetworkUIManager.Instance.usernameField.text
            : $"Player{Client.Instance.myId}");

        SendTCPData(_packet);
    }

    public static void PlayerInput(List<PlayerInput> playerCommands) {
        using Packet _packet = new Packet((byte)ClientPackets.playerInput);

        _packet.Write(playerCommands.Count);
        foreach (PlayerInput playerCommand in playerCommands) {
            _packet.Write(playerCommand.currentTick);
            _packet.Write(playerCommand.up);
            _packet.Write(playerCommand.down);
            _packet.Write(playerCommand.left);
            _packet.Write(playerCommand.right);
            _packet.Write(playerCommand.jumping);
        }

        SendUDPData(_packet);
    }

    #endregion
}