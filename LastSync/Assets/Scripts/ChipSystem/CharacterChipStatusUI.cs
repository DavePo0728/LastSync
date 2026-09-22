using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System;

public class CharacterChipStatusUI : MonoBehaviour
{
    [SerializeField] private ChipSystem chipSystem;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private GunShoot weaponStatus;
    [SerializeField] private TMP_Text hPText;
    [SerializeField] private TMP_Text defenceText;
    [SerializeField] private TMP_Text defenceReductionText;
    [SerializeField] private TMP_Text fireDamageText;
    [SerializeField] private TMP_Text fireRateText;
    [SerializeField] private TMP_Text fireRangeText;
    [SerializeField] private TMP_Text criticalChanceText;
    [SerializeField] private TMP_Text bounsBulletText;
    [SerializeField] private TMP_Text fireAccuracyText;
    [SerializeField] private TMP_Text bulletPeneText;
    [SerializeField] private TMP_Text MovementText;
    [SerializeField] private TMP_Text DashCoolDownText;
    private GameObject player;
    private void Awake()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryResolveReferences();
    }
    private void OnEnable()
    {
        // 每次打開背包／晶片面板時刷新。
        UpdateAllUIText();
    }
    public void UpdateAllUIText()
    {
        if (!TryResolveReferences())
            return;

        if (hPText != null)
        {
            hPText.text = characterStats.CurrentHealth.ToString() /*+ " / " + characterStats.CurrentMaxHealth.ToString()*/;
        }
        if (defenceText != null)
        {
            defenceText.text = characterStats.CurrentDefence.ToString();
        }
        if (defenceReductionText != null)
        {
            defenceReductionText.text = characterStats.CurrentDefenceReduction.ToString();
        }
        if (criticalChanceText != null)
        {
            criticalChanceText.text = weaponStatus.critical_Chance.ToString();
        }
        if (bulletPeneText != null)
        {
            bulletPeneText.text = weaponStatus.fire_BulletPenetration.ToString();
        }
        if (fireDamageText != null) {
            fireDamageText.text = (weaponStatus.Base_Damage + chipSystem.DamageMultiplier).ToString();
        }
        if (fireRateText != null)
        {
            fireRateText.text = (weaponStatus.Fire_Rate + chipSystem.FireRateMultiplier).ToString();
        }
        if (fireRangeText != null)
        {
            fireRangeText.text = (weaponStatus.Fire_Range + chipSystem.RangeMultiplier).ToString();
        }
        if (bounsBulletText != null)
        {
            bounsBulletText.text = (weaponStatus.Base_BulletCount + chipSystem.BonusBulletCount).ToString();
        }
        if (fireAccuracyText != null)
        {
            fireAccuracyText.text = (weaponStatus.Base_FireAccuracy + chipSystem.BonusAccuracyOffset).ToString();
        }
        if(MovementText != null)
        {
            MovementText.text = (characterStats.MoveSpeed + chipSystem.MoveSpeedMultiplier).ToString();
        }
        if(DashCoolDownText != null)
        {
            DashCoolDownText.text = (characterStats.DashCooldown + chipSystem.dashesMultiplier).ToString();
        }

    }
    private bool TryResolveReferences()
    {
        if (chipSystem == null)
            chipSystem = FindAnyObjectByType<ChipSystem>();

        if (characterStats == null || weaponStatus == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                if (characterStats == null)
                    characterStats = player.GetComponent<CharacterStats>();

                if (weaponStatus == null)
                    weaponStatus = player.GetComponentInChildren<GunShoot>(true);
            }
        }

        if (characterStats == null)
        {
            Debug.LogError(
                "CharacterChipStatusUI 找不到 Player 的 CharacterStats。",
                this);
            return false;
        }

        if (weaponStatus == null)
        {
            Debug.LogError(
                "CharacterChipStatusUI 找不到 Player 子物件的 GunShoot。",
                this);
            return false;
        }

        if (chipSystem == null)
        {
            Debug.LogError(
                "CharacterChipStatusUI 找不到 ChipSystem。",
                this);
            return false;
        }

        return true;
    }
}
