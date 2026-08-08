using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChipDisplay : MonoBehaviour
{
    [Header("Existing chip equipment dependencies")]
    [SerializeField] private MouseItemData mouseItemData;
    [SerializeField] private ChipSystem chipSystem;
    [SerializeField] private CharacterChipStatusUI characterChipStatusUI;

    [Header("Slot generation")]
    [SerializeField] private GameObject chipSlotPrefab;
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Sprite plusSprite;
    [SerializeField, Min(1)] private int maxRingCount = 3;
    [SerializeField, Min(1f)] private float slotStep = 110f;
    [SerializeField, Min(0f)] private float contentPadding = 20f;

    [Header("Unlock price")]
    [Tooltip("Price of the first paid slot unlock.")]
    [SerializeField, Min(0)] private int baseUnlockPrice = 1;
    [Tooltip("Added after every paid unlock. Use 0 for a fixed price.")]
    [SerializeField, Min(0)] private int unlockPriceIncreasePerSlot;
    [Tooltip("Optional. The assigned component must implement IChipUnlockPricePayer.")]
    [SerializeField] private MonoBehaviour unlockPricePayerSource;

    private readonly Dictionary<Vector2Int, ChipSlot_UI> slotsByCoordinate = new();
    private readonly List<ChipSlot_UI> equipChipSlots = new();
    private readonly HashSet<ChipSlot_UI> pendingExpansionSlots = new();

    private Func<int, bool> unlockPaymentHandler;
    private IChipUnlockPricePayer unlockPricePayer;
    private int currentRing = 1;
    private int nextGroupIndex;
    private int paidUnlockCount;

    public int CurrentRing => currentRing;
    public int PaidUnlockCount => paidUnlockCount;
    public IReadOnlyDictionary<Vector2Int, ChipSlot_UI> SlotsByCoordinate => slotsByCoordinate;

    public event Action<Vector2Int, int> SlotUnlocked;
    public event Action<int> UnlockPaymentFailed;

    private void Awake()
    {
        if (slotContainer == null)
            slotContainer = transform as RectTransform;

        if (scrollRect == null)
            scrollRect = GetComponentInParent<ScrollRect>();

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

        ConfigureScrollRect();
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
        maxRingCount = Mathf.Max(1, maxRingCount);
        slotStep = Mathf.Max(1f, slotStep);
        baseUnlockPrice = Mathf.Max(0, baseUnlockPrice);
        unlockPriceIncreasePerSlot = Mathf.Max(0, unlockPriceIncreasePerSlot);
    }

    private void ConfigureScrollRect()
    {
        if (slotContainer != null)
        {
            slotContainer.anchorMin = new Vector2(0.5f, 0.5f);
            slotContainer.anchorMax = new Vector2(0.5f, 0.5f);
            slotContainer.pivot = new Vector2(0.5f, 0.5f);
        }

        if (scrollRect == null)
            return;

        scrollRect.content = slotContainer;
        scrollRect.horizontal = true;
        scrollRect.vertical = true;
    }

    private void BuildInitialLayout()
    {
        if (chipSlotPrefab == null || slotContainer == null)
        {
            Debug.LogError("ChipDisplay requires a ChipSlot_UIgpt prefab and a slot container.", this);
            return;
        }

        // The centre Image is a separate, non-interactable scene object.
        CreateSlot(Vector2Int.down, false);
        CreateSlot(Vector2Int.left, false);
        CreateSlot(Vector2Int.right, false);
        CreateSlot(Vector2Int.up, false);

        CreateSlot(new Vector2Int(-1, -1), true);
        CreateSlot(new Vector2Int(-1, 1), true);
        CreateSlot(new Vector2Int(1, 1), true);
        CreateSlot(new Vector2Int(1, -1), true);

        UpdateContentSize();
        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0.5f;
            scrollRect.verticalNormalizedPosition = 0.5f;
        }
    }

    private ChipSlot_UI CreateSlot(Vector2Int gridPosition, bool isExpansionSlot)
    {
        if (slotsByCoordinate.TryGetValue(gridPosition, out ChipSlot_UI existingSlot))
            return existingSlot;

        ChipSlot_UI newSlot = Instantiate(chipSlotPrefab, slotContainer).GetComponent<ChipSlot_UI>();
        newSlot.name = $"Chip Slot ({gridPosition.x}, {gridPosition.y})";

        RectTransform slotRect = (RectTransform)newSlot.transform;
        slotRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.anchoredPosition = GridToAnchoredPosition(gridPosition);

        // An empty data slot is prepared now, but it cannot be used while the UI
        // slot remains in its expansion state.
        InventoryChipSlot inventorySlot = new InventoryChipSlot(null, 0);
        newSlot.Configure(this, gridPosition, inventorySlot, isExpansionSlot, plusSprite);

        slotsByCoordinate.Add(gridPosition, newSlot);

        if (isExpansionSlot)
            pendingExpansionSlots.Add(newSlot);
        else
            equipChipSlots.Add(newSlot);

        return newSlot;
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
        if (!pendingExpansionSlots.Contains(slot))
            return;

        int price = GetCurrentUnlockPrice();
        if (!TryPayForUnlock(price))
        {
            UnlockPaymentFailed?.Invoke(price);
            return;
        }

        pendingExpansionSlots.Remove(slot);
        slot.SetAsUnlockedSlot();
        equipChipSlots.Add(slot);
        paidUnlockCount++;

        SlotUnlocked?.Invoke(slot.GridPosition, price);

        if (pendingExpansionSlots.Count == 0)
            SpawnNextExpansionGroup();
    }

    public int GetCurrentUnlockPrice()
    {
        long calculatedPrice = (long)baseUnlockPrice +
                               (long)paidUnlockCount * unlockPriceIncreasePerSlot;
        return (int)Math.Min(calculatedPrice, int.MaxValue);
    }

    /// <summary>
    /// Alternative to IChipUnlockPricePayer when another system wants to bind
    /// its existing TryPayUnlockPrice method from code.
    /// </summary>
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

    private void SpawnNextExpansionGroup()
    {
        if (currentRing == 1)
        {
            if (maxRingCount <= 1)
                return;

            currentRing = 2;
            nextGroupIndex = 0;
        }
        else if (nextGroupIndex >= GetGroupCountForRing(currentRing))
        {
            if (currentRing >= maxRingCount)
                return;

            currentRing++;
            nextGroupIndex = 0;
        }

        SpawnExpansionGroup(currentRing, nextGroupIndex);
        nextGroupIndex++;
        UpdateContentSize();
    }

    private void SpawnExpansionGroup(int ring, int groupIndex)
    {
        Vector2Int point = GetGroupSeed(ring, groupIndex);

        for (int i = 0; i < 4; i++)
        {
            CreateSlot(point, true);
            point = RotateClockwise(point);
        }
    }

    private static int GetGroupCountForRing(int ring)
    {
        // A square ring contains 8 * ring cells. Each group contains four.
        return 2 * ring;
    }

    private static Vector2Int GetGroupSeed(int ring, int groupIndex)
    {
        int groupCount = GetGroupCountForRing(ring);
        if (ring < 1 || groupIndex < 0 || groupIndex >= groupCount)
            throw new ArgumentOutOfRangeException(nameof(groupIndex));

        // Begin at the bottom midpoint, move clockwise to the bottom-left
        // corner, then continue upwards towards the left midpoint. Rotating this
        // seed supplies the other three evenly distributed cells.
        if (groupIndex <= ring)
            return new Vector2Int(-groupIndex, -ring);

        return new Vector2Int(-ring, groupIndex - (2 * ring));
    }

    private static Vector2Int RotateClockwise(Vector2Int point)
    {
        return new Vector2Int(point.y, -point.x);
    }

    private void UpdateContentSize()
    {
        if (slotContainer == null || chipSlotPrefab == null)
            return;

        RectTransform prefabRect = (RectTransform)chipSlotPrefab.transform;
        Vector2 slotSize = prefabRect.rect.size;
        Vector2 requiredSize = new Vector2(
            currentRing * 2f * slotStep + slotSize.x + contentPadding * 2f,
            currentRing * 2f * slotStep + slotSize.y + contentPadding * 2f);

        if (scrollRect != null)
        {
            RectTransform viewport = scrollRect.viewport != null
                ? scrollRect.viewport
                : scrollRect.transform as RectTransform;

            if (viewport != null)
            {
                requiredSize.x = Mathf.Max(requiredSize.x, viewport.rect.width);
                requiredSize.y = Mathf.Max(requiredSize.y, viewport.rect.height);
            }
        }

        slotContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requiredSize.x);
        slotContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredSize.y);
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
        // ChipSystem owns the actual equipment data. ChipDisplay only refreshes
        // the visuals after that data changes.
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

