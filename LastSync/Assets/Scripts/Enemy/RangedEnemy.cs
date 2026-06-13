using UnityEngine;

public class RangedEnemy : EnemyStats
{
    [Header("遠距設定")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float fleeRange = 4f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private GameObject bulletPrefab;

    private float nextFireTime;

    private void Update()
    {
        if (player == null || CurrentHealth <= 0) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        if (distance < fleeRange)
        {
            // 玩家太近，反向逃跑
            transform.position -= directionToPlayer * moveSpeed * Time.deltaTime;
        }
        else if (distance <= attackRange)
        {
            // 在攻擊範圍內，且距離安全
            if (Time.time >= nextFireTime)
            {
                Attack();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            // 玩家太遠，靠近玩家
            transform.position += directionToPlayer * moveSpeed * Time.deltaTime;
        }
    }

    protected override void Attack()
    {
        // 實作：生成彈幕物件並賦予 CurrentAttack 的傷害值
        Debug.Log("發射彈幕！");
    }
}