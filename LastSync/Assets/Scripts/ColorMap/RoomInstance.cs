using System.Collections.Generic;
using UnityEngine;

public class RoomInstance
{
	public StructureData Data;

	public Vector2 Position;

	public int Rotation;
	public Transform Transform;
	public HashSet<Structure> UsedDoors = new();
	public bool RoleLocked;

	public RoomRole PresetRole => Data.PresetRole;

	public RoomRole AssignedRole = RoomRole.None;

	public int Depth = -1;
}
