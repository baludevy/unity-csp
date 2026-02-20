using UnityEngine;

public class Player : MonoBehaviour {
    public byte id;
    public int tick;
    public string username;

    public PlayerStateManager stateManager;
    public PlayerInputManager inputManager;
    public PlayerMovement movement;
    public PlayerSyncManager syncManager;

    public int desiredInputQueueLength;

    public void Initialize(byte playerId, string playerUsername) {
        id = playerId;
        username = playerUsername;
    }
}