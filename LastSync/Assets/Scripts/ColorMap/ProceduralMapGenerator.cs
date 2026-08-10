using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
	[SerializeField]
	private StructureLibrary roomLibrary;

	[SerializeField]
	private StructureLibrary bridgeLibrary;

	[SerializeField]
	private StructureSpawner structureSpawner;

	private List<RoomInstance> structures = new();

	private List<RoomInstance> rooms = new();

	[SerializeField]
	private Structure bridgeExitDoor;
	private Structure roomEntryDoor;


	private bool[,] occupiedMap;

	public FloorConfig floorConfig;

	private Floor floor;
	[SerializeField]
	private RoomContentSpawner contentSpawner;

	public GameObject playerPrefab;
	private void Start()
	{
		GenerateMap();
		SpawnPlayer();
	}
	private void GenerateMap()
	{
		if (!CanGenerateMap())
		{
			return;
		}

		Random.InitState(floorConfig.Seed);

		InitializeFloor();

		CreateStartRoom();

		ExpandRooms();

		BuildRoomGraph();

		AssignRoomRoles();

		SpawnContents();
	}

	private RoomInstance CreateBridge(
		RoomInstance room,
		Structure roomDoor)
	{

		if (roomDoor == null)
		{
			return null;
		}

		Direction targetDirection =
			Opposite(roomDoor.Direction);

		StructureData bridgeData =
	bridgeLibrary.GetRandomRoom(
		RoomCategory.Bridge,
		targetDirection);

		if (bridgeData == null)
		{
			return null;
		}

		Structure bridgeDoor =
			bridgeData.Doors.First(
				x =>
					x.Direction ==
					targetDirection);

		int roomAngle =
			DirectionToAngle(
				roomDoor.Direction);

		int bridgeAngle =
			DirectionToAngle(
				bridgeDoor.Direction);

		int rotation =
			(
				roomAngle
				-
				bridgeAngle
				+
				180
				+
				360
			) % 360;

		Vector2 roomDoorWorld =
			GetDoorWorldPosition(
				room,
				roomDoor);

		Vector2 bridgeDoorLocal =
			GetDoorLocalPosition(
				bridgeData,
				bridgeDoor);

		bridgeDoorLocal =
			RotatePoint(
				bridgeDoorLocal,
				rotation);

		RoomInstance bridge = new()
		{
			Data = bridgeData,
			Rotation = rotation,

			Position =
				roomDoorWorld
				-
				bridgeDoorLocal
				+
				(Vector2)
				DirectionToVector(
					roomDoor.Direction)
		};

		return bridge;
	}

	private RoomInstance CreateRoom(
		RoomInstance bridge)
	{
		bridgeExitDoor =
			bridge.Data.Doors.First(
				x =>
					!bridge.UsedDoors.Contains(x));

		Direction targetDirection =
			Opposite(
				bridgeExitDoor.Direction);

		StructureData roomData =
	roomLibrary.GetRandomRoom(
		RoomCategory.Normal,
		targetDirection);

		if (roomData == null)
		{
			return null;
		}

		roomEntryDoor =
			roomData.Doors.First(
				door =>
					door.Direction ==
					targetDirection);

		int exitAngle =
			DirectionToAngle(
				bridgeExitDoor.Direction);

		int roomAngle =
			DirectionToAngle(
				roomEntryDoor.Direction);

		int rotation =
		(
			exitAngle
			-
			roomAngle
			+
			180
			+
			360
		) % 360;

		Vector2 exitDoorWorld =
			GetDoorWorldPosition(
				bridge,
				bridgeExitDoor);

		Vector2 roomDoorLocal =
			GetDoorLocalPosition(
				roomData,
				roomEntryDoor);

		roomDoorLocal =
			RotatePoint(
				roomDoorLocal,
				rotation);

		RoomInstance roomB = new()
		{
			Data = roomData,

			Position =
				exitDoorWorld
				-
				roomDoorLocal
				+
				(Vector2)
				DirectionToVector(
					bridgeExitDoor.Direction),

			Rotation = rotation
		};

		return roomB;
	}

	private bool ExpandRoom(
	RoomInstance room)
	{
		Structure roomDoor =
			GetRandomAvailableDoor(room);

		if (roomDoor == null)
		{
			return false;
		}

		RoomInstance bridge =
			CreateBridge(
				room,
				roomDoor);

		if (bridge == null)
		{
			return false;
		}

		RoomInstance nextRoom =
			CreateRoom(bridge);

		if (nextRoom == null)
		{
			return false;
		}

		if (!CanPlaceStructure(bridge))
		{
			return false;
		}

		if (!CanPlaceStructure(nextRoom))
		{
			return false;
		}

		if (!CanPlaceRoom(nextRoom))
		{
			return false;
		}

		CommitExpansion(
			room,
			roomDoor,
			bridge,
			nextRoom);

		return true;
	}

	private void CommitExpansion(
	RoomInstance room,
	Structure roomDoor,
	RoomInstance bridge,
	RoomInstance nextRoom)
	{
		// 橋入口
		Structure bridgeEntry =
			bridge.Data.Doors.First(
				x =>
					x.Direction ==
					Opposite(roomDoor.Direction));

		// 橋出口
		Structure bridgeExit =
			bridge.Data.Doors.First(
				x => x != bridgeEntry);

		// 房間入口
		Structure roomEntry =
			nextRoom.Data.Doors.First(
				x =>
					x.Direction ==
					Opposite(
						bridgeExit.Direction));

		// 標記門已使用
		room.UsedDoors.Add(
			roomDoor);

		bridge.UsedDoors.Add(
			bridgeEntry);

		bridge.UsedDoors.Add(
			bridgeExit);

		nextRoom.UsedDoors.Add(
			roomEntry);

		// 註冊碰撞
		RegisterStructure(
			bridge);

		RegisterStructure(
			nextRoom);

		// 加入列表
		floor.Structures.Add(
			bridge);

		floor.Structures.Add(
			nextRoom);

		floor.Rooms.Add(
			nextRoom);

		floor.Connections.Add(
	new RoomConnection
	{
		From = room,
		To = nextRoom,
		Bridge = bridge
	});

		// 真正生成
		structureSpawner.Spawn(
			bridge,
			false);

		structureSpawner.Spawn(
			nextRoom);
	}
	private int DirectionToAngle(Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				return 0;

			case Direction.Right:
				return 90;

			case Direction.Down:
				return 180;

			case Direction.Left:
				return 270;
		}

		return 0;
	}
	private Vector2 RotatePoint(
		Vector2 p,
		int angle)
	{
		switch (angle)
		{
			case 90:
				return new Vector2(
					-p.y,
					p.x);

			case 180:
				return new Vector2(
					-p.x,
					-p.y);

			case 270:
				return new Vector2(
					p.y,
					-p.x);

			default:
				return p;
		}
	}
	private Vector2 GetStructureCenter(StructureData data)
	{
		return new Vector2(
			data.Size.x - 1,
			data.Size.y - 1) * 0.5f;

	}
	Vector2 GetDoorLocalPosition(
		StructureData data,
		Structure door)
	{
		Vector2 structureCenter =
			GetStructureCenter(data);

		Vector2 doorCenter =
			(
				(Vector2)door.Position
				+
				(Vector2)door.End
			) * 0.5f;

		return doorCenter - structureCenter;
	}
	Vector2 GetDoorWorldPosition(
		RoomInstance room,
		Structure door)
	{
		Vector2 local =
			GetDoorLocalPosition(
				room.Data,
				door);

		local =
			RotatePoint(
				local,
				room.Rotation);

		return
			(Vector2)room.Position
			+
			local;
	}
	private Vector2Int DirectionToVector(Direction dir)
	{
		return dir switch
		{
			Direction.Up => Vector2Int.up,
			Direction.Right => Vector2Int.right,
			Direction.Down => Vector2Int.down,
			Direction.Left => Vector2Int.left,
			_ => Vector2Int.zero
		};
	}
	private Direction Opposite(
	Direction dir)
	{
		switch (dir)
		{
			case Direction.Up:
				return Direction.Down;

			case Direction.Right:
				return Direction.Left;

			case Direction.Down:
				return Direction.Up;

			case Direction.Left:
				return Direction.Right;
		}

		return Direction.None;
	}
	private Structure GetRandomAvailableDoor(
		RoomInstance room)
	{
		List<Structure> availableDoors =
			room.Data.Doors
			.Where(
				door =>
					!room.UsedDoors.Contains(door))
			.ToList();

		if (availableDoors.Count == 0)
		{
			return null;
		}

		return availableDoors[
			Random.Range(
				0,
				availableDoors.Count)];
	}
	private RoomInstance GetRandomRoom()
	{
		List<RoomInstance> candidates =
			floor.Rooms
			.Where(
				room =>
					GetRandomAvailableDoor(room)
					!= null)
			.ToList();

		if (candidates.Count == 0)
		{
			return null;
		}

		return candidates[
			Random.Range(
				0,
				candidates.Count)];
	}
	private void RegisterStructure(
		RoomInstance instance)
	{
		foreach (Structure s in instance.Data.Structures)
		{
			Vector2 center =
				instance.Position +
				RotateCell(
					s.Position,
					instance.Rotation);

			for (int y = -2; y <= 2; y++)
			{
				for (int x = -2; x <= 2; x++)
				{
					floor.OccupiedCells.Add(
						center + new Vector2(x, y));
				}
			}
		}
	}
	private bool CanPlaceStructure(
	RoomInstance instance)
	{
		foreach (Structure s in instance.Data.Structures)
		{
			Vector2 pos =
				instance.Position +
				RotateCell(
					s.Position,
					instance.Rotation);

			if (floor.OccupiedCells.Contains(pos))
			{
				return false;
			}
		}

		return true;
	}
	private Vector2 RotateCell(
	Vector2 p,
	int angle)
	{
		switch (angle)
		{
			case 90:
				return new Vector2(
					-p.y,
					p.x);

			case 180:
				return new Vector2(
					-p.x,
					-p.y);

			case 270:
				return new Vector2(
					p.y,
					-p.x);

			default:
				return p;
		}
	}

	private RoomBounds GetRoomBounds(
	RoomInstance room,
	int padding = 0)
	{
		RoomBounds bounds = new()
		{
			MinX = int.MaxValue,
			MinY = int.MaxValue,
			MaxX = int.MinValue,
			MaxY = int.MinValue
		};

		foreach (Structure structure in room.Data.Structures)
		{
			Vector2 world =
				room.Position +
				RotateCell(
					structure.Position,
					room.Rotation);

			int x = Mathf.RoundToInt(world.x);
			int y = Mathf.RoundToInt(world.y);

			bounds.MinX = Mathf.Min(bounds.MinX, x);
			bounds.MinY = Mathf.Min(bounds.MinY, y);

			bounds.MaxX = Mathf.Max(bounds.MaxX, x);
			bounds.MaxY = Mathf.Max(bounds.MaxY, y);
		}

		bounds.MinX -= padding;
		bounds.MinY -= padding;

		bounds.MaxX += padding;
		bounds.MaxY += padding;

		return bounds;
	}
	private bool CanPlaceRoom(
		RoomInstance room,
		int padding = 2)
	{
		RoomBounds newBounds =
			GetRoomBounds(room, padding);

		foreach (RoomInstance other in floor.Rooms)
		{
			RoomBounds otherBounds =
				GetRoomBounds(other, padding);

			bool overlap =
				newBounds.MinX <= otherBounds.MaxX &&
				newBounds.MaxX >= otherBounds.MinX &&
				newBounds.MinY <= otherBounds.MaxY &&
				newBounds.MaxY >= otherBounds.MinY;

			if (overlap)
			{
				return false;
			}
		}

		return true;
	}
	private void InitializeFloor()
	{
		floor = new Floor
		{
			Config = floorConfig
		};
	}
	private void CreateStartRoom()
	{
		RoomInstance roomA = new()
		{
			Data = roomLibrary.Rooms[0],
			Position = Vector2.zero,
			Rotation = 0
		};

		floor.Rooms.Add(roomA);
		floor.Structures.Add(roomA);

		RegisterStructure(roomA);

		structureSpawner.Spawn(roomA);
	}
	private void ExpandRooms()
	{
		int created = 1;          // 已建立 Start Room
		int failedCount = 0;
		const int MaxFailedCount = 100;

		while (created < floorConfig.MaxRoomCount)
		{
			RoomInstance room = GetRandomRoom();

			if (room == null)
			{
				Debug.Log("沒有可擴展的房間");
				break;
			}

			if (ExpandRoom(room))
			{
				created++;
				failedCount = 0;
			}
			else
			{
				failedCount++;

				if (failedCount >= MaxFailedCount)
				{
					Debug.LogWarning(
						$"連續失敗 {MaxFailedCount} 次，停止生成。");
					break;
				}
			}
		}
	}
	private void BuildRoomGraph()
	{
		floor.Graph.Clear();

		Dictionary<RoomInstance, RoomNode> lookup = new();

		// 建立所有節點
		foreach (RoomInstance room in floor.Rooms)
		{
			room.Depth = -1;

			RoomNode node = new()
			{
				Room = room,
				Depth = -1
			};

			floor.Graph.Add(node);

			lookup.Add(room, node);
		}

		// 建立連線
		foreach (RoomConnection connection in floor.Connections)
		{
			RoomNode from = lookup[connection.From];
			RoomNode to = lookup[connection.To];

			from.Neighbors.Add(to);
			to.Neighbors.Add(from);
		}

		// 從 Spawn 開始計算 Depth
		if (floor.Graph.Count == 0)
		{
			return;
		}

		Queue<RoomNode> queue = new();

		RoomNode start = floor.Graph[0];

		start.Depth = 0;
		start.Room.Depth = 0;

		queue.Enqueue(start);

		while (queue.Count > 0)
		{
			RoomNode current = queue.Dequeue();

			foreach (RoomNode neighbor in current.Neighbors)
			{
				if (neighbor.Depth != -1)
				{
					continue;
				}

				neighbor.Depth = current.Depth + 1;
				neighbor.Room.Depth = neighbor.Depth;

				queue.Enqueue(neighbor);
			}
		}
	}
	private void AssignRoomRoles()
	{
		ClearRoles();

		AssignSpawn();

		AssignTreasure();

		AssignElite();

		AssignBoss();

		AssignEvent();

		AssignStair();

		AssignCombat();

		RenameRooms();
	}
	private void SpawnContents()
	{
		if (contentSpawner != null)
		{
			contentSpawner.Spawn(floor);
		}
	}
	private void ClearRoles()
	{
		foreach (RoomInstance room in floor.Rooms)
		{
			room.AssignedRole = RoomRole.None;
		}
	}
	private void AssignSpawn()
	{
		if (floor.Rooms.Count == 0)
		{
			return;
		}

		floor.Rooms[0].AssignedRole = RoomRole.Spawn;
	}
	private void AssignCombat()
	{
		foreach (RoomInstance room in floor.Rooms)
		{
			if (room.AssignedRole == RoomRole.None)
			{
				room.AssignedRole = RoomRole.Combat;
			}
		}
	}

	private void AssignTreasure()
	{
		for (int i = 0; i < floorConfig.TreasureRoomCount; i++)
		{
			if (!AssignDeepestUnusedRoom(RoomRole.Treasure))
			{
				break;
			}
		}
	}

	private void AssignElite()
	{
		for (int i = 0; i < floorConfig.EliteRoomCount; i++)
		{
			if (!AssignDeepestUnusedRoom(RoomRole.Elite))
			{
				break;
			}
		}
	}

	private void AssignBoss()
	{
		for (int i = 0; i < floorConfig.BossRoomCount; i++)
		{
			if (!AssignDeepestUnusedRoom(RoomRole.Boss))
			{
				break;
			}
		}
	}

	private void AssignEvent()
	{
		for (int i = 0; i < floorConfig.EventRoomCount; i++)
		{
			if (!AssignDeepestUnusedRoom(RoomRole.Event))
			{
				break;
			}
		}
	}

	private void AssignStair()
	{
		for (int i = 0; i < floorConfig.StairRoomCount; i++)
		{
			if (!AssignDeepestUnusedRoom(RoomRole.Stair))
			{
				break;
			}
		}
	}
	private bool AssignDeepestUnusedRoom(RoomRole role)
	{
		RoomInstance room = GetDeepestUnusedRoom();

		if (room == null)
		{
			return false;
		}

		room.AssignedRole = role;

		return true;
	}
	private RoomInstance GetDeepestUnusedRoom()
	{
		return floor.Rooms
			.Where(room => room.AssignedRole == RoomRole.None)
			.OrderByDescending(room => room.Depth)
			.FirstOrDefault();
	}
	private RoomInstance GetRandomUnusedRoom()
	{
		List<RoomInstance> rooms = floor.Rooms
			.Where(room => room.AssignedRole == RoomRole.None)
			.ToList();

		if (rooms.Count == 0)
		{
			return null;
		}

		return rooms[Random.Range(0, rooms.Count)];
	}
	private IEnumerable<RoomInstance> GetRooms(
	RoomRole role)
	{
		return floor.Rooms
			.Where(room =>
				room.AssignedRole == role);
	}
	private void RenameRooms()
	{
		foreach (RoomInstance room in floor.Rooms)
		{
			if (room.Transform == null)
			{
				continue;
			}

			room.Transform.name = room.AssignedRole.ToString();
		}
	}
	private void SpawnPlayer()
	{
		if (floor == null || playerPrefab == null)
		{
			return;
		}

		RoomInstance spawnRoom = floor.Rooms
			.FirstOrDefault(room => room.AssignedRole == RoomRole.Spawn);

		if (spawnRoom?.Transform == null)
		{
			return;
		}

		GameObject player = Instantiate(
			playerPrefab,
			spawnRoom.Transform.position,
			Quaternion.identity);

		CameraFollow cameraFollow =
			Camera.main != null
				? Camera.main.GetComponent<CameraFollow>()
				: null;

		if (cameraFollow != null)
		{
			cameraFollow.SetTarget(player.transform);
		}
	}

	private bool CanGenerateMap()
	{
		if (floorConfig == null)
		{
			Debug.LogError("ProceduralMapGenerator requires a FloorConfig.");
			return false;
		}

		if (roomLibrary == null || roomLibrary.Rooms.Count == 0)
		{
			Debug.LogError("ProceduralMapGenerator requires a room library with at least one room.");
			return false;
		}

		if (bridgeLibrary == null)
		{
			Debug.LogError("ProceduralMapGenerator requires a bridge library.");
			return false;
		}

		if (structureSpawner == null)
		{
			Debug.LogError("ProceduralMapGenerator requires a StructureSpawner.");
			return false;
		}

		return true;
	}
}
