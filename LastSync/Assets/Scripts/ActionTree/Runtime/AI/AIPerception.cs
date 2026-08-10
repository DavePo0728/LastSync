using ActionTree;
using ActionTree.Data;
using UnityEngine;

[DisallowMultipleComponent]
public class AIPerception : MonoBehaviour
{
	#region Parameter

	[Header("Update")]
	[SerializeField]
	private int detectInterval = 30;

	private int frame;

	[Header("Range")]
	[SerializeField]
	private float minAttackRange = 2f;

	[SerializeField]
	private float maxAttackRange = 6f;

	public float MinAttackRange => minAttackRange;
	public float MaxAttackRange => maxAttackRange;

	private Transform player;
	private Transform target;

	public Transform Target => target;

	#endregion

	#region Unity

	private void Update()
	{
		frame++;

		if (frame < detectInterval)
			return;

		frame = 0;

		UpdatePerception();
	}

#if UNITY_EDITOR

	private void OnDrawGizmosSelected()
	{
		DrawCircle(maxAttackRange, Color.green);
		DrawCircle(minAttackRange, Color.red);
	}

#endif

	#endregion

	#region Method

	private void UpdatePerception()
	{
		if (player == null)
		{
			GameObject obj = GameObject.FindGameObjectWithTag("Player");

			if (obj == null)
			{
				target = null;
				return;
			}

			player = obj.transform;
		}

		target = player;
	}

	private bool EnsureTarget()
	{
		if (target != null)
			return true;

		UpdatePerception();
		return target != null;
	}

	public bool HasTarget()
	{
		return EnsureTarget();
	}

	public float DistanceToTarget()
	{
		if (target == null)
			return Mathf.Infinity;

		return Vector3.Distance(transform.position, target.position);
	}

	public Vector3 TargetPosition()
	{
		if (!EnsureTarget())
			return transform.position;

		return target.position;
	}

	public Vector3 DirectionToTarget()
	{
		if (!EnsureTarget())
			return Vector3.zero;

		return (target.position - transform.position).normalized;
	}

	public bool IsTargetInRange(float range)
	{
		return EnsureTarget() &&
			   DistanceToTarget() <= range;
	}

	public bool IsTargetInSkillRange(SkillData skill)
	{
		return skill != null &&
			   IsTargetInRange(skill.Range);
	}

	public bool DamageTargetIfInRange(float range, float damage)
	{
		if (!IsTargetInRange(range))
			return false;

		if (target.TryGetComponent(out AIStatus aiStatus))
		{
			aiStatus.TakeDamage(damage);
			return true;
		}

		return false;
	}

	public bool DamageTargetIfInSkillRange(SkillData skill, float damage)
	{
		if (skill == null)
			return false;

		return DamageTargetIfInRange(skill.Range, damage);
	}

#if UNITY_EDITOR

	private void DrawCircle(float radius, Color color)
	{
		Gizmos.color = color;

		const int segments = 32;

		Vector3 center = transform.position;
		Vector3 previous = center + new Vector3(radius, 0, 0);

		for (int i = 1; i <= segments; i++)
		{
			float angle = i * Mathf.PI * 2f / segments;

			Vector3 current = center + new Vector3(
				Mathf.Cos(angle) * radius,
				0,
				Mathf.Sin(angle) * radius);

			Gizmos.DrawLine(previous, current);
			previous = current;
		}
	}

#endif

	#endregion

	#region Action Node

	#endregion

	#region Condition Node

	public CondNode IsTargetDetected()
	{
		return HasTarget();
	}

	public CondNode IsTargetInAttackRange()
	{
		if (!HasTarget())
			return false;

		float distance = DistanceToTarget();

		return distance >= minAttackRange &&
			   distance <= maxAttackRange;
	}

	public CondNode IsTargetTooClose()
	{
		return HasTarget() &&
			   DistanceToTarget() < minAttackRange;
	}

	public CondNode IsTargetTooFar()
	{
		return !HasTarget() ||
			   DistanceToTarget() > maxAttackRange;
	}

	#endregion
}
