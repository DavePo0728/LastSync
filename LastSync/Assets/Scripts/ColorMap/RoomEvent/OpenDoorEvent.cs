using UnityEngine;

public class OpenDoorEvent : RoomEvent
{
	public override void Execute()
	{
		if (Manager == null || Manager.DoorsRoot == null)
		{
			Debug.LogWarning("OpenDoorEvent¡GDoorsRoot ¤£¦s¦b¡C");
			Finish();
			return;
		}

		DoorController[] doors =
			Manager.DoorsRoot.GetComponentsInChildren<DoorController>(true);

		foreach (DoorController door in doors)
		{
			door.Open();
		}

		Finish();
	}
}