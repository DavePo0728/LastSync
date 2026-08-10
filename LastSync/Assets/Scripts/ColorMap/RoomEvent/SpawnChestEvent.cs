using UnityEngine;

public class SpawnChestEvent : RoomEvent
{
	public override void Execute()
	{
		Debug.Log("Spawn Chest");

		Finish();
	}
}