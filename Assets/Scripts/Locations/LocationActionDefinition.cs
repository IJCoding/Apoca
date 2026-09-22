using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LocationActionDefinition",
    menuName = "Game/Locations/Location Action Definition")]
public class LocationActionDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    [Tooltip("A stable identifier used to distinguish this action from other actions.")]
    private string actionId;

    [SerializeField]
    [Tooltip("The text displayed to the player when this action is presented as a choice.")]
    private string displayName;

    [Header("Narrative")]

    [SerializeField]
    [TextArea(3, 8)]
    [Tooltip("The narrative text displayed when the player selects this action.")]
    private string description;

    [Header("Follow-Up Actions")]

    [SerializeField]
    [Tooltip("The actions presented after this action has been selected.")]
    private LocationActionDefinition[] followUpActions =
        Array.Empty<LocationActionDefinition>();

    public string ActionId => actionId;

    public string DisplayName => displayName;

    public string Description => description;

    public LocationActionDefinition[] FollowUpActions =>
        followUpActions;
}