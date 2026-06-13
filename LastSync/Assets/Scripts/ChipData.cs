using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewChipData", menuName = "Chips System/Chip Data")]
public class ChipData : ScriptableObject
{
    public int chipID; // 晶片ID
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
}