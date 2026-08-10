using UnityEngine;
using UnityEngine.InputSystem;

public class AsuisuiTestGun : MonoBehaviour
{
	[SerializeField] private float bulletSpeed = 20f;
	[SerializeField] private float bulletLifeTime = 2f;
	[SerializeField] private int damage = 100;

	private InputAction fireAction;

	private void Awake()
	{
		// 建立滑鼠左鍵 Action
		fireAction = new InputAction(
			name: "Fire",
			type: InputActionType.Button,
			binding: "<Mouse>/leftButton");

		fireAction.performed += OnFire;
	}

	private void OnEnable()
	{
		fireAction.Enable();
	}

	private void OnDisable()
	{
		fireAction.Disable();
	}

	private void OnDestroy()
	{
		fireAction.performed -= OnFire;
		fireAction.Dispose();
	}

	private void OnFire(InputAction.CallbackContext context)
	{
		Fire();
	}

	private void Fire()
	{
		Debug.Log("Fire!");

		GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Cube);
		bullet.tag = "PlayerBullet";
		bullet.transform.position = transform.position + transform.forward;
		bullet.transform.rotation = transform.rotation;
		bullet.transform.localScale = Vector3.one * 0.2f;

		Collider collider = bullet.GetComponent<Collider>();
		collider.isTrigger = true;

		Rigidbody rb = bullet.AddComponent<Rigidbody>();
		rb.useGravity = false;
		rb.isKinematic = false;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

		Bullet bulletScript = bullet.AddComponent<Bullet>();
		bulletScript.Initialize(transform.forward, bulletSpeed, damage, bulletLifeTime);
	}

	private class Bullet : MonoBehaviour
	{
		private Rigidbody rb;
		private int damage;

		public void Initialize(
			Vector3 direction,
			float speed,
			int damage,
			float lifeTime)
		{
			rb = GetComponent<Rigidbody>();
			this.damage = damage;

#if UNITY_6000_0_OR_NEWER
			rb.linearVelocity = direction * speed;
#else
		rb.velocity = direction * speed;
#endif

			Destroy(gameObject, lifeTime);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out AIStatus aiStatus))
			{
				aiStatus.TakeDamage(damage);
				Destroy(gameObject);
				return;
			}

		}
	}
}
