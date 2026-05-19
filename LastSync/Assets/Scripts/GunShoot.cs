using UnityEngine;
using UnityEngine.InputSystem;

public class GunShoot : MonoBehaviour
{
    [SerializeField]
    GameObject bulletPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
            bulletRB.AddForce(transform.forward * 500f);
            Destroy(bullet, 3f);
        }
    }
    private void OnEnable()
    {
        InputSystem.actions.FindAction("Attack").performed += Shoot;
    }

    private void OnDisable()
    {
        InputSystem.actions.FindAction("Attack").performed -= Shoot;
    }
}
