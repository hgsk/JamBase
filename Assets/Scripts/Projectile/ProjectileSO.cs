using System.Collections.Generic;
using UnityEngine;

public class ProjectileInfoSO : ScriptableObject {
    public string projectileName;
    public float speed;
    public float baseDamage;
    public ProjectileBehaviorSO behavior;
    public float lifeTime;
    public GameObject projectilePrefab; 
    public List<SpecialProperty> specialProperties;
}

public class SpecialProperty {
    public string key;
    public float value;
}

// 基本的なプロジェクタイル振る舞い
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

// 貫通プロジェクタイル
[CreateAssetMenu(fileName = "Piercing Projectile Behavior", menuName = "Projectiles/Behaviors/Piercing")]
public class PiercingProjectileBehaviorSO : ProjectileBehaviorSO
{
    public int maxPenetrations = 3;
    public float damageReductionPerPenetration = 0.2f;

    public override void Initialize(ProjectileInstance projectile)
    {
        projectile.SetRuntimeProperty("Penetrations", 0);
    }

    public override void Update(ProjectileInstance projectile)
    {
        projectile.state.position += projectile.state.direction * projectile.Info.speed * Time.deltaTime;
    }

    public override void OnCollision(ProjectileInstance projectile, GameObject target)
    {
        int penetrations = (int)projectile.GetRuntimeProperty("Penetrations");
        if (penetrations < maxPenetrations)
        {
            float currentDamage = projectile.Info.baseDamage * (1f - damageReductionPerPenetration * penetrations);
            ApplyDamage(target, currentDamage);
            projectile.SetRuntimeProperty("Penetrations", penetrations + 1);
        }
        else
        {
            // 最大貫通数に達した場合、プロジェクタイルを非アクティブにする処理をここに追加
        }
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}

// 爆発プロジェクタイル
[CreateAssetMenu(fileName = "Explosive Projectile Behavior", menuName = "Projectiles/Behaviors/Explosive")]
public class ExplosiveProjectileBehaviorSO : ProjectileBehaviorSO
{
    public float explosionRadius = 5f;
    public float explosionForce = 500f;

    public override void Initialize(ProjectileInstance projectile) { }

    public override void Update(ProjectileInstance projectile)
    {
        projectile.state.position += projectile.state.direction * projectile.Info.speed * Time.deltaTime;
    }

    public override void OnCollision(ProjectileInstance projectile, GameObject target)
    {
        Explode(projectile);
    }

    private void Explode(ProjectileInstance projectile)
    {
        Collider[] hitColliders = Physics.OverlapSphere(projectile.state.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            var damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float distance = Vector3.Distance(projectile.state.position, hitCollider.transform.position);
                float damageRatio = 1f - (distance / explosionRadius);
                float damage = projectile.Info.baseDamage * damageRatio;
                damageable.TakeDamage(damage);
            }

            var rb = hitCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, projectile.state.position, explosionRadius);
            }
        }

        // エフェクトの再生やサウンドの処理をここに追加
    }
}

// ホーミングプロジェクタイル
[CreateAssetMenu(fileName = "Homing Projectile Behavior", menuName = "Projectiles/Behaviors/Homing")]
public class HomingProjectileBehaviorSO : ProjectileBehaviorSO
{
    public float turnSpeed = 2f;
    public float maxTrackingDistance = 20f;

    public override void Initialize(ProjectileInstance projectile)
    {
        projectile.SetRuntimeProperty("Target", 0f);
    }

    public override void Update(ProjectileInstance projectile)
    {
        var target = projectile.visual.gameObject;
        if (target == null)
        {
            target = FindNearestTarget(projectile);
            projectile.SetRuntimeProperty("Target", 0f);
            return;
        }

        Vector3 directionToTarget = (target.transform.position - projectile.state.position).normalized;
        projectile.state.direction = Vector3.Slerp(projectile.state.direction, directionToTarget, turnSpeed * Time.deltaTime);

        projectile.state.position += projectile.state.direction * projectile.Info.speed * Time.deltaTime;
    }

