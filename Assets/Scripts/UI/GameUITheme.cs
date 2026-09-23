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

        public Color NormalColor =>
            normalColor;

        public Color SelectedColor =>
            selectedColor;

        public Color TextColor =>
            textColor;

        public Color DisabledColor =>
            disabledColor;

        public Color DisabledTextColor =>
            disabledTextColor;
    }

    [Header("Approach Styles")]

    [SerializeField]
    private ApproachStyle unaligned =
        new ApproachStyle();

    [SerializeField]
    private ApproachStyle valor =
        new ApproachStyle();

    [SerializeField]
    private ApproachStyle wit =
        new ApproachStyle();

    [SerializeField]
    private ApproachStyle soul =
        new ApproachStyle();

    [SerializeField]
    private ApproachStyle shadow =
        new ApproachStyle();

    [SerializeField]
    private ApproachStyle fortune =
        new ApproachStyle();

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