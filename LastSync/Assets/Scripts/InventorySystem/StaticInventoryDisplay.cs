using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum InventoryCategoryFilter
{
    All,
    Attack,
    Defence,
    Other
}

public class StaticInventoryDisplay : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private InventorySlot_UI[] inventorySlotsUI;
    [SerializeField] private MouseItemData mouseItemData;

    [Header("Category buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defenceButton;
    [SerializeField] private Button otherButton;
    [Tooltip("Uses Button.interactable=false to indicate the selected category.")]
    [SerializeField] private bool disableSelectedCategoryButton = true;

    private InventorySystem inventorySystem;
    private Dictionary<InventorySlot_UI, InventoryChipSlot> slotDictionary;
    private InventoryCategoryFilter currentCategory = InventoryCategoryFilter.All;

    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlot_UI, InventoryChipSlot> SlotDictionary => slotDictionary;
    public InventoryCategoryFilter CurrentCategory => currentCategory;

    private void Awake()
    {
        AddCategoryButtonListeners();
    }

    private void Start()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("StaticInventoryDisplay requires an InventoryManager.", this);
            enabled = false;
            return;
        }

        inventorySystem = inventoryManager.InventorySystem;
        if (inventorySystem == null)
        {
            Debug.LogError("InventoryManager has no initialized InventorySystem.", inventoryManager);
            enabled = false;
            return;
        }

        inventorySystem.OnInventorySlotChange += UpdateSlot;
        AssignSlot(inventorySystem);
        SetCategory(InventoryCategoryFilter.All);
    }

    private void OnDestroy()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventorySlotChange -= UpdateSlot;

        RemoveCategoryButtonListeners();
    }

    private void Update()
    {
        if (mouseItemData == null ||
            mouseItemData.assignedInventorySlot == null ||
            mouseItemData.assignedInventorySlot._chipData == null ||
            Mouse.current == null)
        {
            return;
        }

        mouseItemData.transform.position = Mouse.current.position.ReadValue();

        if (Mouse.current.rightButton.isPressed)
            RightButtonSlotClicked();
    }

    private void AddCategoryButtonListeners()
    {
        allButton?.onClick.AddListener(ShowAll);
        attackButton?.onClick.AddListener(ShowAttack);
        defenceButton?.onClick.AddListener(ShowDefence);
        otherButton?.onClick.AddListener(ShowOther);
    }

    private void RemoveCategoryButtonListeners()
    {
        allButton?.onClick.RemoveListener(ShowAll);
        attackButton?.onClick.RemoveListener(ShowAttack);
        defenceButton?.onClick.RemoveListener(ShowDefence);
        otherButton?.onClick.RemoveListener(ShowOther);
    }

    public void ShowAll()
    {
        SetCategory(InventoryCategoryFilter.All);
    }

    public void ShowAttack()
    {
        SetCategory(InventoryCategoryFilter.Attack);
    }

    public void ShowDefence()
    {
        SetCategory(InventoryCategoryFilter.Defence);
    }

    public void ShowOther()
    {
        SetCategory(InventoryCategoryFilter.Other);
    }

    public void SetCategory(InventoryCategoryFilter category)
    {
        currentCategory = category;
        UpdateCategoryButtonStates();
        RefreshCategoryFilter();
    }

    public void RefreshCategoryFilter()
    {
        if (slotDictionary == null)
            return;

        foreach (KeyValuePair<InventorySlot_UI, InventoryChipSlot> pair in slotDictionary)
        {
            bool shouldShow = ShouldShowSlot(pair.Value);
            if (pair.Key.gameObject.activeSelf != shouldShow)
                pair.Key.gameObject.SetActive(shouldShow);
        }
    }

    private bool ShouldShowSlot(InventoryChipSlot inventorySlot)
    {
        if (currentCategory == InventoryCategoryFilter.All)
            return true;

        if (inventorySlot == null ||
            inventorySlot._chipData == null ||
            inventorySlot._stackSize <= 0)
        {
            return false;
        }

        switch (currentCategory)
        {
            case InventoryCategoryFilter.Attack:
                return inventorySlot._chipData.chipType == ChipType.Attack;

            case InventoryCategoryFilter.Defence:
                return inventorySlot._chipData.chipType == ChipType.Defend;

            case InventoryCategoryFilter.Other:
                return inventorySlot._chipData.chipType == ChipType.Utility;

            default:
                return true;
        }
    }

    private void UpdateCategoryButtonStates()
    {
        bool allButtonsInteractable = !disableSelectedCategoryButton;

        if (allButton != null)
            allButton.interactable = allButtonsInteractable || currentCategory != InventoryCategoryFilter.All;

        if (attackButton != null)
            attackButton.interactable = allButtonsInteractable || currentCategory != InventoryCategoryFilter.Attack;

        if (defenceButton != null)
            defenceButton.interactable = allButtonsInteractable || currentCategory != InventoryCategoryFilter.Defence;

        if (otherButton != null)
            otherButton.interactable = allButtonsInteractable || currentCategory != InventoryCategoryFilter.Other;
    }

    public void AssignSlot(InventorySystem inventoryToDisplay)
    {
        inventorySystem = inventoryToDisplay;
        slotDictionary = new Dictionary<InventorySlot_UI, InventoryChipSlot>();

        int inventorySize = inventorySystem.InventorySize;
        int uiSlotCount = inventorySlotsUI != null ? inventorySlotsUI.Length : 0;
        int assignableCount = Mathf.Min(inventorySize, uiSlotCount);

        if (uiSlotCount != inventorySize)
        {
            Debug.LogWarning(
                $"Inventory/UI slot count mismatch on {name}. " +
                $"Inventory: {inventorySize}, UI: {uiSlotCount}.",
                this);
        }

        for (int i = 0; i < assignableCount; i++)
        {
            InventorySlot_UI uiSlot = inventorySlotsUI[i];
            InventoryChipSlot inventorySlot = inventorySystem.inventory_Slots[i];

            if (uiSlot == null)
            {
                Debug.LogWarning($"Inventory UI slot {i} is not assigned on {name}.", this);
                continue;
            }

            slotDictionary.Add(uiSlot, inventorySlot);
            uiSlot.Init(inventorySlot);
        }

        for (int i = assignableCount; i < uiSlotCount; i++)
        {
            if (inventorySlotsUI[i] != null)
                inventorySlotsUI[i].gameObject.SetActive(false);
        }

        RefreshCategoryFilter();
    }

    private void UpdateSlot(InventoryChipSlot updatedSlot)
    {
        if (slotDictionary == null)
            return;

        foreach (KeyValuePair<InventorySlot_UI, InventoryChipSlot> pair in slotDictionary)
        {
            if (pair.Value != updatedSlot)
                continue;

            pair.Key.UpdateUISlot(updatedSlot);
            break;
        }

        RefreshCategoryFilter();
    }

    public void LeftButtonSlotClicked(InventorySlot_UI clickedUISlot)
    {
        if (clickedUISlot == null ||
            clickedUISlot.assigned_Inventory_Slot == null ||
            mouseItemData == null ||
            mouseItemData.assignedInventorySlot == null)
        {
            return;
        }

        try
        {
            InventoryChipSlot clickedSlot = clickedUISlot.assigned_Inventory_Slot;
            InventoryChipSlot mouseSlot = mouseItemData.assignedInventorySlot;

            // Slot has an item; mouse does not.
            if (clickedSlot._chipData != null && mouseSlot._chipData == null)
            {
                mouseItemData.UpdateMouseSlot(clickedSlot, 1);
                clickedSlot.RemoveFromStack(1);
                clickedUISlot.UpdateUISlot();
                return;
            }

            // Slot is empty; mouse has an item.
            if (clickedSlot._chipData == null && mouseSlot._chipData != null)
            {
                clickedSlot.AssignItem(mouseSlot, 1);
                clickedUISlot.UpdateUISlot();
                mouseItemData.ClearSlot();
                return;
            }

            if (clickedSlot._chipData == null || mouseSlot._chipData == null)
                return;

            if (clickedSlot._chipData == mouseSlot._chipData &&
                clickedSlot.RoomLeftInStack(mouseSlot._stackSize))
            {
                clickedSlot.AssignItem(mouseSlot, 1);
                clickedUISlot.UpdateUISlot();
                mouseItemData.ClearSlot();
                return;
            }

            if (clickedSlot._chipData == mouseSlot._chipData &&
                clickedSlot.RoomLeftInStack(mouseSlot._stackSize, out int leftInStack))
            {
                if (leftInStack < 1)
                {
                    SwapSlots(clickedUISlot);
                }
                else
                {
                    int remainingOnMouse = mouseSlot._stackSize - leftInStack;
                    clickedSlot.AddToStack(leftInStack);
                    clickedUISlot.UpdateUISlot();

                    InventoryChipSlot newItem =
                        new InventoryChipSlot(mouseSlot._chipData, remainingOnMouse);
                    mouseItemData.ClearSlot();
                    mouseItemData.UpdateMouseSlot(newItem);
                }

                return;
            }

            if (clickedSlot._chipData != mouseSlot._chipData)
            {
                InventorySlot_UI originalSlot = CheckInventory(mouseItemData);
                if (originalSlot != null)
                {
                    originalSlot.assigned_Inventory_Slot.AssignItem(mouseSlot, 1);
                    originalSlot.UpdateUISlot();
                    mouseItemData.UpdateMouseSlot(clickedSlot, 1);
                    clickedSlot.RemoveFromStack(1);
                    clickedUISlot.UpdateUISlot();
                }
                else
                {
                    inventoryManager.AddToInventory(mouseSlot._chipData, 1);
                    mouseItemData.UpdateMouseSlot(clickedSlot, 1);
                    clickedSlot.RemoveFromStack(1);
                    clickedUISlot.UpdateUISlot();
                }
            }
        }
        finally
        {
            RefreshCategoryFilter();
        }
    }

    public void RightButtonSlotClicked()
    {
        if (mouseItemData == null ||
            mouseItemData.assignedInventorySlot == null ||
            mouseItemData.assignedInventorySlot._chipData == null)
        {
            return;
        }

        try
        {
            InventorySlot_UI sameItemSlot = CheckInventory(mouseItemData);
            if (sameItemSlot != null)
            {
                sameItemSlot.assigned_Inventory_Slot.AssignItem(
                    mouseItemData.assignedInventorySlot,
                    1);
                sameItemSlot.UpdateUISlot();
                mouseItemData.ClearSlot();
            }
            else
            {
                inventoryManager.AddToInventory(
                    mouseItemData.assignedInventorySlot._chipData,
                    1);
                mouseItemData.ClearSlot();
            }
        }
        finally
        {
            RefreshCategoryFilter();
        }
    }

    private void SwapSlots(InventorySlot_UI clickedUISlot)
    {
        InventoryChipSlot clickedSlot = clickedUISlot.assigned_Inventory_Slot;
        InventoryChipSlot mouseSlot = mouseItemData.assignedInventorySlot;

        InventoryChipSlot oldEquippedSlot =
            new InventoryChipSlot(clickedSlot._chipData, clickedSlot._stackSize);

        clickedSlot.AssignItem(mouseSlot);
        clickedUISlot.UpdateUISlot();
        mouseItemData.UpdateMouseSlot(oldEquippedSlot);
    }

    public InventorySlot_UI CheckInventory(MouseItemData currentMouseItemData)
    {
        if (currentMouseItemData == null ||
            currentMouseItemData.assignedInventorySlot == null ||
            currentMouseItemData.assignedInventorySlot._chipData == null ||
            inventorySlotsUI == null)
        {
            return null;
        }

        ChipData targetChip = currentMouseItemData.assignedInventorySlot._chipData;

        return inventorySlotsUI.FirstOrDefault(slot =>
            slot != null &&
            slot.assigned_Inventory_Slot != null &&
            slot.assigned_Inventory_Slot._chipData == targetChip);
    }
}
