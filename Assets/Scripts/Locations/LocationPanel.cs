using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class LocationPanel : MonoBehaviour
{
    [Header("Location")]

    [SerializeField]
    [Tooltip("Displays the selected location's name.")]
    private TMP_Text locationNameText;

    [SerializeField]
    [Tooltip("Displays the selected location's description.")]
    private TMP_Text locationDescriptionText;

    [Header("Actions")]

    [SerializeField]
    [Tooltip("The RectTransform that contains the generated location action buttons.")]
    private RectTransform actionContainer;

    [SerializeField]
    [Tooltip("The prefab used to display a location action.")]
    private LocationActionButton actionButtonPrefab;

    private readonly List<LocationActionButton> actionButtons =
        new List<LocationActionButton>();

    private void OnDisable()
    {
        ClearActionButtons();
    }

    public void Show(
        LocationDefinition location)
    {
        if (location == null)
        {
            Debug.LogWarning(
                "LocationPanel was asked to display a null LocationDefinition.",
                this);

            return;
        }

        ClearActionButtons();

        locationNameText.text =
            location.DisplayName;

        locationDescriptionText.text =
            location.Description;

        CreateActionButtons(
            location);

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void CreateActionButtons(
        LocationDefinition location)
    {
        foreach (LocationActionDefinition action in location.Actions)
        {
            if (action == null)
            {
                continue;
            }

            LocationActionButton actionButton =
                Instantiate(
                    actionButtonPrefab,
                    actionContainer);

            actionButton.SetAction(
                action);

            actionButton.Selected +=
                HandleActionSelected;

            actionButtons.Add(
                actionButton);
        }
    }

    private void ClearActionButtons()
    {
        foreach (LocationActionButton actionButton in actionButtons)
        {
            if (actionButton == null)
            {
                continue;
            }

            actionButton.Selected -=
                HandleActionSelected;

            Destroy(
                actionButton.gameObject);
        }

        actionButtons.Clear();
    }

    private void HandleActionSelected(
        LocationActionDefinition action)
    {
        locationDescriptionText.text =
            action.Description;
    }
}