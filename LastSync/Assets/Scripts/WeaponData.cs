using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon System/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Identification")]
    public string weaponName;

    [Header("Combat Stats")]
    [Tooltip("每秒發射量 (FPS)")]
    [SerializeField] private float fireRate = 2.0f;

    [Tooltip("攻擊力百分比乘數 (1.2 代表 120%)")]
    [SerializeField] private float attackMultiplier = 1.0f;

    [Tooltip("武器基礎傷害值")]
    [SerializeField] private int baseDamage = 10;

    // 唯讀屬性
    public float FireRate => fireRate;
    public float AttackMultiplier => attackMultiplier;
    public float FireInterval => fireRate > 0f ? 1.0f / fireRate : float.MaxValue;

    /// <summary>
    /// 計算此武器輸出的最終傷害
    /// </summary>
    public int CalculateOutputDamage()
    {
        return Mathf.RoundToInt(baseDamage * attackMultiplier);
    }
}