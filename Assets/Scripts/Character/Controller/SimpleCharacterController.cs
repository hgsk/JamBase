using UnityEngine;
using UnityEngine.InputSystem;
 
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class SimpleCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    
    private Rigidbody rb;
    private bool isGrounded;
    private Vector2 moveInput;

    private PlayerInput playerInput;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    // 移動入力用のパブリックメソッド
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            moveInput = context.ReadValue<Vector2>();
            Debug.Log($"Move Input: {moveInput}");
        }
    }

    // ジャンプ入力用のパブリックメソッド
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            Debug.Log("Jump input received");
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, 0f) * moveSpeed;
        float currentYVelocity = rb.linearVelocity.y;
        rb.linearVelocity = new Vector3(movement.x, currentYVelocity, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 地面との接触を検知
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}