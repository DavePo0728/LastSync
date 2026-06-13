using UnityEngine;

public class SelfDestroyEnemy : EnemyStats
{
    [Header("自爆設定")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float explosionRadius = 1.5f;

    private void Update()
    {
        if (player == null || CurrentHealth <= 0) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= explosionRadius)
        {
            Attack();
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    protected override void Attack()
    {
        // 實作：對玩家造成 CurrentAttack 傷害
        Debug.Log("自爆攻擊！造成範圍傷害。");
        CurrentHealth = 0;
        HandleDeath(); // 自毀
    }
}