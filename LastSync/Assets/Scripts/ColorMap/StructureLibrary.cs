using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "ColorMap/StructureLibrary")]
public class StructureLibrary : ScriptableObject
{
	public List<StructureData> Rooms = new();

	public List<StructureData> GetRooms(RoomCategory category)
	{
		return Rooms
			.Where(room => room != null)
			.Where(room => room.Category == category)
			.ToList();
	}

	public List<StructureData> GetRooms(
		RoomCategory category,
		Direction direction)
	{
		return Rooms
			.Where(room => room != null)
			.Where(room =>
				room.Category == category &&
				room.Doors.Any(door => door.Direction == direction))
			.ToList();
	}

	public StructureData GetRandomRoom(RoomCategory category)
	{
		List<StructureData> rooms = GetRooms(category);

		if (rooms.Count == 0)
		{
			return null;
		}

		return GetWeightedRandomRoom(rooms);
	}

	public StructureData GetRandomRoom(
		RoomCategory category,
		Direction direction)
	{
		List<StructureData> rooms =
			GetRooms(category, direction);

		if (rooms.Count == 0)
		{
			return null;
		}

		return GetWeightedRandomRoom(rooms);
	}

	private StructureData GetWeightedRandomRoom(List<StructureData> rooms)
	{
		int totalWeight = rooms.Sum(room => Mathf.Max(0, room.Weight));

		if (totalWeight <= 0)
		{
			return rooms[Random.Range(0, rooms.Count)];
		}

		int roll = Random.Range(0, totalWeight);

		foreach (StructureData room in rooms)
		{
			roll -= Mathf.Max(0, room.Weight);

			if (roll < 0)
			{
				return room;
			}
		}

		return rooms[rooms.Count - 1];
	}
}
