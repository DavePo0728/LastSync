using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class GunShoot : WeaponBase
{
    [Header("weaponBaseData")]
    [SerializeField]
    private float fireForce = 1000f;
    [SerializeField]
    
    private int BaseBulletAmount = 1;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    private GameObject mainCam;
    [SerializeField]
    GameObject bulletPrefab;
    [SerializeField] private GameObject muzzleFlash;
    [Header("發射音效")]
    [SerializeField] private WeaponShotAudio shotAudio = new WeaponShotAudio();
    private PlayerAnimationController playerAnimationController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (weaponData == null)
        {
            Debug.LogWarning($"{gameObject.name} 找不到 weaponData，將以無乘數狀態運作。");
        }
        impulseSource = GameObject.FindAnyObjectByType<CinemachineImpulseSource>();
        playerAnimationController = GetComponentInParent<PlayerAnimationController>();
    }

    protected override void PerformAttack()
    {
        if (bulletPrefab == null) return;
        int BulletAmount = BaseBulletAmount + ChipSystem.chipSystemInstance.BonusBulletCount;
        if (BulletAmount <= 0) return;
        shotAudio.Play(transform);
        playerAnimationController?.PlayShootAnimation();
        CameraShake(0.1f);
        if (BulletAmount > 0 && muzzleFlash != null)
        {
            Destroy(Instantiate(muzzleFlash, transform.position, transform.rotation), 5f);
        }
        float half = baseFireAccuracy + ChipSystem.chipSystemInstance.BonusAccuracyOffset * 0.5f;
        for (int i = 0; i < BulletAmount; i++)
        {
            // 單發朝正前方，多發在散射範圍內以相等角度間隔排列。
            float yaw = BulletAmount == 1 ? 0f : Mathf.Lerp(-half, half, (float)i / (BulletAmount - 1));
            Quaternion bulletRotation = Quaternion.AngleAxis(yaw, Vector3.up) * transform.rotation;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, bulletRotation);

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
    void CameraShake(float intensity)
    {
        impulseSource.GenerateImpulseWithForce(intensity);
    }
}
