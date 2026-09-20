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

    public string ActionId => actionId;
    public string DisplayName => displayName;
    public string Description => description;
}