using UnityEngine;

[CreateAssetMenu(
    fileName = "CharacterDefinition",
    menuName = "Game/Characters/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    private string characterId;

    [SerializeField]
    private string displayName;

    [Header("Presentation")]

    [SerializeField]
    private GameUITheme theme;

    [SerializeField]
    private Sprite portrait;

    [Header("Typewriter")]

    [SerializeField]
    [Min(1f)]
    private float charactersPerSecond = 45f;

    [SerializeField]
    [Min(0f)]
    private float commaPause = 0.18f;

    [SerializeField]
    [Min(0f)]
    private float sentencePause = 0.42f;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public GameUITheme Theme => theme;
    public Sprite Portrait => portrait;
    public float CharactersPerSecond => charactersPerSecond;
    public float CommaPause => commaPause;
    public float SentencePause => sentencePause;
}
