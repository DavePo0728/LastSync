using UnityEngine;

public class MeleeEnemy : EnemyStats
{
    [Header("近戰設定")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float chargeTime = 1f;

    private bool isCharging = false;
    private float chargeTimer = 0f;

    private void Update()
    {
        if (player == null || CurrentHealth <= 0) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= attackRange)
        {
            if (!isCharging)
            {
                isCharging = true;
                chargeTimer = chargeTime;
            }
            else
            {
                chargeTimer -= Time.deltaTime;
                if (chargeTimer <= 0)
                {
                    Attack();
                    isCharging = false;
                }
            }
        }
        else if (!isCharging) // 不在範圍且未蓄力時才移動
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    protected override void Attack()
    {
        // 實作：發射近戰判定或直接對玩家呼叫 TakeDamage
        Debug.Log("近戰蓄力完成，發動攻擊！");
    }
}