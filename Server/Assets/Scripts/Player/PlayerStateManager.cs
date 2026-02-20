using UnityEngine;

public class PlayerStateManager : MonoBehaviour {
    private Player player;

    private void Awake() {
        player = GetComponent<Player>();
    }

    public PlayerState GetState() {
        return new PlayerState {
            id = player.id,
            position = player.movement.transform.position,
            velocity = player.movement.rb.velocity
        };
    }
}