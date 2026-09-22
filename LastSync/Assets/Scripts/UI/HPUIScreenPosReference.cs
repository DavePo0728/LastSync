using UnityEngine;

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
        if(hpUIGameObject == null)
            return null;

            return hpUIGameObject.transform.Find("HpUIBar").gameObject;
    }
    public void DeadDestroyUI()
    {
        Destroy(hpUIGameObject);
    }
}
