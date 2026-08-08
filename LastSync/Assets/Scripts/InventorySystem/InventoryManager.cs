using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
public class InventoryManager : MonoBehaviour, IChipUnlockPricePayer
{
    public static InventoryManager inventoryManagerInstance;
    [SerializeField]
    private int inventorySize;
    [SerializeField]
    InventorySystem inventorySystem;
    //[SerializeField]
    //private ChipDisplay craftDisplay;

    public InventorySystem InventorySystem => inventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayResquested;
    [SerializeField]
    int currency = 1000; // Example currency amount, replace with your actual currency management
    private void Awake()
    {
        inventoryManagerInstance = this;
        inventorySystem = new InventorySystem(inventorySize);
    }
    public bool CheckInventoryHaveItem(ChipData chipData)
    {
        List<ChipData> chipDataList = new List<ChipData>();
        foreach (var item in inventorySystem.inventory_Slots)
        {
            if (item._chipData != null)
            {
                chipDataList.Add(chipData);
            }
        }
        if (chipDataList.Contains(chipData))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void AddToInventory(ChipData chipToAdd, int amount)
    {

        if (ContainItem(chipToAdd, out List<InventoryChipSlot> invSlot)) //Check whether item exists in inventory.
        {
            foreach (var slot in invSlot)
            {
                if (slot.RoomLeftInStack(amount))
                {
                    slot.AddToStack(amount);
                    inventorySystem.OnInventorySlotChange?.Invoke(slot);
                    return;
                }
            }
        }
        if (HasFreeSlot(out InventoryChipSlot freeSlot))  //Gets the first available slot
        {
            {
                freeSlot.UpdateInventorySlot(chipToAdd,amount);
                inventorySystem.OnInventorySlotChange?.Invoke(freeSlot);

            }
        }
    }
    public bool ContainItem(ChipData itemToAdd, out List<InventoryChipSlot> invSlot)  //Do any of our slots have the items to add to them?
    {
        invSlot = inventorySystem.inventory_Slots.Where(i => i._chipData == itemToAdd).ToList(); //if they do, the get a list of all of them
        return invSlot == null ? false : true;   // if they do return true, if not return false.
    }
    public bool HasFreeSlot(out InventoryChipSlot freeSlot)
    {
        freeSlot = inventorySystem.inventory_Slots.FirstOrDefault(i => i._chipData == null); //Get the first free slot.
        return freeSlot == null ? false : true;  // if they do return true, if not return false.
    }
    public bool TryPayUnlockPrice(int amount)
    {
        // Implement your logic to check if the player has enough currency to pay the unlock price
        // For example, you might have a PlayerCurrencyManager that tracks the player's currency

        if (currency >= amount)
        {
            currency -= amount;
            return true;
        }

        return false; // Replace with actual logic
    }
}
public interface IChipUnlockPricePayer
{
    bool TryPayUnlockPrice(int amount);
}
