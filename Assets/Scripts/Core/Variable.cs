using System;
using System.Collections.Generic;

namespace Core.Data
{
    public interface IData
    {
        string Id { get; }
        void OnValidate();
        void Initialize();
    }

    public abstract class Data : IData
    {
        public string Id { get; protected set; }
        public virtual void OnValidate() {}
        public virtual void Initialize() {}
    }
}

namespace Core.Variables
{
    public interface IValueObserver<T>
    {
        void OnChanged(T oldValue, T newValue);
    }

    public class Variable<T>
    {
        T value;
        readonly HashSet<IValueObserver<T>> observers = new();

        public T Value
        {
            get => value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    var old = this.value;
                    this.value = value;
                    foreach (var observer in observers)
                        observer.OnChanged(old, value);
                }
            }
        }

        public void AddObserver(IValueObserver<T> observer)
        {
            observers.Add(observer);
            observer.OnChanged(value, value);
        }

        public void RemoveObserver(IValueObserver<T> observer) => 
            observers.Remove(observer);
    }

    public class ValueObserver<T> : IValueObserver<T>
    {
        readonly Action<T> onValueChanged;
        public ValueObserver(Action<T> action) => onValueChanged = action;
        public void OnChanged(T oldValue, T newValue) => onValueChanged?.Invoke(newValue);
    }

    public static class VariableExtensions
    {
        public static void Bind<T>(this Variable<T> source, Variable<T> target) =>
            source.AddObserver(new ValueObserver<T>(value => target.Value = value));

        public static void Bind<T>(this Variable<T> source, Action<T> action) =>
            source.AddObserver(new ValueObserver<T>(action));
    }
}

namespace Unity.Variables
{
    using Core.Variables;
    using UnityEngine;

     public abstract class ScriptableVariable<T> : ScriptableObject
    {
        [SerializeField] T defaultValue;
        [SerializeField] bool resetOnPlay = true;
        
        readonly Variable<T> variable = new();
        
        public T Value
        {
            get => variable.Value;
            set => variable.Value = value;
        }

        void OnEnable()
        {
            if (resetOnPlay) variable.Value = defaultValue;
        }

        public void Bind(Variable<T> target) => 
            variable.Bind(target);

        public void Bind(Action<T> action) => 
            variable.Bind(action);

        // 既存のメソッドは残しておく（後方互換性のため）
        public void AddObserver(IValueObserver<T> observer) => 
            variable.AddObserver(observer);

        public void RemoveObserver(IValueObserver<T> observer) => 
            variable.RemoveObserver(observer);
    } 

    [CreateAssetMenu(fileName = "Int", menuName = "Variables/Int")]
    public class IntVariable : ScriptableVariable<int> { }

    [CreateAssetMenu(fileName = "Float", menuName = "Variables/Float")]
    public class FloatVariable : ScriptableVariable<float> { }
}

namespace Game.Character
{
    using Core.Data;
    using Core.Variables;

    public class CharacterStatus : Data
    {
        // プロパティをシンプルかつ直感的に定義
        public Variable<string> Name;
        public Variable<int> Level;
        public Variable<float> Health;
        public Variable<float> MaxHealth;
        public Variable<float> Attack;
        public Variable<float> Defense;

        public override void Initialize()
        {
            Level.Value = 1;
            MaxHealth.Value = 100;
            Health.Value = MaxHealth.Value;
            Attack.Value = 10;
            Defense.Value = 5;
        }

        public void Damage(float amount) =>
            Health.Value = Math.Max(0, Health.Value - amount);

        public void Heal(float amount) =>
            Health.Value = Math.Min(MaxHealth.Value, Health.Value + amount);
    }
}

namespace Game.Enemy
{
    using Core.Data;
    using Core.Variables;

    public enum EnemyType { Normal, Elite, Boss }
    public enum AttackPattern { Melee, Ranged, Magic }

