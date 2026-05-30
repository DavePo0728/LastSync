using UnityEngine;
using UnityEngine.InputSystem;

public class GunShoot : WeaponBase
{
    [Header("weaponBaseData")]
     private float fireForce = 500f;

    [SerializeField]
    GameObject bulletPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ownerStats = GetComponentInParent<CharacterStats>();
        if (ownerStats == null)
        {
            Debug.LogWarning($"{gameObject.name} 找不到 CharacterStats，將以無乘數狀態運作。");
        }
    }

    protected override void PerformAttack()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

        // 傳遞最終傷害給子彈
        if (bullet.TryGetComponent<Bullet>(out Bullet bulletScript))
        {
            bulletScript.SetDamage(GetFinalDamage());
        }

        if (bullet.TryGetComponent<Rigidbody>(out Rigidbody bulletRB))
        {
            bulletRB.AddForce(transform.forward * fireForce);
        }

        Destroy(bullet, 3f);
    }
}
