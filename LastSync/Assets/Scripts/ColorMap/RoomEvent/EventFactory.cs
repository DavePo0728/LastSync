using UnityEngine;

public static class EventFactory
{
	public static void CreateBattleChain(
	  RoomEventManager manager,
	  BattleConfig battleConfig)
	{
		CloseDoorEvent closeDoor =
			manager.gameObject.AddComponent<CloseDoorEvent>();

		BattleEvent battle1 =
			manager.gameObject.AddComponent<BattleEvent>();

		BattleEvent battle2 =
			manager.gameObject.AddComponent<BattleEvent>();

		BattleEvent battle3 =
			manager.gameObject.AddComponent<BattleEvent>();

		OpenDoorEvent openDoor =
			manager.gameObject.AddComponent<OpenDoorEvent>();

		SpawnChestEvent spawnChest =
			manager.gameObject.AddComponent<SpawnChestEvent>();

		// 三波目前都使用同一份 BattleConfig
		battle1.SetBattleConfig(battleConfig);
		battle2.SetBattleConfig(battleConfig);
		battle3.SetBattleConfig(battleConfig);

		// 串接事件
		closeDoor.Next = battle1;
		battle1.Next = battle2;
		battle2.Next = battle3;
		battle3.Next = openDoor;
		openDoor.Next = spawnChest;

		manager.AddEvent(EventTrigger.EnterRoom, closeDoor);
	}

	public static void CreateSpawnChain(
		RoomEventManager manager)
	{
		// 目前沒有事件
	}

	public static void CreateExitChain(
		RoomEventManager manager)
	{
		// 目前沒有事件
	}
}