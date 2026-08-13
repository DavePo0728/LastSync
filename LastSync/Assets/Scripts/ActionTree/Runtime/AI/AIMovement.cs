using ActionTree;
using ActionTree.Data;
using ActionTree.Runtime;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AIStatus))]
public class AIMovement : MonoBehaviour
{
	private const float NavMeshSampleDistance = 3f;

	#region Parameter

	[SerializeField]
	private NavMeshAgent agent;

	[SerializeField]
	private AIStatus status;

	public NavMeshAgent Agent => agent;

	private bool movementLocked;
	private bool rotationLocked;
	private bool controlLocked;
	private bool facingTarget;
	private bool defaultUpdateRotation = true;
	private Coroutine dashCoroutine;

	#endregion

	#region Unity

	private void Awake()
	{
		if (agent == null)
			agent = GetComponent<NavMeshAgent>();

		if (status == null)
			status = GetComponent<AIStatus>();

		defaultUpdateRotation = agent.updateRotation;
		agent.speed = status.MoveSpeed;
	}

	private void Update()
	{
		float targetSpeed = IsMoveLocked() ? 0f : status.MoveSpeed;

		if (agent.speed != targetSpeed)
			agent.speed = targetSpeed;

		if (!IsRotationLocked() && facingTarget && TryGetComponent(out AIPerception perception))
			LookAtTarget(perception);
	}

	#endregion

	#region Method

	public void MoveTo(Vector3 position)
	{
		if (!agent.enabled || IsMoveLocked())
			return;

		if (!EnsureAgentOnNavMesh())
			return;

		if (!TryGetNavMeshPosition(position, out Vector3 destination))
			return;

		agent.isStopped = false;
		agent.SetDestination(destination);
	}

	public void Stop()
	{
		if (!agent.enabled || !agent.isOnNavMesh)
			return;

		agent.isStopped = true;
		agent.ResetPath();
	}

	public void LookAtTarget(AIPerception perception)
	{
		if (IsRotationLocked() || perception == null || !perception.HasTarget())
			return;

		Vector3 direction = perception.TargetPosition() - transform.position;
		direction.y = 0f;

		if (direction.sqrMagnitude < 0.0001f)
			return;

		transform.rotation = Quaternion.LookRotation(direction);
	}

	public bool FaceTarget(AIPerception perception)
	{
		if (IsRotationLocked() || perception == null || !perception.HasTarget())
			return false;

		LookAtTarget(perception);
		return true;
	}

	public bool StartFaceTarget(AIPerception perception)
	{
		if (!FaceTarget(perception))
			return false;

		facingTarget = true;

		if (agent.enabled)
			agent.updateRotation = false;

		return true;
	}

	public void StopFaceTarget()
	{
		facingTarget = false;

		if (agent.enabled && !IsRotationLocked())
			agent.updateRotation = defaultUpdateRotation;
	}

	public bool LockMove()
	{
		movementLocked = true;
		Stop();

		if (agent.enabled)
			agent.speed = 0f;

		return true;
	}

	public bool UnlockMove()
	{
		movementLocked = false;
		rotationLocked = false;
		controlLocked = false;
		StopFaceTarget();

		if (agent.enabled)
		{
			agent.isStopped = false;
			agent.speed = status.MoveSpeed;
		}

		return true;
	}

	public bool IsMoveLocked()
	{
		return movementLocked || controlLocked;
	}

	public bool LockRotation()
	{
		rotationLocked = true;
		StopFaceTarget();

		if (agent.enabled)
			agent.updateRotation = false;

		return true;
	}

	public bool UnlockRotation()
	{
		rotationLocked = false;

		if (agent.enabled && !facingTarget && !controlLocked)
			agent.updateRotation = defaultUpdateRotation;

		return true;
	}

	public bool IsRotationLocked()
	{
		return rotationLocked || controlLocked;
	}

	public bool LockControl()
	{
		movementLocked = true;
		rotationLocked = true;
		controlLocked = true;
		StopFaceTarget();
		StopDash();
		Stop();

		if (agent.enabled)
		{
			agent.updateRotation = false;
			agent.speed = 0f;
		}

		return true;
	}

	public bool DashForward(float distance, float duration)
	{
		if (!agent.enabled || !EnsureAgentOnNavMesh() || controlLocked)
			return false;

		if (dashCoroutine != null)
			StopDash();

		dashCoroutine = StartCoroutine(DashRoutine(distance, duration));
		return true;
	}

	private System.Collections.IEnumerator DashRoutine(float distance, float duration)
	{
		Vector3 direction = transform.forward;
		direction.y = 0f;

		if (direction.sqrMagnitude < 0.0001f)
			direction = Vector3.forward;

		direction.Normalize();

		if (duration <= 0f)
		{
			if (!controlLocked)
				agent.Move(direction * distance);

			dashCoroutine = null;
			yield break;
		}

		float elapsed = 0f;
		float speed = distance / duration;

		while (elapsed < duration && !controlLocked)
		{
			float delta = Mathf.Min(Time.deltaTime, duration - elapsed);
			agent.Move(direction * speed * delta);
			elapsed += delta;
			yield return null;
		}

		dashCoroutine = null;
	}

