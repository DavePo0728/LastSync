using System.Collections;
using UnityEngine;

public class BattleEvent : RoomEvent
{
	[SerializeField]
	private BattleConfig battleConfig;

	private BattleSpawner battleSpawner;

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
		Debug.Log(battleSpawner);
		Debug.Log(battleConfig);

		battleSpawner.Spawn(battleConfig.Enemies);

		StartCoroutine(CheckBattleRoutine());
	}
	private IEnumerator CheckBattleRoutine()
	{
		while (battleSpawner.AliveEnemyCount > 0)
		{
			yield return new WaitForSeconds(0.2f);
		}

		Finish();
	}
}