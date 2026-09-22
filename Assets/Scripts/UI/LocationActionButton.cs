using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class LocationActionButton : MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    [Tooltip("Displays the name of the location action.")]
    private TMP_Text actionNameText;

    [Header("Approach Themes")]

    [SerializeField]
    private Color neutralColor = Color.white;

    [SerializeField]
    private Color valorColor = new Color(0.8f, 0.25f, 0.25f);

    [SerializeField]
    private Color witColor = new Color(0.25f, 0.55f, 0.9f);

    [SerializeField]
    private Color soulColor = new Color(0.65f, 0.35f, 0.8f);

    [SerializeField]
    private Color shadowColor = new Color(0.4f, 0.4f, 0.4f);

    [SerializeField]
    private Color fortuneColor = new Color(0.9f, 0.75f, 0.2f);

    [Header("Selection")]

    [SerializeField]
    [Range(1f, 2f)]
    [Tooltip("How much brighter the selected action appears.")]
    private float selectedBrightness = 1.25f;

    private Button button;
    private Image buttonImage;

    private LocationActionDefinition action;

    private bool isSelected;

    public LocationActionDefinition Action => action;

    public bool IsSelected => isSelected;

    public event Action<LocationActionDefinition> Selected;

    private void Awake()
    {
        button =
            GetComponent<Button>();

        buttonImage =
            GetComponent<Image>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(
            HandleButtonClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(
            HandleButtonClicked);
    }

    public void SetAction(
        LocationActionDefinition locationAction)
    {
        action =
            locationAction;

        isSelected =
            false;

        if (action == null)
        {
            actionNameText.text =
                string.Empty;

            RefreshVisual();

            return;
        }

        actionNameText.text =
            action.DisplayName;

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
        button.interactable =
            interactable;

        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (buttonImage == null)
        {
            return;
        }

        Color baseColor =
            GetApproachColor();

        if (isSelected)
        {
            baseColor =
                new Color(
                    Mathf.Clamp01(baseColor.r * selectedBrightness),
                    Mathf.Clamp01(baseColor.g * selectedBrightness),
                    Mathf.Clamp01(baseColor.b * selectedBrightness),
                    baseColor.a);
        }

        buttonImage.color =
            baseColor;
    }

    private Color GetApproachColor()
    {
        if (action == null)
        {
            return neutralColor;
        }

        switch (action.Approach)
        {
            case ActionApproach.Valor:
                return valorColor;

            case ActionApproach.Wit:
                return witColor;

            case ActionApproach.Soul:
                return soulColor;

            case ActionApproach.Shadow:
                return shadowColor;

            case ActionApproach.Fortune:
                return fortuneColor;

            case ActionApproach.None:
            default:
                return neutralColor;
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