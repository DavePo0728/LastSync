using System.Collections;
using UnityEngine;

public class BattleEvent : RoomEvent
{
	[SerializeField]
	private BattleConfig battleConfig;

	private BattleSpawner battleSpawner;
	private bool isRunning;

	protected override void Awake()
	{
		base.Awake();
		battleSpawner = GetComponent<BattleSpawner>();
	}

	public void SetBattleConfig(BattleConfig config)
	{
		battleConfig = config;
	}

	public override void Execute()
	{
		if (isRunning)
		{
			return;
		}

		if (battleSpawner == null)
		{
			Debug.LogWarning("BattleEvent failed: BattleSpawner is missing.");
			Finish();
			return;
		}

		if (battleConfig == null)
		{
			Debug.LogWarning("BattleEvent failed: BattleConfig is missing.");
			Finish();
			return;
		}

		isRunning = true;
		battleSpawner.Spawn(battleConfig.Enemies);

		StartCoroutine(CheckBattleRoutine());
	}
	private IEnumerator CheckBattleRoutine()
	{
		while (battleSpawner.AliveEnemyCount > 0)
		{
			yield return new WaitForSeconds(0.2f);
		}

		isRunning = false;
		Finish();
	}
}
