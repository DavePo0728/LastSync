using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class FixedMapGenerator : MonoBehaviour
{
	[SerializeField]
	private FixedMapData fixedMapData;

	[SerializeField]
	private StructureSpawner structureSpawner;

	[SerializeField]
	private RoomContentSpawner contentSpawner;

	[SerializeField]
	private GameObject playerPrefab;

	[SerializeField]
	private CinemachineCamera camera;

	[SerializeField]
	private bool generateOnStart = true;

	private readonly List<Mesh> generatedMeshes = new();
	private GameObject runtimeRoot;
	private GameObject generatedPlayer;
	private Floor floor;

	public Floor CurrentFloor => floor;

	private void Start()
	{
		if (generateOnStart)
		{
			Generate();
		}
	}

	[ContextMenu("Generate Fixed Map")]
	public void Generate()
	{
		if (!CanGenerate())
		{
			return;
		}

		ClearGeneratedMap();
		CreateRuntimeContainers(
			out Transform roomsRoot,
			out Transform bridgesRoot,
			out Transform prefabsRoot);

		floor = new Floor();

		SpawnRooms(roomsRoot);
		SpawnBridges(bridgesRoot);
		SpawnPrefabs(prefabsRoot);

		contentSpawner.Spawn(floor);
		SpawnPlayer();
	}

	[ContextMenu("Clear Fixed Map")]
	public void ClearGeneratedMap()
	{
		if (runtimeRoot != null)
		{
			runtimeRoot.SetActive(false);

			BattleSpawner[] battleSpawners =
				runtimeRoot.GetComponentsInChildren<BattleSpawner>(true);

			foreach (BattleSpawner battleSpawner in battleSpawners)
			{
				battleSpawner.ClearSpawnedEnemies();
			}
		}

		if (generatedPlayer != null)
		{
			generatedPlayer.SetActive(false);
			DestroyGeneratedObject(generatedPlayer);
			generatedPlayer = null;
		}

		foreach (Mesh mesh in generatedMeshes)
		{
			if (mesh != null)
			{
				DestroyGeneratedObject(mesh);
			}
		}

		generatedMeshes.Clear();

		if (runtimeRoot != null)
		{
			DestroyGeneratedObject(runtimeRoot);
			runtimeRoot = null;
		}

		floor = null;
	}

	private bool CanGenerate()
	{
		if (fixedMapData == null)
		{
			Debug.LogError(
				"FixedMapGenerator requires a FixedMapData. " +
				"The currently generated map was not cleared.",
				this);
			return false;
		}

		if (structureSpawner == null)
		{
			Debug.LogError(
				$"FixedMapGenerator '{fixedMapData.name}' requires a StructureSpawner. " +
				"The currently generated map was not cleared.",
				this);
			return false;
		}

		if (contentSpawner == null)
		{
			Debug.LogError(
				$"FixedMapGenerator '{fixedMapData.name}' requires a RoomContentSpawner. " +
				"The currently generated map was not cleared.",
				this);
			return false;
		}

		return true;
	}

	private void CreateRuntimeContainers(
		out Transform roomsRoot,
		out Transform bridgesRoot,
		out Transform prefabsRoot)
	{
		runtimeRoot = new GameObject("FixedMap_Runtime");
		runtimeRoot.transform.SetPositionAndRotation(
			Vector3.zero,
			Quaternion.identity);
		runtimeRoot.transform.localScale = Vector3.one;

		roomsRoot = CreateContainer("Rooms", runtimeRoot.transform);
		bridgesRoot = CreateContainer("Bridges", runtimeRoot.transform);
		prefabsRoot = CreateContainer("Prefabs", runtimeRoot.transform);
	}

	private void SpawnRooms(Transform roomsRoot)
	{
		if (fixedMapData.RoomNodes == null)
		{
			return;
		}

		for (int i = 0; i < fixedMapData.RoomNodes.Count; i++)
		{
			FixedRoomNode node = fixedMapData.RoomNodes[i];

			if (node == null)
			{
				LogNodeWarning("room", i, "the node is null");
				continue;
			}

			if (node.Data == null)
			{
				LogNodeWarning("room", i, "StructureData is missing");
				continue;
			}

			if (!IsFinite(node.Position))
			{
				LogNodeWarning("room", i, "Position contains a non-finite value");
				continue;
			}

			RoomRole role = node.Data.PresetRole == RoomRole.None
				? RoomRole.Combat
				: node.Data.PresetRole;

			RoomInstance room = new()
			{
				Data = node.Data,
				Position = node.Position,
				Rotation = 0,
				AssignedRole = role
			};

			floor.Rooms.Add(room);
			floor.Structures.Add(room);

			structureSpawner.Spawn(
				room,
				roomsRoot,
				true,
				generatedMeshes);
		}
	}

	private void SpawnBridges(Transform bridgesRoot)
	{
		if (fixedMapData.BridgeNodes == null)
		{
			return;
		}

		for (int i = 0; i < fixedMapData.BridgeNodes.Count; i++)
		{
			FixedBridgeNode node = fixedMapData.BridgeNodes[i];

			if (node == null)
			{
				LogNodeWarning("bridge", i, "the node is null");
				continue;
			}

			if (node.Data == null)
			{
				LogNodeWarning("bridge", i, "StructureData is missing");
				continue;
			}

			if (!IsFinite(node.Position))
			{
				LogNodeWarning("bridge", i, "Position contains a non-finite value");
				continue;
			}

			if (!Enum.IsDefined(typeof(Direction), node.Direction))
			{
				LogNodeWarning(
					"bridge",
					i,
					$"Direction value {(int)node.Direction} is undefined");
				continue;
			}

			RoomInstance bridge = new()
			{
				Data = node.Data,
				Position = node.Position,
				Rotation = DirectionToAngle(node.Direction)
			};

			floor.Structures.Add(bridge);

			structureSpawner.Spawn(
				bridge,
				bridgesRoot,
				false,
				generatedMeshes);
		}
	}

	private void SpawnPrefabs(Transform prefabsRoot)
	{
		if (fixedMapData.PrefabNodes == null)
		{
			return;
		}

		for (int i = 0; i < fixedMapData.PrefabNodes.Count; i++)
		{
			FixedPrefabNode node = fixedMapData.PrefabNodes[i];

			if (node == null)
			{
				LogNodeWarning("prefab", i, "the node is null");
				continue;
			}

			if (node.Prefab == null)
			{
				LogNodeWarning("prefab", i, "Prefab is missing");
				continue;
			}

			if (!IsFinite(node.Position) ||
				!IsFinite(node.Rotation) ||
				!IsFinite(node.Scale))
			{
				LogNodeWarning(
					"prefab",
					i,
					"Position, Rotation, or Scale contains a non-finite value");
				continue;
			}

			GameObject instance = Instantiate(
				node.Prefab,
				prefabsRoot,
				false);

			if (!node.UsePrefabTransform)
			{
				instance.transform.localPosition = node.Position;
				instance.transform.localRotation = Quaternion.Euler(node.Rotation);
				instance.transform.localScale = node.Scale;
			}
		}
	}

	private void SpawnPlayer()
	{
		if (playerPrefab == null)
		{
			Debug.LogError(
				$"Fixed map '{fixedMapData.name}' cannot create a player: " +
				"Player Prefab is missing.",
				this);
			return;
		}

		List<RoomInstance> spawnRooms = floor.Rooms
			.Where(room => room.AssignedRole == RoomRole.Spawn)
			.ToList();

		if (spawnRooms.Count != 1)
		{
			Debug.LogError(
				$"Fixed map '{fixedMapData.name}' requires exactly one Spawn room " +
				$"to create the player, but found {spawnRooms.Count}. " +
				"The map remains generated without a player.",
				this);
			return;
		}

		RoomInstance spawnRoom = spawnRooms[0];

		if (spawnRoom.Transform == null)
		{
			Debug.LogError(
				$"Fixed map '{fixedMapData.name}' cannot create a player: " +
				"the Spawn room was not instantiated.",
				this);
			return;
		}

		generatedPlayer = Instantiate(
			playerPrefab,
			PlayerSpawnPositionUtility.GetSpawnPosition(spawnRoom),
			Quaternion.identity);

		if (camera != null)
		{
			camera.Follow = generatedPlayer.transform;
		}
	}

	private void LogNodeWarning(
		string nodeType,
		int index,
		string reason)
	{
		Debug.LogWarning(
			$"Fixed map '{fixedMapData.name}' skipped {nodeType} node [{index}]: {reason}.",
			this);
	}

	private static Transform CreateContainer(
		string name,
		Transform parent)
	{
		Transform container = new GameObject(name).transform;
		container.SetParent(parent, false);
		return container;
	}

	private static int DirectionToAngle(Direction direction)
	{
		return direction switch
		{
			Direction.Right => 90,
			Direction.Down => 180,
			Direction.Left => 270,
			_ => 0
		};
	}

	private static bool IsFinite(Vector2 value)
	{
		return IsFinite(value.x) && IsFinite(value.y);
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

	private static void DestroyGeneratedObject(UnityEngine.Object target)
	{
		if (Application.isPlaying)
		{
			Destroy(target);
		}
		else
		{
			DestroyImmediate(target);
		}
	}
}
