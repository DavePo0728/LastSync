using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class InventorySystem
{
    [SerializeField] private List<InventoryChipSlot> inventorySlots; 

    public List<InventoryChipSlot> inventory_Slots => inventorySlots;

    public int InventorySize => inventory_Slots.Count;

    public UnityAction<InventoryChipSlot> OnInventorySlotChange;

    public InventorySystem(int size) //Constructor that sets the amount of slots.
    {
        inventorySlots = new List<InventoryChipSlot>(size);

        for(int i=0;i<size; i++)
        { 
            inventorySlots.Add(new InventoryChipSlot());
        }
    }
}
