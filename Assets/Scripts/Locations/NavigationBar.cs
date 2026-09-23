using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NavigationBar : MonoBehaviour
{
    [Header("Location Panel")]

    [SerializeField]
    [Tooltip("The LocationPanel controlled by these navigation buttons.")]
    private LocationPanel locationPanel;

    [Header("Theme")]

    [SerializeField]
    [Tooltip("Shared UI theme used to style the navigation buttons.")]
    private GameUITheme theme;

    [Header("Navigation Buttons")]

    [SerializeField]
    private Button unalignedButton;

    [SerializeField]
    private Button previousButton;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button nextButton;

    private void OnEnable()
    {
        AddListener(
            unalignedButton,
            HandleUnaligned);

        AddListener(
            previousButton,
            HandlePrevious);

        AddListener(
            confirmButton,
            HandleConfirm);

        AddListener(
            nextButton,
            HandleNext);

        if (locationPanel != null)
        {
            locationPanel.ActionsChanged +=
                Refresh;

            locationPanel.SelectionChanged +=
                Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        RemoveListener(
            unalignedButton,
            HandleUnaligned);

        RemoveListener(
            previousButton,
            HandlePrevious);

        RemoveListener(
            confirmButton,
            HandleConfirm);

        RemoveListener(
            nextButton,
            HandleNext);

        if (locationPanel != null)
        {
            locationPanel.ActionsChanged -=
                Refresh;

            locationPanel.SelectionChanged -=
                Refresh;
        }
    }

    public void Refresh()
    {
        if (locationPanel == null)
        {
            ApplyButtonStyle(
                unalignedButton,
                ActionApproach.None,
                false);

            ApplyButtonStyle(
                previousButton,
                ActionApproach.None,
                false);

            ApplyButtonStyle(
                confirmButton,
                ActionApproach.None,
                false);

            ApplyButtonStyle(
                nextButton,
                ActionApproach.None,
                false);

            return;
        }

        bool hasActions =
            locationPanel.HasActions;

        bool hasSelection =
            locationPanel.HasSelectedAction;

        bool hasUnaligned =
            locationPanel.HasAvailableApproach(
                ActionApproach.None);

        ActionApproach confirmApproach =
            hasSelection
                ? locationPanel.SelectedApproach
                : ActionApproach.None;

        ActionApproach previousApproach =
            hasActions
                ? locationPanel.PreviousApproach
                : ActionApproach.None;

        ActionApproach nextApproach =
            hasActions
                ? locationPanel.NextApproach
                : ActionApproach.None;

        ApplyButtonStyle(
            unalignedButton,
            ActionApproach.None,
            hasUnaligned);

        ApplyButtonStyle(
            previousButton,
            previousApproach,
            hasActions);

        ApplyButtonStyle(
            confirmButton,
            confirmApproach,
            hasSelection);

        ApplyButtonStyle(
            nextButton,
            nextApproach,
            hasActions);
    }

    private void ApplyButtonStyle(
        Button button,
        ActionApproach approach,
        bool interactable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable =
            interactable;

        if (theme == null)
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
                interactable
                    ? style.NormalColor
                    : style.DisabledColor;
        }

        if (text != null)
        {
            text.color =
                interactable
                    ? style.TextColor
                    : style.DisabledTextColor;
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

    private void HandleUnaligned()
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.SelectNextApproach(
            ActionApproach.None);
    }

    private void HandlePrevious()
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.SelectPreviousAction();
    }

    private void HandleConfirm()
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.ConfirmSelectedAction();
    }

    private void HandleNext()
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.SelectNextAction();
    }
}