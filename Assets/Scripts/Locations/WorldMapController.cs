using UnityEngine;

[DisallowMultipleComponent]
public class WorldMapController : MonoBehaviour
{
    [Header("Locations")]

    [SerializeField]
    [Tooltip("The location markers available on this world map.")]
    private LocationMarker[] locationMarkers;

    [Header("UI")]

    [SerializeField]
    [Tooltip("The panel used to display the currently selected location.")]
    private LocationPanel locationPanel;

    private void OnEnable()
    {
        SubscribeToLocationMarkers();
    }

    private void Start()
    {
        locationPanel.Hide();
    }

    private void OnDisable()
    {
        UnsubscribeFromLocationMarkers();
    }

    private void SubscribeToLocationMarkers()
    {
        foreach (LocationMarker locationMarker in locationMarkers)
        {
            if (locationMarker == null)
            {
                continue;
            }

            locationMarker.Selected += HandleLocationSelected;
        }
    }

    private void UnsubscribeFromLocationMarkers()
    {
        foreach (LocationMarker locationMarker in locationMarkers)
        {
            if (locationMarker == null)
            {
                continue;
            }

            locationMarker.Selected -= HandleLocationSelected;
        }
    }

    private void HandleLocationSelected(
        LocationDefinition location)
    {
        locationPanel.Show(
            location);
    }
}