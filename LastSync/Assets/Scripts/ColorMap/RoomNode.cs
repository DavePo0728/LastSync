using System.Collections.Generic;

public class RoomNode
{
	public RoomInstance Room;

	public List<RoomNode> Neighbors = new();

	public int Depth = -1;
}
