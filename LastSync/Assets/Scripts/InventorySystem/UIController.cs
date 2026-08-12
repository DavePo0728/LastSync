using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private GameObject canvasUI;
    public bool gameOver =false;

    private void Awake()
    {
        InputSystem.actions.FindAction("OpenInventory").performed += GetInventoryOpenInput;
    }
    void Start()
    {
        canvasUI.SetActive(false);
    }
    private void Update()
    {

    }
    public void GetInventoryOpenInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnAndOffInventoryAndCraftPanel();
        }
    }
    public void BackToStartScene()
    {
        SceneManager.LoadScene("StartScene");
        Time.timeScale = 1;
    }
    public void BackToGame()
    {
        Time.timeScale = 1;
    }
    public void OnAndOffInventoryAndCraftPanel()
    {
        if (gameOver == false)
        {
            if (canvasUI.gameObject.activeInHierarchy == true)
            {
                canvasUI.SetActive(false);
                //Cursor.visible = true;
            }
            else
            {
                canvasUI.SetActive(true);
                //Cursor.visible = false;
            }
        }
    }
}