using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    int damage = 10; // 假設敵人子彈的傷害固定為 10
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Initialize(int damageAmount)
    {
        damage = damageAmount;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out CharacterStats characterStats))
            {
                characterStats.TakeDamage(damage);
                Destroy(gameObject);
                Debug.Log($"EnemyBullet hit {other.name} and dealt {damage} damage.");
                return;
            }
        }
    }
}
