using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkManager : MonoBehaviour {
    public static NetworkManager Instance;

    public GameObject PlayerPrefab;

    public static TickTimer TickManager;
    private double nextTickTime;

    private void Awake() {
        Instance = this;
        TickManager = new TickTimer();
    }

    private void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = NetworkSettings.tickRate;
        Server.Start(10, 42069);
        nextTickTime = NetworkSettings.tickTime;
    }

    private void Update() {
        ThreadManager.UpdateMain();

        double currentTime = TickManager.GetTime();

        while (currentTime >= nextTickTime) {
            ProcessTick(TickManager.tick);
            nextTickTime += NetworkSettings.tickTime;
        }
    }

    private void ProcessTick(int tick) {
        TickManager.previousLastTickTime = TickManager.lastTickTime;
        TickManager.lastTickTime = TickManager.GetTime();

        // process one input from the command buffer for every client
        foreach (var client in Server.clients.Values.Where(client => client.player != null)) {
            client.player.inputManager.ProcessInput(tick);
        }

        // advance one physics frame
        Physics.Simulate(NetworkSettings.tickTime);

        // send out the state of the 'world' to all clients
        WorldSnapshotManager.Instance.SendWorldSnapshotToClients(tick);

        TickManager.tick++;
    }

    private void OnApplicationQuit() {
        TickManager.Stop();
        Server.Stop();
    }

    public Player InstantiatePlayer() {
        return Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}