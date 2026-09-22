using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class ItemPickupToast : MonoBehaviour
{
    private static ItemPickupToast instance;

    [Header("Position")]
    [SerializeField] private Vector2 startPosition = new Vector2(-850f, -100f);
    [SerializeField] private Vector2 endPosition = new Vector2(-850f, 0f);

    [Header("Timing")]
    [Min(0f)]
    [SerializeField] private float fadeInDuration = 1f;
    [Min(0f)]
    [SerializeField] private float stayDuration = 1f;
    [Min(0f)]
    [SerializeField] private float fadeOutDuration = 1f;

    private readonly Queue<string> pendingMessages = new Queue<string>();
    private RectTransform rectTransform;
    private TMP_Text toastText;
    private Image bg;
    private Coroutine playRoutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Only one ItemPickupToast can be active at a time.", this);
            enabled = false;
            return;
        }
        bg = transform.GetComponentInChildren<Image>();
        instance = this;
        rectTransform = (RectTransform)transform;
        toastText = GetComponent<TMP_Text>();
        ResetVisualState();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void ShowPickup(string itemName, int amount = 1)
    {
        string displayName = string.IsNullOrWhiteSpace(itemName) ? "Item" : itemName;
        Show($"{displayName} x{Mathf.Max(1, amount)}");
    }

    public static void Show(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        ItemPickupToast toast = GetOrCreateInstance();

        if (toast != null)
            toast.Enqueue(message);
    }

    private static ItemPickupToast GetOrCreateInstance()
    {
        if (instance != null)
            return instance;

        instance = FindAnyObjectByType<ItemPickupToast>(FindObjectsInactive.Include);

        if (instance != null)
            return instance;

        Canvas targetCanvas = FindScreenCanvas();

        if (targetCanvas == null)
        {
            Debug.LogWarning("ItemPickupToast could not find an active screen-space Canvas.");
            return null;
        }

        GameObject toastObject = new GameObject(
            "Item Pickup Toast",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        toastObject.layer = targetCanvas.gameObject.layer;
        toastObject.transform.SetParent(targetCanvas.transform, false);
        toastObject.transform.SetAsLastSibling();

        RectTransform toastRect = (RectTransform)toastObject.transform;
        toastRect.anchorMin = new Vector2(0.5f, 0.5f);
        toastRect.anchorMax = new Vector2(0.5f, 0.5f);
        toastRect.pivot = new Vector2(0f, 0.5f);
        toastRect.anchoredPosition = new Vector2(-850f, -100f);
        toastRect.sizeDelta = new Vector2(650f, 60f);

        TextMeshProUGUI text = toastObject.GetComponent<TextMeshProUGUI>();
        text.raycastTarget = false;
        text.fontSize = 32f;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.overflowMode = TextOverflowModes.Overflow;

        if (TMP_Settings.defaultFontAsset != null)
            text.font = TMP_Settings.defaultFontAsset;

        return toastObject.AddComponent<ItemPickupToast>();
    }

    private static Canvas FindScreenCanvas()
    {
        Canvas fallback = null;
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);

        foreach (Canvas candidate in canvases)
        {
            if (!candidate.isRootCanvas || !candidate.gameObject.activeInHierarchy)
                continue;

            if (candidate.renderMode == RenderMode.ScreenSpaceOverlay)
                return candidate;

            if (candidate.renderMode == RenderMode.ScreenSpaceCamera && fallback == null)
                fallback = candidate;
        }

        return fallback;
    }

    private void Enqueue(string message)
    {
        pendingMessages.Enqueue(message);

        if (playRoutine == null)
            playRoutine = StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        while (pendingMessages.Count > 0)
        {
            toastText.text = pendingMessages.Dequeue();
            toastText.enabled = true;
            rectTransform.anchoredPosition = startPosition;
            SetAlpha(0f);

            yield return AnimateIn();
            yield return WaitUnscaled(stayDuration);
            yield return AnimateOut();

            toastText.enabled = false;
        }

        playRoutine = null;
    }

    private IEnumerator AnimateIn()
    {
        if (fadeInDuration <= 0f)
        {
            rectTransform.anchoredPosition = endPosition;
            SetAlpha(1f);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, progress);
            SetAlpha(progress);
            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;
        SetAlpha(1f);
    }

    private IEnumerator AnimateOut()
    {
        if (fadeOutDuration <= 0f)
        {
            SetAlpha(0f);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(1f - Mathf.Clamp01(elapsed / fadeOutDuration));
            yield return null;
        }

        SetAlpha(0f);
    }

    private static IEnumerator WaitUnscaled(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void ResetVisualState()
    {
        rectTransform.anchoredPosition = startPosition;
        SetAlpha(0f);
        toastText.enabled = false;
    }

    private void SetAlpha(float alpha)
    {
        Color color = toastText.color;
        color.a = alpha;
        toastText.color = color;
    }
}
