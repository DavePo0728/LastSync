using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "FixedMapData",
	menuName = "ColorMap/Fixed Map Data")]
public class FixedMapData : ScriptableObject
{
	public List<FixedRoomNode> RoomNodes = new();

	public List<FixedBridgeNode> BridgeNodes = new();

	public List<FixedPrefabNode> PrefabNodes = new();
}

[Serializable]
public class FixedRoomNode
{
	public StructureData Data;

	public Vector2 Position;
}

[Serializable]
public class FixedBridgeNode
{
	public StructureData Data;

	public Vector2 Position;

	public Direction Direction = Direction.Up;
}

[Serializable]
public class FixedPrefabNode
{
	public GameObject Prefab;

	public bool UsePrefabTransform = true;

	public Vector3 Position = Vector3.zero;

	public Vector3 Rotation = Vector3.zero;

	public Vector3 Scale = Vector3.one;
}
