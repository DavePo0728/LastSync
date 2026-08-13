using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
		aliveEnemies.RemoveAll(enemy => enemy == null);

		if (enemies == null)
		{
			Debug.LogWarning("BattleSpawner.Spawn failed: enemy list is null.");
			return;
		}

		foreach (EnemySpawnInfo info in enemies)
		{
			if (info == null || info.Prefab == null || info.Count <= 0)
			{
				continue;
			}

			for (int i = 0; i < info.Count; i++)
			{
				SpawnEnemy(info.Prefab);
			}
		}
	}

	public void SpawnEnemy(GameObject prefab)
	{
		if (prefab == null)
		{
			Debug.LogWarning("BattleSpawner.SpawnEnemy failed: prefab is null.");
			return;
		}

		if (manager == null || manager.Room == null || manager.Room.Data == null || manager.Room.Transform == null)
		{
			Debug.LogWarning("BattleSpawner.SpawnEnemy failed: room data is not ready.");
			return;
		}

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

		if (NavMesh.SamplePosition(
			position,
			out NavMeshHit hit,
			2f,
			NavMesh.AllAreas))
		{
			position = hit.position;
		}
		else
		{
			Debug.LogWarning("BattleSpawner.SpawnEnemy failed: no nearby NavMesh position.");
			return;
		}

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
