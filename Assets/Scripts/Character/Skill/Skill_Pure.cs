// ISkillEffect.cs
using System;
using System.Collections.Generic;

public interface ISkillEffect
{
    //void Apply(ICharacter target);
}

namespace Skill.View
{

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
    public class DamageEffect //: ISkillEffect
    {
        public int DamageAmount { get; set; }

        public void Apply(ICharacter target)
        {
            target.CurrentHealth = Math.Max(0, target.CurrentHealth - DamageAmount);
        }
    }

    // HealEffect.cs
    public class HealEffect //: ISkillEffect
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
                //effect.Apply(target);
            }
        }
    }

}