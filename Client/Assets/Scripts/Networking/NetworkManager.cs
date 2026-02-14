using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static int serverTick;

    private void Update() {
        ThreadManager.UpdateMain();
    }
}