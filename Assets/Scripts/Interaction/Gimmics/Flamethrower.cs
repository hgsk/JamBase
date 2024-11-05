using UnityEngine;
// 火炎放射器
public class Flamethrower : BaseGimmick
{
    public float damage = 10f;
    public float range = 5f;
    public float fireRate = 0.1f;
    private float nextFireTime;

    private void Update()
    {
        if (isActive && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    private void Fire()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range))
        {
            if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
            }
        }
    }
}

