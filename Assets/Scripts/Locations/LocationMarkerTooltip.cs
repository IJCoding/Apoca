using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class LocationMarkerTooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private Vector2 offset = new Vector2(0f, 36f);

    private Canvas rootCanvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        EnsureReferences();
        Hide();
    }

    public void Show(string text, RectTransform target)
    {
        EnsureReferences();

        if (label == null || tooltipRect == null || target == null)
            return;

        label.text = string.IsNullOrWhiteSpace(text) ? "Unnamed Location" : text;
        gameObject.SetActive(true);
        PositionBeside(target);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void EnsureReferences()
    {
        if (tooltipRect == null)
            tooltipRect = transform as RectTransform;

        if (label == null)
            label = GetComponentInChildren<TMP_Text>(true);

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas != null)
            canvasRect = rootCanvas.transform as RectTransform;
    }

    private void PositionBeside(RectTransform target)
    {
        if (canvasRect == null || tooltipRect == null)
            return;

        Camera camera = rootCanvas != null && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? rootCanvas.worldCamera
            : null;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, target.position);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPoint, camera, out Vector2 localPoint))
        {
            tooltipRect.anchoredPosition = localPoint + offset;
            tooltipRect.SetAsLastSibling();
        }
    }
}
