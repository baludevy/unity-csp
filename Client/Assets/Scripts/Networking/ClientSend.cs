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
        using var _packet = new Packet((byte)ClientPackets.welcomeReceived);
        _packet.Write(Client.Instance.myId);

        _packet.Write(NetworkUIManager.Instance.usernameField.text != ""
            ? NetworkUIManager.Instance.usernameField.text
            : $"Player{Client.Instance.myId}");

        SendTCPData(_packet);
    }

    public static void PlayerInput(List<PlayerInput> inputs) {
        using var _packet = new Packet((byte)ClientPackets.playerInput);

        _packet.Write((byte)inputs.Count);
        foreach (var input in inputs) {
            _packet.Write(input.currentTick);
            _packet.Write(input.flags);
        }

        SendUDPData(_packet);
    }

    #endregion
}