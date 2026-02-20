using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    public static Dictionary<int, PlayerManager> players = new();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    private void Awake() {
        Application.targetFrameRate = 0;
        Application.runInBackground = true;
        QualitySettings.vSyncCount = 1;
        Instance = this;
    }

    private void OnApplicationQuit() {
        Client.Instance.Disconnect();
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation) {
        GameObject player;
        if (id == Client.Instance.myId)
            player = Instantiate(localPlayerPrefab, position, rotation);
        else
            player = Instantiate(playerPrefab, position, rotation);

        if (player == null) return;
        player.GetComponent<PlayerManager>().id = id;
        player.GetComponent<PlayerManager>().username = username;
        players.Add(id, player.GetComponent<PlayerManager>());
    }
}