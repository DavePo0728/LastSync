using System;
using UnityEngine;

[Serializable]
public class SpawnPointData
{
	public Vector3 Position;

	public GameObject Prefab;

	[Min(1)]
	public int WaveIndex = 1;
}
