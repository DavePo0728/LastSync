using ActionTree;
using ActionTree.Runtime;
using ActionTree.Data;
using System;
using System.Reflection;
using UnityEngine;

public sealed class SkillExecutor
{
	private const BindingFlags SkillActionFlags =
		BindingFlags.Static |
		BindingFlags.Public |
		BindingFlags.DeclaredOnly;

	public NodeState Run(SkillRuntime runtime, GameObject owner)
	{
		if (runtime == null)
			return NodeState.Failure;

		if (runtime.WaitTimer > 0f)
		{
			runtime.WaitTimer -= Time.deltaTime;
			return NodeState.Running;
		}

		switch (runtime.State)
		{
			case SkillState.Idle:
				return NodeState.Failure;

			case SkillState.Before:
				if (!RunPhase(runtime, owner, runtime.Data.BeforeActions))
					return NodeState.Running;

				if (runtime.Data.CastTime > 0f)
				{
					float prepareWaitTime = RunCastingPrepareActions(runtime, owner);
					runtime.BeginPhase(SkillState.Casting);
					runtime.WaitTimer = prepareWaitTime > 0f
						? prepareWaitTime
						: runtime.Data.CastTime;
				}
				else
				{
					runtime.BeginPhase(SkillState.Attack);
				}

				return NodeState.Running;

			case SkillState.Casting:
				runtime.BeginPhase(SkillState.Attack);
				return NodeState.Running;

			case SkillState.Attack:
				if (runtime.ActionIndex == 0 && runtime.PreparedAttackActionCount > 0)
					runtime.ActionIndex = runtime.PreparedAttackActionCount;

				if (!RunPhase(runtime, owner, runtime.Data.AttackActions))
					return NodeState.Running;

				runtime.BeginPhase(SkillState.After);

				return NodeState.Running;

			case SkillState.After:
				if (!RunPhase(runtime, owner, runtime.Data.AfterActions))
					return NodeState.Running;

				StopFaceTarget(owner);
				runtime.Finish();

				return NodeState.Running;

			case SkillState.Recover:
				// Kept for assets created with the older state machine.
				StopFaceTarget(owner);
				runtime.Finish();

				return NodeState.Running;

			case SkillState.Finished:

				runtime.Reset();

				return NodeState.Success;
		}

		return NodeState.Failure;
	}

	private static void StopFaceTarget(GameObject owner)
	{
		if (owner != null && owner.TryGetComponent(out AIMovement movement))
			movement.StopFaceTarget();
	}

	private static float RunCastingPrepareActions(SkillRuntime runtime, GameObject owner)
	{
		if (runtime.Data.AttackActions == null)
			return 0f;

		runtime.PreparedAttackActionCount = 0;
		float waitTime = 0f;

		foreach (SkillActionData action in runtime.Data.AttackActions)
		{
			if (!IsCastingPrepareAction(action))
				break;

			InvokeAction(runtime, owner, action);
			if (TryGetActionWaitTime(action, out float actionWaitTime))
				waitTime = Mathf.Max(waitTime, actionWaitTime);

			runtime.PreparedAttackActionCount++;
		}

		return waitTime;
	}

	private static bool IsCastingPrepareAction(SkillActionData action)
	{
		return action != null &&
			   (action.MethodName == nameof(SkillProcess.FaceTarget) ||
			    action.MethodName == nameof(SkillProcess.LockMove) ||
			    action.MethodName == nameof(SkillProcess.AttackRange));
	}

	private static bool RunPhase(
		SkillRuntime runtime,
		GameObject owner,
		System.Collections.Generic.List<SkillActionData> actions)
	{
		if (actions == null || actions.Count == 0)
			return true;

		if (runtime.ActionIndex >= actions.Count)
			return true;

		SkillActionData action = actions[runtime.ActionIndex];
		runtime.ActionIndex++;

		if (action == null || string.IsNullOrEmpty(action.MethodName))
			return runtime.ActionIndex >= actions.Count;

		InvokeAction(runtime, owner, action);

		if (TryGetActionWaitTime(action, out float waitTime) && waitTime > 0f)
			runtime.WaitTimer = waitTime;

		return runtime.ActionIndex >= actions.Count && runtime.WaitTimer <= 0f;
	}

	private static void InvokeAction(SkillRuntime runtime, GameObject owner, SkillActionData action)
	{
		MethodInfo method = typeof(SkillProcess).GetMethod(action.MethodName, SkillActionFlags);

		if (method == null)
		{
			Debug.LogWarning($"Skill action not found: {action.MethodName}");
			return;
		}

		AIStatus status = owner != null ? owner.GetComponent<AIStatus>() : null;
		object[] arguments = BuildArguments(method, action, status, runtime.Data);

		try
		{
			method.Invoke(null, arguments);
			Debug.Log($"AISkill -> {runtime.Data.SkillName} -> {action.MethodName}");
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	private static object[] BuildArguments(
		MethodInfo method,
		SkillActionData action,
		AIStatus status,
		SkillData skill)
	{
		ParameterInfo[] methodParameters = method.GetParameters();
		object[] arguments = new object[methodParameters.Length];
		int dataIndex = 0;

		for (int i = 0; i < methodParameters.Length; i++)
		{
			Type type = methodParameters[i].ParameterType;

			if (type == typeof(AIStatus))
			{
				arguments[i] = status;
				continue;
			}

			if (type == typeof(SkillData))
			{
				arguments[i] = skill;
				continue;
			}

			if (action.Parameters == null || dataIndex >= action.Parameters.Count)
			{
				arguments[i] = GetDefault(type);
				continue;
			}

			arguments[i] = ReflectionUtility.BuildArguments(
				new System.Collections.Generic.List<MethodParameter> { action.Parameters[dataIndex] })[0];
			dataIndex++;
		}

		return arguments;
	}

	private static object GetDefault(Type type)
	{
		return type != null && type.IsValueType ? Activator.CreateInstance(type) : null;
	}

	private static bool TryGetActionWaitTime(SkillActionData action, out float waitTime)
	{
		waitTime = 0f;

		if (action.MethodName == nameof(SkillProcess.Recover))
			return TryFindFloat(action, "time", out waitTime);

		if (action.MethodName == nameof(SkillProcess.FaceTarget))
		{
			bool waitUntilFinished = true;

			if (TryFindBool(action, "waitUntilFinished", out bool configuredWait))
				waitUntilFinished = configuredWait;

			if (!waitUntilFinished)
				return false;

			return TryFindFloat(action, "time", out waitTime);
		}

		if (action.MethodName == nameof(SkillProcess.Move))
			return TryFindFloat(action, "duration", out waitTime);

		if (action.MethodName == nameof(SkillProcess.Dash))
			return TryFindFloat(action, "duration", out waitTime);

		return false;
	}

	private static bool TryFindBool(SkillActionData action, string parameterName, out bool value)
	{
		value = false;

		if (action.Parameters == null)
			return false;

		foreach (MethodParameter parameter in action.Parameters)
		{
			if (parameter.parameterName != parameterName)
				continue;

			value = parameter.boolValue;
			return true;
		}

		return false;
	}

	private static bool TryFindFloat(SkillActionData action, string parameterName, out float value)
	{
		value = 0f;

		if (action.Parameters == null)
			return false;

		foreach (MethodParameter parameter in action.Parameters)
		{
			if (parameter.parameterName != parameterName)
				continue;

			value = parameter.floatValue;
			return true;
		}

		return false;
	}
}
