using UnityEngine;
using UnityEngine.UI;

public class HPUIScreenPosReference : MonoBehaviour
{
    [SerializeField]
    GameObject posReferenceObject;
    [SerializeField]
    GameObject hpUiPrefab;
    [SerializeField]
    GameObject enemyUIContainer;
    GameObject hpUIGameObject;
    public GameObject _hpUIGameObject => hpUIGameObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        posReferenceObject = gameObject.transform.Find("HpUiPosReference").gameObject;
        enemyUIContainer = GameObject.Find("EnemyUIContainer");
        hpUIGameObject = Instantiate(hpUiPrefab, posReferenceObject.transform.position, Quaternion.identity, enemyUIContainer.transform);
        // Bind after creation so component Start order cannot leave the bar stale.
        GetComponent<AIStatus>()?.SetHealthBar(GetHealthBar());
    }

    // Update is called once per frame
    void Update()
    {
        if (hpUIGameObject == null)
            return;
        hpUIGameObject.transform.position = Camera.main.WorldToScreenPoint(posReferenceObject.transform.position);
    }
    public GameObject GetHpUIGameObjectChild()
    {
        if (hpUIGameObject == null)
            return null;

        return GetHealthBar()?.fillRect?.gameObject;
    }
    public Slider GetHealthBar()
    {
        return hpUIGameObject != null ? hpUIGameObject.GetComponent<Slider>() : null;
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
