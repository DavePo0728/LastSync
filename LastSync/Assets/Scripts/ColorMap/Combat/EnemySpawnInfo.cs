using System;
using UnityEngine;

[Serializable]
public class EnemySpawnInfo
{
	[Header("Enemy")]
	public GameObject Prefab;

	[Header("Count")]
	public int Count = 1;
}