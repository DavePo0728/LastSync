using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class AIStatus : MonoBehaviour
{
	#region Parameter

	[Header("Health")]
	[SerializeField]
	private float maxHealth = 100f;

	[SerializeField]
	private float currentHealth;

	[Header("Combat")]
	[SerializeField]
	private float attack = 100f;

	[SerializeField]
	private float defense = 0f;

	[Header("Movement")]
	[SerializeField]
	private float moveSpeed = 3.5f;

	[SerializeField]
	private float deathDestroyDelay = 1.5f;
	[SerializeField]
	private List<GameObject> chipDropPrefab;
	[SerializeField]
	private Image healthBar;

	public float MaxHealth => maxHealth;
	public float CurrentHealth => currentHealth;
	public float Attack => attack;
	public float Defense => defense;
	public float MoveSpeed => moveSpeed;

	public event Action<float> Damaged;
	public event Action Died;

	#endregion

	#region Unity

	private void Awake()
	{
		currentHealth = maxHealth;
	}

	#endregion

	#region Method

	public void SetHealth(float value)
	{
		currentHealth = Mathf.Clamp(value, 0f, maxHealth);
	}

	public void Heal(float value)
	{
		currentHealth = Mathf.Min(currentHealth + value, maxHealth);
	}

	public void TakeDamage(float damage)
	{
		float finalDamage = Mathf.Max(1f, damage - defense);

		currentHealth -= finalDamage;
		UpdateUI();
		Damaged?.Invoke(finalDamage);

		if (currentHealth <= 0f)
		{
			currentHealth = 0f;
			Die();
		}
	}
    public virtual void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)CurrentHealth / maxHealth;
        }
    }

    public void Die()
	{
		Died?.Invoke();
		if (chipDropPrefab != null && chipDropPrefab.Count > 0)
		{
			int randomIndex = UnityEngine.Random.Range(0, chipDropPrefab.Count);
			Instantiate(chipDropPrefab[randomIndex], transform.position, transform.rotation);
		}
		Destroy(gameObject, deathDestroyDelay);
	}

	public void SetAttack(float value)
	{
		attack = value;
	}

	public void AddAttack(float value)
	{
		attack += value;
	}

	public void SetDefense(float value)
	{
		defense = value;
	}

	public void AddDefense(float value)
	{
		defense += value;
	}

	public void SetMoveSpeed(float value)
	{
		moveSpeed = value;
	}

	public void AddMoveSpeed(float value)
	{
		moveSpeed += value;
	}

	public bool IsDead()
	{
		return currentHealth <= 0f;
	}

	public bool IsAlive()
	{
		return currentHealth > 0f;
	}

	public bool IsHealthBelowPercent(float percent)
	{
		if (maxHealth <= 0f)
			return false;

		return currentHealth / maxHealth <= percent;
	}

	public bool IsHealthAbovePercent(float percent)
	{
		if (maxHealth <= 0f)
			return false;

		return currentHealth / maxHealth >= percent;
	}

	public bool CanMove()
	{
		return moveSpeed > 0f;
	}

	#endregion

	#region Action Node

	public void Action_Heal(float value)
	{
		Heal(value);
	}

	public void Action_TakeDamage(float damage)
	{
		TakeDamage(damage);
	}

	public void Action_SetHealth(float value)
	{
		SetHealth(value);
	}

	public void Action_SetAttack(float value)
	{
		SetAttack(value);
	}

	public void Action_AddAttack(float value)
	{
		AddAttack(value);
	}

	public void Action_SetDefense(float value)
	{
		SetDefense(value);
	}

	public void Action_AddDefense(float value)
	{
		AddDefense(value);
	}

	public void Action_SetMoveSpeed(float value)
	{
		SetMoveSpeed(value);
	}

	public void Action_AddMoveSpeed(float value)
	{
		AddMoveSpeed(value);
	}

	#endregion

	#region Condition Node

	public bool Condition_IsDead()
	{
		return IsDead();
	}

	public bool Condition_IsAlive()
	{
		return IsAlive();
	}

	public bool Condition_IsHealthBelowPercent(float percent)
	{
		return IsHealthBelowPercent(percent);
	}

	public bool Condition_IsHealthAbovePercent(float percent)
	{
		return IsHealthAbovePercent(percent);
	}

	public bool Condition_CanMove()
	{
		return CanMove();
	}

	#endregion
}