    public class EnemyStatus : Data
    {
        public Variable<string> Name;
        public Variable<EnemyType> Type;
        public Variable<int> Level;
        public Variable<float> Health;
        public Variable<float> MaxHealth;
        public Variable<float> Attack;
        public Variable<float> Defense;
        public Variable<float> MoveSpeed;
        public Variable<float> AttackRange;
        public Variable<float> DetectionRange;
        public Variable<AttackPattern> Pattern;
        public Variable<int> ExperienceValue;

        public override void Initialize()
        {
            Level.Value = 1;
            Type.Value = EnemyType.Normal;
            Pattern.Value = AttackPattern.Melee;
            InitializeStats();
        }

        void InitializeStats()
        {
            var typeMultiplier = Type.Value switch
            {
                EnemyType.Normal => 1.0f,
                EnemyType.Elite => 1.5f,
                EnemyType.Boss => 3.0f,
                _ => 1.0f
            };

            MaxHealth.Value = 50 * typeMultiplier;
            Health.Value = MaxHealth.Value;
            Attack.Value = 8 * typeMultiplier;
            Defense.Value = 3 * typeMultiplier;
            MoveSpeed.Value = 3;
            AttackRange.Value = Pattern.Value switch
            {
                AttackPattern.Melee => 1.5f,
                AttackPattern.Ranged => 8f,
                AttackPattern.Magic => 12f,
                _ => 1.5f
            };
            DetectionRange.Value = AttackRange.Value + 5f;
            ExperienceValue.Value = (int)(25 * typeMultiplier);
        }

        public void Damage(float amount) =>
            Health.Value = Math.Max(0, Health.Value - amount);

        public void Heal(float amount) =>
            Health.Value = Math.Min(MaxHealth.Value, Health.Value + amount);

        public bool IsInRange(float distance) =>
            distance <= AttackRange.Value;

        public bool CanDetectTarget(float distance) =>
            distance <= DetectionRange.Value;
    }
}

namespace Game.Data
{
    using Enemy;
    using Unity.Variables;
    using UnityEngine;
    using Core.Variables;
    using Game.Components;
    using Game.Character;

    [CreateAssetMenu(fileName = "Character", menuName = "Game/Character")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField] string characterName;
        [SerializeField] FloatVariable maxHealth;
        [SerializeField] FloatVariable attack;
        [SerializeField] FloatVariable defense;
        
        private CharacterStatus status;
        public CharacterStatus Runtime => status ??= CreateCharacterStatus();

