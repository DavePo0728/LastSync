using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public ChipDisplay chipDisplay;
    public StaticInventoryDisplay inventoryDisplay;
    public Image backGroundImage;
    public Image cocktailImage;
    public GameObject pauseMenu;
    public bool gameOver =false;

    private void Awake()
    {
        //craftDisplay.gameObject.SetActive(false);
        //inventoryDisplay.gameObject.SetActive(false);
        backGroundImage.gameObject.SetActive(false);
        Cursor.visible = false;
    }

    private void Update()
    {
        if (gameOver == false) {
            if (Input.GetKeyDown(KeyCode.E)) OnAndOffInventoryAndCraftPanel();
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (pauseMenu.activeInHierarchy == false)
                {
                    pauseMenu.SetActive(true);
                    Cursor.visible = true;
                    Time.timeScale = 0;
                }
                else if (pauseMenu.activeInHierarchy == true)
                {
                    BackToGame();
                    Cursor.visible = false;
                }
            }
        }
    }
    public void BackToStartScene()
    {
        SceneManager.LoadScene("StartScene");
        Time.timeScale = 1;
    }
    public void BackToGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }
    public void OnAndOffInventoryAndCraftPanel()
    {
        if (!backGroundImage.gameObject.activeInHierarchy)
        {
            backGroundImage.gameObject.SetActive(true);
            Cursor.visible = true;
        }
        else
        {
            backGroundImage.gameObject.SetActive(false);
            //inventoryDisplay.ResetInventory();
            //chipDisplay.ClearAllSlot(chipData);
            cocktailImage.gameObject.SetActive(false);
            Cursor.visible = false;
        }
    }
}