using UnityEngine;

[CreateAssetMenu(menuName = "Procedural/Floor Config")]
public class FloorConfig : ScriptableObject
{
	[Header("Random")]

	public int Seed = 0;

	[Header("Room Count")]

	public int MaxRoomCount = 5;

	public int SpawnRoomCount = 1;

	public int CombatRoomCount = 3;

	public int EliteRoomCount = 0;

	public int TreasureRoomCount = 0;

	public int EventRoomCount = 0;

	public int StairRoomCount = 1;

	public int BossRoomCount = 0;
}