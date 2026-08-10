using System.Collections.Generic;
using ActionTree.Data;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class AIAnimController : MonoBehaviour
{
	private const string Forward = "Forward";
	private const string Backward = "Backward";
	private const string Left = "Left";
	private const string Right = "Right";
	private const string Defend = "Defend";
	private const string AttackA = "AttackA";
	private const string AttackB = "AttackB";
	private const string TakeDamage = "TakeDamage";
	private const string Die = "Die";

	private Animator animator;

	private NavMeshAgent agent;

	private AIStatus status;

	private AISkill skill;

	[SerializeField]
	private float moveThreshold = 0.05f;

	private readonly Dictionary<int, AnimatorControllerParameterType> parameters = new();

	private void Awake()
	{
		animator = GetComponent<Animator>();
		agent = GetComponent<NavMeshAgent>();
		status = GetComponent<AIStatus>();
		skill = GetComponent<AISkill>();

		CacheParameters();
	}

	private void OnEnable()
	{
		if (status != null)
		{
			status.Damaged += OnDamaged;
			status.Died += OnDied;
		}

		if (skill != null)
			skill.SkillAttackStarted += OnSkillAttackStarted;
	}

	private void OnDisable()
	{
		if (status != null)
		{
			status.Damaged -= OnDamaged;
			status.Died -= OnDied;
		}

		if (skill != null)
			skill.SkillAttackStarted -= OnSkillAttackStarted;
	}

	private void Update()
	{
		UpdateMoveParameters();
	}

	public void PlayAttackA()
	{
		SetTrigger(AttackA);
	}

	public void PlayAttackB()
	{
		SetTrigger(AttackB);
	}

	public void PlayTakeDamage()
	{
		SetTrigger(TakeDamage);
	}

	public void PlayDie()
	{
		ClearMoveParameters();
		SetTrigger(Die);
	}

	public void SetDefend(bool value)
	{
		SetBool(Defend, value);
	}

	private void CacheParameters()
	{
		parameters.Clear();

		if (animator == null)
			return;

		foreach (AnimatorControllerParameter parameter in animator.parameters)
		{
			int hash = Animator.StringToHash(parameter.name);
			parameters[hash] = parameter.type;
		}
	}

	private void UpdateMoveParameters()
	{
		if (animator == null || agent == null)
			return;

		Vector3 velocity = agent.velocity;
		velocity.y = 0f;

		if (velocity.sqrMagnitude <= moveThreshold * moveThreshold)
		{
			ClearMoveParameters();
			return;
		}

		Vector3 localVelocity = transform.InverseTransformDirection(velocity.normalized);
		float absX = Mathf.Abs(localVelocity.x);
		float absZ = Mathf.Abs(localVelocity.z);

		bool forward = absZ >= absX && localVelocity.z > 0f;
		bool backward = absZ >= absX && localVelocity.z < 0f;
		bool right = absX > absZ && localVelocity.x > 0f;
		bool left = absX > absZ && localVelocity.x < 0f;

		SetBool(Forward, forward);
		SetBool(Backward, backward);
		SetBool(Left, left);
		SetBool(Right, right);
	}

	private void ClearMoveParameters()
	{
		SetBool(Forward, false);
		SetBool(Backward, false);
		SetBool(Left, false);
		SetBool(Right, false);
	}

	private void OnSkillAttackStarted(SkillData data, int index)
	{
		if (index == 1)
			PlayAttackB();
		else
			PlayAttackA();
	}

	private void OnDamaged(float damage)
	{
		PlayTakeDamage();
	}

	private void OnDied()
	{
		PlayDie();
	}

	private void SetBool(string parameter, bool value)
	{
		if (!HasParameter(parameter, AnimatorControllerParameterType.Bool))
			return;

		animator.SetBool(parameter, value);
	}

	private void SetTrigger(string parameter)
	{
		if (!HasParameter(parameter, AnimatorControllerParameterType.Trigger))
			return;

		animator.SetTrigger(parameter);
	}

	private bool HasParameter(string parameter, AnimatorControllerParameterType type)
	{
		if (animator == null)
			return false;

		int hash = Animator.StringToHash(parameter);

		return parameters.TryGetValue(hash, out AnimatorControllerParameterType foundType) &&
			   foundType == type;
	}
}
