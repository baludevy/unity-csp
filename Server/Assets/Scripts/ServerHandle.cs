using System.Collections.Generic;
using UnityEngine;

public abstract class ServerHandle {
    public static void WelcomeReceived(byte fromClient, Packet _packet) {
        var clientIdCheck = _packet.ReadByte();
        var username = _packet.ReadString();

        if (fromClient != clientIdCheck)
            Debug.Log(
                $"player \"{username}\" (id: {fromClient}) has assumed the wrong client id ({clientIdCheck})");

        Server.clients[fromClient].SendIntoGame(username);
    }

    public static void PlayerInput(byte fromClient, Packet _packet) {
        var inputCount = _packet.ReadByte();

        var inputs = new List<PlayerInput>();

        for (byte i = 0; i < inputCount; i++) {
            var input = new PlayerInput {
                currentTick = _packet.ReadUInt(),
                flags = _packet.ReadByte(),
            };

            inputs.Add(input);
        }

        Server.clients[fromClient].player.inputManager.AddInputs(inputs);
    }
}