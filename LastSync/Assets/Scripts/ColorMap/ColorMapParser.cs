using System.Collections.Generic;
using UnityEngine;

public class ColorMapParser : MonoBehaviour
{
	public Texture2D Maptexture;

	private HashSet<Vector2Int> wallList = new();
	private HashSet<Vector2Int> doorList = new();
	private HashSet<Vector2Int> floorList = new();

	private HashSet<Structure> structures = new();

	public StructureData Parse(Texture2D texture)
	{
		if (texture == null)
		{
			Debug.LogError("ColorMapParser.Parse failed: texture is null.");
			return null;
		}

		wallList.Clear();
		doorList.Clear();
		floorList.Clear();
		structures.Clear();

		Maptexture = texture;
		LoadMap(texture);

		FindFloors();
		FindPillars();
		FindWalls();
		FindDoors();

		return BuildStructureData();
	}

	public ColorMapData LoadMap(Texture2D texture)
	{
		int width = texture.width;
		int height = texture.height;

		ColorMapData data = new()
		{
			Cells = new CellType[width, height]
		};

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Vector2Int pos = new(x, y);
				Color32 pixel = texture.GetPixel(x, y);

				switch (pixel.r, pixel.g, pixel.b)
				{
					case (255, 0, 0):
						data.Cells[x, y] = CellType.Void;
						break;

					case (0, 0, 0):
						data.Cells[x, y] = CellType.Wall;
						wallList.Add(pos);
						floorList.Add(pos);
						break;

					case (0, 0, 255):
						data.Cells[x, y] = CellType.Door;
						doorList.Add(pos);
						floorList.Add(pos);
						break;

					default:
						data.Cells[x, y] = CellType.Floor;
						floorList.Add(pos);
						break;
				}
			}
		}

		return data;
	}

	private StructureData BuildStructureData()
	{
		StructureData data = new();

		foreach (Structure structure in structures)
		{
			data.Structures.Add(structure);

			if (structure.Type == CellType.Door)
			{
				data.Doors.Add(structure);
			}
		}

		data.StructureID = Maptexture.name;
		data.Size = new Vector2Int(Maptexture.width, Maptexture.height);

		return data;
	}

	private bool IsPillar(Vector2Int pos)
	{
		bool up = wallList.Contains(pos + Vector2Int.up);
		bool down = wallList.Contains(pos + Vector2Int.down);
		bool left = wallList.Contains(pos + Vector2Int.left);
		bool right = wallList.Contains(pos + Vector2Int.right);

		int connections = 0;

		if (up) connections++;
		if (down) connections++;
		if (left) connections++;
		if (right) connections++;

		if (connections == 1)
		{
			return true;
		}

		if (connections >= 3)
		{
			return true;
		}

		if (connections == 2)
		{
			bool straight =
				(up && down) ||
				(left && right);

			return !straight;
		}

		return false;
	}

	private void FindPillars()
	{
		HashSet<Vector2Int> remainingWalls = new();

		foreach (Vector2Int pos in wallList)
		{
			if (IsPillar(pos))
			{
				structures.Add(new Structure(pos)
				{
					Type = CellType.Pillar,
					Orientation = Orientation.None
				});
			}
			else
			{
				remainingWalls.Add(pos);
			}
		}

		wallList = remainingWalls;
	}

	private void FindWalls()
	{
		HashSet<Vector2Int> visited = new();

		foreach (Vector2Int pos in wallList)
		{
			if (visited.Contains(pos))
			{
				continue;
			}

			bool horizontal =
				wallList.Contains(pos + Vector2Int.left) ||
				wallList.Contains(pos + Vector2Int.right);

			Orientation orientation =
				horizontal
					? Orientation.Horizontal
					: Orientation.Vertical;

			Vector2Int start = pos;
			Vector2Int end = pos;

			if (orientation == Orientation.Horizontal)
			{
				while (wallList.Contains(start + Vector2Int.left))
				{
					start += Vector2Int.left;
				}

				while (wallList.Contains(end + Vector2Int.right))
				{
					end += Vector2Int.right;
				}

				for (int x = start.x; x <= end.x; x++)
				{
					visited.Add(new Vector2Int(x, start.y));
				}
			}
			else
			{
				while (wallList.Contains(start + Vector2Int.down))
				{
					start += Vector2Int.down;
				}

				while (wallList.Contains(end + Vector2Int.up))
				{
					end += Vector2Int.up;
				}

				for (int y = start.y; y <= end.y; y++)
				{
					visited.Add(new Vector2Int(start.x, y));
				}
			}

			structures.Add(
				new Structure(start)
				{
					End = end,
					Type = CellType.Wall,
					Orientation = orientation
				});
		}
	}

	private void FindDoors()
	{
		HashSet<Vector2Int> visited = new();

		foreach (Vector2Int pos in doorList)
		{
			if (visited.Contains(pos))
			{
				continue;
			}

			List<Vector2Int> group = new();
			Queue<Vector2Int> queue = new();

			queue.Enqueue(pos);

			while (queue.Count > 0)
			{
				Vector2Int current = queue.Dequeue();

				if (visited.Contains(current))
				{
					continue;
				}

				if (!doorList.Contains(current))
				{
					continue;
				}

				visited.Add(current);
				group.Add(current);

				queue.Enqueue(current + Vector2Int.up);
				queue.Enqueue(current + Vector2Int.down);
				queue.Enqueue(current + Vector2Int.left);
				queue.Enqueue(current + Vector2Int.right);
			}

			CreateDoor(group);
		}
	}

	private void CreateDoor(List<Vector2Int> group)
	{
		Vector2Int start = group[0];
		Vector2Int end = group[0];

		bool horizontal =
			group.Exists(pos => pos.y == start.y && pos.x != start.x);

		Orientation orientation =
			horizontal
				? Orientation.Horizontal
				: Orientation.Vertical;

		foreach (Vector2Int pos in group)
		{
			if (orientation == Orientation.Horizontal)
			{
				if (pos.x < start.x)
				{
					start = pos;
				}

				if (pos.x > end.x)
				{
					end = pos;
				}
			}
			else
			{
				if (pos.y < start.y)
				{
					start = pos;
				}

				if (pos.y > end.y)
				{
					end = pos;
				}
			}
		}

		structures.Add(
			new Structure(start)
			{
				End = end,
				Type = CellType.Door,
				Orientation = orientation,
				Direction = GetDoorDirection(start, end)
			});
	}

	private void FindFloors()
	{
		List<Structure> floorSegments = new();

		for (int y = 0; y < Maptexture.height; y++)
		{
			int x = 0;

			while (x < Maptexture.width)
			{
				Vector2Int start = new(x, y);

				if (!floorList.Contains(start))
				{
					x++;
					continue;
				}

				Vector2Int end = start;

				while (floorList.Contains(end + Vector2Int.right))
				{
					end += Vector2Int.right;
				}

				floorSegments.Add(
					new Structure(start)
					{
						End = end,
						Type = CellType.Floor,
						Orientation = Orientation.Horizontal
					});

				x = end.x + 1;
			}
		}

		MergeFloorSegments(floorSegments);
		structures.UnionWith(floorSegments);
	}

	private void MergeFloorSegments(List<Structure> floorSegments)
	{
		for (int i = 0; i < floorSegments.Count; i++)
		{
			Structure current = floorSegments[i];

			for (int j = i + 1; j < floorSegments.Count; j++)
			{
				Structure next = floorSegments[j];

				bool sameWidth =
					current.Position.x == next.Position.x &&
					current.End.x == next.End.x;

				bool adjacent =
					next.Position.y == current.End.y + 1;

				if (sameWidth && adjacent)
				{
					current.End = new Vector2(
						current.End.x,
						next.End.y);

					floorSegments.RemoveAt(j);
					j--;
				}
			}
		}
	}

	private Direction GetDoorDirection(
		Vector2Int start,
		Vector2Int end)
	{
		if (start.x == 0)
		{
			return Direction.Left;
		}

		if (end.x == Maptexture.width - 1)
		{
			return Direction.Right;
		}

		if (end.y == Maptexture.height - 1)
		{
			return Direction.Up;
		}

		if (start.y == 0)
		{
			return Direction.Down;
		}

		return Direction.None;
	}
}
