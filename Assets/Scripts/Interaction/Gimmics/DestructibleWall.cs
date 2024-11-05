using UnityEngine;
// 破壊可能な壁
public class DestructibleWall : BaseGimmick, IInteractableGimmick
{
    public float health = 100f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<IDamageDealer>(out var damageDealer))
        {
            TakeDamage(damageDealer.DamageAmount);
        }
    }

    private void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}

