using UnityEngine;
using System.Collections.Generic;

// プロジェクタイルの共有データ
public class ProjectileFlyweight
{
    public ProjectileInfoSO info;
    public ProjectileBehaviorSO behavior;

    public ProjectileFlyweight(ProjectileInfoSO info)
    {
        this.info = info;
        this.behavior = info.behavior;
    }
}

// Flyweight Factory
public class ProjectileFlyweightFactory
{
    private Dictionary<string, ProjectileFlyweight> flyweights = new Dictionary<string, ProjectileFlyweight>();

    public ProjectileFlyweight GetFlyweight(ProjectileInfoSO info)
    {
        if (!flyweights.TryGetValue(info.projectileName, out ProjectileFlyweight flyweight))
        {
            flyweight = new ProjectileFlyweight(info);
            flyweights[info.projectileName] = flyweight;
        }
        return flyweight;
    }
}

// プロジェクタイルの固有状態
public class ProjectileState
{
    public Vector3 position;
    public Vector3 direction;
    public float currentLifeTime;
    public Dictionary<string, float> runtimeProperties = new Dictionary<string, float>();

    public ProjectileState(Vector3 startPosition, Vector3 direction)
    {
        this.position = startPosition;
        this.direction = direction.normalized;
        this.currentLifeTime = 0f;
    }
}

public class ProjectileVisual : MonoBehaviour
{
    public void UpdatePosition(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }
}

public class ProjectileInstance
{
    private ProjectileFlyweight flyweight;
    public ProjectileState state;
    public ProjectileVisual visual;

    public ProjectileInstance(ProjectileFlyweight flyweight, Vector3 startPosition, Vector3 direction, ProjectileVisual visual)
    {
        this.flyweight = flyweight;
        this.state = new ProjectileState(startPosition, direction);
        this.visual = visual;

        foreach (var prop in flyweight.info.specialProperties)
        {
            state.runtimeProperties[prop.key] = prop.value;
        }

        flyweight.behavior.Initialize(this);
        UpdateVisual();
    }

    public void Update(float deltaTime)
    {
        state.currentLifeTime += deltaTime;
        flyweight.behavior.Update(this);
        UpdateVisual();
    }

    public bool IsActive()
    {
        return state.currentLifeTime < flyweight.info.lifeTime;
    }

    public void SetRuntimeProperty(string key, float value)
    {
        state.runtimeProperties[key] = value;
    }

    public float GetRuntimeProperty(string key, float defaultValue = 0f)
    {
        return state.runtimeProperties.TryGetValue(key, out float value) ? value : defaultValue;
    }

    public void OnCollision(GameObject target)
    {
        flyweight.behavior.OnCollision(this, target);
    }

    private void UpdateVisual()
    {
        if (visual != null)
        {
            visual.UpdatePosition(state.position, Quaternion.LookRotation(state.direction));
        }
    }

    // Flyweight データへのアクセサ
    public ProjectileInfoSO Info => flyweight.info;
    public ProjectileBehaviorSO Behavior => flyweight.behavior;
}

public class ProjectileManager : MonoBehaviour
{
    public List<ProjectileInfoSO> projectileTypes;
    private ProjectileFlyweightFactory flyweightFactory = new ProjectileFlyweightFactory();
    private Dictionary<string, ObjectPool<ProjectileVisual>> projectilePools = new Dictionary<string, ObjectPool<ProjectileVisual>>();
    private List<ProjectileInstance> activeProjectiles = new List<ProjectileInstance>();

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var projectileInfo in projectileTypes)
        {
            projectilePools[projectileInfo.projectileName] = new ObjectPool<ProjectileVisual>(projectileInfo.projectilePrefab.GetComponent<ProjectileVisual>());
        }
    }

    public void FireProjectile(ProjectileInfoSO projectileInfo, Vector3 startPosition, Vector3 direction)
    {
        ProjectileFlyweight flyweight = flyweightFactory.GetFlyweight(projectileInfo);
        ProjectileVisual visual = projectilePools[projectileInfo.projectileName].Get();
        var projectile = new ProjectileInstance(flyweight, startPosition, direction, visual);
        activeProjectiles.Add(projectile);
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = activeProjectiles.Count - 1; i >= 0; i--)
        {
            var projectile = activeProjectiles[i];
            projectile.Update(deltaTime);

            if (CheckCollision(projectile, out GameObject hitObject))
            {
                projectile.OnCollision(hitObject);
                DeactivateProjectile(projectile, i);
            }
            else if (!projectile.IsActive())
            {
                DeactivateProjectile(projectile, i);
            }
        }
    }

    private void DeactivateProjectile(ProjectileInstance projectile, int index)
    {
        projectilePools[projectile.Info.projectileName].ReturnToPool(projectile.visual);
        activeProjectiles.RemoveAt(index);
    }

    private bool CheckCollision(ProjectileInstance projectile, out GameObject hitObject)
    {
        // Implement collision detection logic here
        hitObject = null;
        return false;
    }
}

public abstract class ProjectileBehaviorSO : ScriptableObject
{
    public abstract void Initialize(ProjectileInstance projectile);
    public abstract void Update(ProjectileInstance projectile);
    public abstract void OnCollision(ProjectileInstance projectile, GameObject target);
}


[CreateAssetMenu(fileName = "Standard Projectile Behavior", menuName = "Projectiles/Behaviors/Standard")]
public class StandardProjectileBehaviorSO : ProjectileBehaviorSO
{
    public override void Initialize(ProjectileInstance projectile) { }

    public override void Update(ProjectileInstance projectile)
    {
        projectile.state.position += projectile.state.direction * projectile.Info.speed * Time.deltaTime;
    }

    public override void OnCollision(ProjectileInstance projectile, GameObject target)
    {
        ApplyDamage(target, projectile.Info.baseDamage);
    }

    protected void ApplyDamage(GameObject target, float damage)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}
