using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(PolygonRaycastGraphic))]
public class InteractiveVisualHotspot :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    private InteractiveVisualDefinition.Hotspot
        definition;

    private LocationMarkerTooltip tooltip;
    private Button button;
    private PolygonRaycastGraphic polygonGraphic;

    public LocationDefinition Location =>
        definition != null
            ? definition.Location
            : null;

    public event Action<LocationDefinition>
        Selected;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        CacheComponents();

        if (button != null)
        {
            button.onClick.AddListener(
                HandleClicked);
        }
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                HandleClicked);
        }

        if (polygonGraphic != null)
        {
            polygonGraphic.SetHovered(
                false);
        }

        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    public void Configure(
        InteractiveVisualDefinition.Hotspot hotspotDefinition,
        LocationMarkerTooltip sharedTooltip)
    {
        definition =
            hotspotDefinition;

        tooltip =
            sharedTooltip;

        CacheComponents();
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (polygonGraphic != null)
        {
            polygonGraphic.SetHovered(
                true);
        }

        if (tooltip == null ||
            definition == null)
        {
            return;
        }

        string displayName =
            definition.DisplayName;

        if (string.IsNullOrWhiteSpace(
                displayName))
        {
            return;
        }

        tooltip.Show(
            displayName,
            transform as RectTransform);
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        if (polygonGraphic != null)
        {
            polygonGraphic.SetHovered(
                false);
        }

        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    private void CacheComponents()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (polygonGraphic == null)
        {
            polygonGraphic =
                GetComponent<
                    PolygonRaycastGraphic>();
        }
    }

    private void HandleClicked()
    {
        if (definition == null)
            return;

        LocationDefinition location =
            definition.Location;

        if (location == null)
            return;

        if (!location.HasSetting(
                LocationSettings.CanTravelTo))
        {
            return;
        }

        Selected?.Invoke(
            location);
    }
}