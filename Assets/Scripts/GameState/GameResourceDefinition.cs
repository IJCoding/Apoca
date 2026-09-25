using UnityEngine;

[CreateAssetMenu(
    fileName = "GameResource",
    menuName = "Game/Game State/Game Resource")]
public class GameResourceDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    [Tooltip("A stable identifier used to distinguish this resource from every other resource.")]
    private string resourceId;

    [SerializeField]
    [Tooltip("The human-readable name of this resource.")]
    private string displayName;

    [SerializeField]
    [TextArea(2, 5)]
    [Tooltip("Editor-facing description explaining what this resource represents.")]
    private string description;

    [Header("Default State")]

    [SerializeField]
    [Tooltip("The value this resource has when starting a new game.")]
    private int defaultValue;

    [Header("Limits")]

    [SerializeField]
    [Tooltip("Whether this resource has a minimum allowed value.")]
    private bool hasMinimumValue = true;

    [SerializeField]
    [Tooltip("The minimum value this resource may have.")]
    private int minimumValue = 0;

    [SerializeField]
    [Tooltip("Whether this resource has a maximum allowed value.")]
    private bool hasMaximumValue;

    [SerializeField]
    [Tooltip("The maximum value this resource may have.")]
    private int maximumValue = 100;

    public string ResourceId =>
        resourceId;

    public string DisplayName =>
        displayName;

    public string Description =>
        description;

    public int DefaultValue =>
        ClampValue(
            defaultValue);

    public bool HasMinimumValue =>
        hasMinimumValue;

    public int MinimumValue =>
        minimumValue;

    public bool HasMaximumValue =>
        hasMaximumValue;

    public int MaximumValue =>
        maximumValue;

    public int ClampValue(
        int value)
    {
        if (hasMinimumValue)
        {
            value =
                Mathf.Max(
                    value,
                    minimumValue);
        }

        if (hasMaximumValue)
        {
            value =
                Mathf.Min(
                    value,
                    maximumValue);
        }

        return value;
    }
}