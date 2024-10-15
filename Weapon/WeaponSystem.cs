using UnityEngine;
using System.Collections;
/*
使用例：

1. 基本的な武器統計の作成：
   - "Create > Weapons > Weapon Stats" を選択して新しい `WeaponStatsSO` アセットを作成。
   - 武器の基本的な統計情報（発射速度、マガジンサイズ、リロード時間など）を設定。

2. 武器データの作成：
   - "Create > Weapons > Weapon Data" （または特殊な武器タイプ）を選択して新しい `WeaponDataSO` アセットを作成。
   - 作成した `WeaponStatsSO` を `baseStats` にアサイン。
   - 必要に応じて `overrideStats` を作成してアサインし、特定の統計値を上書き。

3. 武器バリエーションの作成：
   - 既存の `WeaponDataSO` を複製。
   - 新しい `overrideStats` を作成し、変更したい統計値のみを設定してアサイン。
*/


// 武器の基本情報
[CreateAssetMenu(fileName = "New Weapon Stats", menuName = "Weapons/Weapon Stats")]
public class WeaponStatsSO : ScriptableObject
{
    public string weaponName;
    public float fireRate = 1f;
    public int magazineSize = 30;
    public float reloadTime = 2f;
    public bool isAutomatic = false;
    public float baseDamage = 10f;
    public float accuracy = 0.9f;  // 0.0 to 1.0
    public float range = 100f;
}

// 武器データ
[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    public WeaponStatsSO baseStats;
    public ProjectileInfoSO projectileInfo;
    public GameObject weaponModel;
    public AudioClip fireSound;
    public AudioClip reloadSound;

    // オプションの追加統計情報（baseStatsの値を上書きする）
    public WeaponStatsSO overrideStats;

    // 実際に使用する統計情報を取得するメソッド
    public WeaponStatsSO GetStats()
    {
        return overrideStats != null ? overrideStats : baseStats;
    }

    // 武器固有の発射ロジック
    public virtual void Fire(WeaponSystem weaponSystem, Vector3 position, Vector3 direction)
    {
        WeaponStatsSO stats = GetStats();
        // 精度に基づいて方向をわずかに変更
        Vector3 spread = Random.insideUnitSphere * (1f - stats.accuracy);
        Vector3 finalDirection = (direction + spread).normalized;
        
        weaponSystem.projectileManager.FireProjectile(projectileInfo, position, finalDirection);
    }
}

// 散弾銃用のScriptable Object
[CreateAssetMenu(fileName = "New Shotgun", menuName = "Weapons/Shotgun Data")]
public class ShotgunDataSO : WeaponDataSO
{
    public int pelletCount = 8;
    public float spreadAngle = 15f;

    public override void Fire(WeaponSystem weaponSystem, Vector3 position, Vector3 direction)
    {
        WeaponStatsSO stats = GetStats();
        for (int i = 0; i < pelletCount; i++)
        {
            Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), Vector3.up) *
                                        Quaternion.AngleAxis(Random.Range(-spreadAngle, spreadAngle), Vector3.right);
            Vector3 spreadDirection = randomRotation * direction;
            
            // 各ペレットの精度を適用
            Vector3 finalDirection = Vector3.Slerp(spreadDirection, direction, stats.accuracy);
            
            weaponSystem.projectileManager.FireProjectile(projectileInfo, position, finalDirection);
        }
    }
}

// バースト発射武器用のScriptable Object
[CreateAssetMenu(fileName = "New Burst Weapon", menuName = "Weapons/Burst Weapon Data")]
public class BurstWeaponDataSO : WeaponDataSO
{
    public int burstCount = 3;
    public float burstDelay = 0.1f;

    public override void Fire(WeaponSystem weaponSystem, Vector3 position, Vector3 direction)
    {
        weaponSystem.StartCoroutine(FireBurst(weaponSystem, position, direction));
    }

    private IEnumerator FireBurst(WeaponSystem weaponSystem, Vector3 position, Vector3 direction)
    {
        WeaponStatsSO stats = GetStats();
        for (int i = 0; i < burstCount; i++)
        {
            // 各発射の精度を適用
            Vector3 spread = Random.insideUnitSphere * (1f - stats.accuracy);
            Vector3 finalDirection = (direction + spread).normalized;
            
            weaponSystem.projectileManager.FireProjectile(projectileInfo, position, finalDirection);
            yield return new WaitForSeconds(burstDelay);
        }
    }
}

// WeaponSystem
public class WeaponSystem : MonoBehaviour
{
    public ProjectileManager projectileManager;
    public Transform firePoint;
    public WeaponDataSO currentWeapon;

    private float lastFireTime;
    private int currentAmmo;
    private bool isReloading = false;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        WeaponStatsSO stats = currentWeapon.GetStats();
        if (stats.isAutomatic && Input.GetButton("Fire1"))
        {
            TryFire();
        }
        else if (Input.GetButtonDown("Fire1"))
        {
            TryFire();
        }

        if (Input.GetButtonDown("Reload"))
        {
            StartReload();
        }
    }

    public void EquipWeapon(WeaponDataSO newWeapon)
    {
        currentWeapon = newWeapon;
        WeaponStatsSO stats = currentWeapon.GetStats();
        currentAmmo = stats.magazineSize;
        // ここで武器モデルの切り替えなどを行う
    }

    private void TryFire()
    {
        if (isReloading) return;

        WeaponStatsSO stats = currentWeapon.GetStats();
        if (Time.time - lastFireTime < 1f / stats.fireRate) return;

        if (currentAmmo <= 0)
        {
            StartReload();
            return;
        }

        Fire();
    }

    private void Fire()
    {
        currentWeapon.Fire(this, firePoint.position, firePoint.forward);
        currentAmmo--;
        lastFireTime = Time.time;

        if (currentWeapon.fireSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.fireSound);
        }

        // ここで発射エフェクトなどを追加
    }

    private void StartReload()
    {
        WeaponStatsSO stats = currentWeapon.GetStats();
        if (isReloading || currentAmmo == stats.magazineSize) return;

        isReloading = true;
        
        if (currentWeapon.reloadSound != null)
        {
            audioSource.PlayOneShot(currentWeapon.reloadSound);
        }

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        WeaponStatsSO stats = currentWeapon.GetStats();
        yield return new WaitForSeconds(stats.reloadTime);
        currentAmmo = stats.magazineSize;
        isReloading = false;
    }
}

// 武器の切り替えを管理するコンポーネント（変更なし）
public class WeaponSwitcher : MonoBehaviour
{
    public WeaponSystem weaponSystem;
    public WeaponDataSO[] availableWeapons;
    private int currentWeaponIndex = 0;

    private void Start()
    {
        if (availableWeapons.Length > 0)
        {
            weaponSystem.EquipWeapon(availableWeapons[currentWeaponIndex]);
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("SwitchWeapon"))
        {
            SwitchToNextWeapon();
        }
    }

    private void SwitchToNextWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % availableWeapons.Length;
        weaponSystem.EquipWeapon(availableWeapons[currentWeaponIndex]);
    }
}
