using ActionTree.Data;

public sealed class SkillRuntime
{
	#region Data

	public SkillData Data { get; }

	#endregion

	#region State

	public SkillState State { get; set; }

	public bool Interrupted { get; set; }

	private float attackTurnSpeedMultiplier = 1f;
	public float AttackTurnSpeedMultiplier =>
		State == SkillState.Attack && !Interrupted ? attackTurnSpeedMultiplier : 1f;

	public void SetAttackTurnSpeedMultiplier(float multiplier)
	{
		if (State != SkillState.Attack || Interrupted ||
			float.IsNaN(multiplier) || float.IsInfinity(multiplier))
			return;

		attackTurnSpeedMultiplier = UnityEngine.Mathf.Max(0f, multiplier);
	}
    public bool WaitForHeavyDash { get; set; }

    #endregion

    #region Timer

    public float Timer { get; set; }

	public float Cooldown { get; set; }

	public int ActionIndex { get; set; }

	public float WaitTimer { get; set; }

	public int PreparedAttackActionCount { get; set; }

	#endregion

	public SkillRuntime(SkillData data)
	{
		Data = data;

		Reset();
	}

	public void Begin()
	{
		attackTurnSpeedMultiplier = 1f;
		State = SkillState.Before;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
		Interrupted = false;
        WaitForHeavyDash = false;
    }

	public void Finish()
	{
		attackTurnSpeedMultiplier = 1f;
		State = SkillState.Finished;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
        WaitForHeavyDash = false;
        Cooldown = Data.Cooldown;
	}

	public void Interrupt()
	{
		attackTurnSpeedMultiplier = 1f;
		Interrupted = true;
		State = SkillState.Finished;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
        WaitForHeavyDash = false;
    }

	public void Reset()
	{
		attackTurnSpeedMultiplier = 1f;
		State = SkillState.Idle;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
		Interrupted = false;
        WaitForHeavyDash = false;
    }

	public void BeginPhase(SkillState state)
	{
		attackTurnSpeedMultiplier = 1f;
		State = state;
		ActionIndex = 0;
		WaitTimer = 0f;
        WaitForHeavyDash = false;
    }
}
