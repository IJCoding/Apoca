using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class LocationActionButton : MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    [Tooltip("Displays the name of the location action.")]
    private TMP_Text actionNameText;

    [Header("Theme")]

    [SerializeField]
    [Tooltip("Shared UI theme used to style this action button.")]
    private GameUITheme theme;

    private Button button;
    private Image buttonImage;

    private LocationActionDefinition action;

    private bool isSelected;

    public LocationActionDefinition Action =>
        action;

    public bool IsSelected =>
        isSelected;

    public event Action<LocationActionDefinition> Selected;

    private void Awake()
    {
        button =
            GetComponent<Button>();

        buttonImage =
            GetComponent<Image>();

        RefreshVisual();
    }

    private void OnEnable()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (buttonImage == null)
        {
            buttonImage =
                GetComponent<Image>();
        }

        button.onClick.AddListener(
            HandleButtonClicked);

        RefreshVisual();
    }

    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                HandleButtonClicked);
        }
    }

    public void SetAction(
        LocationActionDefinition locationAction)
    {
        action =
            locationAction;

        isSelected =
            false;

        if (actionNameText != null)
        {
            actionNameText.text =
                action != null
                    ? action.DisplayName
                    : string.Empty;
        }

        RefreshVisual();
    }

    public void SetSelected(
        bool selected)
    {
        isSelected =
            selected;

        RefreshVisual();
    }

    public void SetInteractable(
        bool interactable)
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        button.interactable =
            interactable;

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (buttonImage == null)
        {
            buttonImage =
                GetComponent<Image>();
        }

        if (theme == null ||
            buttonImage == null ||
            button == null)
        {
            return;
        }

        ActionApproach approach =
            action != null
                ? action.Approach
                : ActionApproach.None;

        GameUITheme.ApproachStyle style =
            theme.GetApproachStyle(
                approach);

        if (!button.interactable)
        {
            buttonImage.color =
                style.DisabledColor;

            if (actionNameText != null)
            {
                actionNameText.color =
                    style.DisabledTextColor;
            }

            return;
        }

        buttonImage.color =
            isSelected
                ? style.SelectedColor
                : style.NormalColor;

        if (actionNameText != null)
        {
            actionNameText.color =
                style.TextColor;
        }
    }

    private void HandleButtonClicked()
    {
        if (action == null)
        {
            Debug.LogWarning(
                $"Location action button '{name}' was clicked without an action assigned.",
                this);

            return;
        }

        Selected?.Invoke(
            action);
    }
}