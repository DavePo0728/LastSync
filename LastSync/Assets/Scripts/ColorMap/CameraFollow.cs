using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private Vector3 offset = new(0, 15, -8);

	public void SetTarget(Transform target)
	{
		this.target = target;
	}

	private void LateUpdate()
	{
		if (target == null)
			return;

		transform.position = target.position + offset;
	}
}