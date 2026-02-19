using System;
using UnityEngine;

public class WorldSnapshotManager : MonoBehaviour {
    public static WorldSnapshotManager Instance;

    private void Awake() {
        Instance = this;
    }

    public void ProcessWorldSnapshot(WorldSnapshot snapshot) {
        if (snapshot.serverTick < NetworkManager.serverTick) return;
        NetworkManager.serverTick = snapshot.serverTick;
    }
}