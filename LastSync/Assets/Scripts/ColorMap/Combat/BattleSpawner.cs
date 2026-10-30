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

	public void SpawnAtPoints(IReadOnlyList<SpawnPointData> spawnPoints)
	{
		aliveEnemies.RemoveAll(enemy => enemy == null);

		if (spawnPoints == null)
		{
			Debug.LogWarning(
				"BattleSpawner.SpawnAtPoints received a null SpawnPoint list. " +
				"No random enemies will be used.");
			return;
		}

		for (int i = 0; i < spawnPoints.Count; i++)
		{
			SpawnPointData spawnPoint = spawnPoints[i];
			int sourceIndex = GetSourceIndex(spawnPoint, i);

			if (spawnPoint == null ||
				spawnPoint.Prefab == null ||
				spawnPoint.WaveIndex < 1 ||
				!IsFinite(spawnPoint.Position))
			{
				Debug.LogWarning(
					$"BattleSpawner.SpawnAtPoints skipped invalid SpawnPoint [{sourceIndex}].",
					this);
				continue;
			}

			SpawnEnemyAtPoint(
				spawnPoint.Prefab,
				spawnPoint.Position,
				sourceIndex,
				spawnPoint.WaveIndex);
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

		TrySpawnEnemyAtWorldPosition(
			prefab,
			position,
			"BattleSpawner.SpawnEnemy");
	}

	public void ClearSpawnedEnemies()
	{
		foreach (GameObject enemy in aliveEnemies)
		{
			if (enemy == null)
			{
				continue;
			}

			enemy.SetActive(false);

			if (Application.isPlaying)
			{
				Destroy(enemy);
			}
			else
			{
				DestroyImmediate(enemy);
			}
		}

		aliveEnemies.Clear();
	}

	public int AliveEnemyCount
	{
		get
		{
			aliveEnemies.RemoveAll(enemy => enemy == null);
			return aliveEnemies.Count;
		}
	}

	private void SpawnEnemyAtPoint(
		GameObject prefab,
		Vector3 localPosition,
		int spawnPointIndex,
		int waveIndex)
	{
		if (manager == null ||
			manager.Room == null ||
			manager.Room.Transform == null)
		{
			Debug.LogWarning(
				$"BattleSpawner.SpawnAtPoints skipped SpawnPoint [{spawnPointIndex}] " +
				$"in wave {waveIndex}: room data is not ready.",
				this);
			return;
		}

		Vector3 worldPosition =
			manager.Room.Transform.TransformPoint(localPosition);

		TrySpawnEnemyAtWorldPosition(
			prefab,
			worldPosition,
			$"BattleSpawner.SpawnAtPoints SpawnPoint [{spawnPointIndex}] " +
			$"in wave {waveIndex}");
	}

	private int GetSourceIndex(
		SpawnPointData spawnPoint,
		int fallbackIndex)
	{
		if (spawnPoint == null ||
			manager?.Room?.Data?.SpawnPoint == null)
		{
			return fallbackIndex;
		}

		int sourceIndex = manager.Room.Data.SpawnPoint.IndexOf(spawnPoint);
		return sourceIndex >= 0 ? sourceIndex : fallbackIndex;
	}

	private bool TrySpawnEnemyAtWorldPosition(
		GameObject prefab,
		Vector3 requestedPosition,
		string operation)
	{
		if (!NavMesh.SamplePosition(
				requestedPosition,
				out NavMeshHit hit,
				2f,
				NavMesh.AllAreas))
		{
			Debug.LogWarning(
				$"{operation} failed: no nearby NavMesh position.",
				this);
			return false;
		}

		GameObject enemy = Instantiate(
			prefab,
			hit.position,
			Quaternion.identity);

		if (enemy == null)
		{
			Debug.LogWarning(
				$"{operation} failed: the enemy could not be instantiated.",
				this);
			return false;
		}

		aliveEnemies.Add(enemy);
		return true;
	}

	private static bool IsFinite(Vector3 value)
	{
		return IsFinite(value.x) &&
			IsFinite(value.y) &&
			IsFinite(value.z);
	}

	private static bool IsFinite(float value)
	{
		return !float.IsNaN(value) && !float.IsInfinity(value);
	}
}
