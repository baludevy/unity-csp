using UnityEngine;

public class ClientManager : MonoBehaviour {
    public static ClientManager Instance;

    private void Awake() {
        Instance = this;
    }

    public void ClientConnected() {
        NetworkUIManager.Instance.DisableConnectUI();
    }

    public void ClientDisconnected() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        foreach (var player in GameManager.players.Values) Destroy(player.gameObject);

        if (PlayerMovement.Instance != null) Destroy(PlayerMovement.Instance.transform.root.gameObject);

        GameManager.players.Clear();

        NetworkUIManager.Instance.EnableConnectUI();
    }
}