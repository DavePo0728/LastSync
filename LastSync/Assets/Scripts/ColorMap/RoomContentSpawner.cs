using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class RoomContentSpawner : MonoBehaviour
{
	[SerializeField]
	private BattleConfig defaultBattleConfig;

	public void Spawn(Floor floor)
	{
		if (floor == null)
		{
			Debug.LogError("RoomContentSpawner.Spawn failed: floor is null.");
			return;
		}

		foreach (RoomInstance room in floor.Rooms)
		{
			if (room.Transform == null)
			{
				continue;
			}

			RoomEventManager manager = room.Transform.GetComponent<RoomEventManager>();

			if (manager == null)
			{
				manager = room.Transform.gameObject.AddComponent<RoomEventManager>();
			}

			manager.Initialize(room);
			BindRoomTriggers(room, manager);

			switch (room.AssignedRole)
			{
				case RoomRole.Spawn:
					EventFactory.CreateSpawnChain(manager);
					break;

				case RoomRole.Combat:
					EnsureBattleSpawner(room);
					BuildRoomNavMesh(room);

					if (room.Data.SpawnPoint == null ||
						room.Data.SpawnPoint.Count == 0)
					{
						EventFactory.CreateBattleChain(
							manager,
							defaultBattleConfig);
					}
					else
					{
						EventFactory.CreateFixedBattleChain(
							manager,
							room.Data.SpawnPoint);
					}

					break;

				case RoomRole.Elite:
					EnsureBattleSpawner(room);
					break;

				case RoomRole.Stair:
					EventFactory.CreateExitChain(manager);
					break;

				case RoomRole.Event:
					if (room.Data.SpawnPoint != null &&
						room.Data.SpawnPoint.Count > 0)
					{
						Debug.LogWarning(
							$"Event room '{room.Transform.name}' contains SpawnPoint data. " +
							"The enemy configuration is ignored.",
							room.Transform);
					}

					break;

				case RoomRole.Boss:
					EnsureBattleSpawner(room);
					break;
			}
		}
	}

	private void BindRoomTriggers(
		RoomInstance room,
		RoomEventManager manager)
	{
		RoomFloorTrigger[] triggers =
			room.Transform.GetComponentsInChildren<RoomFloorTrigger>(true);

		foreach (RoomFloorTrigger trigger in triggers)
		{
			trigger.roomEventManager = manager;
		}
	}

	private void EnsureBattleSpawner(RoomInstance room)
	{
		if (room.Transform.GetComponent<BattleSpawner>() == null)
		{
			room.Transform.gameObject.AddComponent<BattleSpawner>();
		}
	}

	private void BuildRoomNavMesh(RoomInstance room)
	{
		if (room.Transform == null)
		{
			return;
		}

		NavMeshSurface surface =
			room.Transform.GetComponent<NavMeshSurface>();

		if (surface == null)
		{
			surface = room.Transform.gameObject.AddComponent<NavMeshSurface>();
		}

		surface.collectObjects = CollectObjects.Children;
		surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
		surface.BuildNavMesh();
	}
}
