using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseItemData : MonoBehaviour
{
    public Image chipIcon;
    //public Text ItemCount;
    public InventoryChipSlot assignedInventorySlot;

    private void Awake()
    {
        chipIcon = gameObject.GetComponentInChildren<Image>();
        //ItemCount = gameObject.GetComponentInChildren<Text>();
        chipIcon.color = Color.clear;
        //ItemCount.text = "";
    }
    public void UpdateMouseSlot(InventoryChipSlot invSlot)
    {
        assignedInventorySlot.AssignItem(invSlot);
        chipIcon.sprite = invSlot._chipData.icon;
        //ItemCount.text = invSlot.stackSize.ToString();
        chipIcon.color = Color.white;
    }
    public void UpdateMouseSlot(InventoryChipSlot invSlot, int stackSize)
    {
        assignedInventorySlot.AssignItem(invSlot, stackSize);

        chipIcon.sprite = invSlot._chipData.icon;
        chipIcon.color = Color.white;
    }
    public void ClearSlot()
    {
        assignedInventorySlot.ClearSlot();
        chipIcon.sprite = null;
        chipIcon.color = Color.clear;
        //ItemCount.text = "";
    }
    
}
