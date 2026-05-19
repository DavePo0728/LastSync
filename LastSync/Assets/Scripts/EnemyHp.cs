using UnityEngine;

public class EnemyHp : MonoBehaviour
{
    [SerializeField]
    GameObject drop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Bullet")
        {
            Destroy(other.gameObject);
            Destroy(gameObject);
            Instantiate(drop, transform.position, Quaternion.identity);
        }
    }
}
