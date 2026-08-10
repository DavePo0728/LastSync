using UnityEngine;

[System.Serializable]
public class Structure
{
	public Structure(Vector2 position)
	{
		Position = position;
		End = position;
	}

	public Vector2 Position;
	public Vector2 End;

	public CellType Type;
	public Direction Direction;
	public Orientation Orientation;
}
