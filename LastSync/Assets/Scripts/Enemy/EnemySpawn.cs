using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField]List<GameObject> enemyPrefab ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SpawnEnemy();
        }
    }
    private void OnEnable()
    {
        InputSystem.actions.FindAction("TestSpawn").performed += HandleInput;
    }

    private void OnDisable()
    {
        InputSystem.actions.FindAction("TestSpawn").performed -= HandleInput;
    }
    void SpawnEnemy()
    {
        // 在這裡實現生成敵人的邏輯，例如：
        if (enemyPrefab != null)
        {
            Vector3 RandomSpawnPosition = new Vector3(Random.Range(-5f, 5f)+transform.position.x,transform.position.y , Random.Range(-5f, 5f)+transform.position.z);
            Instantiate(enemyPrefab[0], RandomSpawnPosition, Quaternion.identity);
        }
    }
}
