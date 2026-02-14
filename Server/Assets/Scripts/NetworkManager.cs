using System.Linq;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance;

    public GameObject playerPrefab;

    public static TickTimer tickManager;
    private double nextTickTime;

    private void Awake() {
        Instance = this;
        tickManager = new TickTimer();
    }

    private void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = NetworkSettings.tickRate;
        Server.Start(10, 42069);
        nextTickTime = NetworkSettings.tickTime;
    }

    private void Update() {
        ThreadManager.UpdateMain();

        double currentTime = tickManager.GetTime();

        while (currentTime >= nextTickTime) {
            ProcessTick(tickManager.tick);
            nextTickTime += NetworkSettings.tickTime;
        }
    }

    private void ProcessTick(int tick) {
        tickManager.previousLastTickTime = tickManager.lastTickTime;
        tickManager.lastTickTime = tickManager.GetTime();

        // process one input from the command buffer for every client
        foreach (var client in Server.clients.Values.Where(client => client.player != null)) {
            client.player.inputManager.ProcessInput(tick);
        }

        // advance one physics frame
        Physics.Simulate(NetworkSettings.tickTime);

        // send out the state of the 'world' to all clients
        WorldSnapshotManager.Instance.SendWorldSnapshotToClients(tick);

        tickManager.tick++;
    }

    private void OnApplicationQuit() {
        tickManager.Stop();
        Server.Stop();
    }

    public Player InstantiatePlayer() {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}