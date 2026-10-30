using System.Linq;
using UnityEngine;

public static class PlayerSpawnPositionUtility
{
	public static Vector3 GetSpawnPosition(RoomInstance room)
	{
        if (room?.Data == null || room.Transform == null)
        {
            return Vector3.zero;
        }

        if (room.Data.HasPlayerSpawnPoint)
        {
            Vector2 structureCenter = new Vector2(
                room.Data.Size.x - 1,
                room.Data.Size.y - 1) * 0.5f;

            Vector2 localPosition =
                room.Data.PlayerSpawnPoint - structureCenter;

            return room.Transform.TransformPoint(
                new Vector3(
                    localPosition.x,
                    0,
                    localPosition.y));
        }

        return GetFallbackSpawnPosition(room);
    }

	private static float GetStructureArea(Structure structure)
	{
		float width =
			Mathf.Abs(
				structure.End.x -
				structure.Position.x) + 1;

		float height =
			Mathf.Abs(
				structure.End.y -
				structure.Position.y) + 1;

		return width * height;
	}
	private static Vector3 GetFallbackSpawnPosition(RoomInstance room)
    {
        if (room?.Data == null || room.Transform == null)
        {
            return Vector3.zero;
        }

        Structure floorStructure = room.Data.Structures
            .Where(structure => structure.Type == CellType.Floor)
            .OrderByDescending(GetStructureArea)
            .FirstOrDefault();

        if (floorStructure == null)
        {
            return room.Transform.position;
        }

        Vector2 floorCenter =
            (floorStructure.Position + floorStructure.End) * 0.5f;

        Vector2 structureCenter = new Vector2(
            room.Data.Size.x - 1,
            room.Data.Size.y - 1) * 0.5f;

        Vector2 localPosition = floorCenter - structureCenter;

        return room.Transform.TransformPoint(
            new Vector3(
                localPosition.x,
                0,
                localPosition.y));
    }
}
