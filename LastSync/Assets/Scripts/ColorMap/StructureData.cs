using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ColorMap/StructureData")]
public class StructureData : ScriptableObject
{
	public List<Structure> Structures = new();

	public List<Structure> Doors = new();

	public string StructureID;

	public Vector2Int Size;

	public RoomCategory Category;

	public RoomRole PresetRole = RoomRole.None;

	public StageType Stage;

	public int Weight = 1;

	public List<RoomTag> Tags = new();
	[HideInInspector]
	public bool HasPlayerSpawnPoint;
    public Vector2Int PlayerSpawnPoint;
	public List<SpawnPointData> SpawnPoint = new();
}
public enum RoomTag
{
	None,

	DeadEnd,
	Junction,
	Corridor,

	Small,
	Medium,
	Large,

	Indoor,
	Outdoor,

	Secret,
	Puzzle
}
