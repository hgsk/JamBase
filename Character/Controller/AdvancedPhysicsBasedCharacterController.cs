using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

// 能力のインターフェース
public interface IAbility
{
    void Initialize(AdvancedPhysicsBasedCharacterController controller);
    void ProcessAbility(float deltaTime);
    void OnInputReceived(EnhancedInputComponent.InputContext context);
}

// キャラクターコントローラー
public class AdvancedPhysicsBasedCharacterController : MonoBehaviour
{
    [System.Serializable]
    public class InputEvents
    {
        public UnityEvent<Vector2> OnMove;
        public UnityEvent<bool> OnJump;
        public UnityEvent<string, EnhancedInputComponent.InputContext> OnCustomAction;
    }

    [Header("Input")]
    public InputEvents inputEvents;

    [Header("Movement")]
    public float maxSpeed = 5f;
    public float acceleration = 30f;
    public float maxAccelerationForce = 150f;
    public float turnAccelerationMultiplier = 2f;

    [Header("Floating")]
    public float desiredHeight = 1f;
    public float springForce = 100f;
    public float dampingForce = 10f;

    [Header("Orientation")]
    public float turnSpeed = 720f;

    // Public properties
    public Rigidbody Rigidbody { get; private set; }
    public bool IsGrounded { get; private set; }
    public Vector3 DesiredMoveDirection { get; private set; }

    private Vector3 currentMoveDirection;
    private Vector2 moveInput;
    private Dictionary<string, IAbility> abilities = new Dictionary<string, IAbility>();

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
        Rigidbody.useGravity = false;

        inputEvents.OnMove.AddListener(HandleMoveInput);
        inputEvents.OnCustomAction.AddListener(HandleCustomAction);

        InitializeAbilities();
    }

    void OnDisable()
    {
        inputEvents.OnMove.RemoveListener(HandleMoveInput);
        inputEvents.OnCustomAction.RemoveListener(HandleCustomAction);
    }

    void InitializeAbilities()
    {
        // Initialize and add abilities
        abilities["Jump"] = new JumpAbility();
        abilities["Dash"] = new DashAbility();

        foreach (var ability in abilities.Values)
        {
            ability.Initialize(this);
        }
    }

    public void SwitchInputStrategy(InputStrategySO newStrategy)
    {
        var inputHandler = GetComponent<CharacterInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.SetInputStrategy(newStrategy);
        }
    }

    public void SwitchInputStrategy(InputStrategySO newStrategy)
    {
        var inputHandler = GetComponent<CharacterInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.SetInputStrategy(newStrategy);
        }
    }
    public void SwitchInputHandler<T>() where T : CharacterInputHandler
    {
        var currentHandler = GetComponent<CharacterInputHandler>();
        if (currentHandler != null)
        {
            Destroy(currentHandler);
        }

        var newHandler = gameObject.AddComponent<T>();
        newHandler.inputEvents = this.inputEvents;
    }

    void HandleMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    void HandleCustomAction(string actionName, EnhancedInputComponent.InputContext context)
    {
        if (abilities.TryGetValue(actionName, out var ability))
        {
            ability.OnInputReceived(context);
        }
    }

    void FixedUpdate()
    {
        ApplyFloatingForce();
        HandleMovement();
        ProcessAbilities();
        KeepUpright();
    }

    void ProcessAbilities()
    {
        foreach (var ability in abilities.Values)
        {
            ability.ProcessAbility(Time.fixedDeltaTime);
        }
    }

    void ApplyFloatingForce()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, desiredHeight + 0.5f))
        {
            float distanceToGround = hit.distance;
            float heightError = desiredHeight - distanceToGround;
            float springForceAmount = heightError * springForce - Rigidbody.velocity.y * dampingForce;
            
            Rigidbody.AddForce(Vector3.up * springForceAmount);
            
            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(-Vector3.up * springForceAmount, hit.point);
            }

            IsGrounded = distanceToGround <= desiredHeight + 0.1f;
        }
        else
        {
            IsGrounded = false;
        }
    }

    void HandleMovement()
    {
        DesiredMoveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        
        Vector3 desiredVelocity = DesiredMoveDirection * maxSpeed;
        Vector3 velocityChange = desiredVelocity - Rigidbody.velocity;
        velocityChange.y = 0f;

        float directionDifference = Vector3.Angle(currentMoveDirection, DesiredMoveDirection) / 180f;
        float accelerationMultiplier = 1f + (turnAccelerationMultiplier - 1f) * directionDifference;

        Vector3 accelerationForce = velocityChange * (acceleration * accelerationMultiplier);
        float maxForce = maxAccelerationForce * accelerationMultiplier;

        if (accelerationForce.magnitude > maxForce)
        {
            accelerationForce = accelerationForce.normalized * maxForce;
        }

        Rigidbody.AddForce(accelerationForce);
        
        currentMoveDirection = Rigidbody.velocity.normalized;
    }

    void KeepUpright()
    {
        if (DesiredMoveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(DesiredMoveDirection, Vector3.up);
            Rigidbody.MoveRotation(Quaternion.RotateTowards(Rigidbody.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Physics.IgnoreFriction(contact);
        }
    }
}

// ジャンプ能力
public class JumpAbility : IAbility
{
    private AdvancedPhysicsBasedCharacterController controller;
    private float jumpForce = 5f;
    private float downwardForce = 20f;
    private float coyoteTime = 0.1f;
    private float jumpBufferTime = 0.1f;
    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool isJumpPressed;

    public void Initialize(AdvancedPhysicsBasedCharacterController controller)
    {
        this.controller = controller;
    }

    public void ProcessAbility(float deltaTime)
    {
        if (controller.IsGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (Time.time - lastGroundedTime <= coyoteTime && Time.time - lastJumpPressedTime <= jumpBufferTime)
        {
            PerformJump();
        }

        if (!isJumpPressed && controller.Rigidbody.velocity.y > 0)
        {
            controller.Rigidbody.AddForce(Vector3.down * downwardForce);
        }
    }

    public void OnInputReceived(EnhancedInputComponent.InputContext context)
    {
        if (context.IsPressed)
        {
            lastJumpPressedTime = Time.time;
        }
        isJumpPressed = context.IsPressed;
    }

    private void PerformJump()
    {
        controller.Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        lastJumpPressedTime = 0f;
    }
}

// ダッシュ能力
public class DashAbility : IAbility
{
    private AdvancedPhysicsBasedCharacterController controller;
    private float dashForce = 10f;
    private float dashDuration = 0.2f;
    private float dashCooldown = 1f;
    private float lastDashTime;
    private bool isDashing;

    public void Initialize(AdvancedPhysicsBasedCharacterController controller)
    {
        this.controller = controller;
    }

    public void ProcessAbility(float deltaTime)
    {
        if (isDashing)
        {
            if (Time.time - lastDashTime > dashDuration)
            {
                isDashing = false;
            }
        }
    }

    public void OnInputReceived(EnhancedInputComponent.InputContext context)
    {
        if (context.IsPressed && Time.time - lastDashTime > dashCooldown)
        {
            PerformDash();
        }
    }

    private void PerformDash()
    {
        Vector3 dashDirection = controller.DesiredMoveDirection != Vector3.zero 
            ? controller.DesiredMoveDirection 
            : controller.transform.forward;
        
        controller.Rigidbody.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        isDashing = true;
        lastDashTime = Time.time;
    }
}
