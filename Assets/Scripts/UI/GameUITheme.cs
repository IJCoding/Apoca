using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "GameUITheme",
    menuName = "Game/UI/Game UI Theme")]
public class GameUITheme : ScriptableObject
{
    [Serializable]
    public class ApproachStyle
    {
        [Header("Colours")]

        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color selectedColor = Color.white;

        [SerializeField]
        private Color textColor = Color.black;

        [SerializeField]
        private Color disabledColor = Color.gray;

        [SerializeField]
        private Color disabledTextColor = Color.gray;

        public Color NormalColor => normalColor;
        public Color SelectedColor => selectedColor;
        public Color TextColor => textColor;
        public Color DisabledColor => disabledColor;
        public Color DisabledTextColor => disabledTextColor;
    }

    [Serializable]
    public class NarrativePalette
    {
        [SerializeField]
        private Color backgroundColor = Color.black;

        [SerializeField]
        private Color panelColor = new Color(0.12f, 0.12f, 0.12f, 1f);

        [SerializeField]
        private Color primaryTextColor = Color.white;

        [SerializeField]
        private Color secondaryTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);

        [SerializeField]
        private Color accentColor = Color.white;

        public Color BackgroundColor => backgroundColor;
        public Color PanelColor => panelColor;
        public Color PrimaryTextColor => primaryTextColor;
        public Color SecondaryTextColor => secondaryTextColor;
        public Color AccentColor => accentColor;
    }

    [Header("Narrative Palette")]

    [SerializeField]
    private NarrativePalette narrative =
        new NarrativePalette();

    [Header("Approach Styles")]

    [SerializeField]
    private ApproachStyle unaligned = new ApproachStyle();

    [SerializeField]
    private ApproachStyle valor = new ApproachStyle();

    [SerializeField]
    private ApproachStyle wit = new ApproachStyle();

    [SerializeField]
    private ApproachStyle soul = new ApproachStyle();

    [SerializeField]
    private ApproachStyle shadow = new ApproachStyle();

    [SerializeField]
    private ApproachStyle fortune = new ApproachStyle();

    public NarrativePalette Narrative => narrative;

    public ApproachStyle GetApproachStyle(
        ActionApproach approach)
    {
        switch (approach)
        {
            case ActionApproach.Valor:
                return valor;

            case ActionApproach.Wit:
                return wit;

            case ActionApproach.Soul:
                return soul;

            case ActionApproach.Shadow:
                return shadow;

            case ActionApproach.Fortune:
                return fortune;

            case ActionApproach.None:
            default:
                return unaligned;
        }
    }
}
