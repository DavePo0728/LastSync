using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [Header("基礎屬性 (Base)")]
    [SerializeField] private int baseMaxHealth = 50;
    [SerializeField] private int baseMaxShield = 0;
    [SerializeField] private int baseAttackDamage = 10;

    [Header("成長屬性 (每級增加量)")]
    [SerializeField] private int healthGrowth = 10;
    [SerializeField] private int shieldGrowth = 5;
    [SerializeField] private int attackGrowth = 2;

    [Header("掉落系統")]
    [SerializeField] private GameObject itemDropPrefab;
    [SerializeField] private GameObject ExpDropPrefab;
    [SerializeField][Range(0f, 1f)] private float itemDropChance = 0.3f; // 30% 掉落率
    [SerializeField] private int baseExperience = 20;
    [SerializeField] private int expGrowth = 5;

    [Header("UI 元件")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image shieldBar;
    // 當前狀態
    public int CurrentHealth { get; private set; }
    public int CurrentShield { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    // 動態計算當前攻擊力
    public int CurrentAttack => baseAttackDamage + (CurrentLevel - 1) * attackGrowth;

    /// <summary>
    /// 敵人生成時由外部呼叫，注入關卡等級並初始化數值
    /// </summary>
    public void InitializeLevel(int level)
    {
        CurrentLevel = Mathf.Max(1, level); // 防呆，等級最低為 1

        CurrentHealth = baseMaxHealth + (CurrentLevel - 1) * healthGrowth;
        CurrentShield = baseMaxShield + (CurrentLevel - 1) * shieldGrowth;
    }

    /// <summary>
    /// 處理受擊邏輯
    /// </summary>
    public void TakeDamage(int damage)
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

        if (CurrentHealth <= 0)
        {
            HandleDeath();
        }
    }
    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)CurrentHealth / baseMaxHealth;
        }
    }

    private void HandleDeath()
    {
        // 1. 計算並發送經驗值
        int expToDrop = baseExperience + (CurrentLevel - 1) * expGrowth;
        Debug.Log($"{gameObject.name} 死亡，掉落 {expToDrop} 點經驗值。");
        // TODO: 呼叫玩家的 AddExperience(expToDrop) 方法
        Instantiate(ExpDropPrefab, transform.position, Quaternion.identity);
        // 2. 機率掉落道具
        //if (itemDropPrefab != null && Random.value <= itemDropChance)
        //{
        //    Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
        //}

        Destroy(gameObject);
    }
}
