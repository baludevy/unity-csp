using System.Collections.Generic;

public class WorldSnapshot {
    public int serverTick;
    public List<PlayerState> playerStates = new();
}