using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LocationActionDefinition",
    menuName = "Game/Locations/Location Action Definition")]
public class LocationActionDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    private string actionId;

    [SerializeField]
    private string displayName;

    [SerializeField]
    private ActionApproach approach = ActionApproach.None;

    [Header("Narrative")]

    [SerializeField]
    [TextArea(2, 8)]
    private string description;

    [Header("Requirements")]

    [SerializeField]
    [Tooltip("When enabled, this action is completely hidden while any requirement is unmet. When disabled, the action remains visible but unavailable.")]
    private bool hideWhenRequirementsNotMet;

    [SerializeReference]
    private List<GameCondition> requirements =
        new List<GameCondition>();

    [Header("Effects")]

    [SerializeReference]
    private List<GameEffect> effects =
        new List<GameEffect>();

    [Header("Move")]

    [SerializeField]
    private MoveDefinition move;

    [Header("Move Result Text")]

    [SerializeField]
    [TextArea(2, 8)]
    private string strongHitText;

    [SerializeField]
    [TextArea(2, 8)]
    private string weakHitText;

    [SerializeField]
    [TextArea(2, 8)]
    private string missText;

    [Header("Move Result Effects")]

    [SerializeReference]
    private List<GameEffect> strongHitEffects =
        new List<GameEffect>();

    [SerializeReference]
    private List<GameEffect> weakHitEffects =
        new List<GameEffect>();

    [SerializeReference]
    private List<GameEffect> missEffects =
        new List<GameEffect>();

    [Header("Follow-Up Actions")]

    [SerializeField]
    private LocationActionDefinition[] followUpActions =
        Array.Empty<LocationActionDefinition>();

    [SerializeField]
    private LocationActionDefinition[] strongHitFollowUpActions =
        Array.Empty<LocationActionDefinition>();

    [SerializeField]
    private LocationActionDefinition[] weakHitFollowUpActions =
        Array.Empty<LocationActionDefinition>();

    [SerializeField]
    private LocationActionDefinition[] missFollowUpActions =
        Array.Empty<LocationActionDefinition>();

    public string ActionId => actionId;
    public string DisplayName => displayName;
    public ActionApproach Approach => approach;
    public string Description => description;

    public bool HideWhenRequirementsNotMet =>
        hideWhenRequirementsNotMet;

    public IReadOnlyList<GameCondition> Requirements =>
        requirements;

    public IReadOnlyList<GameEffect> Effects =>
        effects;

    public MoveDefinition Move => move;

    public bool RequiresMove =>
        move != null;

    public string StrongHitText => strongHitText;
    public string WeakHitText => weakHitText;
    public string MissText => missText;

    public LocationActionDefinition[] FollowUpActions =>
        followUpActions;

    public bool AreRequirementsMet(
        PlayerGameState gameState)
    {
        if (requirements == null ||
            requirements.Count == 0)
        {
            return true;
        }

        foreach (GameCondition requirement in requirements)
        {
            if (requirement == null)
            {
                continue;
            }

            if (!requirement.IsMet(
                    gameState))
            {
                return false;
            }
        }

        return true;
    }

    public void ApplyEffects(
        PlayerGameState gameState)
    {
        ApplyEffectList(
            effects,
            gameState);
    }

    public void ApplyResultEffects(
        MoveResult result,
        PlayerGameState gameState)
    {
        switch (result)
        {
            case MoveResult.StrongHit:
                ApplyEffectList(
                    strongHitEffects,
                    gameState);
                break;

            case MoveResult.WeakHit:
                ApplyEffectList(
                    weakHitEffects,
                    gameState);
                break;

            case MoveResult.Miss:
                ApplyEffectList(
                    missEffects,
                    gameState);
                break;
        }
    }

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
                return Array.Empty<LocationActionDefinition>();
        }
    }

    private static void ApplyEffectList(
        List<GameEffect> effectList,
        PlayerGameState gameState)
    {
        if (effectList == null)
        {
            return;
        }

        foreach (GameEffect effect in effectList)
        {
            if (effect == null)
            {
                continue;
            }

            effect.Apply(
                gameState);
        }
    }
}
