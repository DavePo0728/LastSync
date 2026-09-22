using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class InventoryManager : MonoBehaviour, IChipUnlockPricePayer
{
    public static InventoryManager inventoryManagerInstance;
    [SerializeField]
    private int inventorySize;
    [SerializeField]
    InventorySystem inventorySystem;
    public InventorySystem InventorySystem => inventorySystem;
    //public static UnityAction<InventorySystem> OnDynamicInventoryDisplayResquested;
    public event Action<ChipType> OnCategoryHasNewItem;
    private List<ChipType> unreadCategories =new List<ChipType>();
    [SerializeField]
    int currency = 1000; // Example currency amount, replace with your actual currency management
    private void Awake()
    {
        inventoryManagerInstance = this;
        inventorySystem = new InventorySystem(inventorySize);
    }
    public bool TryAddChip(
       ChipData chipData,
       int amount,
       out bool isNewChip)
    {
        isNewChip = false;

        if (chipData == null || amount <= 0)
            return false;

        isNewChip = !CheckInventoryHaveItem(chipData);

        // 最好讓 AddToInventory 回傳 bool，
        // 用來表示背包是否成功加入。
        bool addedSuccessfully = AddToInventory(chipData, amount);

        if (!addedSuccessfully)
            return false;

        if (isNewChip)
        {
            unreadCategories.Add(chipData.chipType);
            OnCategoryHasNewItem?.Invoke(chipData.chipType);
            Debug.Log($"New chip added: {chipData.chipName}, Type: {chipData.chipType}");
        }
        return true;
    }

    public bool HasUnreadCategory(ChipType chipType)
    {
        return unreadCategories.Contains(chipType);
    }

    public void MarkCategoryAsRead(ChipType chipType)
    {
        unreadCategories.Remove(chipType);
    }
    public bool CheckInventoryHaveItem(ChipData chipData)
    {
        List<ChipData> chipDataList = new List<ChipData>();
        if (chipData == null)
            return false;

        return inventorySystem.inventory_Slots.Any(slot =>
            slot._chipData != null &&
            slot._chipData.chipID == chipData.chipID);
    }
    public bool AddToInventory(ChipData chipToAdd, int amount)
    {

        if (ContainItem(chipToAdd, out List<InventoryChipSlot> invSlot)) //Check whether item exists in inventory.
        {
            foreach (var slot in invSlot)
            {
                if (slot.RoomLeftInStack(amount))
                {
                    slot.AddToStack(amount);
                    inventorySystem.OnInventorySlotChange?.Invoke(slot);
                    return true;
                }
            }
        }
        if (HasFreeSlot(out InventoryChipSlot freeSlot))  //Gets the first available slot
        {
            {
                freeSlot.UpdateInventorySlot(chipToAdd,amount);
                inventorySystem.OnInventorySlotChange?.Invoke(freeSlot);
                return true;
            }
        }
        return false;
    }
    public bool ContainItem(ChipData itemToAdd, out List<InventoryChipSlot> invSlot)  //Do any of our slots have the items to add to them?
    {
        invSlot = inventorySystem.inventory_Slots
                .Where(slot =>
                    slot._chipData != null &&
                    slot._chipData.chipID == itemToAdd.chipID)
                .ToList();

        return invSlot.Count > 0;
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
