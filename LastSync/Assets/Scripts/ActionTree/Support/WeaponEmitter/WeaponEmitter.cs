using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponEmitter : MonoBehaviour
{
	[SerializeField]
	private GameObject projectilePrefab;

	[SerializeField]
	private Transform muzzle;

	[SerializeField]
	private int burstCount = 3;

	[SerializeField]
	private float burstInterval = 0.12f;

	[SerializeField]
	private float projectileSpeed = 12f;

	[SerializeField]
	private float projectileLifeTime = 3f;

	private Coroutine fireRoutine;

	public bool IsFiring => fireRoutine != null;

	private Transform Muzzle => muzzle != null ? muzzle : transform;

	public void Fire()
	{
		if (!isActiveAndEnabled || IsFiring)
			return;

		fireRoutine = StartCoroutine(FireBurstRoutine());
	}

	public void StopFire()
	{
		if (fireRoutine == null)
			return;

		StopCoroutine(fireRoutine);
		fireRoutine = null;
	}

	private IEnumerator FireBurstRoutine()
	{
		int count = Mathf.Max(1, burstCount);

		for (int i = 0; i < count; i++)
		{
			SpawnProjectile();

			if (i < count - 1 && burstInterval > 0f)
				yield return new WaitForSeconds(burstInterval);
		}

		fireRoutine = null;
	}

	private void SpawnProjectile()
	{
		Transform firePoint = Muzzle;
		GameObject projectile = projectilePrefab != null
			? Instantiate(projectilePrefab, firePoint.position, firePoint.rotation)
			: CreateFallbackProjectile(firePoint);

		if (projectile.TryGetComponent(out Rigidbody body))
		{
#if UNITY_6000_0_OR_NEWER
			body.linearVelocity = firePoint.forward * projectileSpeed;
#else
			body.velocity = firePoint.forward * projectileSpeed;
#endif
		}

		if (projectileLifeTime > 0f)
			Destroy(projectile, projectileLifeTime);
	}

	private static GameObject CreateFallbackProjectile(Transform firePoint)
	{
		GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		projectile.name = "Emitter Projectile";
		projectile.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
		projectile.transform.localScale = Vector3.one * 0.2f;

		Collider collider = projectile.GetComponent<Collider>();
		collider.isTrigger = true;

		Rigidbody body = projectile.AddComponent<Rigidbody>();
		body.useGravity = false;
		body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		return projectile;
	}
}
