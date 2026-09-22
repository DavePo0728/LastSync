using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class CharacterStats : MonoBehaviour
{
    [Header("Base stats")]
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private int baseDefence = 50;
    private float defenceReduction = 0.5f; // 50% 防禦減傷
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("Shield")]
    [SerializeField] private float shieldRegenDelay = 3.0f;
    [SerializeField] private int shieldRegenPerSecond = 5;

    [Header("UI Elements")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hitFlashImage;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    // 當前狀態數值
    public int CurrentHealth { get; private set; }
    public int CurrentMaxHealth { get; private set; }
    public int CurrentDefence { get; private set; }
    public float CurrentDefenceReduction { get; private set; }

    // 乘數儲存清單
    private List<float> damageTakenModifiers = new List<float>();
    private List<float> moveSpeedModifiers = new List<float>();
    private List<float> attackModifiers = new List<float>();

    // 內部計時與運算變數
    private float lastDamageTime;
    private float shieldRegenAccumulator = 0f;

    #region 動態屬性計算

    public float DamageTakenMultiplier => CalculateMultiplier(damageTakenModifiers);
    public float MoveSpeedMultiplier => CalculateMultiplier(moveSpeedModifiers);

    public float MoveSpeed => baseMoveSpeed ;
    public float DashCooldown => dashCooldown;


    #endregion

    private void Awake()
    {
        CurrentHealth = baseMaxHealth;
        CurrentMaxHealth = baseMaxHealth;
        CurrentDefence = baseDefence;
        CurrentDefenceReduction = defenceReduction;
        lastDamageTime = -shieldRegenDelay;
    }
    void Start()
    {
        hitFlashImage = GameObject.Find("HitFlashImage")?.GetComponent<Image>();
        hitFlashImageInitalize();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    private void Update()
    {
        //HandleShieldRegeneration();
    }
    void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)CurrentHealth / CurrentMaxHealth;
        }
    }

    #region 乘數管理系統

    private float CalculateMultiplier(List<float> modifiers)
    {
        if(modifiers.Count == 0)
            return 1.0f;
        float finalMultiplier = 1.0f;
        foreach (float mod in modifiers)
        {
            finalMultiplier *= mod;
        }
        return finalMultiplier;
    }

    public void AddMoveSpeedModifier(float mod) => moveSpeedModifiers.Add(mod);
    public void RemoveMoveSpeedModifier(float mod) => moveSpeedModifiers.Remove(mod);

    public void AddDamageTakenModifier(float mod) => damageTakenModifiers.Add(mod);
    public void RemoveDamageTakenModifier(float mod) => damageTakenModifiers.Remove(mod);

    #endregion

    #region 戰鬥與傷害邏輯

    public void TakeDamage(int rawDamage)
    {
        int actualDamage = Mathf.RoundToInt(rawDamage * DamageTakenMultiplier);

        lastDamageTime = Time.time;
        shieldRegenAccumulator = 0f;

        //if (CurrentShield > 0)
        //{
        //    if (CurrentShield >= actualDamage)
        //    {
        //        CurrentShield -= actualDamage;
        //        actualDamage = 0;
        //    }
        //    else
        //    {
        //        actualDamage -= CurrentShield;
        //        CurrentShield = 0;
        //    }
        //}

        if (actualDamage > 0)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth - actualDamage, 0, baseMaxHealth);
            CameraShake(0.1f); 
            hitFlashImage.gameObject.SetActive(true);
            Invoke("InactiveFlashImage", 0.02f);
            //shakeTimer = hurtShakeDuration;   //hpbar shake timer
            //currentShakeStrength = hurtShakeStrength;    //hpbar shake strength
            UpdateUI();
        }

        if (CurrentHealth <= 0)
        {
            HandleDeath();
        }
    }

    //private void HandleShieldRegeneration()
    //{
    //    if (CurrentShield < baseMaxShield && (Time.time - lastDamageTime) >= shieldRegenDelay)
    //    {
    //        shieldRegenAccumulator += shieldRegenPerSecond * Time.deltaTime;

    //        if (shieldRegenAccumulator >= 1f)
    //        {
    //            int regenAmount = Mathf.FloorToInt(shieldRegenAccumulator);
    //            CurrentShield = Mathf.Clamp(CurrentShield + regenAmount, 0, baseMaxShield);
    //            shieldRegenAccumulator -= regenAmount;
    //        }
    //    }
    //}
    void hitFlashImageInitalize()
    {
        if (hitFlashImage != null)
        {
            Color c = hitFlashImage.color;
            c.a = 0.69f;
            hitFlashImage.color = c;
        }
        hitFlashImage.gameObject.SetActive(false);
    }
    private void HandleDeath()
    {
        Debug.Log($"{gameObject.name} 判定死亡。");
    }

    #endregion
    void InactiveFlashImage()
    {
        hitFlashImage.gameObject.SetActive(false);
    }
    void CameraShake(float intensity)
    {
        impulseSource.GenerateImpulseWithForce(intensity);
    }
}

