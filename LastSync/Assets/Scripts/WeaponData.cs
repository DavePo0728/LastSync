using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon System/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Identification")]
    public string weaponName;

    [Header("Combat Stats")]
    [Tooltip("每秒發射量 (FPS)")]
    [SerializeField] private float fireRate = 2.0f;

    [Tooltip("武器基礎傷害值")]
    [SerializeField] private int baseDamage = 10;

    [SerializeField] private int accuracy = 10; // 武器精確度
    [SerializeField] private float fireRange = 10f;
    // 唯讀屬性
    public float fire_Rate => fireRate;
    public int base_Damage => baseDamage;
    public int _accuracy => accuracy;
    public float fire_Interval => fireRate > 0f ? 1.0f / fireRate : 0.5f;
    public float fire_Range => fireRange;
}