using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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

internal interface IDamageDealer
{
    float DamageAmount { get; set; }
}

// スイング可能な綱
public class SwingableRope : BaseGimmick, IInteractableGimmick
{
    public float swingForce = 5f;
    public Transform ropeEnd;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 swingDirection = (ropeEnd.position - interactor.transform.position).normalized;
            rb.AddForce(swingDirection * swingForce, ForceMode.Impulse);
        }
    }
}

// サイズ変更ポーション
public class SizeChangePotion : BaseGimmick, IInteractableGimmick
{
    public float sizeMultiplier = 2f;
    public float duration = 10f;

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            StartCoroutine(ChangeSizeCoroutine(interactor));
        }
    }

    private IEnumerator ChangeSizeCoroutine(GameObject target)
    {
        Vector3 originalScale = target.transform.localScale;
        target.transform.localScale *= sizeMultiplier;
        yield return new WaitForSeconds(duration);
        target.transform.localScale = originalScale;
    }
}

// 一方通行の壁
public class OneWayWall : BaseGimmick
{
    public Vector3 allowedDirection = Vector3.right;

    private void OnTriggerEnter(Collider other)
    {
        if (isActive)
        {
            Vector3 enterDirection = other.transform.position - transform.position;
            if (Vector3.Dot(enterDirection, allowedDirection) < 0)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), other, true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), other, false);
    }
}

// 移動プラットフォーム
public class MovingPlatform : BaseGimmick
{
    public Vector3[] waypoints;
    public float speed = 1f;
    private int currentWaypointIndex = 0;

    private void Update()
    {
        if (isActive && waypoints.Length > 1)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
    }
}

// 回転する障害物
public class RotatingObstacle : BaseGimmick
{
    public float rotationSpeed = 50f;
    public Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        if (isActive)
        {
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
        }
    }
}

// 風の流れ
public class WindStream : BaseGimmick, IAreaGimmick
{
    public Vector3 windDirection = Vector3.right;
    public float windForce = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(windDirection * windForce);
        }
    }
}

// 伸縮する足場
public class ExtendablePlatform : BaseGimmick, IInteractableGimmick
{
    public float extendedLength = 5f;
    public float extendSpeed = 1f;
    private Vector3 originalScale;
    private bool isExtended = false;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            StartCoroutine(ExtendCoroutine());
        }
    }

    private IEnumerator ExtendCoroutine()
    {
        Vector3 targetScale = isExtended ? originalScale : new Vector3(extendedLength, originalScale.y, originalScale.z);
        while (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, extendSpeed * Time.deltaTime);
            yield return null;
        }
        isExtended = !isExtended;
    }
}

// 隠し通路
public class HiddenPassage : BaseGimmick, IInteractableGimmick
{
    public GameObject hiddenObject;

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            hiddenObject.SetActive(!hiddenObject.activeSelf);
        }
    }
}

// 反射板
public class ReflectiveSurface : BaseGimmick
{
    private void OnCollisionEnter(Collision collision)
    {
        if (isActive && collision.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 reflectedVelocity = Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
            rb.velocity = reflectedVelocity;
        }
    }
}

// 一時スローモーション（バレットタイム）
public class BulletTime : BaseGimmick, ITimedGimmick
{
    public float slowdownFactor = 0.2f;
    public float Duration { get; set; } = 5f;

    public void StartTimer()
    {
        StartCoroutine(BulletTimeCoroutine());
    }

    private IEnumerator BulletTimeCoroutine()
    {
        Time.timeScale = slowdownFactor;
        yield return new WaitForSecondsRealtime(Duration);
        Time.timeScale = 1f;
    }
}

// ダッシュパネル
public class DashPanel : BaseGimmick, IInteractableGimmick
{
    public float dashForce = 20f;
    public Vector3 dashDirection = Vector3.forward;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        }
    }
}

// 重力操作装置
public class GravityManipulator : BaseGimmick, IAreaGimmick
{
    public Vector3 gravityDirection = Vector3.down;
    public float gravityStrength = 9.81f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = true;
        }
    }
}

// 分身生成装置
public class CloneGenerator : BaseGimmick, IInteractableGimmick
{
    public GameObject clonePrefab;
    public int maxClones = 3;
    private List<GameObject> activeClones = new List<GameObject>();

    public void Interact(GameObject interactor)
    {
        if (isActive && activeClones.Count < maxClones)
        {
            GameObject clone = Instantiate(clonePrefab, interactor.transform.position, interactor.transform.rotation);
            activeClones.Add(clone);
            StartCoroutine(DestroyCloneAfterDelay(clone, 10f));
        }
    }

