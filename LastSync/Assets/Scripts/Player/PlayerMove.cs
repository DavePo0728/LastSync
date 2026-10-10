using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Vector2 moveInput;
    Vector3 moveDirection;
    [SerializeField]
    CharacterStats characterStat;
    Rigidbody rb;
    Collider playerCollider;
    float moveSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float dashCooldown = 2f; // Dash cooldown in seconds
    float dashTimer = 0f; // Timer for dash cooldown

    float dashDuration = 0.1f; // Duration of the dash in seconds

    bool isDashing = false; // Flag to check if the player is currently dashing
    void Start()
    {
        characterStat = GetComponent<CharacterStats>();
        if(characterStat != null)
        {
            moveSpeed = characterStat.MoveSpeed;
        }
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDashing)
            return; // 衝刺期間，暫時不讓一般移動覆蓋衝刺速度

        Vector3 direction = Vector3.ClampMagnitude(moveDirection, 1f);

        rb.linearVelocity = new Vector3(
            direction.x * moveSpeed,
            rb.linearVelocity.y, // 保留重力、跳躍等垂直速度
            direction.z * moveSpeed
        );
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        //Debug.Log("Move Input: " + moveDirection);
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
        isDashing = true;
        rb.AddForce(moveDirection.normalized * 20f, ForceMode.Impulse);
        playerCollider.enabled = false; // Disable collider to avoid collisions during dash
        Invoke("ResetIsDashing", dashDuration);
        Debug.Log("Dash performed");
    }
    void ResetIsDashing()
    {
        isDashing = false;
        playerCollider.enabled = true; // Re-enable collider after dash
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
}