	private void StopDash()
	{
		if (dashCoroutine == null)
			return;

		StopCoroutine(dashCoroutine);
		dashCoroutine = null;
	}

	public bool HasPath()
	{
		return agent.enabled &&
			   agent.isOnNavMesh &&
			   agent.hasPath;
	}

	public bool IsMoving()
	{
		return agent.enabled &&
			   agent.isOnNavMesh &&
			   !agent.isStopped &&
			   agent.hasPath &&
			   !agent.pathPending &&
			   agent.remainingDistance > agent.stoppingDistance;
	}

	public bool Arrived()
	{
		if (!agent.enabled || !agent.isOnNavMesh)
			return false;

		if (agent.pathPending)
			return false;

		return agent.remainingDistance <= agent.stoppingDistance;
	}

	public Vector3 Destination()
	{
		if (!agent.enabled || !agent.isOnNavMesh)
			return transform.position;

		return agent.destination;
	}

	public float RemainingDistance()
	{
		if (!agent.enabled || !agent.isOnNavMesh)
			return Mathf.Infinity;

		return agent.remainingDistance;
	}

	private bool EnsureAgentOnNavMesh()
	{
		if (agent.isOnNavMesh)
			return true;

		if (!NavMesh.SamplePosition(
			transform.position,
			out NavMeshHit hit,
			NavMeshSampleDistance,
			agent.areaMask))
		{
			return false;
		}

		return agent.Warp(hit.position);
	}

	private bool TryGetNavMeshPosition(
		Vector3 position,
		out Vector3 navMeshPosition)
	{
		if (NavMesh.SamplePosition(
			position,
			out NavMeshHit hit,
			NavMeshSampleDistance,
			agent.areaMask))
		{
			navMeshPosition = hit.position;
			return true;
		}

		navMeshPosition = position;
		return false;
	}

	#endregion

	#region Action Node

	public ActNode MoveToTarget(AIPerception perception)
	{
		if (perception == null || !perception.HasTarget())
			return false;

		if (!perception.IsTargetTooFar())
		{
			Stop();
			return true;
		}

		MoveTo(perception.TargetPosition());

		return false;
	}

	public ActNode ChaseTarget()
	{
		if (!TryGetComponent(out AIPerception perception))
			return false;

		MoveToTarget(perception);
		return true;
	}

	public ActNode MoveAwayFromTarget(AIPerception perception)
	{
		if (perception == null || !perception.HasTarget())
			return false;

		if (!perception.IsTargetTooClose())
		{
			Stop();
			StopFaceTarget();
			return true;
		}

		StartFaceTarget(perception);

		Vector3 direction = (transform.position - perception.TargetPosition()).normalized;

		Vector3 destination =
			perception.TargetPosition() +
			direction * perception.MinAttackRange;

		MoveTo(destination);

		return false;
	}

	public ActNode MoveAwayToSkillRange(SkillData skill)
	{
		if (skill == null || !TryGetComponent(out AIPerception perception))
			return false;

		if (!perception.HasTarget())
			return false;

		float targetDistance = skill.Range;
		float currentDistance = perception.DistanceToTarget();

		if (currentDistance >= targetDistance)
		{
			Stop();
			StopFaceTarget();
			return true;
		}

		StartFaceTarget(perception);

		Vector3 direction = transform.position - perception.TargetPosition();
		direction.y = 0f;

		if (direction.sqrMagnitude < 0.0001f)
			direction = -transform.forward;

		direction.Normalize();

		Vector3 destination =
			perception.TargetPosition() +
			direction * targetDistance;

		MoveTo(destination);

		return true;
	}

	public ActNode MoveToSkillRange(SkillData skill)
	{
		if (skill == null || !TryGetComponent(out AIPerception perception))
			return false;

		if (!perception.HasTarget())
			return false;

		if (perception.DistanceToTarget() <= skill.Range)
		{
			Stop();
			return true;
		}

		MoveTo(perception.TargetPosition());
		return true;
	}

	public ActNode Action_LockMove()
	{
		return LockMove();
	}

	public ActNode Action_UnlockMove()
	{
		return UnlockMove();
	}

	public ActNode Action_LockRotation()
	{
		return LockRotation();
	}

	public ActNode Action_UnlockRotation()
	{
		return UnlockRotation();
	}

	public ActNode Action_FaceTarget(AIPerception perception)
	{
		return FaceTarget(perception);
	}

	public ActNode Action_DashForward(float distance, float duration)
	{
		return DashForward(distance, duration);
	}

	#endregion

	#region Condition Node

	public CondNode Condition_IsMoveLocked()
	{
		return IsMoveLocked();
	}

	public CondNode Condition_IsRotationLocked()
	{
		return IsRotationLocked();
	}

	#endregion
}
