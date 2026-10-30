using System.Collections;
using UnityEngine;

/// <summary>
/// 掛在 Player 根物件，讀取 PlayerMove 寫入 Rigidbody 的速度並傳給角色 Animator。
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator characterAnimator;
    [SerializeField] private string shootLayerName = "Shoot";
    [SerializeField] private string shootStateName = "Shoot.BAKED_walkG";
    [SerializeField] private float shootDuration = 2.5f;
    [SerializeField] private float shootFadeInDuration = 0.1f;
    [SerializeField] private float shootFadeOutDuration = 0.2f;

    private Rigidbody playerRigidbody;
    private CharacterStats characterStats;
    private int speedParameterHash;
    private int shootLayerIndex = -1;
    private Coroutine shootCoroutine;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        characterStats = GetComponent<CharacterStats>();

        if (characterAnimator == null)
            characterAnimator = GetComponentInChildren<Animator>();

        speedParameterHash = Animator.StringToHash("Speed");
        shootLayerIndex = characterAnimator != null
            ? characterAnimator.GetLayerIndex(shootLayerName)
            : -1;

        if (shootLayerIndex >= 0)
            characterAnimator.SetLayerWeight(shootLayerIndex, 0f);
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

        normalizedSpeed = Mathf.Round(normalizedSpeed * 1000f) / 1000f;

        characterAnimator.SetFloat(speedParameterHash, normalizedSpeed);
    }

    public void PlayShootAnimation()
    {
        if (characterAnimator == null || shootLayerIndex < 0)
            return;

        if (shootCoroutine != null)
            StopCoroutine(shootCoroutine);

        characterAnimator.Play(shootStateName, shootLayerIndex, 0f);
        shootCoroutine = StartCoroutine(PlayShootLayer());
    }

    private IEnumerator PlayShootLayer()
    {
        yield return FadeShootLayer(1f, shootFadeInDuration);

        float holdDuration = Mathf.Max(
            0f,
            shootDuration - shootFadeInDuration - shootFadeOutDuration);

        yield return new WaitForSeconds(holdDuration);
        yield return FadeShootLayer(0f, shootFadeOutDuration);
        shootCoroutine = null;
    }

    private IEnumerator FadeShootLayer(float targetWeight, float duration)
    {
        float startWeight = characterAnimator.GetLayerWeight(shootLayerIndex);

        if (duration <= 0f)
        {
            characterAnimator.SetLayerWeight(shootLayerIndex, targetWeight);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            characterAnimator.SetLayerWeight(
                shootLayerIndex,
                Mathf.Lerp(startWeight, targetWeight, progress));
            yield return null;
        }

        characterAnimator.SetLayerWeight(shootLayerIndex, targetWeight);
    }
}
