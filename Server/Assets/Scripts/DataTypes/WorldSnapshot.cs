using System.Collections.Generic;

public class WorldSnapshot {
    public int tick;
    public float timeDelta;
    public List<PlayerState> playerStates = new List<PlayerState>();
}