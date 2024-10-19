using UnityEngine;
using UnityEngine.InputSystem;

public class DoubleJumpCharacterController : AdvancedPhysicsBasedCharacterController
{
    InputAction jumpAction;
    [Header("Double Jump")]
    public float doubleJumpForce = 4f; // Usually slightly lower than the initial jump
    public float doubleJumpCooldown = 0.1f; // Small cooldown to prevent accidental double jumps

    private bool canDoubleJump = false;
    private bool hasDoubleJumped = false;
    private float lastJumpTime = 0f;

    // Override the HandleJump method to add double jump functionality
    protected new void HandleJump()
    {
        base.HandleJump(); // Call the original HandleJump method

        if (IsGrounded)
        {
            canDoubleJump = false;
            hasDoubleJumped = false;
        }

        if (jumpAction.triggered)
        {
            if (!IsGrounded && canDoubleJump && !hasDoubleJumped && Time.time - lastJumpTime > doubleJumpCooldown)
            {
                PerformDoubleJump();
            }
            else if (IsGrounded)
            {
                canDoubleJump = true;
                lastJumpTime = Time.time;
            }
        }
    }

    private void PerformDoubleJump()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset vertical velocity
        rb.AddForce(Vector3.up * doubleJumpForce, ForceMode.Impulse);
        hasDoubleJumped = true;
        canDoubleJump = false;
        lastJumpTime = Time.time;
    }
}
