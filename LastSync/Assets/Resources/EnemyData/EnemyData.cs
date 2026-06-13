using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy System/Enemy Data")]

public class EnemyData : ScriptableObject
{
    [Header("基礎屬性 (Base)")]
    [SerializeField] private int baseMaxHealth = 50;
    [SerializeField] private int baseMaxShield = 0;
    [SerializeField] private int baseAttackDamage = 10;

    [Header("成長屬性 (每級增加量)")]
    [SerializeField] private int healthGrowth = 10;
    [SerializeField] private int shieldGrowth = 5;
    [SerializeField] private int attackGrowth = 2;

    [SerializeField] private int baseExperience = 20;
    [SerializeField] private int expGrowth = 5;

    public int BaseMaxHealth => baseMaxHealth;
    public int BaseMaxShield => baseMaxShield;
    public int BaseAttackDamage => baseAttackDamage;
    public int HealthGrowth => healthGrowth;
    public int ShieldGrowth => shieldGrowth;
    public int AttackGrowth => attackGrowth;
    public int BaseExperience => baseExperience;
    public int ExpGrowth => expGrowth;

}
