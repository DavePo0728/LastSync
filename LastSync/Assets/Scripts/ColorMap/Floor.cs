using System.Collections.Generic;
using UnityEngine;

public class Floor
{
	public FloorConfig Config;

	public List<RoomInstance> Rooms = new();

	public List<RoomInstance> Structures = new();

	public HashSet<Vector2> OccupiedCells = new();

	public List<RoomConnection> Connections = new();

	public List<RoomNode> Graph = new();
}
