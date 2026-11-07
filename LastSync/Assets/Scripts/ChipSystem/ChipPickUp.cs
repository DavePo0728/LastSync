using UnityEngine;

public class ChipPickUp : MonoBehaviour
{
    [SerializeField]
    ChipSelectionController controller;

    void Awake()
    {
        controller = FindAnyObjectByType<ChipSelectionController>(FindObjectsInactive.Include);
    }
    private bool selectionRequested;

    private void OnTriggerEnter(Collider other)
    {
        if (selectionRequested || !other.CompareTag("Player"))
        {
            return;
        }


        if (controller == null)
        {
            Debug.LogWarning(
                "Chip pickup failed: ChipSelectionController is missing.",
                this);
            controller = FindAnyObjectByType<ChipSelectionController>(FindObjectsInactive.Include);
        }
        controller.gameObject.SetActive(true);
        selectionRequested = controller.TryOpenSelection(this);
    }

    public void CompletePickup()
    {
        Destroy(gameObject);
    }
}
