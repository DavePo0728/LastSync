using ActionTree;
using ActionTree.Data;
using UnityEngine;

[DisallowMultipleComponent]
public class AIPerception : MonoBehaviour	// This class is responsible for detecting the player and managing the AI's perception of its surroundings
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

    [Header("Wall Detection")]
    [SerializeField]
    private CapsuleCollider bodyCollider;

    [SerializeField, Min(0f)]
    private float wallContactPadding = 0.05f;

    private readonly Collider[] wallContactResults = new Collider[16];

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

	private void UpdatePerception()     // Update the perception of the AI to detect the player
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
    private bool EnsureTarget()     // Ensure that the target is detected and updated
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

	public Vector3 DirectionToTarget()// Get the direction from the AI to the target
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

		if (target.TryGetComponent(out CharacterStats characterStats))
		{
			characterStats.TakeDamage((int)damage);
			return true;
		}
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
    public CondNode IsTouchingWall() // Check if the AI has hit a wall while moving towards the target
    {
        if (bodyCollider == null)
            bodyCollider = GetComponent<CapsuleCollider>();
        if (bodyCollider == null || !bodyCollider.enabled)
            return false;

        GetWorldCapsule(
            bodyCollider,
            out Vector3 pointA,
            out Vector3 pointB,
            out float radius);

        int hitCount = Physics.OverlapCapsuleNonAlloc(
            pointA,
            pointB,
            radius + wallContactPadding,
            wallContactResults,
            Physics.AllLayers,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = wallContactResults[i];

            if (hit != null && hit.CompareTag("Wall"))
                return true;
        }

        return false;
    }
    public CondNode CheckWallBetweenTarget() // Check if there is a wall between the AI and the target
    {
        if (!EnsureTarget())
            return false;
        Vector3 direction = DirectionToTarget();
        float distance = DistanceToTarget();
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
        {
            if (hit.transform.CompareTag("Wall"))
                return true; // There is a wall between the AI and the target
        }
        return false; // No wall between the AI and the target
    }
    public CondNode IsFacingTarget(float angleThreshold = 10f) // Check if the AI is facing the target within a certain angle threshold
    {
        if (!EnsureTarget())
            return false;
        Vector3 directionToTarget = DirectionToTarget();
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle <= angleThreshold;
    }
    #endregion
    private static void GetWorldCapsule(
    CapsuleCollider capsule,
    out Vector3 pointA,
    out Vector3 pointB,
    out float radius)
    {
        Transform capsuleTransform = capsule.transform;
        Vector3 scale = capsuleTransform.lossyScale;

        Vector3 localAxis;
        float axisScale;
        float radiusScale;

        switch (capsule.direction)
        {
            case 0:
                localAxis = Vector3.right;
                axisScale = Mathf.Abs(scale.x);
                radiusScale = Mathf.Max(
                    Mathf.Abs(scale.y),
                    Mathf.Abs(scale.z));
                break;

            case 2:
                localAxis = Vector3.forward;
                axisScale = Mathf.Abs(scale.z);
                radiusScale = Mathf.Max(
                    Mathf.Abs(scale.x),
                    Mathf.Abs(scale.y));
                break;

            default:
                localAxis = Vector3.up;
                axisScale = Mathf.Abs(scale.y);
                radiusScale = Mathf.Max(
                    Mathf.Abs(scale.x),
                    Mathf.Abs(scale.z));
                break;
        }

        Vector3 center =
            capsuleTransform.TransformPoint(capsule.center);

        Vector3 worldAxis =
            capsuleTransform.TransformDirection(localAxis).normalized;

        radius = capsule.radius * radiusScale;

        float height = Mathf.Max(
            capsule.height * axisScale,
            radius * 2f);

        float halfSegmentLength =
            height * 0.5f - radius;

        Vector3 offset =
            worldAxis * halfSegmentLength;

        pointA = center + offset;
        pointB = center - offset;
    }
}
