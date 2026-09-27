using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class LocationMarker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Location")]
    [SerializeField, Tooltip("Location represented by this LocationMarker.")]
    private LocationDefinition location;

    [Header("UI")]
    [SerializeField, Tooltip("Optional permanent marker label. Leave empty for icon-only map hotspots.")]
    private TMP_Text locationNameText;

    [SerializeField, Tooltip("Optional shared hover tooltip.")]
    private LocationMarkerTooltip tooltip;

    private Button button;

    public LocationDefinition Location => location;
    public event Action<LocationDefinition> Selected;

    private void Awake()
    {
        button = GetComponent<Button>();
        UpdateDisplay();
        UpdateInteractableState();
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(HandleButtonClicked);
        UpdateInteractableState();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleButtonClicked);

        if (tooltip != null)
            tooltip.Hide();
    }

    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();

        UpdateDisplay();
        UpdateInteractableState();
    }

    public void SetLocation(LocationDefinition newLocation)
    {
        location = newLocation;
        UpdateDisplay();
        UpdateInteractableState();
    }

    public void SetTooltip(LocationMarkerTooltip newTooltip)
    {
        tooltip = newTooltip;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (location != null && tooltip != null)
            tooltip.Show(location.DisplayName, transform as RectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.Hide();
    }

    private void UpdateDisplay()
    {
        if (locationNameText == null)
            return;

        locationNameText.text = location == null
            ? "Unassigned Location"
            : location.DisplayName;
    }

    private void UpdateInteractableState()
    {
        if (button != null)
        {
            button.interactable =
                location != null &&
                location.HasSetting(LocationSettings.CanTravelTo);
        }
    }

    private void HandleButtonClicked()
    {
        if (location == null)
        {
            Debug.LogWarning(
                $"Location Marker '{name}' was clicked but no Location Definition assigned.",
                this);
            return;
        }

        if (!location.HasSetting(LocationSettings.CanTravelTo))
            return;

        if (tooltip != null)
            tooltip.Hide();

        Selected?.Invoke(location);
    }
}
