using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AImUIFollow : MonoBehaviour
{
    private RectTransform crosshairRect;
    private Canvas parentCanvas;
    Image aimImage;
    [SerializeField] private WeaponBase targetWeapon;
    void Start()
    {
        crosshairRect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        Cursor.visible = false;
        aimImage = GetComponentInParent<Image>();

    }

    void Update()
    {
        // 1. Read the raw mouse screen position using the New Input System API
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // 2. Convert the screen point safely into local Canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            mouseScreenPos,
            parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera,
            out Vector2 localPoint
        );

        // 3. Update the UI Image position
        crosshairRect.anchoredPosition = localPoint;
        if (targetWeapon != null && aimImage != null)
        {
            // 將武器的冷卻進度賦值給 UI 的 Fill Amount
            aimImage.fillAmount = targetWeapon.FireCooldownRatio;
        }
    }
    public void SetTargetWeapon(WeaponBase newWeapon)
    {
        targetWeapon = newWeapon;

        // 切換武器時重置 UI 顯示
        if (aimImage != null)
        {
            aimImage.fillAmount = targetWeapon != null ? targetWeapon.FireCooldownRatio : 0f;
        }
    }
}