    private IEnumerator DestroyCloneAfterDelay(GameObject clone, float delay)
    {
        yield return new WaitForSeconds(delay);
        activeClones.Remove(clone);
        Destroy(clone);
    }
}

// 壁走り可能エリア
public class WallRunArea : BaseGimmick, IAreaGimmick
{
    public float wallRunForce = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 wallNormal = transform.up;
            Vector3 wallRunDirection = Vector3.Cross(wallNormal, Vector3.up);
            rb.AddForce(wallRunDirection * wallRunForce);
        }
    }
}

// 磁力フィールド
public class MagneticField : BaseGimmick, IAreaGimmick
{
    public float magneticForce = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 directionToCenter = (transform.position - other.transform.position).normalized;
            rb.AddForce(directionToCenter * magneticForce);
        }
    }
}

public interface IGimmick
{
    void Activate();
    void Deactivate();
    bool IsActive { get; }
}

public interface IInteractableGimmick : IGimmick
{
    void Interact(GameObject interactor);
}

public interface ITimedGimmick : IGimmick
{
    float Duration { get; set; }
    void StartTimer();
}

public interface IAreaGimmick : IGimmick
{
    bool IsInArea(Vector3 position);
}

// ギミック共通実装
public abstract class BaseGimmick : MonoBehaviour, IGimmick
{
    protected bool isActive;

    public virtual void Activate()
    {
        isActive = true;
    }

    public virtual void Deactivate()
    {
        isActive = false;
    }

    public bool IsActive => isActive;
}

// ジャンプパッド
public class JumpPad : BaseGimmick, IInteractableGimmick
{
    public float jumpForce = 10f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}

// 滑る床
public class SlipperyFloor : BaseGimmick, IAreaGimmick
{
    public float frictionCoefficient = 0.1f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isActive && collision.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.AddForce(velocity.normalized * frictionCoefficient, ForceMode.Acceleration);
        }
    }
}

// テレポーター
public class Teleporter : BaseGimmick, IInteractableGimmick
{
    public Transform destination;

    public void Interact(GameObject interactor)
    {
        if (isActive && destination != null)
        {
            interactor.transform.position = destination.position;
        }
    }
}

// 重力反転装置
public class GravityReverser : BaseGimmick, IAreaGimmick, ITimedGimmick
{
    public float Duration { get; set; } = 5f;
    public Collider areaCollider;
    private float timeLeft;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    public void StartTimer()
    {
        timeLeft = Duration;
        StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        Deactivate();
    }

    public override void Activate()
    {
        base.Activate();
        Physics.gravity = new Vector3(0, 9.81f, 0);
        StartTimer();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }
}

// 属性変更ゲート
public class ElementChangeGate : BaseGimmick, IInteractableGimmick
{
    public enum Element { Fire, Water, Earth, Air }
    public Element newElement;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<IElemental>(out var elemental))
        {
            elemental.ChangeElement(newElement);
        }
    }
}

internal interface IElemental
{
    void ChangeElement(ElementChangeGate.Element newElement);
}

// ポータルドア
public class PortalDoor : BaseGimmick, IInteractableGimmick
{
    public Transform exitPortal;

    public void Interact(GameObject interactor)
    {
        if (isActive && exitPortal != null)
        {
            interactor.transform.position = exitPortal.position;
            interactor.transform.rotation = exitPortal.rotation;
        }
    }
}

// 浮遊プラットフォーム
public class FloatingPlatform : BaseGimmick
{
    public float floatHeight = 5f;
    public float floatSpeed = 1f;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isActive)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}

// トランポリン
public class Trampoline : BaseGimmick, IInteractableGimmick
{
    public float bounceForce = 20f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
        }
    }
}

// 氷の床
public class IcyFloor : BaseGimmick, IAreaGimmick
{
    public float frictionCoefficient = 0.05f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isActive && collision.gameObject.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.AddForce(velocity.normalized * (1f - frictionCoefficient), ForceMode.Acceleration);
        }
    }
}

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


public class Switch : BaseGimmick, IInteractableGimmick
{
    public bool IsActivated { get; private set; }

    public void Interact(GameObject interactor)
    {
        IsActivated = !IsActivated;
    }
}
// スイッチ連動ギミック
public class SwitchActivatedGimmick : BaseGimmick
{
    public List<Switch> switches;
    public bool requireAllSwitches = true;

