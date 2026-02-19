using System.Collections.Generic;
using UnityEngine;

public abstract class ServerHandle {
    public static void WelcomeReceived(byte fromClient, Packet _packet) {
        byte clientIdCheck = _packet.ReadByte();
        string username = _packet.ReadString();

        Debug.Log($"Client {username} with user id {clientIdCheck} connected.");
        if (fromClient != clientIdCheck) {
            Debug.Log(
                $"Player \"{username}\" (id: {fromClient}) has assumed the wrong client id ({clientIdCheck})!");
        }

        Server.clients[fromClient].SendIntoGame(username);
    }

    public static void PlayerInput(byte fromClient, Packet _packet) {
        int playerCommandsCount = _packet.ReadInt();

        List<PlayerInput> inputs = new List<PlayerInput>();

        for (int i = 0; i < playerCommandsCount; i++) {
            PlayerInput input = new PlayerInput() {
                currentTick = _packet.ReadInt(),
                up = _packet.ReadBool(),
                down = _packet.ReadBool(),
                left = _packet.ReadBool(),
                right = _packet.ReadBool(),
                jumping = _packet.ReadBool(),
            };

            inputs.Add(input);
        }

        Server.clients[fromClient].player.inputManager.AddInputs(inputs);
    }
}