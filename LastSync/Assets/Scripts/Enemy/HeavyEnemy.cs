using UnityEngine;

public class HeavyEnemy : EnemyStats
{
    [Header("衝鋒設定")]
    [SerializeField] private float normalSpeed = 1.5f;
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float chargeDuration = 1f;
    [SerializeField] private float chargeCooldown = 5f;

    private bool isCharging = false;
    private float chargeEndTime;
    private float nextChargeTime;
    private Vector3 chargeDirection;

    private void Update()
    {
        if (player == null || CurrentHealth <= 0) return;

        if (isCharging)
        {
            // 衝鋒狀態：往鎖定的方向高速移動
            transform.position += chargeDirection * chargeSpeed * Time.deltaTime;
            
            if (Time.time >= chargeEndTime)
            {
                isCharging = false;
                nextChargeTime = Time.time + chargeCooldown;
            }
        }
        else
        {
            // 正常狀態：慢速追蹤
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, normalSpeed * Time.deltaTime);

            if (Time.time >= nextChargeTime)
            {
                Attack(); // 準備衝鋒
            }
        }
    }

    protected override void Attack()
    {
        isCharging = true;
        chargeEndTime = Time.time + chargeDuration;
        
        // 鎖定玩家當下位置的方向進行衝鋒
        chargeDirection = (player.transform.position - transform.position).normalized;
        
        // 由於 Y 軸(高度)可能導致敵人飛起或遁地，通常會將方向的 y 設為 0
        chargeDirection.y = 0; 
        chargeDirection.Normalize();

        Debug.Log("重裝敵人發動衝鋒！");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 衝鋒期間碰到玩家則造成傷害
        if (isCharging && collision.gameObject == player)
        {
            // 實作：對玩家造成傷害
            CharacterStats characterStats = player.GetComponent<CharacterStats>();
            characterStats.TakeDamage(20);
            Debug.Log("衝鋒撞擊到玩家！");
            isCharging = false; // 撞到後提早結束衝鋒
        }
    }
}