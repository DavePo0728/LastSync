using System.Collections.Generic;
using UnityEngine;

public class ChipSystem : MonoBehaviour
{
    public static ChipSystem chipSystemInstance { get; private set; }
    [SerializeField]
    private int ChipSlotCount = 9; // 晶片槽位數量
    [Header("已裝備晶片清單")]
    [SerializeField] private List<ChipData> equippedChips = new List<ChipData>();

    // --- 計算後的最終加成面板 ---
    // 百分比乘數 (基礎值預設為 1.0)
    public float FireRateMultiplier { get; private set; }
    public float RangeMultiplier { get; private set; }
    public float DamageMultiplier { get; private set; }

    // 絕對值加成 (基礎值預設為 0)
    public int BonusBulletCount { get; private set; }
    public float BonusAccuracyOffset { get; private set; }

    private void Awake()
    {
        chipSystemInstance = this;
        RecalculateStats();
    }

    /// <summary>
    /// 裝備晶片並刷新數值
    /// </summary>
    public void EquipChip(ChipData chip)
    {
        if (chip != null && !equippedChips.Contains(chip))
        {
            equippedChips.Add(chip);
            RecalculateStats();
        }
    }

    /// <summary>
    /// 卸下晶片並刷新數值
    /// </summary>
    public void UnequipChip(ChipData chip)
    {
        if (equippedChips.Remove(chip))
        {
            RecalculateStats();
        }
    }

    /// <summary>
    /// 重新計算所有晶片加成
    /// </summary>
    private void RecalculateStats()
    {
        // 1. 初始化/重置基礎加成
        FireRateMultiplier = 1.0f;
        RangeMultiplier = 1.0f;
        DamageMultiplier = 1.0f;

        BonusBulletCount = 0;
        BonusAccuracyOffset = 0f;

        // 2. 遍歷所有裝備中的晶片進行疊加
        foreach (var chip in equippedChips)
        {
            foreach (var mod in chip.Modifiers)
            {
                switch (mod.statType)
                {
                    // 百分比邏輯：基礎 1.0 + 0.15 = 1.15
                    case StatType.FireRate:
                        FireRateMultiplier += mod.value;
                        break;
                    case StatType.Range:
                        RangeMultiplier += mod.value;
                        break;
                    case StatType.Damage:
                        DamageMultiplier += mod.value;
                        break;

                    // 絕對值邏輯：強制轉型或直接相加
                    case StatType.BulletCount:
                        BonusBulletCount += Mathf.RoundToInt(mod.value);
                        break;
                    case StatType.Accuracy:
                        BonusAccuracyOffset += mod.value;
                        break;
                }
            }
        }
        Debug.Log($"Stats Recalculated: FireRate x{FireRateMultiplier}, Range x{RangeMultiplier}, Damage x{DamageMultiplier}, BonusBulletCount {BonusBulletCount}, BonusAccuracyOffset {BonusAccuracyOffset}");
    }
}
