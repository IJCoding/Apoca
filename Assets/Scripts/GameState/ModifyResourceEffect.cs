using System;
using UnityEngine;

[Serializable]
public class ModifyResourceEffect : GameEffect
{
    [SerializeField]
    [Tooltip("The resource changed when this effect is applied.")]
    private GameResourceDefinition resource;

    [SerializeField]
    [Tooltip("How this effect changes the resource.")]
    private ResourceModification modification =
        ResourceModification.Increase;

    [SerializeField]
    [Tooltip("The value used by this resource modification.")]
    private int value = 1;

    public GameResourceDefinition Resource =>
        resource;

    public ResourceModification Modification =>
        modification;

    public int Value =>
        value;

    public override void Apply(
        PlayerGameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogWarning(
                "ModifyResourceEffect cannot be applied because PlayerGameState is null.");

            return;
        }

        if (resource == null)
        {
            Debug.LogWarning(
                "ModifyResourceEffect has no GameResourceDefinition assigned.");

            return;
        }

        int currentValue =
            gameState.GetResource(
                resource);

        switch (modification)
        {
            case ResourceModification.Increase:
                gameState.SetResource(
                    resource,
                    currentValue + Mathf.Abs(value));
                break;

            case ResourceModification.Decrease:
                gameState.SetResource(
                    resource,
                    currentValue - Mathf.Abs(value));
                break;

            case ResourceModification.Set:
                gameState.SetResource(
                    resource,
                    value);
                break;

            case ResourceModification.Multiply:
                gameState.SetResource(
                    resource,
                    currentValue * value);
                break;

            case ResourceModification.Divide:
                if (value == 0)
                {
                    Debug.LogWarning(
                        $"Cannot divide resource '{resource.name}' by zero.");

                    return;
                }

                gameState.SetResource(
                    resource,
                    currentValue / value);
                break;

            default:
                Debug.LogWarning(
                    $"Unsupported ResourceModification '{modification}'.");
                break;
        }
    }

    public override string GetDescription()
    {
        if (resource == null)
        {
            return "Missing Resource";
        }

        string resourceName =
            string.IsNullOrWhiteSpace(
                resource.DisplayName)
                ? resource.name
                : resource.DisplayName;

        switch (modification)
        {
            case ResourceModification.Increase:
                return
                    $"{resourceName} + {Mathf.Abs(value)}";

            case ResourceModification.Decrease:
                return
                    $"{resourceName} - {Mathf.Abs(value)}";

            case ResourceModification.Set:
                return
                    $"{resourceName} = {value}";

            case ResourceModification.Multiply:
                return
                    $"{resourceName} × {value}";

            case ResourceModification.Divide:
                return
                    $"{resourceName} ÷ {value}";

            default:
                return
                    $"{resourceName} ?";
        }
    }
}