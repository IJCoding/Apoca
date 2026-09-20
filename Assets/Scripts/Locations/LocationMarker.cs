using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class LocationMarker : MonoBehaviour
{
    [Header("Location")]
    [SerializeField]
    [Tooltip("Location represented by this LocationMarker")]
    private LocationDefinition location;

    private Button button;
    public LocationDefinition Location => location;
    public event Action<LocationDefinition> Selected;
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(HandleButtonClicked);
    }
    private void OnDisable()
    {
        button.onClick.RemoveListener(HandleButtonClicked);
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

        Selected?.Invoke(location);
    }

}
