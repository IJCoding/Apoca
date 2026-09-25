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
    private bool isAvailable = true;

    public LocationActionDefinition Action =>
        action;

    public bool IsSelected =>
        isSelected;

    public bool IsAvailable =>
        isAvailable;

    public event Action<LocationActionDefinition> Selected;

    private void Awake()
    {
        CacheComponents();
        RefreshVisual();
    }

    private void OnEnable()
    {
        CacheComponents();

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

        isAvailable =
            true;

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
            selected && isAvailable;

        RefreshVisual();
    }

    public void SetAvailable(
        bool available)
    {
        isAvailable =
            available;

        if (!isAvailable)
        {
            isSelected =
                false;
        }

        RefreshVisual();
    }

    public void SetInteractable(
        bool interactable)
    {
        SetAvailable(
            interactable);
    }

    private void CacheComponents()
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
    }

    private void RefreshVisual()
    {
        CacheComponents();

        if (button != null)
        {
            button.interactable =
                isAvailable;
        }

        if (theme == null ||
            buttonImage == null)
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

        if (!isAvailable)
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

        if (!isAvailable)
        {
            return;
        }

        Selected?.Invoke(
            action);
    }
}