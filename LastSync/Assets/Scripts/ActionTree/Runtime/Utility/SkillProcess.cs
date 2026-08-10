using UnityEngine;

public static class SkillProcess
{
	[SkillAction("啟動移動限制")]
	public static void LockMove(AIStatus status)
	{
		if (status == null)
			return;

		if (status.TryGetComponent(out AIMovement movement))
			movement.LockMove();
	}

	[SkillAction("解除移動限制")]
	public static void UnlockMove(AIStatus status)
	{
		if (status == null)
			return;

		if (status.TryGetComponent(out AIMovement movement))
			movement.UnlockMove();
	}

	[SkillAction("啟動旋轉限制")]
	public static void LockRotation(AIStatus status)
	{
		if (status == null)
			return;

		if (status.TryGetComponent(out AIMovement movement))
			movement.LockRotation();
	}

	[SkillAction("解除旋轉限制")]
	public static void UnlockRotation(AIStatus status)
	{
		if (status == null)
			return;

		if (status.TryGetComponent(out AIMovement movement))
			movement.UnlockRotation();
	}

	[SkillAction("面向目標")]
	public static void FaceTarget(AIStatus status, float time, bool waitUntilFinished)
	{
		if (status == null)
			return;

		if (!status.TryGetComponent(out AIMovement movement))
			return;

		if (status.TryGetComponent(out AIPerception perception))
			movement.StartFaceTarget(perception);
	}

	[SkillAction("硬直")]
	public static void Recover(AIStatus status, float time)
	{
		if (status == null)
			return;

		if (status.TryGetComponent(out AIMovement movement))
			movement.LockControl();
	}

	[SkillAction("位移")]
	public static void Move(AIStatus status, float distance, float duration)
	{
		if (status == null)
			return;

		if (status.TryGetComponent(out AIMovement movement))
			movement.DashForward(distance, duration);
	}

	[SkillAction("Dash")]
	public static void Dash(AIStatus status, float distance, float duration)
	{
		Move(status, distance, duration);
	}

	[SkillAction("AttackRange")]
	public static void AttackRange(AIStatus status)
	{
		if (status == null)
			return;

		if (!status.TryGetComponent(out AISkill skill) || skill.CurrentSkill == null)
			return;

		AttackRangeRect range = status.GetComponentInChildren<AttackRangeRect>(true);

		if (range == null)
			return;

		range.Play(skill.CurrentSkill.Data.Range, GetPrepareDuration(skill));
	}

	[SkillAction("FireWeapon")]
	public static void FireWeapon(AIStatus status)
	{
		if (status == null)
			return;

		WeaponEmitter emitter = status.GetComponentInChildren<WeaponEmitter>(true);

		if (emitter != null)
			emitter.Fire();
	}

	[SkillAction("造成傷害")]
	public static void Damage(AIStatus status, float damage)
	{
		if (status == null)
			return;

		if (!status.TryGetComponent(out AIPerception perception))
			return;

		float range = perception.MaxAttackRange;
		float finalDamage = damage;

		if (status.TryGetComponent(out AISkill skill) && skill.CurrentSkill != null)
		{
			range = skill.CurrentSkill.Data.Range;

			if (finalDamage <= 0f)
				finalDamage = skill.CurrentSkill.Data.Damage;
		}

		perception.DamageTargetIfInRange(range, finalDamage);
	}

	private static float GetPrepareDuration(AISkill skill)
	{
		float duration = skill.CurrentSkill.Data.CastTime;

		foreach (SkillActionData action in skill.CurrentSkill.Data.AttackActions)
		{
			if (action == null || action.MethodName != nameof(FaceTarget))
				continue;

			bool waitUntilFinished = true;

			if (TryFindBool(action, "waitUntilFinished", out bool configuredWait))
				waitUntilFinished = configuredWait;

			if (waitUntilFinished && TryFindFloat(action, "time", out float faceTime) && faceTime > 0f)
				return faceTime;
		}

		return duration;
	}

	private static bool TryFindFloat(SkillActionData action, string parameterName, out float value)
	{
		value = 0f;

		if (action.Parameters == null)
			return false;

		foreach (ActionTree.MethodParameter parameter in action.Parameters)
		{
			if (parameter.parameterName != parameterName)
				continue;

			value = parameter.floatValue;
			return true;
		}

		return false;
	}

	private static bool TryFindBool(SkillActionData action, string parameterName, out bool value)
	{
		value = false;

		if (action.Parameters == null)
			return false;

		foreach (ActionTree.MethodParameter parameter in action.Parameters)
		{
			if (parameter.parameterName != parameterName)
				continue;

			value = parameter.boolValue;
			return true;
		}

		return false;
	}
}
