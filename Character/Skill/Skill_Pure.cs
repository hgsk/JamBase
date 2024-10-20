// ISkillEffect.cs
public interface ISkillEffect
{
    void Apply(ICharacter target);
}

// Skill.cs
public class Skill
{
    public string Name { get; set; }
    public int ManaCost { get; set; }
    public float Cooldown { get; set; }
    public ISkillEffect Effect { get; set; }

    public void Use(ICharacter user, ICharacter target)
    {
        if (user.CurrentMana >= ManaCost)
        {
            user.CurrentMana -= ManaCost;
            Effect.Apply(target);
            // クールダウンの処理は別途実装が必要
        }
    }
}

// ICharacter.cs
public interface ICharacter
{
    string Name { get; set; }
    int CurrentMana { get; set; }
    int MaxMana { get; set; }
    int CurrentHealth { get; set; }
    int MaxHealth { get; set; }
}

// DamageEffect.cs
public class DamageEffect : ISkillEffect
{
    public int DamageAmount { get; set; }

    public void Apply(ICharacter target)
    {
        target.CurrentHealth = Math.Max(0, target.CurrentHealth - DamageAmount);
    }
}

// HealEffect.cs
public class HealEffect : ISkillEffect
{
    public int HealAmount { get; set; }

    public void Apply(ICharacter target)
    {
        target.CurrentHealth = Math.Min(target.MaxHealth, target.CurrentHealth + HealAmount);
    }
}

// CompositeSkillEffect.cs
public class CompositeSkillEffect : ISkillEffect
{
    private List<ISkillEffect> effects = new List<ISkillEffect>();

    public void AddEffect(ISkillEffect effect)
    {
        effects.Add(effect);
    }

    public void Apply(ICharacter target)
    {
        foreach (var effect in effects)
        {
            effect.Apply(target);
        }
    }
}
