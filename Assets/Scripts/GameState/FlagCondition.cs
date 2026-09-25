using System;
using UnityEngine;

[Serializable]
public class FlagCondition : GameCondition
{
    [SerializeField]
    [Tooltip("The flag whose current value will be checked.")]
    private GameFlagDefinition flag;

    [SerializeField]
    [Tooltip("The value the flag must have for this condition to be met.")]
    private bool requiredValue = true;

    public GameFlagDefinition Flag =>
        flag;

    public bool RequiredValue =>
        requiredValue;

    public override bool IsMet(
        PlayerGameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogWarning(
                "FlagCondition cannot be evaluated because PlayerGameState is null.");

            return false;
        }

        if (flag == null)
        {
            Debug.LogWarning(
                "FlagCondition has no GameFlagDefinition assigned.");

            return false;
        }

        return gameState.GetFlag(
            flag) == requiredValue;
    }

    public override string GetDescription()
    {
        if (flag == null)
        {
            return "Missing Flag";
        }

        string flagName =
            string.IsNullOrWhiteSpace(
                flag.DisplayName)
                ? flag.name
                : flag.DisplayName;

        return requiredValue
            ? $"{flagName} = True"
            : $"{flagName} = False";
    }
}