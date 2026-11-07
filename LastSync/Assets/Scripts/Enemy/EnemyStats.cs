using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EnemyStats : MonoBehaviour
{
    [SerializeField] protected bool test;
    [SerializeField] protected GameObject player;
    [SerializeField] protected EnemyData enemyData; // 從 ScriptableObject 讀取數值
    [Header("基礎屬性 (Base)")]
    [SerializeField] protected int baseMaxHealth;
    [SerializeField] protected int baseMaxShield;
    [SerializeField] protected int baseAttackDamage;

    [Header("成長屬性 (每級增加量)")]
    [SerializeField] protected int healthGrowth;
    [SerializeField] protected int shieldGrowth;
    [SerializeField] protected int attackGrowth;
    [Header("掉落系統")]
    [SerializeField] protected int baseExperience;
    [SerializeField] protected int expGrowth;

    [Header("UI 元件")]
    [SerializeField] protected Image healthBar;
    [SerializeField] protected Image shieldBar;
    Canvas canvas;
    protected int currentMaxHealth;
    // 當前狀態
    public int CurrentHealth { get; protected set; }
    public int CurrentShield { get; protected set; }
    public int CurrentLevel { get; protected set; } = 1;

    // 動態計算當前攻擊力
    public int CurrentAttack => baseAttackDamage + (CurrentLevel - 1) * attackGrowth;

    /// <summary>
    /// 敵人生成時由外部呼叫，注入關卡等級並初始化數值
    /// </summary>
    protected virtual void InitializeLevel(EnemyData _enemyData)
    {
        if (_enemyData != null)
        {
            enemyData = _enemyData;
            baseMaxHealth = enemyData.BaseMaxHealth;
            baseMaxShield = enemyData.BaseMaxShield;
            baseAttackDamage = enemyData.BaseAttackDamage;
            healthGrowth = enemyData.HealthGrowth;
            shieldGrowth = enemyData.ShieldGrowth;
            attackGrowth = enemyData.AttackGrowth;
        }
        canvas = gameObject.GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        currentMaxHealth = baseMaxHealth + (CurrentLevel - 1) * healthGrowth;
        CurrentHealth = currentMaxHealth;
        CurrentShield = baseMaxShield + (CurrentLevel - 1) * shieldGrowth;
    }
    protected virtual void Start()
    {
        if (test)
        {
            InitializeLevel(Resources.Load<EnemyData>("EnemyData/MeleeEnemy"));
        }
        UpdateUI();
        player = GameObject.FindGameObjectWithTag("Player");
    }
    /// <summary>
    /// 處理受擊邏輯
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        if (CurrentShield > 0)
        {
            if (damage <= CurrentShield)
            {
                CurrentShield -= damage;
                UpdateUI();
                return;
            }
            damage -= CurrentShield;
            CurrentShield = 0;
        }

        CurrentHealth -= damage;
        //Debug.Log($"{gameObject.name} 受到 {damage} 點傷害，剩餘生命 {CurrentHealth}，剩餘護盾 {CurrentShield}。");
        UpdateUI();

        if (CurrentHealth <= 0)
        {
            HandleDeath();
        }
    }
    protected virtual void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)CurrentHealth / currentMaxHealth;
        }
    }

    protected virtual void HandleDeath()
    {
        // 1. 計算並發送經驗值
        int expToDrop = baseExperience + (CurrentLevel - 1) * expGrowth;
        Debug.Log($"{gameObject.name} 死亡，掉落 {expToDrop} 點經驗值。");
        Destroy(gameObject);
    }
    protected abstract void Attack();
}
