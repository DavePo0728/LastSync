using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChipData", menuName = "Chips System/Chip Data")]
public class ChipData : ScriptableObject
{
    public int chipID; // 晶片ID
    public Sprite icon; // 晶片圖示
    public string chipName; // 晶片名稱
    public ChipType chipType; // 晶片類型
    public int maxStackSize=99;
    [Header("數值加成清單")]
    [SerializeField] private List<ChipModifier> modifiers = new List<ChipModifier>();
    public IReadOnlyList<ChipModifier> Modifiers => modifiers;
    [Header("特殊機制 (未來擴展)")]
    [SerializeField] private List<SpecialMechanic> specialMechanics = new List<SpecialMechanic>();

    /// <summary>
    /// 檢查晶片是否包含特定的特殊機制
    /// </summary>
    public bool HasSpecialMechanic(SpecialMechanic mechanic)
    {
        return specialMechanics.Contains(mechanic);
    }
    public string getChipValuesFromModifiers()
    {
        string values = "";
        foreach (var modifier in modifiers)
        {
            values += $"{modifier.statType}: {modifier.value}\n";
        }
        return values.TrimEnd('\n');
    }
}
public enum ChipType
{
    Attack,
    Defend,
    Movement,
    Support,
    Other,
}