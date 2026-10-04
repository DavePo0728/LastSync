using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class StructureSpawner : MonoBehaviour
{
	private const int NotWalkableArea = 1;

	[SerializeField]
	private GameObject pillarPrefab;

	[SerializeField]
	private GameObject wallPrefab;

	[SerializeField]
	private GameObject doorPrefab;

	[SerializeField]
	private GameObject floorPrefab;

	[SerializeField]
	private GameObject trapFloorPrefab;

	[SerializeField]
	private Material floorMaterial;

	[SerializeField]
	private Material floorInsideMaterial;

	[SerializeField]
	private Material floorOuterSideMaterial;
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

		SpawnFloorMesh(
			room,
			floorRoot,
			pivot);

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

				case CellType.TrapFloor:
					SpawnTrapFloorSegment(
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
		SetNotWalkable(obj);

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
		SetNotWalkable(obj);

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

	private void SpawnFloorMesh(
		RoomInstance room,
		Transform parent,
		Vector3 pivot)
	{
		List<Vector3> vertices = new();
		List<int> triangles = new();
		List<Vector2> uvs = new();

		foreach (Structure structure in room.Data.Structures)
		{
			if (structure.Type != CellType.Floor)
			{
				continue;
			}

			AddFloorSurface(
				structure,
				pivot,
				vertices,
				triangles,
				uvs);
		}

		if (vertices.Count == 0)
		{
			return;
		}

		Mesh mesh = new()
		{
			name = $"{room.Data.StructureID}_FloorMesh"
		};

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		GameObject floorObject = parent.gameObject;
		MeshFilter filter = floorObject.AddComponent<MeshFilter>();
		filter.sharedMesh = mesh;

		MeshRenderer renderer = floorObject.AddComponent<MeshRenderer>();
		MeshRenderer prefabRenderer =
			floorPrefab.GetComponentInChildren<MeshRenderer>();
		Material resolvedFloorMaterial = floorMaterial;

		if (resolvedFloorMaterial == null && prefabRenderer != null)
		{
			resolvedFloorMaterial = prefabRenderer.sharedMaterial;
		}

		renderer.sharedMaterial = resolvedFloorMaterial;

		MeshCollider floorCollider = floorObject.AddComponent<MeshCollider>();
		floorCollider.sharedMesh = mesh;

		bool[,] floorCells = BuildFloorCellMap(room.Data);
		SpawnRoomFloorTrigger(
			room.Data,
			parent,
			pivot,
			floorCells);

		SpawnFloorSideMeshes(
			room.Data,
			parent.parent,
			pivot,
			floorCells,
			floorOuterSideMaterial != null
				? floorOuterSideMaterial
				: resolvedFloorMaterial,
			floorInsideMaterial != null
				? floorInsideMaterial
				: resolvedFloorMaterial);
	}

	private bool[,] BuildFloorCellMap(StructureData data)
	{
		bool[,] cells = new bool[data.Size.x, data.Size.y];

		foreach (Structure structure in data.Structures)
		{
			if (structure.Type != CellType.Floor)
			{
				continue;
			}

			int minX = Mathf.RoundToInt(
				Mathf.Min(structure.Position.x, structure.End.x));
			int maxX = Mathf.RoundToInt(
				Mathf.Max(structure.Position.x, structure.End.x));
			int minY = Mathf.RoundToInt(
				Mathf.Min(structure.Position.y, structure.End.y));
			int maxY = Mathf.RoundToInt(
				Mathf.Max(structure.Position.y, structure.End.y));

			for (int y = minY; y <= maxY; y++)
			{
				for (int x = minX; x <= maxX; x++)
				{
					cells[x, y] = true;
				}
			}
		}

		return cells;
	}

	private void SpawnRoomFloorTrigger(
		StructureData data,
		Transform parent,
		Vector3 pivot,
		bool[,] floorCells)
	{
		if (!TryGetFloorBounds(
				floorCells,
				out int minX,
				out int minY,
				out int maxX,
				out int maxY))
		{
			Debug.LogWarning(
				$"Room trigger was not created for {data.StructureID}: " +
				"the room has no floor cells.");
			return;
		}

		float width = maxX - minX - 1;
		float height = maxY - minY - 1;

		if (width <= 0f || height <= 0f)
		{
			Debug.LogWarning(
				$"Room trigger was not created for {data.StructureID}: " +
				"the room is smaller than the one-cell inset.");
			return;
		}

		GameObject triggerObject = new GameObject("RoomFloorTrigger");
		triggerObject.transform.SetParent(parent, false);

		BoxCollider collider = triggerObject.AddComponent<BoxCollider>();
		collider.isTrigger = true;
		collider.center = new Vector3(
			(minX + maxX) * 0.5f - pivot.x,
			0f,
			(minY + maxY) * 0.5f - pivot.z);
		collider.size = new Vector3(width, 1f, height);
		triggerObject.AddComponent<RoomFloorTrigger>();
	}

	private bool TryGetFloorBounds(
		bool[,] floorCells,
		out int minX,
		out int minY,
		out int maxX,
		out int maxY)
	{
		minX = int.MaxValue;
		minY = int.MaxValue;
		maxX = int.MinValue;
		maxY = int.MinValue;

		for (int y = 0; y < floorCells.GetLength(1); y++)
		{
			for (int x = 0; x < floorCells.GetLength(0); x++)
			{
				if (!floorCells[x, y])
				{
					continue;
				}

				minX = Mathf.Min(minX, x);
				minY = Mathf.Min(minY, y);
				maxX = Mathf.Max(maxX, x);
				maxY = Mathf.Max(maxY, y);
			}
		}

		return minX != int.MaxValue;
	}

	private void SpawnFloorSideMeshes(
		StructureData data,
		Transform parent,
		Vector3 pivot,
		bool[,] floorCells,
		Material outerSideMaterial,
		Material insideMaterial)
	{
		List<Vector3> outerVertices = new();
		List<int> outerTriangles = new();
		List<Vector2> outerUvs = new();
		List<Vector3> holeVertices = new();
		List<int> holeTriangles = new();
		List<Vector2> holeUvs = new();

		for (int y = 0; y < data.Size.y; y++)
		{
			for (int x = 0; x < data.Size.x; x++)
			{
				if (!floorCells[x, y])
				{
					continue;
				}

				AddFloorBoundary(
					x,
					y,
					-1,
					0,
					data,
					pivot,
					floorCells,
					outerVertices,
					outerTriangles,
					outerUvs,
					holeVertices,
					holeTriangles,
					holeUvs);

				AddFloorBoundary(
					x,
					y,
					1,
					0,
					data,
					pivot,
					floorCells,
					outerVertices,
					outerTriangles,
					outerUvs,
					holeVertices,
					holeTriangles,
					holeUvs);

				AddFloorBoundary(
					x,
					y,
					0,
					-1,
					data,
					pivot,
					floorCells,
					outerVertices,
					outerTriangles,
					outerUvs,
					holeVertices,
					holeTriangles,
					holeUvs);

				AddFloorBoundary(
					x,
					y,
					0,
					1,
					data,
					pivot,
					floorCells,
					outerVertices,
					outerTriangles,
					outerUvs,
					holeVertices,
					holeTriangles,
					holeUvs);
			}
		}

		CreateFloorSideObject(
			"FloorSide",
			parent,
			$"{data.StructureID}_FloorSideMesh",
			outerSideMaterial,
			outerVertices,
			outerTriangles,
			outerUvs);

		CreateFloorSideObject(
			"FloorHoleSide",
			parent,
			$"{data.StructureID}_FloorHoleSideMesh",
			insideMaterial,
			holeVertices,
			holeTriangles,
			holeUvs);
	}

	private void AddFloorBoundary(
		int x,
		int y,
		int offsetX,
		int offsetY,
		StructureData data,
		Vector3 pivot,
		bool[,] floorCells,
		List<Vector3> outerVertices,
		List<int> outerTriangles,
		List<Vector2> outerUvs,
		List<Vector3> holeVertices,
		List<int> holeTriangles,
		List<Vector2> holeUvs)
	{
		int neighbourX = x + offsetX;
		int neighbourY = y + offsetY;
		bool outside =
			neighbourX < 0 ||
			neighbourX >= data.Size.x ||
			neighbourY < 0 ||
			neighbourY >= data.Size.y;

		if (!outside && floorCells[neighbourX, neighbourY])
		{
			return;
		}

		if (outside)
		{
			AddFloorSideQuad(
				x,
				y,
				offsetX,
				offsetY,
				pivot,
				outerVertices,
				outerTriangles,
				outerUvs);
		}
		else
		{
			AddFloorSideQuad(
				x,
				y,
				offsetX,
				offsetY,
				pivot,
				holeVertices,
				holeTriangles,
				holeUvs);
		}
	}

	private void AddFloorSideQuad(
		int x,
		int y,
		int offsetX,
		int offsetY,
		Vector3 pivot,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs)
	{
		float minX = x - 0.5f - pivot.x;
		float maxX = x + 0.5f - pivot.x;
		float minZ = y - 0.5f - pivot.z;
		float maxZ = y + 0.5f - pivot.z;
		const float bottomY = -1f;

		Vector3 topA;
		Vector3 bottomA;
		Vector3 bottomB;
		Vector3 topB;

		if (offsetX < 0)
		{
			topA = new Vector3(minX, 0f, minZ);
			bottomA = new Vector3(minX, bottomY, minZ);
			bottomB = new Vector3(minX, bottomY, maxZ);
			topB = new Vector3(minX, 0f, maxZ);
		}
		else if (offsetX > 0)
		{
			topA = new Vector3(maxX, 0f, maxZ);
			bottomA = new Vector3(maxX, bottomY, maxZ);
			bottomB = new Vector3(maxX, bottomY, minZ);
			topB = new Vector3(maxX, 0f, minZ);
		}
		else if (offsetY < 0)
		{
			topA = new Vector3(minX, 0f, minZ);
			bottomA = new Vector3(maxX, 0f, minZ);
			bottomB = new Vector3(maxX, bottomY, minZ);
			topB = new Vector3(minX, bottomY, minZ);
		}
		else
		{
			topA = new Vector3(maxX, 0f, maxZ);
			bottomA = new Vector3(minX, 0f, maxZ);
			bottomB = new Vector3(minX, bottomY, maxZ);
			topB = new Vector3(maxX, bottomY, maxZ);
		}

		int start = vertices.Count;
		vertices.Add(topA);
		vertices.Add(bottomA);
		vertices.Add(bottomB);
		vertices.Add(topB);
		triangles.Add(start);
		triangles.Add(start + 1);
		triangles.Add(start + 2);
		triangles.Add(start);
		triangles.Add(start + 2);
		triangles.Add(start + 3);
		uvs.Add(new Vector2(0f, 1f));
		uvs.Add(Vector2.zero);
		uvs.Add(new Vector2(1f, 0f));
		uvs.Add(Vector2.one);
	}

	private void CreateFloorSideObject(
		string objectName,
		Transform parent,
		string meshName,
		Material material,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs)
	{
		if (vertices.Count == 0)
		{
			return;
		}

		Mesh mesh = new()
		{
			name = meshName
		};

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		GameObject sideObject = new GameObject(objectName);
		sideObject.transform.SetParent(parent, false);

		MeshFilter filter = sideObject.AddComponent<MeshFilter>();
		filter.sharedMesh = mesh;

		MeshRenderer renderer = sideObject.AddComponent<MeshRenderer>();
		renderer.sharedMaterial = material;

		MeshCollider collider = sideObject.AddComponent<MeshCollider>();
		collider.sharedMesh = mesh;
	}

	private void AddFloorSurface(
		Structure floor,
		Vector3 pivot,
		List<Vector3> vertices,
		List<int> triangles,
		List<Vector2> uvs)
	{
		float minX = floor.Position.x - 0.5f - pivot.x;
		float maxX = floor.End.x + 0.5f - pivot.x;
		float minZ = floor.Position.y - 0.5f - pivot.z;
		float maxZ = floor.End.y + 0.5f - pivot.z;
		int start = vertices.Count;
		vertices.Add(new Vector3(minX, 0f, minZ));
		vertices.Add(new Vector3(maxX, 0f, minZ));
		vertices.Add(new Vector3(maxX, 0f, maxZ));
		vertices.Add(new Vector3(minX, 0f, maxZ));

		// Up-facing winding. Neighbouring floor segments no longer add side faces.
		triangles.Add(start);
		triangles.Add(start + 2);
		triangles.Add(start + 1);
		triangles.Add(start);
		triangles.Add(start + 3);
		triangles.Add(start + 2);

		for (int i = 0; i < 4; i++)
		{
			Vector3 vertex = vertices[start + i];
			uvs.Add(new Vector2(vertex.x, vertex.z));
		}
	}
    public void SpawnTrapFloorSegment(
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
                trapFloorPrefab,
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

    private void SetNotWalkable(GameObject obj)
	{
		NavMeshModifier modifier =
			obj.GetComponent<NavMeshModifier>();

		if (modifier == null)
		{
			modifier = obj.AddComponent<NavMeshModifier>();
		}

		modifier.overrideArea = true;
		modifier.area = NotWalkableArea;
	}
}
