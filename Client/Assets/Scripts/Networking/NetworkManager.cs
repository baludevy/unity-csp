using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static uint serverTick;

    private void Update() {
        ThreadManager.UpdateMain();
    }
}