using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// Base interface for character control, whether AI or player-controlled
public interface ICharacterControl
{
    void UpdateControl();
}

public class AIControl : MonoBehaviour, ICharacterControl
{
    private AIBrain brain;
    private CharacterAction actionLayer;

    private void Awake()
    {
        brain = GetComponent<AIBrain>();
        actionLayer = GetComponent<CharacterAction>();
    }

    public void UpdateControl()
    {
        AIDecision decision = brain.MakeDecision();
        ExecuteDecision(decision);
    }

    private void ExecuteDecision(AIDecision decision)
    {
        switch (decision.Type)
        {
            case AIDecisionType.Move:
                actionLayer.Move(decision.MoveDirection);
                break;
            case AIDecisionType.Jump:
                actionLayer.TryJump();
                break;
            case AIDecisionType.Attack:
                actionLayer.TryAttack();
                break;
            case AIDecisionType.Idle:
                actionLayer.StopMoving();
                break;
        }
    }
}

// Player Control implementation
public class PlayerControl : MonoBehaviour, ICharacterControl
{
    private CharacterAction actionLayer;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    private void Awake()
    {
        actionLayer = GetComponent<CharacterAction>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];
    }

    public void UpdateControl()
    {
        if (jumpAction.triggered)
        {
            actionLayer.TryJump();
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        if (moveInput != Vector2.zero)
        {
            actionLayer.Move(moveInput);
        }
        else
        {
            actionLayer.StopMoving();
        }

        if (attackAction.triggered)
        {
            actionLayer.TryAttack();
        }
    }
}

// AI Brain for decision making
public class AIBrain : MonoBehaviour
{
    public float detectionRadius = 10f;
    public float attackRange = 2f;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public AIDecision MakeDecision()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            return new AIDecision { Type = AIDecisionType.Attack };
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            return new AIDecision { Type = AIDecisionType.Move, MoveDirection = direction };
        }
        else
        {
            return new AIDecision { Type = AIDecisionType.Idle };
        }
    }
}

public enum AIDecisionType
{
    Move,
    Jump,
    Attack,
    Idle
}

public struct AIDecision
{
    public AIDecisionType Type;
    public Vector2 MoveDirection;
}

// Action Layer: Determines high-level actions based on input or AI decisions
public class CharacterAction : MonoBehaviour
{
    public CharacterBody BodyLayer { get; private set; }
    public CharacterHead HeadLayer { get; private set; }
    public CharacterWeapon WeaponLayer { get; private set; }

    private void Awake()
    {
        BodyLayer = GetComponent<CharacterBody>();
        HeadLayer = GetComponent<CharacterHead>();
        WeaponLayer = GetComponent<CharacterWeapon>();
    }

    public void TryJump()
    {
        if (BodyLayer.CanJump())
        {
            BodyLayer.Jump();
        }
    }

    public void Move(Vector2 direction)
    {
        BodyLayer.Move(direction);
        HeadLayer.LookInDirection(direction);
    }

    public void StopMoving()
    {
        BodyLayer.StopMoving();
    }

    public void TryAttack()
    {
        if (WeaponLayer.CanAttack())
        {
            WeaponLayer.Attack();
            BodyLayer.PlayAttackAnimation();
        }
    }
}

public class CharacterHead : MonoBehaviour
{
    public void LookInDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }
}

public class CharacterWeapon : MonoBehaviour
{
    public bool CanAttack()
    {
        // Implement attack cooldown logic
        return true;
    }

    public void Attack()
    {
        // Implement attack logic
    }
}

// Body Layer: Handles character body movement and animation
public class CharacterBody : MonoBehaviour
{
    public HighLevelAnimation AnimationLayer { get; private set; }
    private CharacterController characterController;

    private void Awake()
    {
        AnimationLayer = GetComponent<HighLevelAnimation>();
        characterController = GetComponent<CharacterController>();
    }

    public bool CanJump()
    {
        return characterController.isGrounded;
    }

    public void Jump()
    {
        // Implement jump logic
        AnimationLayer.PlayAnimation("Jump");
    }

    public void Move(Vector2 direction)
    {
        Vector3 movement = new Vector3(direction.x, 0, direction.y);
        characterController.Move(movement * Time.deltaTime);
        AnimationLayer.PlayAnimation("Walk");
    }

    public void StopMoving()
    {
        AnimationLayer.PlayAnimation("Idle");
    }

    public void PlayAttackAnimation()
    {
        AnimationLayer.PlayAnimation("Attack");
    }
}

// Head and Weapon layers remain the same as in the previous implementation

// High Level Animation Layer: Determines which animations to play
public class HighLevelAnimation : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animationTrigger)
    {
        animator.SetTrigger(animationTrigger);
    }
}

// Main controller that can handle both AI and player-controlled characters
public class CharacterManager : MonoBehaviour
{
    private ICharacterControl controlLayer;

    private void Start()
    {
        // Determine if this is an AI or player-controlled character
        if (GetComponent<AIControl>() != null)
        {
            controlLayer = GetComponent<AIControl>();
        }
        else
        {
            controlLayer = GetComponent<PlayerControl>();
        }
    }

    private void Update()
    {
        controlLayer.UpdateControl();
    }
}