        private CharacterStatus CreateCharacterStatus()
        {
            var newStatus = new CharacterStatus();
            newStatus.Name.Value = characterName;
            
            maxHealth.Bind(newStatus.MaxHealth);
            attack.Bind(newStatus.Attack);
            defense.Bind(newStatus.Defense);
            
            newStatus.Initialize();
            return newStatus;
        }
    }
    [CreateAssetMenu(fileName = "Enemy", menuName = "Game/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        [Required]
        [SerializeField] string enemyName;
        [SerializeField] EnemyType type;
        [SerializeField] AttackPattern attackPattern;
        
        [Header("Base Stats")]
        [Required]
        [SerializeField] FloatVariable baseHealth;
        [Required]
        [SerializeField] FloatVariable baseAttack;
        [Required]
        [SerializeField] FloatVariable baseDefense;
        [Required]
        [SerializeField] FloatVariable moveSpeed;
        
        [Header("Combat Settings")]
        [Required]
        [SerializeField] FloatVariable attackRange;
        [Required]
        [SerializeField] FloatVariable detectionRange;
        [Required]
        [SerializeField] IntVariable experienceValue;

        EnemyStatus status;
        public EnemyStatus Runtime => status ??= CreateStatus();

        EnemyStatus CreateStatus()
        {
            var newStatus = new EnemyStatus();
            newStatus.Name.Value = enemyName;
            newStatus.Type.Value = type;
            newStatus.Pattern.Value = attackPattern;
            
            baseHealth.Bind(newStatus.MaxHealth);
            baseAttack.Bind(newStatus.Attack);
            baseDefense.Bind(newStatus.Defense);
            moveSpeed.Bind(newStatus.MoveSpeed);
            attackRange.Bind(newStatus.AttackRange);
            detectionRange.Bind(newStatus.DetectionRange);
            experienceValue.Bind(newStatus.ExperienceValue);
            
            newStatus.Initialize();
            return newStatus;
        }
    }
}
namespace Game.Components
{
    using UnityEngine;
    using Game.Data;
    using Core.Variables;
    using Game.Enemy;
    using System.Collections;
    using Unity.Variables;
    using Game.Character;
    
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public class RequiredAttribute : PropertyAttribute { }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (property.objectReferenceValue == null)
            {
                var originalColor = GUI.color;
                GUI.color = new Color(1, 0.4f, 0.4f);
                EditorGUI.PropertyField(position, property, label);
                GUI.color = originalColor;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

            EditorGUI.EndProperty();
        }
    }
    #endif

    public class Enemy : MonoBehaviour
    {
        [Required]
        [SerializeField] EnemyData data;
        [Required]
        [SerializeField] UI.HealthBar healthBar;
        [SerializeField] float attackInterval = 2f;
        
        EnemyStatus status;
        Transform target;
        bool isAttacking;

        void Start()
        {
            status = data.Runtime;
            status.Health.Bind(_ => UpdateUI());
            UpdateUI();
        }

        void UpdateUI() => 
            healthBar.SetHealth(status.Health.Value, status.MaxHealth.Value);

        void Update()
        {
            if (target == null) return;

            var distance = Vector3.Distance(transform.position, target.position);
            
            if (status.CanDetectTarget(distance))
            {
                if (status.IsInRange(distance) && !isAttacking)
                    StartCoroutine(AttackRoutine());
                else if (!status.IsInRange(distance))
                    MoveTowardsTarget();
            }
        }

        void MoveTowardsTarget()
        {
            var direction = (target.position - transform.position).normalized;
            transform.position += direction * (status.MoveSpeed.Value * Time.deltaTime);
        }

        IEnumerator AttackRoutine()
        {
            isAttacking = true;
            
            switch (status.Pattern.Value)
            {
                case AttackPattern.Melee:
                    PerformMeleeAttack();
                    break;
                case AttackPattern.Ranged:
                    PerformRangedAttack();
                    break;
                case AttackPattern.Magic:
                    PerformMagicAttack();
                    break;
            }

            yield return new WaitForSeconds(attackInterval);
            isAttacking = false;
        }

        void PerformMeleeAttack()
        {
            if (target.TryGetComponent<Character>(out var character))
                character.Damage(status.Attack.Value);
        }

        void PerformRangedAttack()
        {
            // 遠距離攻撃の実装
        }

        void PerformMagicAttack()
        {
            // 魔法攻撃の実装
        }

        public void SetTarget(Transform newTarget) => 
            target = newTarget;

        public void Damage(float amount)
        {
            status.Damage(amount);
            if (status.Health.Value <= 0)
                Die();
        }

        void Die()
        {
            if (target.TryGetComponent<Character>(out var character))
                character.AddExperience(status.ExperienceValue.Value);
            
            Destroy(gameObject);
        }
    }

    public class Character : MonoBehaviour
    {
        [Required]
        [SerializeField] CharacterData data;
        [Required]
        [SerializeField] UI.HealthBar healthBar;
        [Required]
        [SerializeField] IntVariable experience;

        CharacterStatus status;

        void Start()
        {
            status = data.Runtime;
            status.Health.Bind(_ => UpdateUI());
            UpdateUI();
        }

        void UpdateUI() => 
            healthBar.SetHealth(status.Health.Value, status.MaxHealth.Value);

        public void Damage(float amount) => status.Damage(amount);

        public void AddExperience(int amount) => 
            experience.Value += amount;
    }
}

namespace Game.UI
{
    using UnityEngine;
    using UnityEngine.UI;
    using Game.Components;

    public class HealthBar : MonoBehaviour
    {
        [Required]
        [SerializeField] Image fillImage;
        [Required]
        [SerializeField] Text healthText;

        public void SetHealth(float current, float max)
        {
            fillImage.fillAmount = current / max;
            healthText.text = $"{Mathf.Ceil(current)}/{Mathf.Ceil(max)}";
        }
    }
}


