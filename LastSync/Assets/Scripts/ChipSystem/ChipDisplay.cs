using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChipDisplay : MonoBehaviour
{
    private const int GridWidth = 3;
    private const int GridHeight = 5;
    private const int MinGridX = -1;
    private const int MaxGridX = 1;
    private const int MinGridY = -2;
    private const int MaxGridY = 2;

    private static readonly Vector2Int[] CardinalDirections =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    private static readonly Vector2Int[] InitialUnlockedPositions =
    {
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up
    };

    [Header("Existing chip equipment dependencies")]
    [SerializeField] private MouseItemData mouseItemData;
    [SerializeField] private ChipSystem chipSystem;
    [SerializeField] private CharacterChipStatusUI characterChipStatusUI;

    [Header("3 x 5 slot generation")]
    [SerializeField] private ChipSlot_UI chipSlotPrefab;
    [SerializeField] private RectTransform slotContainer;
    [SerializeField, Min(1f)] private float slotStep = 135f;
    [SerializeField] private Sprite plusSprite;

    [Header("Unlock price")]
    [Tooltip("Price of the first paid slot unlock.")]
    [SerializeField, Min(0)] private int baseUnlockPrice = 1;
    [Tooltip("Added after every paid unlock. Use 0 for a fixed price.")]
    [SerializeField, Min(0)] private int unlockPriceIncreasePerSlot;
    [Tooltip("Optional. The assigned component must implement IChipUnlockPricePayer.")]
    [SerializeField] private MonoBehaviour unlockPricePayerSource;

    private readonly Dictionary<Vector2Int, ChipSlot_UI> slotsByCoordinate = new();
    private readonly List<ChipSlot_UI> equipChipSlots = new();

    private Func<int, bool> unlockPaymentHandler;
    private IChipUnlockPricePayer unlockPricePayer;
    private int paidUnlockCount;

    public int PaidUnlockCount => paidUnlockCount;
    public int UnlockedSlotCount => equipChipSlots.Count;
    public int MaxChipSlotCount => GridWidth * GridHeight - 1;
    public IReadOnlyDictionary<Vector2Int, ChipSlot_UI> SlotsByCoordinate => slotsByCoordinate;

    public event Action<Vector2Int, int> SlotUnlocked;
    public event Action<int> UnlockPaymentFailed;

    private void Awake()
    {
        if (slotContainer == null)
            slotContainer = transform as RectTransform;

        if (unlockPricePayerSource != null)
        {
            unlockPricePayer = unlockPricePayerSource as IChipUnlockPricePayer;
            if (unlockPricePayer == null)
            {
                Debug.LogError(
                    $"{unlockPricePayerSource.GetType().Name} must implement " +
                    $"{nameof(IChipUnlockPricePayer)}.",
                    unlockPricePayerSource);
            }
        }
    }

    private void Start()
    {
        if (chipSystem != null)
            chipSystem.buffUIChange += ClearAllSlot;

        BuildInitialLayout();
    }

    private void OnDestroy()
    {
        if (chipSystem != null)
            chipSystem.buffUIChange -= ClearAllSlot;
    }

    private void OnValidate()
    {
        slotStep = Mathf.Max(1f, slotStep);
        baseUnlockPrice = Mathf.Max(0, baseUnlockPrice);
        unlockPriceIncreasePerSlot = Mathf.Max(0, unlockPriceIncreasePerSlot);
    }

    private void BuildInitialLayout()
    {
        if (chipSlotPrefab == null || slotContainer == null)
        {
            Debug.LogError("ChipDisplay requires a ChipSlot_UI prefab and slot container.", this);
            return;
        }

        if (slotsByCoordinate.Count > 0)
            return;

        // Create all four initial unlocked slots first. This prevents one of
        // them being created as an expansion slot while neighbours are revealed.
        foreach (Vector2Int position in InitialUnlockedPositions)
            CreateSlot(position, false);

        // Every valid cardinal neighbour of an initial slot becomes available
        // for expansion immediately. There is no four-slot batch limit.
        foreach (Vector2Int position in InitialUnlockedPositions)
            RevealExpansionNeighbours(position);
    }

    private ChipSlot_UI CreateSlot(Vector2Int gridPosition, bool isExpansionSlot)
    {
        if (!CanCreateSlotAt(gridPosition))
            return null;

        if (slotsByCoordinate.TryGetValue(gridPosition, out ChipSlot_UI existingSlot))
            return existingSlot;

        ChipSlot_UI newSlot = Instantiate(chipSlotPrefab, slotContainer);
        newSlot.name = $"Chip Slot ({gridPosition.x}, {gridPosition.y})";

        RectTransform slotRect = (RectTransform)newSlot.transform;
        slotRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.anchoredPosition = GridToAnchoredPosition(gridPosition);

        InventoryChipSlot inventorySlot = new InventoryChipSlot();
        newSlot.Configure(
            this,
            gridPosition,
            inventorySlot,
            isExpansionSlot,
            plusSprite);

        slotsByCoordinate.Add(gridPosition, newSlot);

        if (!isExpansionSlot)
            equipChipSlots.Add(newSlot);

        return newSlot;
    }

    private void RevealExpansionNeighbours(Vector2Int unlockedPosition)
    {
        foreach (Vector2Int direction in CardinalDirections)
        {
            Vector2Int neighbourPosition = unlockedPosition + direction;

            if (!CanCreateSlotAt(neighbourPosition) ||
                slotsByCoordinate.ContainsKey(neighbourPosition))
            {
                continue;
            }

            CreateSlot(neighbourPosition, true);
        }
    }

    private bool CanCreateSlotAt(Vector2Int gridPosition)
    {
        bool insideGrid =
            gridPosition.x >= MinGridX && gridPosition.x <= MaxGridX &&
            gridPosition.y >= MinGridY && gridPosition.y <= MaxGridY;

        if (!insideGrid)
            return false;

        // (0,0) is permanently occupied by the non-interactable centre Image.
        return gridPosition != Vector2Int.zero;
    }

    public Vector2 GridToAnchoredPosition(Vector2Int gridPosition)
    {
        return new Vector2(gridPosition.x * slotStep, gridPosition.y * slotStep);
    }

    public bool TryGetSlot(Vector2Int gridPosition, out ChipSlot_UI slot)
    {
        return slotsByCoordinate.TryGetValue(gridPosition, out slot);
    }

    /// <summary>
    /// Allows an external save/inventory system to bind its data by stable grid ID.
    /// </summary>
    public bool BindInventorySlot(Vector2Int gridPosition, InventoryChipSlot inventorySlot)
    {
        if (!slotsByCoordinate.TryGetValue(gridPosition, out ChipSlot_UI slot))
            return false;

        slot.BindInventorySlot(inventorySlot);
        return true;
    }

    public void LeftButtonSlotClicked(ChipSlot_UI clickedUISlot)
    {
        if (clickedUISlot == null)
            return;

        if (clickedUISlot.IsExpansionSlot)
        {
            TryUnlockExpansionSlot(clickedUISlot);
            return;
        }

        HandleEquipmentSlotClick(clickedUISlot);
    }

    private void TryUnlockExpansionSlot(ChipSlot_UI slot)
    {
        if (!slot.IsExpansionSlot ||
            !slotsByCoordinate.TryGetValue(slot.GridPosition, out ChipSlot_UI registeredSlot) ||
            registeredSlot != slot)
        {
            return;
        }

        int price = GetCurrentUnlockPrice();
        if (!TryPayForUnlock(price))
        {
            UnlockPaymentFailed?.Invoke(price);
            return;
        }

        slot.SetAsUnlockedSlot();
        equipChipSlots.Add(slot);
        paidUnlockCount++;

        // Only the newly unlocked slot reveals its own four neighbours.
        RevealExpansionNeighbours(slot.GridPosition);
        SlotUnlocked?.Invoke(slot.GridPosition, price);
    }

    public int GetCurrentUnlockPrice()
    {
        long calculatedPrice = (long)baseUnlockPrice +
                               (long)paidUnlockCount * unlockPriceIncreasePerSlot;
        return (int)Math.Min(calculatedPrice, int.MaxValue);
    }

    public void SetUnlockPaymentHandler(Func<int, bool> tryPayUnlockPrice)
    {
        unlockPaymentHandler = tryPayUnlockPrice;
    }

    private bool TryPayForUnlock(int amount)
    {
        if (unlockPaymentHandler != null)
            return unlockPaymentHandler(amount);

        if (unlockPricePayer != null)
            return unlockPricePayer.TryPayUnlockPrice(amount);

        Debug.LogError(
            "No unlock payment handler is connected to ChipDisplay. " +
            "Assign an IChipUnlockPricePayer or call SetUnlockPaymentHandler().",
            this);
        return false;
    }

    private void HandleEquipmentSlotClick(ChipSlot_UI clickedUISlot)
    {
        if (clickedUISlot.assigned_Inventory_Slot == null ||
            mouseItemData == null ||
            mouseItemData.assignedInventorySlot == null ||
            chipSystem == null)
        {
            return;
        }

        InventoryChipSlot clickedSlot = clickedUISlot.assigned_Inventory_Slot;
        InventoryChipSlot mouseSlot = mouseItemData.assignedInventorySlot;

        if (clickedSlot._chipData != null && mouseSlot._chipData == null)
        {
            mouseItemData.UpdateMouseSlot(clickedSlot);
            chipSystem.UnequipChip(clickedSlot._chipData);
            clickedUISlot.ClearSlot();
            return;
        }

        if (clickedSlot._chipData == null && mouseSlot._chipData != null)
        {
            clickedSlot.AssignItem(mouseSlot);
            clickedUISlot.UpdateUISlot();
            mouseItemData.ClearSlot();
            chipSystem.EquipChip(clickedSlot._chipData);
            return;
        }

        if (clickedSlot._chipData != null &&
            mouseSlot._chipData != null &&
            clickedSlot._chipData != mouseSlot._chipData)
        {
            SwapSlots(clickedUISlot);
        }
    }

    private void SwapSlots(ChipSlot_UI clickedUISlot)
    {
        InventoryChipSlot clickedSlot = clickedUISlot.assigned_Inventory_Slot;
        InventoryChipSlot mouseSlot = mouseItemData.assignedInventorySlot;

        InventoryChipSlot oldEquippedSlot =
            new InventoryChipSlot(clickedSlot._chipData, clickedSlot._stackSize);

        chipSystem.UnequipChip(clickedSlot._chipData);
        clickedSlot.AssignItem(mouseSlot);
        clickedUISlot.UpdateUISlot();
        chipSystem.EquipChip(clickedSlot._chipData);

        mouseItemData.UpdateMouseSlot(oldEquippedSlot);
    }

    public void ClearAllSlot(ChipData chipData)
    {
        foreach (ChipSlot_UI slot in equipChipSlots)
            slot.UpdateUISlot();
    }

    public ChipSlot_UI CheckInventory(MouseItemData currentMouseItemData)
    {
        if (currentMouseItemData?.assignedInventorySlot == null)
            return null;

        return equipChipSlots.FirstOrDefault(slot =>
            slot.assigned_Inventory_Slot != null &&
            slot.assigned_Inventory_Slot._chipData ==
            currentMouseItemData.assignedInventorySlot._chipData);
    }
}
