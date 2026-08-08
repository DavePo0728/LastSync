using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot_UI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image chipIcon;
    [SerializeField] private TMP_Text chipNameText;
    [SerializeField] private TMP_Text chipValuesText;
    [SerializeField] private TMP_Text chipStackSizeText;
    [SerializeField] private InventoryChipSlot assignedInventorySlot;

    public StaticInventoryDisplay staticInventoryDisplay { get; private set; }
    public InventoryChipSlot assigned_Inventory_Slot => assignedInventorySlot;

    private void Awake()
    {
        ClearVisuals();
        staticInventoryDisplay = GetComponentInParent<StaticInventoryDisplay>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            staticInventoryDisplay?.LeftButtonSlotClicked(this);

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Reserved for per-slot right-click behaviour.
        }
    }

    public void Init(InventoryChipSlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot();
    }

    public void UpdateUISlot(InventoryChipSlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot();
    }

    public void UpdateUISlot()
    {
        if (assignedInventorySlot != null &&
            assignedInventorySlot._chipData != null &&
            assignedInventorySlot._stackSize > 0)
        {
            ChipData chipData = assignedInventorySlot._chipData;
            chipIcon.sprite = chipData.icon;
            chipIcon.color = Color.white;
            chipNameText.text = chipData.chipName;
            chipValuesText.text = chipData.getChipValuesFromModifiers();
            chipStackSizeText.text = $"x{assignedInventorySlot._stackSize}";
            return;
        }

        ClearVisuals();
    }

    /// <summary>
    /// Clears both the inventory data and its visuals.
    /// </summary>
    public void ClearSlot()
    {
        assignedInventorySlot?.ClearSlot();
        ClearVisuals();
    }

    public void ClearOldSlot()
    {
        ClearSlot();
    }

    private void ClearVisuals()
    {
        chipIcon.sprite = null;
        chipIcon.color = Color.clear;
        chipNameText.text = string.Empty;
        chipValuesText.text = string.Empty;
        chipStackSizeText.text = string.Empty;
    }
}
