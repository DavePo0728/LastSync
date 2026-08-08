using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChipSlot_UI : MonoBehaviour
{
    [SerializeField] private Image itemSprite;
    [SerializeField] private Button button;
    [SerializeField] private InventoryChipSlot assignedInventorySlot;

    public InventoryChipSlot assigned_Inventory_Slot => assignedInventorySlot;
    public ChipDisplay chipDisplay { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public bool IsExpansionSlot { get; private set; }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (chipDisplay == null)
            chipDisplay = GetComponentInParent<ChipDisplay>();

        button.onClick.AddListener(HandleButtonClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleButtonClicked);
    }

    /// <summary>
    /// Used by ChipDisplay for slots that it creates at runtime.
    /// </summary>
    public void Configure(
        ChipDisplay owner,
        Vector2Int gridPosition,
        InventoryChipSlot inventorySlot,
        bool isExpansionSlot,
        Sprite plusSprite)
    {
        chipDisplay = owner;
        GridPosition = gridPosition;
        assignedInventorySlot = inventorySlot;

        if (isExpansionSlot)
            SetAsExpansionSlot(plusSprite);
        else
            SetAsUnlockedSlot();
    }

    /// <summary>
    /// Kept for compatibility with the original inventory binding code.
    /// Initializing UI must not clear the supplied inventory data.
    /// </summary>
    public void Init(InventoryChipSlot slot)
    {
        assignedInventorySlot = slot;
        SetAsUnlockedSlot();
        UpdateUISlot();
    }

    public void BindInventorySlot(InventoryChipSlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot();
    }

    public void SetAsExpansionSlot(Sprite plusSprite)
    {
        IsExpansionSlot = true;
        button.interactable = true;
        itemSprite.sprite = plusSprite;
        itemSprite.color = Color.white;
    }

    public void SetAsUnlockedSlot()
    {
        IsExpansionSlot = false;
        button.interactable = true;
        UpdateUISlot();
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }

    private void HandleButtonClicked()
    {
        if (button == null || !button.interactable)
            return;

        chipDisplay?.LeftButtonSlotClicked(this);
    }

    public void UpdateUISlot(InventoryChipSlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot();
    }

    public void UpdateUISlot()
    {
        if (IsExpansionSlot)
            return;

        if (assignedInventorySlot != null && assignedInventorySlot._chipData != null)
        {
            itemSprite.sprite = assignedInventorySlot._chipData.icon;
            itemSprite.color = Color.white;
            return;
        }

        itemSprite.sprite = null;
        itemSprite.color = Color.white;
    }

    public void ClearSlot()
    {
        if (IsExpansionSlot)
            return;

        assignedInventorySlot?.ClearSlot();
        itemSprite.sprite = null;
        itemSprite.color = Color.white;
    }
}
