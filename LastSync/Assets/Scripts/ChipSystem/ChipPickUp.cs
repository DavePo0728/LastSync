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
                InventoryManager.inventoryManagerInstance.AddToInventory(chipData,1);
                Debug.Log($"[系統] 玩家拾取並裝備了晶片: {chipData.chipID}");

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
