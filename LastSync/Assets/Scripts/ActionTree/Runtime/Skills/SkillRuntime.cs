using ActionTree.Data;

public sealed class SkillRuntime
{
	#region Data

	public SkillData Data { get; }

	#endregion

	#region State

	public SkillState State { get; set; }

	public bool Interrupted { get; set; }

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
		State = SkillState.Before;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
		Interrupted = false;
	}

	public void Finish()
	{
		State = SkillState.Finished;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
		Cooldown = Data.Cooldown;
	}

	public void Interrupt()
	{
		Interrupted = true;
		State = SkillState.Finished;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
	}

	public void Reset()
	{
		State = SkillState.Idle;
		Timer = 0f;
		ActionIndex = 0;
		WaitTimer = 0f;
		PreparedAttackActionCount = 0;
		Interrupted = false;
	}

	public void BeginPhase(SkillState state)
	{
		State = state;
		ActionIndex = 0;
		WaitTimer = 0f;
	}
}
