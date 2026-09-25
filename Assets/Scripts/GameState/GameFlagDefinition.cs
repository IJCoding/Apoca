using UnityEngine;

[CreateAssetMenu(
    fileName = "GameFlag",
    menuName = "Game/Game State/Game Flag")]
public class GameFlagDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    [Tooltip("A stable identifier used to distinguish this flag from every other flag.")]
    private string flagId;

    [SerializeField]
    [Tooltip("The human-readable name of this flag.")]
    private string displayName;

    [SerializeField]
    [TextArea(2, 5)]
    [Tooltip("Editor-facing description explaining what this flag represents.")]
    private string description;

    [Header("Default State")]

    [SerializeField]
    [Tooltip("The value this flag has when starting a new game.")]
    private bool defaultValue;

    public string FlagId =>
        flagId;

    public string DisplayName =>
        displayName;

    public string Description =>
        description;

    public bool DefaultValue =>
        defaultValue;
}