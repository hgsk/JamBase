using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

// Skill interface
public interface ISkill
{
    string Name { get; }
    float Cooldown { get; }
    bool IsReady { get; }
    float MaxCooldown { get; }

    void Use(GameObject user);
    void UpdateCooldown(float deltaTime);
}

// Powerful Attack Skill implementation
public class PowerfulAttackSkill : ISkill
{
    public string Name => "Powerful Attack";
    public float Cooldown => 10f; // 10 seconds cooldown
    public bool IsReady => currentCooldown <= 0;
    public float MaxCooldown => Cooldown;

    private float currentCooldown = 0f;

    public void Use(GameObject user)
    {
        if (IsReady)
        {
            Debug.Log($"Using {Name}!");
            PerformPowerfulAttack(user);
            currentCooldown = Cooldown;
        }
        else
        {
            Debug.Log($"{Name} is not ready. Cooldown: {currentCooldown}");
        }
    }

    public void UpdateCooldown(float deltaTime)
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= deltaTime;
        }
    }

    private void PerformPowerfulAttack(GameObject user)
    {
        // Implement powerful attack logic here
        Debug.Log($"{user.name} performs a powerful attack!");
        
        // Example: Area of effect damage
        Collider[] hitColliders = Physics.OverlapSphere(user.transform.position, 3f);
        foreach (var hitCollider in hitColliders)
        {
            var enemy = hitCollider.GetComponent<IDamageable>();
            if (enemy != null)
            {
                enemy.TakeDamage(30f); // 30 damage for powerful attack
            }
        }

        // Trigger animation if available
        var animator = user.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("PowerfulAttack");
        }
    }
}

// Skill Manager component
public abstract class SkillManager : MonoBehaviour
{
    public List<ISkill> skills = new List<ISkill>();

    public UnityEvent OnSkillUsed { get; internal set; }

    void Awake()
    {
        // Add skills here
        skills.Add(new PowerfulAttackSkill());
    }

    void Update()
    {
        foreach (var skill in skills)
        {
            skill.UpdateCooldown(Time.deltaTime);
        }
    }

    public virtual void UseSkill(string skillName)
    {
        var skill = skills.Find(s => s.Name == skillName);
        if (skill != null)
        {
            skill.Use(gameObject);
        }
        else
        {
            Debug.LogWarning($"Skill {skillName} not found.");
        }
    }

    internal void AddSkill(Skill skill)
    {
        throw new NotImplementedException();
    }

    internal IEnumerable<ISkill> GetSkills()
    {
        return skills;
    }
}

// Skill User interface
public interface ISkillUser
{
    void UseSkill(string skillName);
}

// AI Skill User component
[RequireComponent(typeof(SkillManager))]
public class AISkillUser : MonoBehaviour, ISkillUser
{
    private SkillManager skillManager;

    private void Awake()
    {
        skillManager = GetComponent<SkillManager>();
    }

    public void UseSkill(string skillName)
    {
        skillManager.UseSkill(skillName);
    }

    // This method would be called by your AI decision-making system
    public void ConsiderUsingSkill()
    {
        if (Random.value < 0.2f) // 20% chance to use skill
        {
            UseSkill("Powerful Attack");
        }
    }
}

// Player Skill User component
[RequireComponent(typeof(SkillManager))]
public class PlayerSkillUser : MonoBehaviour, ISkillUser
{
    public InputAction inputAction;
    private SkillManager skillManager;

    private void Awake()
    {
        inputAction = new InputAction("UseSkill", binding: "<Keyboard>/q");
        skillManager = GetComponent<SkillManager>();
        
    }

    private void Update()
    {
        // Example: Use skill on key press
        if (inputAction.triggered)
        {
            UseSkill("Powerful Attack");
        }
    }

    public void UseSkill(string skillName)
    {
        skillManager.UseSkill(skillName);
    }
}

// Interface for damageable entities (if not already defined)
public interface IDamageable
{
    void TakeDamage(float damage);
}
