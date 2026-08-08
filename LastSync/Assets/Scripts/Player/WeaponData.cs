using System;
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

    [SerializeField] private int baseFireAccuracy = 10; // 武器精確度
    [SerializeField] private float fireRange = 10f;
    [SerializeField] private int baseBulletCount = 1; // 基礎子彈數量
    // 唯讀屬性
    public float Fire_Rate => fireRate;
    public int Base_Damage => baseDamage;
    public int Base_FireAccuracy => baseFireAccuracy;
    public float Fire_Interval => fireRate > 0f ? 1.0f / fireRate : 0.5f;
    public float Fire_Range => fireRange;
    public int Base_BulletCount => baseBulletCount;
}