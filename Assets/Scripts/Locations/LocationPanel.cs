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

    [Header("Moves")]

    [SerializeField]
    [Tooltip("Resolves PbtA moves triggered by location actions.")]
    private MoveResolver moveResolver;

    private readonly List<GameObject> generatedContent =
        new List<GameObject>();

    private readonly List<LocationActionButton> actionButtons =
        new List<LocationActionButton>();

    private int selectedActionIndex = -1;

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
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

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
        selectedActionIndex = -1;

        if (actions == null)
        {
            return;
        }

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

    public void SelectNextAction()
    {
        if (actionButtons.Count == 0)
        {
            return;
        }

        int nextIndex =
            selectedActionIndex + 1;

        if (nextIndex >= actionButtons.Count)
        {
            nextIndex = 0;
        }

        SelectActionAtIndex(
            nextIndex);
    }

    public void SelectPreviousAction()
    {
        if (actionButtons.Count == 0)
        {
            return;
        }

        int previousIndex =
            selectedActionIndex - 1;

        if (previousIndex < 0)
        {
            previousIndex =
                actionButtons.Count - 1;
        }

        SelectActionAtIndex(
            previousIndex);
    }

    public void SelectNextApproach(
        ActionApproach approach)
    {
        if (actionButtons.Count == 0)
        {
            return;
        }

        int startIndex =
            selectedActionIndex;

        for (int offset = 1;
             offset <= actionButtons.Count;
             offset++)
        {
            int index =
                (startIndex + offset) %
                actionButtons.Count;

            LocationActionButton actionButton =
                actionButtons[index];

            if (actionButton == null ||
                actionButton.Action == null)
            {
                continue;
            }

            if (actionButton.Action.Approach != approach)
            {
                continue;
            }

            SelectActionAtIndex(
                index);

            return;
        }
    }

    public void ConfirmSelectedAction()
    {
        if (selectedActionIndex < 0 ||
            selectedActionIndex >= actionButtons.Count)
        {
            return;
        }

        LocationActionButton selectedButton =
            actionButtons[selectedActionIndex];

        if (selectedButton == null ||
            selectedButton.Action == null)
        {
            return;
        }

        HandleActionSelected(
            selectedButton.Action);
    }

    private void SelectActionAtIndex(
        int index)
    {
        if (index < 0 ||
            index >= actionButtons.Count)
        {
            return;
        }

        for (int i = 0;
             i < actionButtons.Count;
             i++)
        {
            LocationActionButton actionButton =
                actionButtons[i];

            if (actionButton == null)
            {
                continue;
            }

            actionButton.SetSelected(
                i == index);
        }

        selectedActionIndex =
            index;

        ScrollSelectedActionIntoView();
    }

    private void HandleActionSelected(
        LocationActionDefinition action)
    {
        ClearActionButtons();

        AppendChoiceHistory(
            action);

        if (action.RequiresMove)
        {
            ResolveMoveAction(
                action);
        }
        else
        {
            ResolveNarrativeAction(
                action);
        }

        ScrollToBottom();
    }

    private void ResolveNarrativeAction(
        LocationActionDefinition action)
    {
        AppendNarrative(
            action.Description);

        CreateActionButtons(
            action.FollowUpActions);
    }

    private void ResolveMoveAction(
        LocationActionDefinition action)
    {
        if (moveResolver == null)
        {
            Debug.LogError(
                $"LocationPanel cannot resolve action '{action.DisplayName}' because no MoveResolver is assigned.",
                this);

            return;
        }

        MoveDefinition move =
            action.Move;

        MoveResolution resolution =
            moveResolver.Resolve(
                move);

        AppendMoveRoll(
            move,
            resolution);

        AppendNarrative(
            action.GetResultText(
                resolution.Result));

        CreateActionButtons(
            action.GetFollowUpActions(
                resolution.Result));
    }

    private void AppendMoveRoll(
        MoveDefinition move,
        MoveResolution resolution)
    {
        string modifierText =
            resolution.Modifier >= 0
                ? $"+{resolution.Modifier}"
                : resolution.Modifier.ToString();

        string rollText =
            $"{move.DisplayName}\n" +
            $"Roll: {resolution.DieOne} + {resolution.DieTwo} {modifierText} {move.Stat} = {resolution.Total}\n" +
            $"{GetResultDisplayName(resolution.Result)}";

        AppendNarrative(
            rollText);
    }

    private string GetResultDisplayName(
        MoveResult result)
    {
        switch (result)
        {
            case MoveResult.StrongHit:
                return "Strong Hit";

            case MoveResult.WeakHit:
                return "Weak Hit";

            case MoveResult.Miss:
                return "Miss";

            default:
                return result.ToString();
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

            generatedContent.Remove(
                actionButton.gameObject);

            Destroy(
                actionButton.gameObject);
        }

        actionButtons.Clear();

        selectedActionIndex = -1;
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

    private void ScrollSelectedActionIntoView()
    {
        if (scrollRect == null ||
            selectedActionIndex < 0 ||
            selectedActionIndex >= actionButtons.Count)
        {
            return;
        }

        StartCoroutine(
            ScrollSelectedActionNextFrame());
    }

    private IEnumerator ScrollSelectedActionNextFrame()
    {
        yield return null;

        Canvas.ForceUpdateCanvases();

        LocationActionButton selectedButton =
            actionButtons[selectedActionIndex];

        if (selectedButton == null)
        {
            yield break;
        }

        RectTransform viewport =
            scrollRect.viewport;

        RectTransform selectedRect =
            selectedButton.transform as RectTransform;

        if (viewport == null ||
            selectedRect == null)
        {
            yield break;
        }

        Bounds bounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport,
                selectedRect);

        Rect viewportRect =
            viewport.rect;

        float moveAmount = 0f;

        if (bounds.max.y > viewportRect.yMax)
        {
            moveAmount =
                bounds.max.y -
                viewportRect.yMax;
        }
        else if (bounds.min.y < viewportRect.yMin)
        {
            moveAmount =
                bounds.min.y -
                viewportRect.yMin;
        }

        if (Mathf.Approximately(
            moveAmount,
            0f))
        {
            yield break;
        }

        Vector2 contentPosition =
            contentContainer.anchoredPosition;

        contentPosition.y +=
            moveAmount;

        contentContainer.anchoredPosition =
            contentPosition;
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