    public override void OnCollision(ProjectileInstance projectile, GameObject target)
    {
        ApplyDamage(target, projectile.Info.baseDamage);
    }

    private GameObject FindNearestTarget(ProjectileInstance projectile)
    {
        GameObject nearestTarget = null;
        float nearestDistance = float.MaxValue;

        // この部分は、ゲームの敵管理システムに応じて最適化できます
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            float distance = Vector3.Distance(projectile.state.position, enemy.transform.position);
            if (distance < nearestDistance && distance <= maxTrackingDistance)
            {
                nearestDistance = distance;
                nearestTarget = enemy;
            }
        }

        return nearestTarget;
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }
}

// 分裂プロジェクタイル
[CreateAssetMenu(fileName = "Splitting Projectile Behavior", menuName = "Projectiles/Behaviors/Splitting")]
public class SplittingProjectileBehaviorSO : ProjectileBehaviorSO
{
    public int splitCount = 3;
    public float splitAngle = 30f;
    public float childProjectileSpeedMultiplier = 0.8f;
    public float childProjectileDamageMultiplier = 0.5f;

    public override void Initialize(ProjectileInstance projectile) { }

    public override void Update(ProjectileInstance projectile)
    {
        projectile.state.position += projectile.state.direction * projectile.Info.speed * Time.deltaTime;
    }

    public override void OnCollision(ProjectileInstance projectile, GameObject target)
    {
        ApplyDamage(target, projectile.Info.baseDamage);
        Split(projectile);
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }

    private void Split(ProjectileInstance projectile)
    {
        for (int i = 0; i < splitCount; i++)
        {
            float angle = ((i / (float)(splitCount - 1)) - 0.5f) * splitAngle;
            Vector3 splitDirection = Quaternion.AngleAxis(angle, Vector3.up) * projectile.state.direction;

            // 新しいプロジェクタイルを生成
            // 注: この部分は実際の実装ではProjectileManagerを通じて行う必要があります
            ProjectileInfoSO childInfo = ScriptableObject.CreateInstance<ProjectileInfoSO>();
            childInfo.speed = projectile.Info.speed * childProjectileSpeedMultiplier;
            childInfo.baseDamage = projectile.Info.baseDamage * childProjectileDamageMultiplier;
            childInfo.behavior = projectile.Behavior; // 同じ振る舞いを使用

            // ProjectileManagerの参照が必要です
            // projectileManager.FireProjectile(childInfo, projectile.state.position, splitDirection);
        }
    }
}

// バウンシングプロジェクタイル
[CreateAssetMenu(fileName = "Bouncing Projectile Behavior", menuName = "Projectiles/Behaviors/Bouncing")]
public class BouncingProjectileBehaviorSO : ProjectileBehaviorSO
{
    public int maxBounces = 3;
    public float bounceDamageMultiplier = 0.8f;

    public override void Initialize(ProjectileInstance projectile)
    {
        projectile.SetRuntimeProperty("Bounces", 0);
    }

    public override void Update(ProjectileInstance projectile)
    {
        projectile.state.position += projectile.state.direction * projectile.Info.speed * Time.deltaTime;
    }

    public override void OnCollision(ProjectileInstance projectile, GameObject target)
    {
        int bounces = (int)projectile.GetRuntimeProperty("Bounces");
        if (bounces < maxBounces)
        {
            ApplyDamage(target, projectile.Info.baseDamage * Mathf.Pow(bounceDamageMultiplier, bounces));
            Bounce(projectile, target);
            projectile.SetRuntimeProperty("Bounces", bounces + 1);
        }
        else
        {
            // 最大バウンス回数に達した場合、プロジェクタイルを非アクティブにする処理をここに追加
        }
    }

    private void ApplyDamage(GameObject target, float damage)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
    }

    private void Bounce(ProjectileInstance projectile, GameObject target)
    {
        // 簡単な反射計算。より複雑な物理挙動が必要な場合は、この部分を拡張してください。
        Vector3 normal = (target.transform.position - projectile.state.position).normalized;
        projectile.state.direction = Vector3.Reflect(projectile.state.direction, normal);
    }
}
