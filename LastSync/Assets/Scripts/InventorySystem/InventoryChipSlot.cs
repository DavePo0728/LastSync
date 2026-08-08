using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryChipSlot
{
    [SerializeField] private ChipData chipData; //reference to the data
    [SerializeField] private int stackSize;

    public ChipData _chipData => chipData;
    public int _stackSize => stackSize;
    public InventoryChipSlot(ChipData source, int amount)  //Constructor to make a occupied Inventory slot.
    {
        chipData = source;
        stackSize = amount;
    }

    public InventoryChipSlot() //Constructor to make an empty inventory slot.
    {
        ClearSlot();
    }
    public void ClearSlot()
    {
        chipData = null;
        stackSize = -1;
    }
    public void AssignItem(InventoryChipSlot invSlot) // Assign the hold stack item to the slot
    {
        if (chipData == invSlot.chipData) { AddToStack(invSlot.stackSize);} //Does the slot contain the same item? 
        else  //overwrite slot with the inventory slot that we are passing in.
        {
            chipData = invSlot.chipData;
            stackSize = 0;
            AddToStack(invSlot.stackSize);
        }

    }
    public void AssignItem(InventoryChipSlot invSlot, int amount) //Assign an item to the slot
    {
        if (chipData == invSlot.chipData)//Does the slot contain the same item? 
        {
            AddToStack(amount);
        }
        else  //overwrite slot with the inventory slot that we are passing in.
        {
            chipData = invSlot.chipData;
            stackSize = 0;
            AddToStack(amount);
        }
    }
    public InventoryChipSlot CreateCopy()
    {
        if (chipData == null)
            return new InventoryChipSlot();
        return new InventoryChipSlot(chipData, stackSize);
    }
    public void UpdateInventorySlot(ChipData data,int amount) //Updates slot directly
    {
        chipData = data;
        //Debug.Log(itemData);
        stackSize = amount;
    }
    public void AddToStack(int amount)
    {
        stackSize += amount;
    }
    public void RemoveFromStack(int amount)
    {
        stackSize -= amount;
        if(stackSize < 1)
        {
            chipData = null;
            stackSize = 0;
        }
    }
    public bool RoomLeftInStack(int amountToAdd, out int amountRemaining)  // would the be enough room in the stack for the amount we are trying to add.
    {
        amountRemaining = chipData.maxStackSize - stackSize;
        return RoomLeftInStack(amountToAdd);
    }
    public bool RoomLeftInStack(int amountToAdd)
    {
        if (stackSize + amountToAdd <= chipData.maxStackSize)
        {
            return true;
        }
        else return false;
    }
}
