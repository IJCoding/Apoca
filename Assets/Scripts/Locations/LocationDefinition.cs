using System;
using UnityEngine;

[Flags]
public enum LocationSettings
{
    None = 0,

    ShowOnMap = 1 << 0,
    CanTravelTo = 1 << 1,
    RequiresTimeToVisit = 1 << 2,
    CanRestHere = 1 << 3
}

[CreateAssetMenu(
    fileName = "LocationDefinition",
    menuName = "Game/Locations/Location Definition")]
public class LocationDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    [Tooltip("A stable identifier used to distinguish this location from every other location.")]
    private string locationId;

    [SerializeField]
    [Tooltip("The name displayed to the player.")]
    private string displayName;

    [Header("Description")]

    [SerializeField]
    [TextArea(3, 8)]
    [Tooltip("The description displayed when the player inspects this location.")]
    private string description;

    [Header("Settings")]

    [SerializeField]
    [Tooltip("Defines the behaviours supported by this location.")]
    private LocationSettings settings =
        LocationSettings.ShowOnMap |
        LocationSettings.CanTravelTo;

    [Header("Actions")]

    [SerializeField]
    [Tooltip("The actions that can be performed at this location.")]
    private LocationActionDefinition[] actions;

    public string LocationId => locationId;

    public string DisplayName => displayName;

    public string Description => description;

    public LocationSettings Settings => settings;

    public LocationActionDefinition[] Actions => actions;

    public bool HasSetting(
        LocationSettings setting)
    {
        return (settings & setting) == setting;
    }
}