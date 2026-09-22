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
    [Tooltip("The narrative text displayed when this action does not require a move.")]
    private string description;

    [Header("Move")]

    [SerializeField]
    [Tooltip("Optional move resolved when this action is selected. Leave empty if this action does not require a move.")]
    private MoveDefinition move;

    [SerializeField]
    [TextArea(3, 8)]
    [Tooltip("Narrative displayed when the move produces a Strong Hit.")]
    private string strongHitText;

    [SerializeField]
    [TextArea(3, 8)]
    [Tooltip("Narrative displayed when the move produces a Weak Hit.")]
    private string weakHitText;

    [SerializeField]
    [TextArea(3, 8)]
    [Tooltip("Narrative displayed when the move produces a Miss.")]
    private string missText;

    [Header("Standard Follow-Up Actions")]

    [SerializeField]
    [Tooltip("Actions presented after this action when no move is required.")]
    private LocationActionDefinition[] followUpActions =
        Array.Empty<LocationActionDefinition>();

    [Header("Move Result Follow-Up Actions")]

    [SerializeField]
    [Tooltip("Actions presented after a Strong Hit.")]
    private LocationActionDefinition[] strongHitFollowUpActions =
        Array.Empty<LocationActionDefinition>();

    [SerializeField]
    [Tooltip("Actions presented after a Weak Hit.")]
    private LocationActionDefinition[] weakHitFollowUpActions =
        Array.Empty<LocationActionDefinition>();

    [SerializeField]
    [Tooltip("Actions presented after a Miss.")]
    private LocationActionDefinition[] missFollowUpActions =
        Array.Empty<LocationActionDefinition>();

    public string ActionId => actionId;

    public string DisplayName => displayName;

    public string Description => description;

    public MoveDefinition Move => move;

    public bool RequiresMove => move != null;

    public string StrongHitText => strongHitText;

    public string WeakHitText => weakHitText;

    public string MissText => missText;

    public LocationActionDefinition[] FollowUpActions =>
        followUpActions;

    public string GetResultText(
        MoveResult result)
    {
        switch (result)
        {
            case MoveResult.StrongHit:
                return strongHitText;

            case MoveResult.WeakHit:
                return weakHitText;

            case MoveResult.Miss:
                return missText;

            default:
                Debug.LogWarning(
                    $"LocationActionDefinition '{name}' received unsupported MoveResult '{result}'.",
                    this);

                return string.Empty;
        }
    }

    public LocationActionDefinition[] GetFollowUpActions(
        MoveResult result)
    {
        switch (result)
        {
            case MoveResult.StrongHit:
                return strongHitFollowUpActions;

            case MoveResult.WeakHit:
                return weakHitFollowUpActions;

            case MoveResult.Miss:
                return missFollowUpActions;

            default:
                Debug.LogWarning(
                    $"LocationActionDefinition '{name}' received unsupported MoveResult '{result}'.",
                    this);

                return Array.Empty<LocationActionDefinition>();
        }
    }
}