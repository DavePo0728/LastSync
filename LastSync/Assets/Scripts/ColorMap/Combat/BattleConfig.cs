using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Battle/Battle Config")]
public class BattleConfig : ScriptableObject
{
	public List<EnemySpawnInfo> Enemies = new();
}