    private void Update()
    {
        bool shouldActivate = requireAllSwitches ? 
            switches.TrueForAll(s => s.IsActivated) : 
            switches.Exists(s => s.IsActivated);

        if (shouldActivate)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }
}

// 巨大ファン
public class GiantFan : BaseGimmick, IAreaGimmick
{
    public float windForce = 20f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(transform.forward * windForce);
        }
    }
}

// 水中呼吸エリア
public class UnderwaterBreathingZone : BaseGimmick, IAreaGimmick
{
    public float oxygenReplenishRate = 10f;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isActive && other.TryGetComponent<IOxygenDependent>(out var oxygenDependent))
        {
            oxygenDependent.ReplenishOxygen(oxygenReplenishRate * Time.deltaTime);
        }
    }
}

internal interface IOxygenDependent
{
    void ReplenishOxygen(float v);
}

// 重力制御装置
public class GravityController : BaseGimmick, IInteractableGimmick
{
    public Vector3 gravityDirection = Vector3.down;
    public float gravityStrength = 9.81f;

    public void Interact(GameObject interactor)
    {
        if (isActive)
        {
            Physics.gravity = gravityDirection * gravityStrength;
        }
    }

    public override void Deactivate()
    {
        base.Deactivate();
        Physics.gravity = new Vector3(0, -9.81f, 0);
    }
}

// 時間逆行装置
public class TimeReverser : BaseGimmick, ITimedGimmick
{
    public float Duration { get; set; } = 5f;
    private List<IReversible> reversibleObjects = new List<IReversible>();

    public void StartTimer()
    {
        StartCoroutine(ReverseTimeCoroutine());
    }

    private IEnumerator ReverseTimeCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < Duration)
        {
            foreach (var reversible in reversibleObjects)
            {
                reversible.ReverseTime(Time.deltaTime);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IReversible>(out var reversible))
        {
            reversibleObjects.Add(reversible);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IReversible>(out var reversible))
        {
            reversibleObjects.Remove(reversible);
        }
    }
}

internal interface IReversible
{
    void ReverseTime(float deltaTime);
}

// 影潜り能力付与装置
public class ShadowMergeDevice : BaseGimmick, IInteractableGimmick
{
    public float duration = 10f;

    public void Interact(GameObject interactor)
    {
        if (isActive && interactor.TryGetComponent<IShadowMergeable>(out var shadowMergeable))
        {
            StartCoroutine(ShadowMergeCoroutine(shadowMergeable));
        }
    }

    private IEnumerator ShadowMergeCoroutine(IShadowMergeable shadowMergeable)
    {
        shadowMergeable.EnableShadowMerge();
        yield return new WaitForSeconds(duration);
        shadowMergeable.DisableShadowMerge();
    }
}

internal interface IShadowMergeable
{
    void DisableShadowMerge();
    void EnableShadowMerge();
}

// 壁すり抜けゾーン
public class WallPhaseZone : BaseGimmick, IAreaGimmick
{
    public LayerMask wallLayers;
    public Collider areaCollider;

    public bool IsInArea(Vector3 position)
    {
        return areaCollider.bounds.Contains(position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive)
        {
            Physics.IgnoreLayerCollision(other.gameObject.layer, wallLayers, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Physics.IgnoreLayerCollision(other.gameObject.layer, wallLayers, false);
    }
}

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



// ランダム瞬間移動装置
public class TeleportationDevice : BaseGimmick, IInteractableGimmick
{
    public Transform[] teleportPoints;

    public void Interact(GameObject interactor)
    {
        if (isActive && teleportPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, teleportPoints.Length);
            interactor.transform.position = teleportPoints[randomIndex].position;
        }
    }
}

// 床すり抜けトラップ
public class FloorPhaseTrapdoor : BaseGimmick, ITimedGimmick
{
    public float Duration { get; set; } = 3f;
    private Collider floorCollider;

    private void Start()
    {
        floorCollider = GetComponent<Collider>();
    }

    public void StartTimer()
    {
        StartCoroutine(PhaseCoroutine());
    }

    private IEnumerator PhaseCoroutine()
    {
        floorCollider.enabled = false;
        yield return new WaitForSeconds(Duration);
        floorCollider.enabled = true;
    }
}

// 動く的
public class MovingTarget : BaseGimmick
{
    public float moveSpeed = 5f;
    public float moveDistance = 10f;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + transform.right * moveDistance;
    }

    private void Update()
    {
        if (isActive)
        {
            Vector3 targetPosition = movingToEnd ? endPosition : startPosition;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (transform.position == targetPosition)
            {
                movingToEnd = !movingToEnd;
            }
        }
    }
}

