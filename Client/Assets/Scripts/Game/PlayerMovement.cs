using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement Instance;
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public Rigidbody rb;

    private Vector3 inputDirection;
    private bool isGrounded;
    private bool jumping;
    private Vector3 velocity;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    public void SetInput(float x, float y, bool jump) {
        var moveDir = new Vector3(x, 0, y);
        inputDirection = moveDir.normalized;
        jumping = jump;
    }

    public void AdvanceLogic() {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        if (jumping && isGrounded)
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Sqrt(jumpHeight * -2f * gravity), rb.velocity.z);

        var move = inputDirection * speed;
        rb.velocity = new Vector3(move.x, rb.velocity.y + gravity * NetworkSettings.tickTime, move.z);
    }
}