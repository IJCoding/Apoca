using UnityEngine;

[CreateAssetMenu(
    fileName = "MoveDefinition",
    menuName = "Game/Moves/Move Definition")]
public class MoveDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    [Tooltip("A stable identifier used to distinguish this move from other moves.")]
    private string moveId;

    [SerializeField]
    [Tooltip("The name of the move displayed to the player.")]
    private string displayName;

    [Header("Move")]

    [SerializeField]
    [TextArea(2, 5)]
    [Tooltip("Describes when this move is triggered or what the move represents.")]
    private string description;

    [SerializeField]
    [Tooltip("The character stat added to the 2d6 roll for this move.")]
    private CharacterStat stat;

    public string MoveId => moveId;

    public string DisplayName => displayName;

    public string Description => description;

    public CharacterStat Stat => stat;
}