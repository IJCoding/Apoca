using UnityEngine;

[DisallowMultipleComponent]
public class MoveResolver : MonoBehaviour
{
    [Header("Character")]

    [SerializeField]
    [Tooltip("The character stats used when resolving moves.")]
    private CharacterStats characterStats;

#if UNITY_EDITOR
    [Header("Debug")]

    [SerializeField]
    [Tooltip("Overrides move results for testing while in the Unity Editor.")]
    private MoveDebugMode debugMode =
        MoveDebugMode.Normal;
#endif

    public MoveResolution Resolve(
        MoveDefinition move)
    {
        if (move == null)
        {
            Debug.LogError(
                "MoveResolver was asked to resolve a null MoveDefinition.",
                this);

            return default;
        }

        if (characterStats == null)
        {
            Debug.LogError(
                "MoveResolver cannot resolve a move because no CharacterStats are assigned.",
                this);

            return default;
        }

        int dieOne =
            Random.Range(
                1,
                7);

        int dieTwo =
            Random.Range(
                1,
                7);

        int modifier =
            characterStats.GetValue(
                move.Stat);

        int total =
            dieOne +
            dieTwo +
            modifier;

        MoveResult result =
            DetermineResult(
                total);

#if UNITY_EDITOR
        result =
            ApplyDebugOverride(
                result);
#endif

        return new MoveResolution(
            dieOne,
            dieTwo,
            modifier,
            result);
    }

    private MoveResult DetermineResult(
        int total)
    {
        if (total >= 10)
        {
            return MoveResult.StrongHit;
        }

        if (total >= 7)
        {
            return MoveResult.WeakHit;
        }

        return MoveResult.Miss;
    }

#if UNITY_EDITOR
    private MoveResult ApplyDebugOverride(
        MoveResult rolledResult)
    {
        switch (debugMode)
        {
            case MoveDebugMode.ForceMiss:
                return MoveResult.Miss;

            case MoveDebugMode.ForceWeakHit:
                return MoveResult.WeakHit;

            case MoveDebugMode.ForceStrongHit:
                return MoveResult.StrongHit;

            default:
                return rolledResult;
        }
    }
#endif
}