using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ChipSelectOptionView : MonoBehaviour
{
    [SerializeField]
    private Button button;
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TMP_Text chipNameText;
    [SerializeField]
    private TMP_Text descriptionText;

    private ChipData chipData;
    private Action<ChipData> selected;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Reset()
    {
        ResolveReferences();
    }

    public void Bind(
        ChipData data,
        Action<ChipData> onSelected)
    {
        ResolveReferences();

        chipData = data;
        selected = onSelected;

        if (icon != null)
        {
            icon.sprite = data != null ? data.icon : null;
        }

        if (chipNameText != null)
        {
            chipNameText.text = data == null
                ? string.Empty
                : string.IsNullOrWhiteSpace(data.chipName)
                    ? $"Chip {data.chipID}"
                    : data.chipName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data == null
                ? string.Empty
                : data.getChipValuesFromModifiers();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.onClick.AddListener(HandleClick);
            button.interactable = data != null;
        }
    }

    public void Clear()
    {
        chipData = null;
        selected = null;

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
            button.interactable = false;
        }
    }

    private void HandleClick()
    {
        if (chipData != null)
        {
            selected?.Invoke(chipData);
        }
    }

    private void ResolveReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (icon == null)
        {
            icon = transform.Find("ChipIcon")?.GetComponent<Image>();
        }

        if (chipNameText == null)
        {
            chipNameText = transform.Find("ChipName")?.GetComponent<TMP_Text>();
        }

        if (descriptionText == null)
        {
            descriptionText = transform.Find("ChipStatusDescription")
                ?.GetComponent<TMP_Text>();
        }
    }
}
