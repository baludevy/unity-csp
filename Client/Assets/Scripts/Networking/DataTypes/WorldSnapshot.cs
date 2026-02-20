using System.Collections.Generic;

public class WorldSnapshot {
    public List<PlayerState> playerStates = new();
    public uint serverTick;
}