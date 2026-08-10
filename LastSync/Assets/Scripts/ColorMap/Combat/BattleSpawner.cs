using System.Collections.Generic;
using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
	private RoomEventManager manager;
	private readonly List<GameObject> aliveEnemies = new();

	private void Awake()
	{
		manager = GetComponent<RoomEventManager>();
	}

	public void Spawn(List<EnemySpawnInfo> enemies)
	{
		foreach (EnemySpawnInfo info in enemies)
		{
			for (int i = 0; i < info.Count; i++)
			{
				SpawnEnemy(info.Prefab);
			}
		}
	}

	public void SpawnEnemy(GameObject prefab)
	{
		RoomInstance room = manager.Room;

		Vector2Int size = room.Data.Size;

		float x = Random.Range(
			-size.x * 0.5f + 1,
			 size.x * 0.5f - 1);

		float z = Random.Range(
			-size.y * 0.5f + 1,
			 size.y * 0.5f - 1);

		Vector3 position =
			room.Transform.position +
			new Vector3(x, 0, z);

		GameObject enemy = Instantiate(
			prefab,
			position,
			Quaternion.identity);

		aliveEnemies.Add(enemy);
	}

	public int AliveEnemyCount
	{
		get
		{
			aliveEnemies.RemoveAll(enemy => enemy == null);
			return aliveEnemies.Count;
		}
	}
}