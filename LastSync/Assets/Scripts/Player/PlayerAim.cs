using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField]
    Transform aimTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        aimTarget = GameObject.Find("HoverObject").transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.LookAt(aimTarget);
        transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
    }
}
