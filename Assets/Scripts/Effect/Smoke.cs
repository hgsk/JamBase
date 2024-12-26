// Domain/SmokeParameters.cs
// UnityEngineに依存しない純粋なドメインモデル
using UnityEngine;

public record SmokeParameters
{
    public float Lifetime;
    public float SpreadRadius;
    public float RiseSpeed;
    public float DissipationRate;
    public int ParticleCount;
}

// Domain/SmokeType.cs
// 煙の種類を表す列挙型
public enum SmokeType
{
    HotSpring,    // 湯けむり
    BreathVapor,  // 白い吐息
    SmallFire,    // 小火の煙
    Explosion     // 爆発後の煙
}

// Domain/ISmokeEffect.cs
// エフェクトの基本インターフェース
public interface ISmokeEffect
{
    void Initialize(SmokeParameters parameters);
    void UpdateEffect(float deltaTime);
    bool IsComplete { get; }
}

// ScriptableObjects/SmokeBehaviorData.cs

[CreateAssetMenu(fileName = "SmokeBehavior", menuName = "Effects/Smoke Behavior")]
public class SmokeBehaviorData : ScriptableObject
{
    [Header("基本パラメータ")]
    [SerializeField] private float baseLifetime = 3f;
    [SerializeField] private float baseSpreadRadius = 1f;
    [SerializeField] private float baseRiseSpeed = 1f;
    [SerializeField] private float baseDissipationRate = 1f;
    [SerializeField] private int baseParticleCount = 50;

    [Header("環境影響係数")]
    [SerializeField] private float windInfluence = 1f;
    [SerializeField] private float temperatureInfluence = 1f;

    public SmokeParameters CreateParameters()
    {
        return new SmokeParameters
        {
            Lifetime = baseLifetime,
            SpreadRadius = baseSpreadRadius,
            RiseSpeed = baseRiseSpeed,
            DissipationRate = baseDissipationRate,
            ParticleCount = baseParticleCount
        };
    }
}

// Runtime/SmokeEffectController.cs

public class SmokeEffectController : MonoBehaviour
{
    [SerializeField] private SmokeBehaviorData behaviorData;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private SmokeType smokeType;

    private ISmokeEffect currentEffect;

    private void Start()
    {
        InitializeEffect();
    }

    private void InitializeEffect()
    {
        var parameters = behaviorData.CreateParameters();
        currentEffect = smokeType switch
        {
            SmokeType.HotSpring => new HotSpringEffect(particleSystem),
            SmokeType.BreathVapor => new BreathVaporEffect(particleSystem),
            SmokeType.SmallFire => new SmallFireEffect(particleSystem),
            SmokeType.Explosion => new ExplosionEffect(particleSystem),
            _ => throw new System.ArgumentException("Invalid smoke type")
        };

        currentEffect.Initialize(parameters);
    }

    private void Update()
    {
        if (currentEffect != null && !currentEffect.IsComplete)
        {
            currentEffect.UpdateEffect(Time.deltaTime);
        }
    }
}

internal class ExplosionEffect : SmallFireEffect
{
    private ParticleSystem particleSystem;
    public ExplosionEffect(ParticleSystem particleSystem) : base(particleSystem)
    {
        this.particleSystem = particleSystem;
    }
}

internal class SmallFireEffect : BreathVaporEffect
{
    private ParticleSystem particleSystem;
    public SmallFireEffect(ParticleSystem particleSystem) : base(particleSystem)
    {
        this.particleSystem = particleSystem;
    }
}

internal class BreathVaporEffect : HotSpringEffect
{
    private ParticleSystem particleSystem;
    public BreathVaporEffect(ParticleSystem particleSystem) : base(particleSystem)
    {
        this.particleSystem = particleSystem;
    }
}

// Effects/HotSpringEffect.cs

public class HotSpringEffect : ISmokeEffect
{
    private ParticleSystem particleSystem;
    private SmokeParameters parameters;
    private float elapsedTime;

    public HotSpringEffect(ParticleSystem particleSystem)
    {
        this.particleSystem = particleSystem;
    }

    public bool IsComplete => elapsedTime >= parameters.Lifetime;

    public void Initialize(SmokeParameters parameters)
    {
        this.parameters = parameters;
        elapsedTime = 0f;

        var main = particleSystem.main;
        main.duration = parameters.Lifetime;
        main.startLifetime = parameters.Lifetime;

        var emission = particleSystem.emission;
        emission.rateOverTime = parameters.ParticleCount / parameters.Lifetime;

        var shape = particleSystem.shape;
        shape.radius = parameters.SpreadRadius;

        particleSystem.Play();
    }

    public void UpdateEffect(float deltaTime)
    {
        elapsedTime += deltaTime;

        // 湯けむり特有の上昇と拡散の動き
        var velocityOverLifetime = particleSystem.velocityOverLifetime;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(
            parameters.RiseSpeed * (1f - elapsedTime / parameters.Lifetime)
        );
    }
}