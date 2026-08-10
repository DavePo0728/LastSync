using UnityEngine;

public class StructureSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject pillarPrefab;

	[SerializeField]
	private GameObject wallPrefab;

	[SerializeField]
	private GameObject doorPrefab;

	[SerializeField]
	private GameObject floorPrefab;

	[SerializeField]
	private Transform mapRoot;
	[SerializeField]
	private Transform bRoot;
	public GameObject Spawn(
		RoomInstance room,
		bool spawnDoor = true)
	{

		GameObject roomRoot =
			new GameObject(room.Data.StructureID);

		if (spawnDoor)
		{
			roomRoot.transform.SetParent(mapRoot, false);
		}
		else
		{
			roomRoot.transform.SetParent(bRoot, false);
		}
		Transform doorRoot = new GameObject("Doors").transform;
		doorRoot.SetParent(roomRoot.transform, false);

		Transform floorRoot = new GameObject("Floor").transform;
		floorRoot.SetParent(roomRoot.transform, false);

		Transform wallRoot = new GameObject("Walls").transform;
		wallRoot.SetParent(roomRoot.transform, false);





		roomRoot.transform.position =
			new Vector3(
				room.Position.x,
				0,
				room.Position.y);

		roomRoot.transform.rotation =
			Quaternion.Euler(
				0,
				room.Rotation,
				0);

		Vector3 pivot = GetPivot(room.Data);

		foreach (Structure structure in room.Data.Structures)
		{
			switch (structure.Type)
			{
				case CellType.Pillar:
					SpawnPillar(
						structure,
						wallRoot,
						pivot);
					break;

				case CellType.Wall:
					SpawnWallSegment(
						structure,
						wallRoot,
						pivot);
					break;

				case CellType.Door:

					if (spawnDoor)
					{
						SpawnDoor(
							structure,
							doorRoot,
							pivot);
					}

					break;

				case CellType.Floor:
					SpawnFloorSegment(
						structure,
						floorRoot,
						pivot);
					break;
			}
		}

		room.Transform = roomRoot.transform;

		return roomRoot;
	}

	private Vector3 GetPivot(
		StructureData data)
	{
		return new Vector3(
			(data.Size.x - 1) * 0.5f,
			0,
			(data.Size.y - 1) * 0.5f);
	}

	public void SpawnPillar(
		Structure pillar,
		Transform parent,
		Vector3 pivot)
	{
		GameObject obj =
			Instantiate(
				pillarPrefab,
				parent);

		obj.transform.localPosition =
			new Vector3(
				pillar.Position.x,
				0,
				pillar.Position.y)
			-
			pivot;

		obj.name =
			$"Pillar ({pillar.Position.x},{pillar.Position.y})";
	}

	public void SpawnWallSegment(
		Structure wall,
		Transform parent,
		Vector3 pivot)
	{
		float length;

		Vector3 center =
		(
			new Vector3(
				wall.Position.x,
				0,
				wall.Position.y)
			+
			new Vector3(
				wall.End.x,
				0,
				wall.End.y)
		) * 0.5f;

		if (wall.Orientation == Orientation.Horizontal)
		{
			length =
				Mathf.Abs(
					wall.End.x -
					wall.Position.x) + 1;
		}
		else
		{
			length =
				Mathf.Abs(
					wall.End.y -
					wall.Position.y) + 1;
		}

		GameObject obj =
			Instantiate(
				wallPrefab,
				parent);

		obj.transform.localPosition =
			center - pivot;

		obj.transform.localRotation =
			Quaternion.identity;

		obj.transform.localScale =
			new Vector3(
				length,
				1,
				1);

		if (wall.Orientation == Orientation.Vertical)
		{
			obj.transform.localRotation =
				Quaternion.Euler(
					0,
					90,
					0);
		}
	}

	public void SpawnDoor(
		Structure door,
		Transform parent,
		Vector3 pivot)
	{
		Vector3 center =
		(
			new Vector3(
				door.Position.x,
				0,
				door.Position.y)
			+
			new Vector3(
				door.End.x,
				0,
				door.End.y)
		) * 0.5f;

		Quaternion rotation =
			door.Orientation ==
			Orientation.Vertical
			?
			Quaternion.Euler(
				0,
				90,
				0)
			:
			Quaternion.identity;

		GameObject obj =
			Instantiate(
				doorPrefab,
				parent);

		obj.transform.localPosition =
			center - pivot;

		obj.transform.localRotation =
			rotation;
	}

	public void SpawnFloorSegment(
		Structure floor,
		Transform parent,
		Vector3 pivot)
	{
		Vector3 center =
		(
			new Vector3(
				floor.Position.x,
				0,
				floor.Position.y)
			+
			new Vector3(
				floor.End.x,
				0,
				floor.End.y)
		) * 0.5f;

		float width =
			Mathf.Abs(
				floor.End.x -
				floor.Position.x) + 1;

		float height =
			Mathf.Abs(
				floor.End.y -
				floor.Position.y) + 1;

		GameObject obj =
			Instantiate(
				floorPrefab,
				parent);

		obj.transform.localPosition =
			center - pivot;

		obj.transform.localRotation =
			Quaternion.identity;

		obj.transform.localScale =
			new Vector3(
				width,
				1,
				height);
	}
}