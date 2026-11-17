using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    void Start()
    {
        // 確保遊戲開始時時間縮放為正常速度
        Time.timeScale = 1;
    }
    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
