using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerSyncManager : MonoBehaviour {
    public byte syncState; // 0 = stay, 1 = slow down, 2 = speed up 


    public void UpdateSyncState(int playerTick) {
    }
}