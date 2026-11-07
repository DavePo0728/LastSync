using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ChipSelectionController : MonoBehaviour
{
    private const int ChoiceCount = 3;

    public static ChipSelectionController Instance { get; private set; }

    [SerializeField]
    private List<ChipData> chipPool = new();

    private ChipSelectOptionView[] optionViews;
    private ChipPickUp activePickup;
    private bool selectionOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "Only one ChipSelectionController can be active in a scene.",
                this);
            enabled = false;
            return;
        }

        Instance = this;
        ResolveOptionViews();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool TryOpenSelection(ChipPickUp pickup)
    {
        if (selectionOpen || pickup == null)
        {
            return false;
        }

        ResolveOptionViews();

        if (optionViews.Length != ChoiceCount)
        {
            Debug.LogError(
                $"ChipSelectPanel requires exactly {ChoiceCount} ChipSelectOptionView components, " +
                $"but found {optionViews.Length}.",
                this);
            return false;
        }

        List<ChipData> candidates = BuildUniqueCandidates();

        if (candidates.Count < ChoiceCount)
        {
            Debug.LogError(
                $"Chip selection requires at least {ChoiceCount} unique ChipData entries, " +
                $"but found {candidates.Count}.",
                this);
            return false;
        }

        activePickup = pickup;
        selectionOpen = true;

        for (int i = 0; i < ChoiceCount; i++)
        {
            int randomIndex = Random.Range(i, candidates.Count);
            (candidates[i], candidates[randomIndex]) =
                (candidates[randomIndex], candidates[i]);

            optionViews[i].Bind(candidates[i], SelectChip);
        }

        gameObject.SetActive(true);
        return true;
    }

    private void SelectChip(ChipData chipData)
    {
        if (!selectionOpen || chipData == null)
        {
            return;
        }

        InventoryManager inventoryManager =
            InventoryManager.inventoryManagerInstance;

        if (inventoryManager == null)
        {
            Debug.LogWarning(
                "Chip selection failed: InventoryManager is missing.",
                this);
            return;
        }

        if (!inventoryManager.TryAddChip(
                chipData,
                1,
                out bool isNewChip))
        {
            Debug.LogWarning(
                $"[系統] 玩家無法取得晶片: {chipData.chipID}，背包可能已滿。",
                this);
            return;
        }

        Debug.Log(
            $"[系統] 玩家選擇了晶片: {chipData.chipID}，新晶片: {isNewChip}",
            this);

        string pickupName = string.IsNullOrWhiteSpace(chipData.chipName)
            ? $"Chip {chipData.chipID}"
            : chipData.chipName;

        ItemPickupToast.ShowPickup(pickupName, 1);

        ChipPickUp completedPickup = activePickup;
        activePickup = null;
        selectionOpen = false;

        foreach (ChipSelectOptionView optionView in optionViews)
        {
            optionView.Clear();
        }

        gameObject.SetActive(false);

        if (completedPickup != null)
        {
            completedPickup.CompletePickup();
        }
    }

    private void ResolveOptionViews()
    {
        optionViews = GetComponentsInChildren<ChipSelectOptionView>(true);

        System.Array.Sort(
            optionViews,
            (left, right) => left.transform.GetSiblingIndex()
                .CompareTo(right.transform.GetSiblingIndex()));
    }

    private List<ChipData> BuildUniqueCandidates()
    {
        List<ChipData> candidates = new();
        HashSet<int> chipIds = new();

        foreach (ChipData chipData in chipPool)
        {
            if (chipData == null || !chipIds.Add(chipData.chipID))
            {
                continue;
            }

            candidates.Add(chipData);
        }

        return candidates;
    }
}
