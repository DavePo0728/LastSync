
using UnityEngine;

public static class AsuisuiUtility
{
	/// <summary>
	/// 立即看向目標（只旋轉 Y 軸）
	/// </summary>
	public static void LookAt(Transform self, Transform target)
	{
		Vector3 direction = target.position - self.position;
		direction.y = 0f;

		if (direction.sqrMagnitude < 0.0001f)
			return;

		self.rotation = Quaternion.LookRotation(direction);
	}

	/// <summary>
	/// 平滑看向目標（Lerp）
	/// </summary>
	public static void LookAtLerp(Transform self, Transform target, float speed)
	{
		Vector3 direction = target.position - self.position;
		direction.y = 0f;

		if (direction.sqrMagnitude < 0.0001f)
			return;

		Quaternion targetRotation = Quaternion.LookRotation(direction);

		self.rotation = Quaternion.Lerp(
			self.rotation,
			targetRotation,
			speed * Time.deltaTime);
	}

	/// <summary>
	/// 等速看向目標（RotateTowards）
	/// </summary>
	public static void LookAtRotate(Transform self, Transform target, float rotateSpeed)
	{
		Vector3 direction = target.position - self.position;
		direction.y = 0f;

		if (direction.sqrMagnitude < 0.0001f)
			return;

		Quaternion targetRotation = Quaternion.LookRotation(direction);

		self.rotation = Quaternion.RotateTowards(
			self.rotation,
			targetRotation,
			rotateSpeed * Time.deltaTime);
	}
}

