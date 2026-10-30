using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEvent : RoomEvent
{
	private enum SpawnMode
	{
		Unconfigured,
		BattleConfig,
		SpawnPoints
	}

	[SerializeField]
	private BattleConfig battleConfig;

	private List<SpawnPointData> spawnPoints;
	private BattleSpawner battleSpawner;
	private bool isRunning;
	private SpawnMode spawnMode;

	protected override void Awake()
	{
		base.Awake();
		battleSpawner = GetComponent<BattleSpawner>();
	}

	public void SetBattleConfig(BattleConfig config)
	{
		battleConfig = config;
		spawnPoints = null;
		spawnMode = SpawnMode.BattleConfig;
	}

	public void SetSpawnPoints(IReadOnlyList<SpawnPointData> points)
	{
		battleConfig = null;
		spawnPoints = points == null
			? new List<SpawnPointData>()
			: new List<SpawnPointData>(points);
		spawnMode = SpawnMode.SpawnPoints;
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

		switch (spawnMode)
		{
			case SpawnMode.BattleConfig:
				if (battleConfig == null)
				{
					Debug.LogWarning("BattleEvent failed: BattleConfig is missing.");
					Finish();
					return;
				}

				battleSpawner.Spawn(battleConfig.Enemies);
				break;

			case SpawnMode.SpawnPoints:
				battleSpawner.SpawnAtPoints(spawnPoints);
				break;

			default:
				Debug.LogWarning("BattleEvent failed: no spawn mode was configured.");
				Finish();
				return;
		}

		isRunning = true;
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

	private void OnDisable()
	{
		StopAllCoroutines();
		isRunning = false;
	}
}
