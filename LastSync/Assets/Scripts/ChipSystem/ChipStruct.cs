using UnityEngine;

// 1. 定義可修改的屬性
public enum StatType
{
    FireRate,
    Accuracy,
    BulletCount,
    Range,
    Damage,
}
// 3. 預留特殊機制標籤 (未來有新機制直接新增於此)
public enum SpecialMechanic
{
    None,
    BulletBounce, // 子彈反彈
    Pierce,       // 穿透
    Ignite        // 燃燒
}

[System.Serializable]
public struct ChipModifier
{
    public StatType statType;
    public float value;

    //[TextArea(1, 2)]
    //public string description;
}