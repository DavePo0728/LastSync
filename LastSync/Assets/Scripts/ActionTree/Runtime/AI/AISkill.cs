using System.Collections.Generic;
using System;
using ActionTree;
using ActionTree.Data;
using ActionTree.Runtime;
using UnityEngine;

[DisallowMultipleComponent]
public class AISkill : MonoBehaviour
{
	[SerializeField]
	private List<SkillData> skills = new();

	private readonly Dictionary<SkillData, SkillRuntime> runtimes = new();

	private SkillRuntime currentSkill;

	private readonly SkillExecutor executor = new();

	public IReadOnlyList<SkillData> Skills => skills;

	public SkillRuntime CurrentSkill => currentSkill;

	public int Count => skills.Count;

	public event Action<SkillData, int> SkillAttackStarted;

	private void Awake()
	{
		runtimes.Clear();

		foreach (SkillData skill in skills)
		{
			if (skill == null)
				continue;

			runtimes.Add(skill, new SkillRuntime(skill));
		}
	}

	private void Update()
	{
		foreach (SkillRuntime runtime in runtimes.Values)
		{
			if (runtime.Cooldown <= 0f)
				continue;

			runtime.Cooldown = Mathf.Max(0f, runtime.Cooldown - Time.deltaTime);
		}

		if (currentSkill != null)
			Execute();
	}

	public NodeState Execute()
	{
		if (currentSkill == null)
			return NodeState.Failure;

		SkillState previousState = currentSkill.State;
		NodeState state = executor.Run(currentSkill, gameObject);

		if (currentSkill != null &&
			previousState != SkillState.Attack &&
			currentSkill.State == SkillState.Attack)
		{
			SkillAttackStarted?.Invoke(
				currentSkill.Data,
				skills.IndexOf(currentSkill.Data));
		}

		if (state == NodeState.Success)
		{
			currentSkill = null;
		}

		return state;
	}
	public SkillRuntime GetRuntime(SkillData skill)
	{
		if (skill == null)
			return null;

		runtimes.TryGetValue(skill, out SkillRuntime runtime);

		return runtime;
	}

	public SkillData GetSkill(int index)
	{
		if (index < 0 || index >= skills.Count)
			return null;

		return skills[index];
	}

	public bool Contains(SkillData skill)
	{
		return skills.Contains(skill);
	}

	public void AddSkill(SkillData skill)
	{
		if (skill == null || skills.Contains(skill))
			return;

		skills.Add(skill);
		runtimes.Add(skill, new SkillRuntime(skill));
	}

	public void RemoveSkill(SkillData skill)
	{
		if (skill == null)
			return;

		skills.Remove(skill);
		runtimes.Remove(skill);

		if (currentSkill != null && currentSkill.Data == skill)
			currentSkill = null;
	}

	public void Clear()
	{
		skills.Clear();
		runtimes.Clear();
		currentSkill = null;
	}

	#region Skill

	public CondNode CanUse(SkillData skill)
	{
		SkillRuntime runtime = GetRuntime(skill);

		if (runtime == null)
			return false;

		if (currentSkill != null)
			return false;

		return runtime.Cooldown <= 0f;
	}

	public CondNode IsSillRange(SkillData skill)
	{
		if (skill == null)
			return false;

		if (!TryGetComponent(out AIPerception perception))
			return false;

		return perception.IsTargetInSkillRange(skill);
	}

	public ActNode Use(SkillData skill)
	{
		if (!CanUse(skill))
			return false;

		currentSkill = GetRuntime(skill);
		currentSkill.Begin();

		Debug.Log($"AISkill -> {skill.SkillName} -> Use");

		Execute();
		return true;
	}

	public CondNode IsCooldown(SkillData skill)
	{
		SkillRuntime runtime = GetRuntime(skill);

		if (runtime == null)
			return false;

		return runtime.Cooldown > 0f;
	}

	public CondNode IsCurrentSkill(SkillData skill)
	{
		return currentSkill != null &&
			   currentSkill.Data == skill;
	}

	public CondNode IsRunning()
	{
		return currentSkill != null;
	}

	public ActNode Cancel()
	{
		if (currentSkill == null)
			return false;

		Debug.Log($"Cancel Skill : {currentSkill.Data.SkillName}");

		currentSkill.Interrupt();
		currentSkill = null;

		if (TryGetComponent(out AIMovement movement))
			movement.StopFaceTarget();

		return true;
	}

	#endregion
}
