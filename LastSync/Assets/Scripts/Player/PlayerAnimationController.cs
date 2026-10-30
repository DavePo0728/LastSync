using UnityEngine;

/// <summary>
/// 掛在 Player 根物件，讀取 PlayerMove 寫入 Rigidbody 的速度並傳給角色 Animator。
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField, Range(0f, 1f)] private float dampingTime = 0.1f;

    private Rigidbody playerRigidbody;
    private CharacterStats characterStats;
    private int speedParameterHash;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        characterStats = GetComponent<CharacterStats>();

        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>();

        speedParameterHash = Animator.StringToHash("Speed");
    }

    private void Update()
    {
        if (playerRigidbody == null || characterStats == null || characterAnimator == null)
            return;

        Vector3 horizontalVelocity = playerRigidbody.linearVelocity;
        horizontalVelocity.y = 0f;

        float normalizedSpeed = characterStats.MoveSpeed > 0f
            ? Mathf.Clamp01(horizontalVelocity.magnitude / characterStats.MoveSpeed)
            : 0f;

        characterAnimator.SetFloat(
            speedParameterHash,
            normalizedSpeed,
            dampingTime,
            Time.deltaTime);
    }
}
