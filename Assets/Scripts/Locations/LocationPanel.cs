using System;
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

    public event Action ActionsChanged;
    public event Action SelectionChanged;

    public bool HasActions =>
        actionButtons.Count > 0;

    public bool HasSelectedAction =>
        selectedActionIndex >= 0 &&
        selectedActionIndex < actionButtons.Count;

    public ActionApproach SelectedApproach
    {
        get
        {
            if (!HasSelectedAction)
            {
                return ActionApproach.None;
            }

            LocationActionButton selectedButton =
                actionButtons[selectedActionIndex];

            if (selectedButton == null ||
                selectedButton.Action == null)
            {
                return ActionApproach.None;
            }

            return selectedButton.Action.Approach;
        }
    }

    public ActionApproach PreviousApproach
    {
        get
        {
            int index =
                GetPreviousActionIndex();

            return GetApproachAtIndex(
                index);
        }
    }

    public ActionApproach NextApproach
    {
        get
        {
            int index =
                GetNextActionIndex();

            return GetApproachAtIndex(
                index);
        }
    }

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

        if (locationNameText != null)
        {
            locationNameText.text =
                location.DisplayName;
        }

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

    public bool HasAvailableApproach(
        ActionApproach approach)
    {
        foreach (LocationActionButton actionButton in actionButtons)
        {
            if (actionButton == null ||
                actionButton.Action == null)
            {
                continue;
            }

            if (actionButton.Action.Approach == approach)
            {
                return true;
            }
        }

        return false;
    }

    public void SelectNextAction()
    {
        int nextIndex =
            GetNextActionIndex();

        if (nextIndex < 0)
        {
            return;
        }

        SelectActionAtIndex(
            nextIndex);
    }

    public void SelectPreviousAction()
    {
        int previousIndex =
            GetPreviousActionIndex();

        if (previousIndex < 0)
        {
            return;
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
        if (!HasSelectedAction)
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

    private int GetNextActionIndex()
    {
        if (actionButtons.Count == 0)
        {
            return -1;
        }

        if (!HasSelectedAction)
        {
            return 0;
        }

        int nextIndex =
            selectedActionIndex + 1;

        if (nextIndex >= actionButtons.Count)
        {
            nextIndex = 0;
        }

        return nextIndex;
    }

    private int GetPreviousActionIndex()
    {
        if (actionButtons.Count == 0)
        {
            return -1;
        }

        if (!HasSelectedAction)
        {
            return actionButtons.Count - 1;
        }

        int previousIndex =
            selectedActionIndex - 1;

        if (previousIndex < 0)
        {
            previousIndex =
                actionButtons.Count - 1;
        }

        return previousIndex;
    }

    private ActionApproach GetApproachAtIndex(
        int index)
    {
        if (index < 0 ||
            index >= actionButtons.Count)
        {
            return ActionApproach.None;
        }

        LocationActionButton actionButton =
            actionButtons[index];

        if (actionButton == null ||
            actionButton.Action == null)
        {
            return ActionApproach.None;
        }

        return actionButton.Action.Approach;
    }

    private NarrativeText AppendNarrative(
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (narrativeTextPrefab == null ||
            contentContainer == null)
        {
            Debug.LogError(
                "LocationPanel is missing its NarrativeText prefab or Content Container.",
                this);

            return null;
        }

        NarrativeText narrativeText =
            Instantiate(
                narrativeTextPrefab,
                contentContainer);

        narrativeText.SetText(
            text);

        generatedContent.Add(
            narrativeText.gameObject);

        return narrativeText;
    }

    private NarrativeText AppendChoiceHistory(
        LocationActionDefinition action)
    {
        if (action == null)
        {
            return null;
        }

        return AppendNarrative(
            $"> {action.DisplayName}");
    }

    private void CreateActionButtons(
        LocationActionDefinition[] actions)
    {
        selectedActionIndex = -1;

        if (actions != null)
        {
            foreach (LocationActionDefinition action in actions)
            {
                if (action == null)
                {
                    continue;
                }

                if (actionButtonPrefab == null ||
                    contentContainer == null)
                {
                    Debug.LogError(
                        "LocationPanel is missing its LocationActionButton prefab or Content Container.",
                        this);

                    break;
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

        ActionsChanged?.Invoke();
        SelectionChanged?.Invoke();
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

        SelectionChanged?.Invoke();

        ScrollSelectedActionIntoView();
    }

    private void HandleActionSelected(
        LocationActionDefinition action)
    {
        if (action == null)
        {
            return;
        }

        ClearActionButtons();

        NarrativeText choiceHistory =
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

        if (choiceHistory != null)
        {
            RectTransform choiceRect =
                choiceHistory.transform as RectTransform;

            ScrollEntryToTop(
                choiceRect);
        }
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
        if (move == null)
        {
            return;
        }

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

        ActionsChanged?.Invoke();
        SelectionChanged?.Invoke();
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
            !HasSelectedAction)
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

        if (!HasSelectedAction ||
            scrollRect == null)
        {
            yield break;
        }

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
            selectedRect == null ||
            contentContainer == null)
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

    private void ScrollEntryToTop(
        RectTransform entry)
    {
        if (scrollRect == null ||
            entry == null)
        {
            return;
        }

        StartCoroutine(
            ScrollEntryToTopNextFrame(
                entry));
    }

    private IEnumerator ScrollEntryToTopNextFrame(
        RectTransform entry)
    {
        /*
         * The action buttons have just been destroyed and new narrative
         * content has just been created.
         *
         * Unity's layout system therefore needs to finish rebuilding
         * before we calculate where the new "> Choice" entry actually is.
         */
        yield return new WaitForEndOfFrame();

        if (entry == null ||
            scrollRect == null ||
            contentContainer == null)
        {
            yield break;
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            contentContainer);

        Canvas.ForceUpdateCanvases();

        RectTransform viewport =
            scrollRect.viewport;

        if (viewport == null)
        {
            yield break;
        }

        /*
         * Work in world coordinates here.
         *
         * GetWorldCorners:
         *
         * 0 = bottom-left
         * 1 = top-left
         * 2 = top-right
         * 3 = bottom-right
         */
        Vector3[] entryCorners =
            new Vector3[4];

        Vector3[] viewportCorners =
            new Vector3[4];

        entry.GetWorldCorners(
            entryCorners);

        viewport.GetWorldCorners(
            viewportCorners);

        float entryTop =
            entryCorners[1].y;

        float viewportTop =
            viewportCorners[1].y;

        /*
         * Determine how far apart the top of the choice entry
         * and the top of the viewport currently are.
         */
        float worldDifference =
            viewportTop -
            entryTop;

        /*
         * anchoredPosition is in the content parent's local space,
         * so convert the world-space distance into local-space distance.
         */
        RectTransform contentParent =
            contentContainer.parent as RectTransform;

        if (contentParent == null)
        {
            yield break;
        }

        Vector3 worldStart =
            contentParent.TransformPoint(
                Vector3.zero);

        Vector3 worldEnd =
            worldStart +
            new Vector3(
                0f,
                worldDifference,
                0f);

        Vector3 localStart =
            contentParent.InverseTransformPoint(
                worldStart);

        Vector3 localEnd =
            contentParent.InverseTransformPoint(
                worldEnd);

        float localDifference =
            localEnd.y -
            localStart.y;

        Vector2 contentPosition =
            contentContainer.anchoredPosition;

        /*
         * Moving the content upward moves entries upward through
         * the viewport.
         */
        contentPosition.y +=
            localDifference;

        contentContainer.anchoredPosition =
            contentPosition;

        /*
         * Clamp the ScrollRect afterwards.
         *
         * If the selected choice is near the very end of all available
         * content, Unity cannot physically place it at the top unless
         * enough content exists below it. In that case the ScrollRect
         * naturally stops at its valid limit.
         */
        Canvas.ForceUpdateCanvases();

        scrollRect.StopMovement();
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

        if (scrollRect == null)
        {
            yield break;
        }

        scrollRect.verticalNormalizedPosition =
            1f;
    }
}