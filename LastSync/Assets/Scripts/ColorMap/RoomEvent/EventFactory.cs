using System.Collections.Generic;
using UnityEngine;

public static class EventFactory
{
	public static void CreateBattleChain(
	  RoomEventManager manager,
	  BattleConfig battleConfig)
	{
		CloseDoorEvent closeDoor =
			manager.gameObject.AddComponent<CloseDoorEvent>();

		BattleEvent battle1 =
			manager.gameObject.AddComponent<BattleEvent>();

		BattleEvent battle2 =
			manager.gameObject.AddComponent<BattleEvent>();

		BattleEvent battle3 =
			manager.gameObject.AddComponent<BattleEvent>();

		OpenDoorEvent openDoor =
			manager.gameObject.AddComponent<OpenDoorEvent>();

		SpawnChestEvent spawnChest =
			manager.gameObject.AddComponent<SpawnChestEvent>();

		// 三波目前都使用同一份 BattleConfig
		battle1.SetBattleConfig(battleConfig);
		battle2.SetBattleConfig(battleConfig);
		battle3.SetBattleConfig(battleConfig);

		// 串接事件
		closeDoor.Next = battle1;
		battle1.Next = battle2;
		battle2.Next = battle3;
		battle3.Next = openDoor;
		openDoor.Next = spawnChest;

		manager.AddEvent(EventTrigger.EnterRoom, closeDoor);
	}

	public static void CreateFixedBattleChain(
		RoomEventManager manager,
		IReadOnlyList<SpawnPointData> spawnPoints)
	{
		if (manager == null)
		{
			Debug.LogError(
				"EventFactory.CreateFixedBattleChain failed: manager is null.");
			return;
		}

		CloseDoorEvent closeDoor =
			manager.gameObject.AddComponent<CloseDoorEvent>();
		OpenDoorEvent openDoor =
			manager.gameObject.AddComponent<OpenDoorEvent>();
		SpawnChestEvent spawnChest =
			manager.gameObject.AddComponent<SpawnChestEvent>();

		SortedDictionary<int, List<SpawnPointData>> waves = new();
		string roomName = GetRoomName(manager);

		if (spawnPoints == null)
		{
			Debug.LogWarning(
				$"Fixed battle in room '{roomName}' received a null SpawnPoint list. " +
				"No random enemies will be used.",
				manager);
		}
		else
		{
			for (int i = 0; i < spawnPoints.Count; i++)
			{
				SpawnPointData spawnPoint = spawnPoints[i];

				if (!IsValidSpawnPoint(spawnPoint, manager, roomName, i))
				{
					continue;
				}

				if (!waves.TryGetValue(
						spawnPoint.WaveIndex,
						out List<SpawnPointData> wave))
				{
					wave = new List<SpawnPointData>();
					waves.Add(spawnPoint.WaveIndex, wave);
				}

				wave.Add(spawnPoint);
			}
		}

		RoomEvent current = closeDoor;

		foreach (KeyValuePair<int, List<SpawnPointData>> wave in waves)
		{
			BattleEvent battle =
				manager.gameObject.AddComponent<BattleEvent>();

			battle.SetSpawnPoints(wave.Value);
			current.Next = battle;
			current = battle;
		}

		current.Next = openDoor;
		openDoor.Next = spawnChest;

		manager.AddEvent(EventTrigger.EnterRoom, closeDoor);
	}

	private static bool IsValidSpawnPoint(
		SpawnPointData spawnPoint,
		RoomEventManager manager,
		string roomName,
		int index)
	{
		if (spawnPoint == null)
		{
			Debug.LogWarning(
				$"Fixed battle in room '{roomName}' skipped SpawnPoint [{index}]: " +
				"the entry is null.",
				manager);
			return false;
		}

		if (spawnPoint.Prefab == null)
		{
			Debug.LogWarning(
				$"Fixed battle in room '{roomName}' skipped SpawnPoint [{index}] " +
				$"in wave {spawnPoint.WaveIndex}: Prefab is missing.",
				manager);
			return false;
		}

		if (spawnPoint.WaveIndex < 1)
		{
			Debug.LogWarning(
				$"Fixed battle in room '{roomName}' skipped SpawnPoint [{index}]: " +
				$"WaveIndex {spawnPoint.WaveIndex} must be at least 1.",
				manager);
			return false;
		}

		if (!IsFinite(spawnPoint.Position))
		{
			Debug.LogWarning(
				$"Fixed battle in room '{roomName}' skipped SpawnPoint [{index}] " +
				$"in wave {spawnPoint.WaveIndex}: Position contains a non-finite value.",
				manager);
			return false;
		}

		return true;
	}

	private static string GetRoomName(RoomEventManager manager)
	{
		if (manager.Room?.Data != null)
		{
			return manager.Room.Data.name;
		}

		return manager.name;
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

	public static void CreateSpawnChain(
		RoomEventManager manager)
	{
		// 目前沒有事件
	}

	public static void CreateExitChain(
		RoomEventManager manager)
	{
		// 目前沒有事件
	}
}
