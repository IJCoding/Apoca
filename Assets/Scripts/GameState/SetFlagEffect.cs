using System;
using UnityEngine;

[Serializable]
public class SetFlagEffect : GameEffect
{
    [SerializeField]
    [Tooltip("The flag changed when this effect is applied.")]
    private GameFlagDefinition flag;

    [SerializeField]
    [Tooltip("The value assigned to the flag.")]
    private bool value = true;

    public GameFlagDefinition Flag =>
        flag;

    public bool Value =>
        value;

    public override void Apply(
        PlayerGameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogWarning(
                "SetFlagEffect cannot be applied because PlayerGameState is null.");

            return;
        }

        if (flag == null)
        {
            Debug.LogWarning(
                "SetFlagEffect has no GameFlagDefinition assigned.");

            return;
        }

        gameState.SetFlag(
            flag,
            value);
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

        return value
            ? $"{flagName} = True"
            : $"{flagName} = False";
    }
}