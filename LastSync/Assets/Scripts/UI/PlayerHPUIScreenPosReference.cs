using UnityEngine;
using UnityEngine.UI;

public class PlayerHPUIScreenPosReference : MonoBehaviour
{
    [SerializeField]
    GameObject posReferenceObject;
    [SerializeField]
    GameObject hpUiPrefab;
    [SerializeField]
    GameObject playerUIContainer;
    GameObject hpUIGameObject;
    CharacterStats characterStats;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterStats = GetComponent<CharacterStats>();
        posReferenceObject = gameObject.transform.Find("HpUiPosReference").gameObject;
        playerUIContainer = GameObject.Find("PlayerUIContainer");
        hpUIGameObject = Instantiate(hpUiPrefab, posReferenceObject.transform.position, Quaternion.identity, playerUIContainer.transform);
        characterStats.SetHealthBar(hpUIGameObject.GetComponent<Slider>());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hpUIGameObject == null)
            return;
        hpUIGameObject.transform.position = Camera.main.WorldToScreenPoint(posReferenceObject.transform.position);
    }
    public GameObject GetHpUIGameObjectChild()
    {
        if (hpUIGameObject == null)
            return null;

        return hpUIGameObject.GetComponent<Slider>()?.fillRect?.gameObject;
    }
    private void OnDestroy()
    {
        DeadDestroyUI();
    }
    public void DeadDestroyUI()
    {
        Destroy(hpUIGameObject);
        hpUIGameObject = null;
    }
}
