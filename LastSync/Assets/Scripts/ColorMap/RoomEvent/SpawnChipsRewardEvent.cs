using UnityEngine;

public class SpawnChipsRewardEvent : RoomEvent
{
    [SerializeField]
    private GameObject chipPrefab;

    private bool hasSpawned;

    public void SetChipPrefab(GameObject prefab)
    {
        chipPrefab = prefab;
    }

    public override void Execute()
    {
        if (hasSpawned)
        {
            Finish();
            return;
        }

        if (chipPrefab == null)
        {
            Debug.LogWarning(
                "SpawnChipsRewardEvent failed: chip reward prefab is missing.",
                this);
            Finish();
            return;
        }

        Transform roomTransform = Manager?.Room?.Transform;

        if (roomTransform == null)
        {
            Debug.LogWarning(
                "SpawnChipsRewardEvent failed: room transform is missing.",
                this);
            Finish();
            return;
        }

        float verticalOffset = Mathf.Max(
            0f,
            chipPrefab.transform.localPosition.y);

        Vector3 spawnPosition =
            roomTransform.position + roomTransform.up * verticalOffset;
        Quaternion spawnRotation =
            roomTransform.rotation * chipPrefab.transform.localRotation;

        Instantiate(
            chipPrefab,
            spawnPosition,
            spawnRotation,
            roomTransform);

        hasSpawned = true;
        Finish();
    }
}
