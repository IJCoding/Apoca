using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ApproachBar : MonoBehaviour
{
    [Header("Location Panel")]

    [SerializeField]
    [Tooltip("The LocationPanel whose available actions this bar represents.")]
    private LocationPanel locationPanel;

    [Header("Theme")]

    [SerializeField]
    [Tooltip("Shared UI theme used by the approach bar.")]
    private GameUITheme theme;

    [Header("Approach Buttons")]

    [SerializeField]
    private Button valorButton;

    [SerializeField]
    private Button witButton;

    [SerializeField]
    private Button soulButton;

    [SerializeField]
    private Button shadowButton;

    [SerializeField]
    private Button fortuneButton;

    private void Awake()
    {
        SetButtonLabel(
            valorButton,
            "VALOR");

        SetButtonLabel(
            witButton,
            "WIT");

        SetButtonLabel(
            soulButton,
            "SOUL");

        SetButtonLabel(
            shadowButton,
            "SHADOW");

        SetButtonLabel(
            fortuneButton,
            "FORTUNE");
    }

    private void OnEnable()
    {
        AddListener(
            valorButton,
            HandleValor);

        AddListener(
            witButton,
            HandleWit);

        AddListener(
            soulButton,
            HandleSoul);

        AddListener(
            shadowButton,
            HandleShadow);

        AddListener(
            fortuneButton,
            HandleFortune);

        if (locationPanel != null)
        {
            locationPanel.ActionsChanged +=
                Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        RemoveListener(
            valorButton,
            HandleValor);

        RemoveListener(
            witButton,
            HandleWit);

        RemoveListener(
            soulButton,
            HandleSoul);

        RemoveListener(
            shadowButton,
            HandleShadow);

        RemoveListener(
            fortuneButton,
            HandleFortune);

        if (locationPanel != null)
        {
            locationPanel.ActionsChanged -=
                Refresh;
        }
    }

    public void Refresh()
    {
        RefreshButton(
            valorButton,
            ActionApproach.Valor);

        RefreshButton(
            witButton,
            ActionApproach.Wit);

        RefreshButton(
            soulButton,
            ActionApproach.Soul);

        RefreshButton(
            shadowButton,
            ActionApproach.Shadow);

        RefreshButton(
            fortuneButton,
            ActionApproach.Fortune);
    }

    private void RefreshButton(
        Button button,
        ActionApproach approach)
    {
        if (button == null)
        {
            return;
        }

        bool available =
            locationPanel != null &&
            locationPanel.HasAvailableApproach(
                approach);

        button.interactable =
            available;

        ApplyTheme(
            button,
            approach,
            available);
    }

    private void ApplyTheme(
        Button button,
        ActionApproach approach,
        bool available)
    {
        if (theme == null ||
            button == null)
        {
            return;
        }

        GameUITheme.ApproachStyle style =
            theme.GetApproachStyle(
                approach);

        Image image =
            button.GetComponent<Image>();

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        if (image != null)
        {
            image.color =
                available
                    ? style.NormalColor
                    : style.DisabledColor;
        }

        if (text != null)
        {
            text.color =
                available
                    ? style.TextColor
                    : style.DisabledTextColor;
        }
    }

    private void SetButtonLabel(
        Button button,
        string label)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text =
                label;
        }
    }

    private void AddListener(
        Button button,
        UnityEngine.Events.UnityAction callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(
            callback);
    }

    private void RemoveListener(
        Button button,
        UnityEngine.Events.UnityAction callback)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(
            callback);
    }

    private void HandleValor()
    {
        SelectApproach(
            ActionApproach.Valor);
    }

    private void HandleWit()
    {
        SelectApproach(
            ActionApproach.Wit);
    }

    private void HandleSoul()
    {
        SelectApproach(
            ActionApproach.Soul);
    }

    private void HandleShadow()
    {
        SelectApproach(
            ActionApproach.Shadow);
    }

    private void HandleFortune()
    {
        SelectApproach(
            ActionApproach.Fortune);
    }

    private void SelectApproach(
        ActionApproach approach)
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.SelectNextApproach(
            approach);
    }
}