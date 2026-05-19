using UnityEngine;
using UnityEngine.InputSystem;

public class HoverTransform : MonoBehaviour
{
    Camera mainCam;
    Vector3 mousePosition,mouseToWorldPosition;
    Ray mouseRay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCam = Camera.main;
        
        Debug.DrawRay(mouseRay.origin, mouseRay.direction, Color.red);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mousePosition = Mouse.current.position.ReadValue();
        mouseRay = mainCam.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hitInfo))
        {
            transform.position = hitInfo.point;
        }
    }
}
