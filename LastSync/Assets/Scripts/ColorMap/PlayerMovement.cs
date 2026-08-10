using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField]
	private float moveSpeed = 6f;

	private Rigidbody rb;
	private Camera mainCamera;

	private InputAction moveAction;
	private InputAction lookAction;

	private Plane groundPlane;

	private Vector2 moveInput;
	private Vector2 mousePosition;

	private void Awake()
	{
		InitializeComponents();
		InitializeInput();
	}

	private void OnEnable()
	{
		moveAction.Enable();
		lookAction.Enable();
	}

	private void OnDisable()
	{
		moveAction.Disable();
		lookAction.Disable();
	}

	private void Update()
	{
		ReadInput();
	}

	private void FixedUpdate()
	{
		Move();
		Rotate();
	}

	/// <summary>
	/// 初始化元件
	/// </summary>
	private void InitializeComponents()
	{
		rb = GetComponent<Rigidbody>();

		rb.interpolation = RigidbodyInterpolation.Interpolate;

		mainCamera = Camera.main;

		groundPlane = new Plane(Vector3.up, Vector3.zero);
	}

	/// <summary>
	/// 初始化輸入
	/// </summary>
	private void InitializeInput()
	{
		moveAction = new InputAction("Move", InputActionType.Value);

		moveAction.AddCompositeBinding("2DVector")
			.With("Up", "<Keyboard>/w")
			.With("Down", "<Keyboard>/s")
			.With("Left", "<Keyboard>/a")
			.With("Right", "<Keyboard>/d");

		moveAction.AddBinding("<Gamepad>/leftStick");

		lookAction = new InputAction("Look", InputActionType.Value);

		lookAction.AddBinding("<Mouse>/position");
	}

	/// <summary>
	/// 讀取輸入
	/// </summary>
	private void ReadInput()
	{
		moveInput = moveAction.ReadValue<Vector2>();
		mousePosition = lookAction.ReadValue<Vector2>();
	}

	/// <summary>
	/// 玩家移動
	/// </summary>
	private void Move()
	{
		Vector3 direction = new Vector3(
			moveInput.x,
			0,
			moveInput.y);

		Vector3 targetPosition =
			rb.position +
			direction.normalized *
			moveSpeed *
			Time.fixedDeltaTime;

		rb.MovePosition(targetPosition);
	}

	/// <summary>
	/// 玩家朝向滑鼠
	/// </summary>
	private void Rotate()
	{
		if (mainCamera == null)
			return;

		Ray ray = mainCamera.ScreenPointToRay(mousePosition);

		if (!groundPlane.Raycast(ray, out float distance))
			return;

		Vector3 hitPoint = ray.GetPoint(distance);

		Vector3 direction = hitPoint - rb.position;
		direction.y = 0;

		if (direction.sqrMagnitude < 0.001f)
			return;

		Quaternion rotation = Quaternion.LookRotation(direction);

		rb.MoveRotation(rotation);
	}
}