using UnityEngine;
using UnityEngine.InputSystem;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("武器基礎數值")]
    [SerializeField] protected int baseDamage;
    [SerializeField] protected float fireRnage;
    [SerializeField] protected float fireRate;
    [SerializeField] protected int baseBulletCount;

    [SerializeField]
    protected WeaponData weaponData;
    public float Base_Damage => baseDamage;
    public float Fire_Range => fireRnage;
    public float Fire_Rate => fireRate;
    public int Base_BulletCount => baseBulletCount;

    public float Base_FireAccuracy => baseFireAccuracy;
    private float fireIntervalTimer;
    protected float fireInterval;

    protected float baseFireAccuracy;

    protected float fireRangeMultiplier; // 預設射程乘數

    protected virtual void Awake()
    {
        weaponData = Resources.Load<WeaponData>("WeaponData/BasicGun");
        Debug.Log($"{gameObject.name} 加載了 weaponData: {weaponData.name}");
        fireInterval = weaponData != null ? weaponData.Fire_Interval : 0.5f; // 預設射速間隔
        baseFireAccuracy = weaponData != null ? weaponData.Base_FireAccuracy : 0f; // 預設準確度偏移
        baseDamage = weaponData.Base_Damage;
        fireRnage = weaponData.Fire_Range;
        fireRate = weaponData.Fire_Rate;
        baseBulletCount = weaponData.Base_BulletCount;
    }

    private void OnEnable()
    {
        InputSystem.actions.FindAction("Attack").performed += HandleInput;
    }

    private void OnDisable()
    {
        InputSystem.actions.FindAction("Attack").performed -= HandleInput;
    }
    protected virtual void Update()
    {
        // 更新射速計時器
        fireIntervalTimer += Time.deltaTime;
    }
    private void HandleInput(InputAction.CallbackContext context)
    {
        // 結合射速限制
        if (fireIntervalTimer >= fireInterval)
        {
            PerformAttack();
            fireIntervalTimer = 0f; // 重置計時器
        }
    }
    public float FireCooldownRatio
    {
        get
        {
            // 避免除以 0 的錯誤，並將結果限制在 0 ~ 1 之間
            if (fireInterval <= 0f) return 1f;
            return Mathf.Clamp01(fireIntervalTimer / fireInterval);
        }
    }
    /// <summary>
    /// 取得計算後的最終傷害值
    /// </summary>
    protected int GetFinalDamage()
    {
        float damageMultiplier = ChipSystem.chipSystemInstance.DamageMultiplier;
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }
    protected int GetBulletAmount()
    {
        return ChipSystem.chipSystemInstance.BonusBulletCount;
    }
    protected float GetFinalFireRange()
    {
        float rangeMultiplier = ChipSystem.chipSystemInstance.RangeMultiplier;
        return weaponData != null ? weaponData.Fire_Range * rangeMultiplier : 0f;
    }
    /// <summary>
    /// 子類別必須實作的攻擊行為
    /// </summary>
    protected abstract void PerformAttack();
}
