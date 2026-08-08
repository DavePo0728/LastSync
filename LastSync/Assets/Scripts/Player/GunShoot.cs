using UnityEngine;
using UnityEngine.InputSystem;

public class GunShoot : WeaponBase
{
    [Header("weaponBaseData")]
    [SerializeField]
    private float fireForce = 1000f;
    [SerializeField]
    
    private int BaseBulletAmount = 1;

    [SerializeField]
    GameObject bulletPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"{gameObject.name} 找不到 weaponData，將以無乘數狀態運作。");
        }
    }

    protected override void PerformAttack()
    {
        if (bulletPrefab == null) return;
        int BulletAmount = BaseBulletAmount + ChipSystem.chipSystemInstance.BonusBulletCount;
        for (int i = 0; i < BulletAmount; i++)
        {
            float half = baseFireAccuracy+ChipSystem.chipSystemInstance.BonusAccuracyOffset * 0.5f;
            float yaw = Random.Range(-half, half);
            float pitch = Random.Range(-half, half);

            // 把偏航與俯仰疊加到火點的朝向上
            Quaternion randomRot = transform.rotation * Quaternion.Euler(pitch, yaw, 0);
            GameObject bullet = Instantiate(bulletPrefab, transform.position, randomRot);

            // 傳遞最終傷害給子彈
            if (bullet.TryGetComponent<Bullet>(out Bullet bulletScript))
            {
                bulletScript.Initialize(GetFinalDamage(), GetFinalFireRange());

            }

            if (bullet.TryGetComponent<Rigidbody>(out Rigidbody bulletRB))
            {
                bulletRB.AddForce(bullet.transform.forward * fireForce);
            }
        }
    }
}
