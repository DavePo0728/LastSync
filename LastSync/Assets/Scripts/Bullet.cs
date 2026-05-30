using UnityEngine;

public class Bullet : MonoBehaviour
{
    int damage;
    public int Damage => damage; // 這樣外部就可以讀取 damage，但只能透過 SetDamage 設定
    public void SetDamage(int damage)
    {
        this.damage = damage;
        // 這裡可以將 damage 儲存到一個變數中，或直接在碰撞時使用
        Debug.Log($"Bullet received damage value: {damage}");
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<EnemyStats>(out EnemyStats enemyStats))
            {
                enemyStats.TakeDamage(Damage);
                Debug.Log($"Bullet hit {other.gameObject.name} for {Damage} damage.");
            }
            Destroy(gameObject); // 子彈碰撞後銷毀
        }
    }
}
