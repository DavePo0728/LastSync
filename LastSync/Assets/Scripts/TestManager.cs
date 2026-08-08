using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestManager : MonoBehaviour
{
    [SerializeField]
    List<ChipData> chipDataList;
    private void Start()
    {
        InputSystem.actions.FindAction("Test").performed += TestGetChip;
    }
    private void TestGetChip(InputAction.CallbackContext context)
    {
            int chipIndex = Random.Range(0, chipDataList.Count);
            InventoryManager.inventoryManagerInstance.AddToInventory(chipDataList[chipIndex],1);
            //Debug.Log($"[系統] 玩家拾取了晶片: {chipDataList[chipIndex].chipID}");
    }
}
