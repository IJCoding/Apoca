using UnityEngine;

public enum CharacterStat
{
    Valor,
    Wit,
    Soul,
    Shadow,
    Fortune
}

[DisallowMultipleComponent]
public class CharacterStats : MonoBehaviour
{
    private const int MinimumStatValue = -2;
    private const int MaximumStatValue = 3;

    [Header("Stats")]

    [SerializeField]
    [Range(MinimumStatValue, MaximumStatValue)]
    [Tooltip("Physicality: strength, endurance, force, and direct physical action.")]
    private int valor;

    [SerializeField]
    [Range(MinimumStatValue, MaximumStatValue)]
    [Tooltip("Mentality: reasoning, perception, knowledge, and intellectual action.")]
    private int wit;

    [SerializeField]
    [Range(MinimumStatValue, MaximumStatValue)]
    [Tooltip("Interpersonality: empathy, persuasion, presence, and emotional action.")]
    private int soul;

    [SerializeField]
    [Range(MinimumStatValue, MaximumStatValue)]
    [Tooltip("Subtlety: stealth, deception, finesse, and indirect action.")]
    private int shadow;

    [SerializeField]
    [Range(MinimumStatValue, MaximumStatValue)]
    [Tooltip("Chance: luck, risk, coincidence, and acts governed by fortune.")]
    private int fortune;

    public int Valor => valor;
    public int Wit => wit;
    public int Soul => soul;
    public int Shadow => shadow;
    public int Fortune => fortune;

    public int GetValue(
        CharacterStat stat)
    {
        switch (stat)
        {
            case CharacterStat.Valor:
                return valor;

            case CharacterStat.Wit:
                return wit;

            case CharacterStat.Soul:
                return soul;

            case CharacterStat.Shadow:
                return shadow;

            case CharacterStat.Fortune:
                return fortune;

            default:
                Debug.LogWarning(
                    $"CharacterStats received an unsupported CharacterStat: {stat}.",
                    this);

                return 0;
        }
    }
}