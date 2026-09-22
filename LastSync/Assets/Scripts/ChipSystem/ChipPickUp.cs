using UnityEngine;

public class ChipPickUp : MonoBehaviour
{
    [Header("綁定的晶片資料")]
    [SerializeField] private ChipData chipData;

    private void OnTriggerEnter(Collider other)
    {
        // 確認觸碰者是否為玩家 (請確保玩家物件有設定 Player 標籤)
        if (other.CompareTag("Player"))
        {
            if (chipData != null)
            {
                if (!InventoryManager.inventoryManagerInstance.TryAddChip(chipData, 1, out bool isNewChip))
                {
                    Debug.Log(isNewChip);
                    Debug.LogWarning($"[系統] 玩家無法拾取晶片: {chipData.chipID}，背包可能已滿或其他原因。");
                    return; // 如果無法加入背包，則直接返回
                }
                Debug.Log($"[系統] 玩家拾取了晶片: {chipData.chipID}");

                string pickupName = string.IsNullOrWhiteSpace(chipData.chipName)
                    ? $"Chip {chipData.chipID}"
                    : chipData.chipName;
                ItemPickupToast.ShowPickup(pickupName, 1);

                // 實作：可在此處播放拾取音效或特效

                Destroy(gameObject); // 拾取後銷毀場上的掉落物實體
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} 的 ChipData 未設定！");
            }
        }
    }
}
