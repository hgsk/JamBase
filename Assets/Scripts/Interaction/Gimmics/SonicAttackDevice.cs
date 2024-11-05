using UnityEngine;
// 音波攻撃装置
public class SonicAttackDevice : BaseGimmick
{
    public float damage = 5f;
    public float radius = 10f;
    public float attackInterval = 1f;
    private float nextAttackTime;

    private void Update()
    {
        if (isActive && Time.time >= nextAttackTime)
        {
            PerformSonicAttack();
            nextAttackTime = Time.time + attackInterval;
        }
    }

    private void PerformSonicAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }
    }
}

