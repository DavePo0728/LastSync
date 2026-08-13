using UnityEngine;

public class Bullet : MonoBehaviour
{
    int damage;
    Vector3 startPosition;
    float maxRangeSqr;
    public int Damage => damage; // 這樣外部就可以讀取 damage，但只能透過 SetDamage 設定
    public void Initialize(int finalDamage, float maxRange)
    {
        damage = finalDamage;
        startPosition = transform.position;

        // 預先計算並儲存射程的平方
        maxRangeSqr = maxRange * maxRange;
    }
    private void Update()
    {
        if ((transform.position - startPosition).sqrMagnitude >= maxRangeSqr)
        {
            HandleMaxRangeReached();
        }
    }
    private void HandleMaxRangeReached()
    {
        // 實作：可在此加入子彈消散的粒子特效
        Destroy(gameObject);
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent(out AIStatus aiStatus))
            {
                aiStatus.TakeDamage(damage);
                Destroy(gameObject);
                Debug.Log($"Bullet hit {other.name} and dealt {damage} damage.");
                return;
            }
        }
    }
}
