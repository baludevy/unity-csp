public class PlayerInput {
    public int currentTick;
    public int lastRenderedTick;
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public bool jumping;

    public float x() {
        return (right ? 1f : 0f) - (left ? 1f : 0f);
    }

    public float y() {
        return (up ? 1f : 0f) - (down ? 1f : 0f);
    }
}