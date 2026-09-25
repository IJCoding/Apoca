using System;
using UnityEngine;

[Serializable]
public class ResourceCondition : GameCondition
{
    [SerializeField]
    [Tooltip("The resource whose current value will be checked.")]
    private GameResourceDefinition resource;

    [SerializeField]
    [Tooltip("The comparison used when checking the resource.")]
    private ResourceComparison comparison =
        ResourceComparison.GreaterThanOrEqual;

    [SerializeField]
    [Tooltip("The value the resource is compared against.")]
    private int value;

    public GameResourceDefinition Resource =>
        resource;

    public ResourceComparison Comparison =>
        comparison;

    public int Value =>
        value;

    public override bool IsMet(
        PlayerGameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogWarning(
                "ResourceCondition cannot be evaluated because PlayerGameState is null.");

            return false;
        }

        if (resource == null)
        {
            Debug.LogWarning(
                "ResourceCondition has no GameResourceDefinition assigned.");

            return false;
        }

        int currentValue =
            gameState.GetResource(
                resource);

        switch (comparison)
        {
            case ResourceComparison.Equal:
                return currentValue == value;

            case ResourceComparison.NotEqual:
                return currentValue != value;

            case ResourceComparison.GreaterThan:
                return currentValue > value;

            case ResourceComparison.GreaterThanOrEqual:
                return currentValue >= value;

            case ResourceComparison.LessThan:
                return currentValue < value;

            case ResourceComparison.LessThanOrEqual:
                return currentValue <= value;

            default:
                Debug.LogWarning(
                    $"Unsupported ResourceComparison '{comparison}'.");

                return false;
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

        return
            $"{resourceName} {GetComparisonSymbol()} {value}";
    }

    private string GetComparisonSymbol()
    {
        switch (comparison)
        {
            case ResourceComparison.Equal:
                return "==";

            case ResourceComparison.NotEqual:
                return "!=";

            case ResourceComparison.GreaterThan:
                return ">";

            case ResourceComparison.GreaterThanOrEqual:
                return ">=";

            case ResourceComparison.LessThan:
                return "<";

            case ResourceComparison.LessThanOrEqual:
                return "<=";

            default:
                return "?";
        }
    }
}