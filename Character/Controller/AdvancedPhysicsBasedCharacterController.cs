using UnityEngine;

public class AdvancedPhysicsBasedCharacterController : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 30f;
    public float maxAccelerationForce = 150f;
    public float turnAccelerationMultiplier = 2f;

    [Header("Floating")]
    public float desiredHeight = 1f;
    public float springForce = 100f;
    public float dampingForce = 10f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float downwardForce = 20f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Orientation")]
    public float turnSpeed = 720f;

    private Rigidbody rb;
    private bool isGrounded;
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private Vector3 currentMoveDirection;
    private Vector3 desiredMoveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        ApplyFloatingForce();
        HandleMovement();
        HandleJump();
        KeepUpright();
    }

    void ApplyFloatingForce()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, desiredHeight + 0.5f))
        {
            float distanceToGround = hit.distance;
            float heightError = desiredHeight - distanceToGround;
            float springForceAmount = heightError * springForce - rb.velocity.y * dampingForce;
            
            rb.AddForce(Vector3.up * springForceAmount);
            
            // Apply force to object under character
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(-Vector3.up * springForceAmount, hit.point);
            }
        }
    }

    void HandleMovement()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        desiredMoveDirection = new Vector3(input.x, 0f, input.y).normalized;
        
        // Adjust for camera angle if needed
        // desiredMoveDirection = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * desiredMoveDirection;

        Vector3 desiredVelocity = desiredMoveDirection * maxSpeed;
        Vector3 velocityChange = desiredVelocity - rb.velocity;
        velocityChange.y = 0f;

        float directionDifference = Vector3.Angle(currentMoveDirection, desiredMoveDirection) / 180f;
        float accelerationMultiplier = 1f + (turnAccelerationMultiplier - 1f) * directionDifference;

        Vector3 accelerationForce = velocityChange * (acceleration * accelerationMultiplier);
        float maxForce = maxAccelerationForce * accelerationMultiplier;

        if (accelerationForce.magnitude > maxForce)
        {
            accelerationForce = accelerationForce.normalized * maxForce;
        }

        rb.AddForce(accelerationForce);
        
        currentMoveDirection = rb.velocity.normalized;
    }

    void HandleJump()
    {
        if (IsGrounded())
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressedTime = Time.time;
        }

        if (Time.time - lastGroundedTime <= coyoteTime && Time.time - lastJumpPressedTime <= jumpBufferTime)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpPressedTime = 0f;
        }

        // Analog jump: apply downward force when button is released or at jump peak
        if (!Input.GetButton("Jump") && rb.velocity.y > 0)
        {
            rb.AddForce(Vector3.down * downwardForce);
        }
    }

    void KeepUpright()
    {
        Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, desiredHeight + 0.1f);
    }

    void OnCollisionStay(Collision collision)
    {
        // Reduce friction between character and world objects
        foreach (ContactPoint contact in collision.contacts)
        {
            Physics.IgnoreFriction(contact);
        }
    }
}
