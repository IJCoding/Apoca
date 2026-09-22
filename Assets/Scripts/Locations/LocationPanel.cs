using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LocationPanel : MonoBehaviour
{
    [Header("Location")]

    [SerializeField]
    [Tooltip("Displays the selected location's name.")]
    private TMP_Text locationNameText;

    [Header("Narrative")]

    [SerializeField]
    [Tooltip("The RectTransform that contains narrative entries and current action buttons.")]
    private RectTransform contentContainer;

    [SerializeField]
    [Tooltip("The prefab used to display a piece of narrative text.")]
    private NarrativeText narrativeTextPrefab;

    [SerializeField]
    [Tooltip("The ScrollRect containing the narrative history.")]
    private ScrollRect scrollRect;

    [Header("Actions")]

    [SerializeField]
    [Tooltip("The prefab used to display a location action.")]
    private LocationActionButton actionButtonPrefab;

    private readonly List<GameObject> generatedContent =
        new List<GameObject>();

    private readonly List<LocationActionButton> actionButtons =
        new List<LocationActionButton>();

    private void OnDisable()
    {
        ClearGeneratedContent();
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

        ClearGeneratedContent();

        locationNameText.text =
            location.DisplayName;

        gameObject.SetActive(true);

        AppendNarrative(
            location.Description);

        CreateActionButtons(
            location.Actions);

        ScrollToTop();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void AppendNarrative(
        string text)
    {
        NarrativeText narrativeText =
            Instantiate(
                narrativeTextPrefab,
                contentContainer);

        narrativeText.SetText(
            text);

        generatedContent.Add(
            narrativeText.gameObject);
    }

    private void AppendChoiceHistory(
        LocationActionDefinition action)
    {
        AppendNarrative(
            $"> {action.DisplayName}");
    }

    private void CreateActionButtons(
        LocationActionDefinition[] actions)
    {
        foreach (LocationActionDefinition action in actions)
        {
            if (action == null)
            {
                continue;
            }

            LocationActionButton actionButton =
                Instantiate(
                    actionButtonPrefab,
                    contentContainer);

            actionButton.SetAction(
                action);

            actionButton.Selected +=
                HandleActionSelected;

            actionButtons.Add(
                actionButton);

            generatedContent.Add(
                actionButton.gameObject);
        }
    }

    private void HandleActionSelected(
        LocationActionDefinition action)
    {
        ClearActionButtons();

        AppendChoiceHistory(
            action);

        AppendNarrative(
            action.Description);

        CreateActionButtons(
            action.FollowUpActions);

        ScrollToBottom();
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

            generatedContent.Remove(
                actionButton.gameObject);

            Destroy(
                actionButton.gameObject);
        }

        actionButtons.Clear();
    }

    private void ClearGeneratedContent()
    {
        ClearActionButtons();

        foreach (GameObject content in generatedContent)
        {
            if (content == null)
            {
                continue;
            }

            Destroy(
                content);
        }

        generatedContent.Clear();
    }

    private void ScrollToTop()
    {
        if (scrollRect == null)
        {
            return;
        }

        StartCoroutine(
            ScrollToTopNextFrame());
    }

    private IEnumerator ScrollToTopNextFrame()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition =
            1f;
    }

    private void ScrollToBottom()
    {
        if (scrollRect == null)
        {
            return;
        }

        StartCoroutine(
            ScrollToBottomNextFrame());
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition =
            0f;
    }
}