using UnityEngine;
using UnityEngine.InputSystem;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("武器基礎數值")]
    [SerializeField] protected int baseDamage = 10;
    [SerializeField] protected float fireRate = 0.5f; // 開火間隔 (秒)

    protected CharacterStats ownerStats;
    private float nextFireTime = 0f;

    protected virtual void Awake()
    {
        ownerStats = GetComponentInParent<CharacterStats>();
    }

    private void OnEnable()
    {
        InputSystem.actions.FindAction("Attack").performed += HandleInput;
    }

    private void OnDisable()
    {
        InputSystem.actions.FindAction("Attack").performed -= HandleInput;
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        // 結合射速限制
        if (Time.time >= nextFireTime)
        {
            PerformAttack();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// 取得計算後的最終傷害值
    /// </summary>
    protected int GetFinalDamage()
    {
        float currentMultiplier = ownerStats != null ? ownerStats.AttackMultiplier : 1.0f;
        return Mathf.RoundToInt(baseDamage * currentMultiplier);
    }

    /// <summary>
    /// 子類別必須實作的攻擊行為
    /// </summary>
    protected abstract void PerformAttack();
}
