using UnityEngine;

public class PlayerAim : MonoBehaviour
{
	[SerializeField] private Transform aimTarget;
	[SerializeField] private float rotationSpeed = 12f;

	Transform aimPivot;


    void Start()
	{
		aimTarget = GameObject.Find("HoverObject").transform;
	}

	void Update()
	{
		aimPivot = transform.parent;

		Vector3 flatTarget = new Vector3(
			aimTarget.position.x,
			aimPivot.position.y,
			aimTarget.position.z);

		Vector3 direction = flatTarget - aimPivot.position;

		if (direction.sqrMagnitude > 0.001f)
		{
			Quaternion targetRotation = Quaternion.LookRotation(direction);

			Quaternion smoothedRotation = Quaternion.Slerp(
				aimPivot.rotation,
				targetRotation,
				rotationSpeed * Time.deltaTime);

			float roundedY = Mathf.Round(
				smoothedRotation.eulerAngles.y * 1000f) / 1000f;

			aimPivot.rotation = Quaternion.Euler(0f, roundedY, 0f);
		}
	}
}