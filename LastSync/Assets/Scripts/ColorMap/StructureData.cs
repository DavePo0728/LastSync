using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ColorMap/StructureData")]
public class StructureData : ScriptableObject
{
	// 原有資料
	public List<Structure> Structures = new();

	public List<Structure> Doors = new();

	public string StructureID;

	public Vector2Int Size;

	// 房間分類（Normal、Boss、Bridge...）
	public RoomCategory Category;

	// 固定用途（Boss房、Spawn房...）
	public RoomRole PresetRole = RoomRole.None;

	// 所屬關卡
	public StageType Stage;

	// 被抽中的權重
	public int Weight = 1;

	// 房間特性
	public List<RoomTag> Tags = new();
}
public enum RoomTag
{
	None,

	// 地形
	DeadEnd,       // 死路
	Junction,      // 岔路
	Corridor,      // 長走道

	// 大小
	Small,
	Medium,
	Large,

	// 特殊
	Indoor,
	Outdoor,

	// 保留
	Secret,
	Puzzle
}