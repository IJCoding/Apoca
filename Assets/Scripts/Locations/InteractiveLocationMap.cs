using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InteractiveLocationMap : MonoBehaviour
{
    [Header("Definition")]

    [SerializeField]
    [Tooltip(
        "Definition containing the visual and its polygon hotspots.")]
    private InteractiveVisualDefinition visualDefinition;

    [Header("Visual")]

    [SerializeField]
    [Tooltip(
        "Image used to display the interactive visual.")]
    private Image visualImage;

    [Header("Hotspots")]

    [SerializeField]
    [Tooltip(
        "RectTransform containing the generated polygon hotspots.")]
    private RectTransform hotspotContainer;

    [SerializeField]
    [Tooltip(
        "Shared tooltip displayed while hovering over a hotspot.")]
    private LocationMarkerTooltip tooltip;

    [Header("Normal Hotspot Display")]

    [SerializeField]
    [Tooltip(
        "Fill color shown on hotspots normally.")]
    private Color normalHotspotFillColor =
        new Color(
            0.1f,
            0.35f,
            0.65f,
            0.18f);

    [SerializeField]
    [Tooltip(
        "Outline color shown on hotspots normally.")]
    private Color normalHotspotOutlineColor =
        new Color(
            0.3f,
            0.7f,
            1f,
            0.75f);

    [SerializeField]
    [Min(0f)]
    [Tooltip(
        "Normal hotspot outline thickness.")]
    private float normalHotspotOutlineThickness =
        2f;

    [Header("Hovered Hotspot Display")]

    [SerializeField]
    [Tooltip(
        "Fill color shown while the pointer is over a hotspot.")]
    private Color hoverHotspotFillColor =
        new Color(
            0.2f,
            0.65f,
            1f,
            0.32f);

    [SerializeField]
    [Tooltip(
        "Outline color shown while the pointer is over a hotspot.")]
    private Color hoverHotspotOutlineColor =
        new Color(
            0.65f,
            0.9f,
            1f,
            1f);

    [SerializeField]
    [Min(0f)]
    [Tooltip(
        "Outline thickness while hovering.")]
    private float hoverHotspotOutlineThickness =
        4f;

    [Header("Hotspot Debug Display")]

    [SerializeField]
    [Tooltip(
        "Overrides normal and hover visuals with diagnostic colors.")]
    private bool showHotspotDebug;

    [SerializeField]
    private Color hotspotDebugFillColor =
        new Color(
            1f,
            0.1f,
            0.05f,
            0.35f);

    [SerializeField]
    private Color hotspotDebugOutlineColor =
        new Color(
            1f,
            0.85f,
            0.05f,
            1f);

    [SerializeField]
    [Min(0f)]
    private float hotspotDebugOutlineThickness =
        4f;

    [Header("Legacy Location Buttons")]

    [SerializeField]
    private GameObject legacyLocationButtonsRoot;

    [SerializeField]
    private bool showLegacyLocationButtons;

    [Header("Destination")]

    [SerializeField]
    private LocationPanel locationPanel;

    private readonly List<InteractiveVisualHotspot>
        runtimeHotspots =
            new List<InteractiveVisualHotspot>();

    private void Awake()
    {
        if (locationPanel == null)
        {
            locationPanel =
                FindFirstObjectByType<LocationPanel>();
        }

        Rebuild();
        ApplyLegacyButtonVisibility();
    }

    private void OnEnable()
    {
        ApplyLegacyButtonVisibility();

        if (Application.isPlaying)
        {
            RefreshHotspotAppearance();
        }
    }

    private void OnDisable()
    {
        if (tooltip != null)
        {
            tooltip.Hide();
        }
    }

    private void OnValidate()
    {
        normalHotspotOutlineThickness =
            Mathf.Max(
                0f,
                normalHotspotOutlineThickness);

        hoverHotspotOutlineThickness =
            Mathf.Max(
                0f,
                hoverHotspotOutlineThickness);

        hotspotDebugOutlineThickness =
            Mathf.Max(
                0f,
                hotspotDebugOutlineThickness);

        ApplyLegacyButtonVisibility();

        if (!Application.isPlaying)
        {
            ApplyVisual();
        }
        else
        {
            RefreshHotspotAppearance();
        }
    }

    public void SetDefinition(
        InteractiveVisualDefinition definition)
    {
        visualDefinition =
            definition;

        Rebuild();
    }

    public void SetLegacyLocationButtonsVisible(
        bool visible)
    {
        showLegacyLocationButtons =
            visible;

        ApplyLegacyButtonVisibility();
    }

    public void SetHotspotDebugVisible(
        bool visible)
    {
        showHotspotDebug =
            visible;

        RefreshHotspotAppearance();
    }

    public void Rebuild()
    {
        ApplyVisual();
        ClearRuntimeHotspots();

        if (visualDefinition == null)
        {
            Debug.LogWarning(
                $"Interactive Location Map '{name}' has no Interactive Visual Definition assigned.",
                this);

            return;
        }

        if (hotspotContainer == null)
        {
            Debug.LogWarning(
                $"Interactive Location Map '{name}' has no Hotspot Container assigned.",
                this);

            return;
        }

        IReadOnlyList<
            InteractiveVisualDefinition.Hotspot>
            definitions =
                visualDefinition.Hotspots;

        for (int i = 0;
             i < definitions.Count;
             i++)
        {
            InteractiveVisualDefinition.Hotspot
                definition =
                    definitions[i];

            if (definition == null ||
                definition.Points == null ||
                definition.Points.Count < 3)
            {
                continue;
            }

            CreateRuntimeHotspot(
                definition,
                i);
        }

        RefreshHotspotAppearance();
    }

    private void CreateRuntimeHotspot(
        InteractiveVisualDefinition.Hotspot definition,
        int index)
    {
        GameObject hotspotObject =
            new GameObject(
                $"Hotspot_{index}_{definition.DisplayName}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(PolygonRaycastGraphic),
                typeof(Button),
                typeof(InteractiveVisualHotspot));

        RectTransform rectTransform =
            hotspotObject.GetComponent<
                RectTransform>();

        rectTransform.SetParent(
            hotspotContainer,
            false);

        rectTransform.anchorMin =
            Vector2.zero;

        rectTransform.anchorMax =
            Vector2.one;

        rectTransform.pivot =
            new Vector2(
                0.5f,
                0.5f);

        rectTransform.offsetMin =
            Vector2.zero;

        rectTransform.offsetMax =
            Vector2.zero;

        PolygonRaycastGraphic polygon =
            hotspotObject.GetComponent<
                PolygonRaycastGraphic>();

        polygon.raycastTarget =
            true;

        polygon.SetPoints(
            definition.Points);

        ApplyAppearance(
            polygon);

        Button button =
            hotspotObject.GetComponent<Button>();

        button.targetGraphic =
            polygon;

        button.transition =
            Selectable.Transition.None;

        button.interactable =
            definition.Location != null &&
            definition.Location.HasSetting(
                LocationSettings.CanTravelTo);

        InteractiveVisualHotspot hotspot =
            hotspotObject.GetComponent<
                InteractiveVisualHotspot>();

        hotspot.Configure(
            definition,
            tooltip);

        hotspot.Selected +=
            HandleLocationSelected;

        runtimeHotspots.Add(
            hotspot);
    }

    private void RefreshHotspotAppearance()
    {
        for (int i = 0;
             i < runtimeHotspots.Count;
             i++)
        {
            InteractiveVisualHotspot hotspot =
                runtimeHotspots[i];

            if (hotspot == null)
                continue;

            PolygonRaycastGraphic polygon =
                hotspot.GetComponent<
                    PolygonRaycastGraphic>();

            if (polygon == null)
                continue;

            ApplyAppearance(
                polygon);
        }
    }

    private void ApplyAppearance(
        PolygonRaycastGraphic polygon)
    {
        polygon.SetNormalDisplay(
            normalHotspotFillColor,
            normalHotspotOutlineColor,
            normalHotspotOutlineThickness);

        polygon.SetHoverDisplay(
            hoverHotspotFillColor,
            hoverHotspotOutlineColor,
            hoverHotspotOutlineThickness);

        polygon.SetDebugDisplay(
            showHotspotDebug,
            hotspotDebugFillColor,
            hotspotDebugOutlineColor,
            hotspotDebugOutlineThickness);
    }

    private void ApplyVisual()
    {
        if (visualImage == null)
            return;

        Sprite sprite =
            visualDefinition != null
                ? visualDefinition.Visual
                : null;

        visualImage.sprite =
            sprite;

        visualImage.enabled =
            sprite != null;

        visualImage.raycastTarget =
            false;
    }

    private void ClearRuntimeHotspots()
    {
        for (int i =
                 runtimeHotspots.Count - 1;
             i >= 0;
             i--)
        {
            InteractiveVisualHotspot hotspot =
                runtimeHotspots[i];

            if (hotspot == null)
                continue;

            hotspot.Selected -=
                HandleLocationSelected;

            if (Application.isPlaying)
            {
                Destroy(
                    hotspot.gameObject);
            }
            else
            {
                DestroyImmediate(
                    hotspot.gameObject);
            }
        }

        runtimeHotspots.Clear();

        if (hotspotContainer == null)
            return;

        for (int i =
                 hotspotContainer.childCount - 1;
             i >= 0;
             i--)
        {
            Transform child =
                hotspotContainer.GetChild(i);

            if (child.GetComponent<
                    InteractiveVisualHotspot>() ==
                null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(
                    child.gameObject);
            }
            else
            {
                DestroyImmediate(
                    child.gameObject);
            }
        }
    }

    private void HandleLocationSelected(
        LocationDefinition location)
    {
        if (location == null)
            return;

        if (!location.HasSetting(
                LocationSettings.CanTravelTo))
        {
            return;
        }

        if (tooltip != null)
        {
            tooltip.Hide();
        }

        if (locationPanel != null)
        {
            locationPanel.Show(
                location);
        }
    }

    private void ApplyLegacyButtonVisibility()
    {
        if (legacyLocationButtonsRoot != null)
        {
            legacyLocationButtonsRoot.SetActive(
                showLegacyLocationButtons);
        }
    }
}