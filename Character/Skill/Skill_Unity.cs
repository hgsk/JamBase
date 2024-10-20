using UnityEngine;
using UnityEngine.Events;

// SkillScriptableObject.cs
[CreateAssetMenu(fileName = "New Skill", menuName = "RPG/Skill")]
public class SkillScriptableObject : ScriptableObject
{
    public string skillName;
    public int manaCost;
    public float cooldown;
    public SkillEffectScriptableObject effect;

    public Skill CreateSkill()
    {
        return new Skill
        {
            Name = skillName,
            ManaCost = manaCost,
            Cooldown = cooldown,
            Effect = effect.CreateEffect()
        };
    }
}

// SkillEffectScriptableObject.cs
public abstract class SkillEffectScriptableObject : ScriptableObject
{
    public abstract ISkillEffect CreateEffect();
}

// DamageEffectScriptableObject.cs
[CreateAssetMenu(fileName = "New Damage Effect", menuName = "RPG/Skill Effects/Damage")]
public class DamageEffectScriptableObject : SkillEffectScriptableObject
{
    public int damageAmount;

    public override ISkillEffect CreateEffect()
    {
        return new DamageEffect { DamageAmount = damageAmount };
    }
}

// HealEffectScriptableObject.cs
[CreateAssetMenu(fileName = "New Heal Effect", menuName = "RPG/Skill Effects/Heal")]
public class HealEffectScriptableObject : SkillEffectScriptableObject
{
    public int healAmount;

    public override ISkillEffect CreateEffect()
    {
        return new HealEffect { HealAmount = healAmount };
    }
}

// CharacterBehaviour.cs
public class CharacterBehaviour : MonoBehaviour, ICharacter
{
    public string Name { get; set; }
    public int CurrentMana { get; set; }
    public int MaxMana { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    public UnityEvent<int> OnHealthChanged;
    public UnityEvent<int> OnManaChanged;

    private void Start()
    {
        OnHealthChanged.Invoke(CurrentHealth);
        OnManaChanged.Invoke(CurrentMana);
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged.Invoke(CurrentHealth);
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged.Invoke(CurrentHealth);
    }

    public void UseMana(int amount)
    {
        CurrentMana = Mathf.Max(0, CurrentMana - amount);
        OnManaChanged.Invoke(CurrentMana);
    }
}

// SkillUser.cs
public class SkillUser : MonoBehaviour
{
    public SkillScriptableObject[] skillDefinitions;
    private List<Skill> skills = new List<Skill>();
    private CharacterBehaviour character;

    private void Start()
    {
        character = GetComponent<CharacterBehaviour>();
        foreach (var skillDef in skillDefinitions)
        {
            skills.Add(skillDef.CreateSkill());
        }
    }

    public void UseSkill(int index, CharacterBehaviour target)
    {
        if (index >= 0 && index < skills.Count)
        {
            skills[index].Use(character, target);
        }
    }
}
