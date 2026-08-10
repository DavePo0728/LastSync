using UnityEngine;

public class CloseDoorEvent : RoomEvent
{
	public override void Execute()
	{
		if (Manager == null || Manager.DoorsRoot == null)
		{
			Debug.LogWarning("CloseDoorEvent¡GDoorsRoot ¤£¦s¦b¡C");
			Finish();
			return;
		}

		DoorController[] doors =
			Manager.DoorsRoot.GetComponentsInChildren<DoorController>(true);

		foreach (DoorController door in doors)
		{
			door.Close();
		}

		Finish();
	}
}