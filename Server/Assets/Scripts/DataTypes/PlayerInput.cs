public class PlayerInput {
    public uint currentTick;
    public bool down;
    public bool jumping;
    public bool left;
    public bool right;
    public bool up;

    public float x() {
        return (right ? 1f : 0f) - (left ? 1f : 0f);
    }

    public float y() {
        return (up ? 1f : 0f) - (down ? 1f : 0f);
    }
}