using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public static PlayerMovement Instance;

    private Vector3 inputDirection;
    private Vector3 velocity;
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public Rigidbody rb;
    private bool isGrounded;

    private void Awake() {
        Instance = this;
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }


    public void SetInput(float x, float y, bool jump) {
        Vector3 moveDir = new Vector3(x, 0, y);
        inputDirection = moveDir.normalized;

        if (jump && isGrounded) {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Sqrt(jumpHeight * -2f * gravity), rb.velocity.z);
        }
    }

    public void AdvanceLogic() {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        Vector3 move = inputDirection * speed;
        rb.velocity = new Vector3(move.x, rb.velocity.y + (gravity * NetworkSettings.tickTime), move.z);
    }
}