using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Vector2 moveInput;
    Vector3 moveDirection;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(moveDirection * Time.fixedDeltaTime * 5f);
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        Debug.Log("Move Input: " + moveDirection);
    }
    private void OnEnable()
    {
        InputSystem.actions.FindAction("Move").performed += OnMove;
        InputSystem.actions.FindAction("Move").canceled += OnMove;
    }

    private void OnDisable()
    {
        InputSystem.actions.FindAction("Move").performed -= OnMove;
        InputSystem.actions.FindAction("Move").canceled -= OnMove;
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
}
