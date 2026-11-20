using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Vector2 moveInput;
    [SerializeField]
    CharacterStats characterStat;
    Rigidbody rb;
    Collider[] playerColliders;
    LayerMask[] originalExcludeLayers;
    int[] originalOverridePriorities;
    CollisionDetectionMode originalCollisionDetection;
    int dashIgnoreLayers;
    float moveSpeed;
    [SerializeField]
    float DashForce = 20f; // Force applied during dash
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float dashCooldown = 2f; // Dash cooldown in seconds
    float dashTimer = 0f; // Timer for dash cooldown

    float dashDuration = 0.1f; // Duration of the dash in seconds

    bool isDashing = false; // Flag to check if the player is currently dashing
    public bool IsDashing => isDashing;
    void Start()
    {
        characterStat = GetComponent<CharacterStats>();
        if(characterStat != null)
        {
            moveSpeed = characterStat.MoveSpeed;
        }
        rb = GetComponent<Rigidbody>();
        // Include child colliders belonging to this body, but not separate rigidbodies.
        playerColliders = Array.FindAll(GetComponentsInChildren<Collider>(true),
            collider => collider.attachedRigidbody == rb);
        originalExcludeLayers = new LayerMask[playerColliders.Length];
        originalOverridePriorities = new int[playerColliders.Length];
        dashIgnoreLayers = LayerMask.GetMask("Enemy", "Bullet", "Trap");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDashing)
            return; // 衝刺期間，暫時不讓一般移動覆蓋衝刺速度

        Vector3 direction = GetCameraRelativeDirection();

        rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            rb.linearVelocity.y, // 保留重力、跳躍等垂直速度
            direction.z * moveSpeed
        );
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(dashTimer <= 0f)
            {
                Dash();
                StartCoroutine(StartDashCoolDownCountDown());
            }
            else
            {
                Debug.Log("Dash is on cooldown. Time left: " + dashTimer);
            }

        }
    }
    private void OnEnable()
    {
        InputSystem.actions.FindAction("Move").performed += OnMove;
        InputSystem.actions.FindAction("Move").canceled += OnMove;
        InputSystem.actions.FindAction("Dash").performed += OnDash;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ResetIsDashing));
        ResetIsDashing();
        StopAllCoroutines();
        dashTimer = 0f;
        InputSystem.actions.FindAction("Move").performed -= OnMove;
        InputSystem.actions.FindAction("Move").canceled -= OnMove;
        InputSystem.actions.FindAction("Dash").performed -= OnDash;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Drop")
        {
            Debug.Log("get");
            Destroy(other.gameObject);
            // Implement player damage logic here
        }
    }
    private void Dash()
    {
        if (isDashing)
            return;

        isDashing = true;
        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider collider = playerColliders[i];
            if (collider == null)
                continue;

            originalExcludeLayers[i] = collider.excludeLayers;
            originalOverridePriorities[i] = collider.layerOverridePriority;
            collider.excludeLayers = originalExcludeLayers[i].value | dashIgnoreLayers;
            collider.layerOverridePriority = int.MaxValue;
        }

        // Keep wall contacts active and prevent fast movement tunnelling through thin walls.
        originalCollisionDetection = rb.collisionDetectionMode;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.AddForce(GetCameraRelativeDirection() * DashForce, ForceMode.Impulse);
        Invoke(nameof(ResetIsDashing), dashDuration);
        Debug.Log("Dash performed");
    }
    void ResetIsDashing()
    {
        if (!isDashing)
            return;

        for (int i = 0; i < playerColliders.Length; i++)
        {
            if (playerColliders[i] == null)
                continue;

            playerColliders[i].excludeLayers = originalExcludeLayers[i];
            playerColliders[i].layerOverridePriority = originalOverridePriorities[i];
        }

        if (rb != null)
        {
            // Remove the dash velocity before returning to the original detection mode.
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.collisionDetectionMode = originalCollisionDetection;
        }
        isDashing = false;
    }
    IEnumerator StartDashCoolDownCountDown()
    {
        dashTimer = dashCooldown;
        while (dashTimer > 0f)
        {
            dashTimer -= Time.deltaTime;
            yield return null;
        }
    }

    private Vector3 GetCameraRelativeDirection()
    {
        if (Camera.main == null)
            return Vector3.zero;

        Transform cameraTransform = Camera.main.transform;

        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        return Vector3.ClampMagnitude(
            cameraRight * moveInput.x + cameraForward * moveInput.y,
            1f);
    }
}
