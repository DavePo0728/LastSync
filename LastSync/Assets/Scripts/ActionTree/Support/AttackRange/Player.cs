using UnityEngine;

public class Player : MonoBehaviour
{
	[Header("Status")]
	[SerializeField] private int maxHP = 100;

	[SerializeField] private int currentHP;

	private void Awake()
	{
		currentHP = maxHP;
	}

	public void TakeDamage(int damage)
	{
		currentHP -= damage;

		if (currentHP < 0)
			currentHP = 0;

		Debug.Log($"玩家受到 {damage} 點傷害，目前 HP：{currentHP}");

		if (currentHP == 0)
		{
			Die();
		}
	}

	private void Die()
	{
		Debug.Log("玩家死亡");
	}

	public int GetHP()
	{
		return currentHP;
	}

	public int GetMaxHP()
	{
		return maxHP;
	}
}