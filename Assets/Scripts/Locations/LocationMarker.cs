using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class LocationMarker : MonoBehaviour
{
    [Header("Location")]

    [SerializeField]
    [Tooltip("Location represented by this LocationMarker.")]
    private LocationDefinition location;

    [Header("UI")]

    [SerializeField]
    [Tooltip("Text used to display the location's name on the world map.")]
    private TMP_Text locationNameText;

    private Button button;

    public LocationDefinition Location => location;

    public event Action<LocationDefinition> Selected;

    private void Awake()
    {
        button = GetComponent<Button>();

        UpdateDisplay();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(
            HandleButtonClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(
            HandleButtonClicked);
    }

    private void OnValidate()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (locationNameText == null)
        {
            locationNameText =
                GetComponentInChildren<TMP_Text>();
        }

        if (locationNameText == null)
        {
            return;
        }

        if (location == null)
        {
            locationNameText.text =
                "Unassigned Location";

            return;
        }

        locationNameText.text =
            location.DisplayName;
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

        Selected?.Invoke(
            location);
    }
}