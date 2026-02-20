public class PlayerInput {
    public uint currentTick;

    public byte flags;

    private const byte UP = 1 << 0;
    private const byte DOWN = 1 << 1;
    private const byte LEFT = 1 << 2;
    private const byte RIGHT = 1 << 3;
    private const byte JUMPING = 1 << 4;

    public bool up {
        get => (flags & UP) != 0;
        set => flags = value ? (byte)(flags | UP) : (byte)(flags & (0xFF ^ UP));
    }

    public bool down {
        get => (flags & DOWN) != 0;
        set => flags = value ? (byte)(flags | DOWN) : (byte)(flags & (0xFF ^ DOWN));
    }

    public bool left {
        get => (flags & LEFT) != 0;
        set => flags = value ? (byte)(flags | LEFT) : (byte)(flags & (0xFF ^ LEFT));
    }

    public bool right {
        get => (flags & RIGHT) != 0;
        set => flags = value ? (byte)(flags | RIGHT) : (byte)(flags & (0xFF ^ RIGHT));
    }

    public bool jumping {
        get => (flags & JUMPING) != 0;
        set => flags = value ? (byte)(flags | JUMPING) : (byte)(flags & (0xFF ^ JUMPING));
    }

    public float x() {
        return (right ? 1f : 0f) - (left ? 1f : 0f);
    }

    public float y() {
        return (up ? 1f : 0f) - (down ? 1f : 0f);
    }
}