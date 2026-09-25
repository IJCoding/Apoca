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

    [Header("Game State")]

    [SerializeField]
    [Tooltip("The runtime game state used to evaluate requirements and apply effects.")]
    private PlayerGameState gameState;

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
        selectedActionIndex < actionButtons.Count &&
        IsActionAvailableAtIndex(
            selectedActionIndex);

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

    private void Awake()
    {
        if (gameState == null)
        {
            gameState =
                FindFirstObjectByType<PlayerGameState>();
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

        EnsureGameState();

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
                actionButton.Action == null ||
                !actionButton.IsAvailable)
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
                Mod(
                    startIndex + offset,
                    actionButtons.Count);

            LocationActionButton actionButton =
                actionButtons[index];

            if (actionButton == null ||
                actionButton.Action == null ||
                !actionButton.IsAvailable)
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
            selectedButton.Action == null ||
            !selectedButton.IsAvailable)
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

        int startIndex =
            HasSelectedAction
                ? selectedActionIndex
                : -1;

        for (int offset = 1;
             offset <= actionButtons.Count;
             offset++)
        {
            int index =
                Mod(
                    startIndex + offset,
                    actionButtons.Count);

            if (IsActionAvailableAtIndex(
                index))
            {
                return index;
            }
        }

        return -1;
    }

    private int GetPreviousActionIndex()
    {
        if (actionButtons.Count == 0)
        {
            return -1;
        }

        int startIndex =
            HasSelectedAction
                ? selectedActionIndex
                : 0;

        for (int offset = 1;
             offset <= actionButtons.Count;
             offset++)
        {
            int index =
                Mod(
                    startIndex - offset,
                    actionButtons.Count);

            if (IsActionAvailableAtIndex(
                index))
            {
                return index;
            }
        }

        return -1;
    }

    private bool IsActionAvailableAtIndex(
        int index)
    {
        if (index < 0 ||
            index >= actionButtons.Count)
        {
            return false;
        }

        LocationActionButton actionButton =
            actionButtons[index];

        return actionButton != null &&
               actionButton.Action != null &&
               actionButton.IsAvailable;
    }

    private ActionApproach GetApproachAtIndex(
        int index)
    {
        if (!IsActionAvailableAtIndex(
            index))
        {
            return ActionApproach.None;
        }

        LocationActionButton actionButton =
            actionButtons[index];

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

        EnsureGameState();

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

                bool isAvailable =
                    action.AreRequirementsMet(
                        gameState);

                actionButton.SetAvailable(
                    isAvailable);

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
        if (!IsActionAvailableAtIndex(
            index))
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

        EnsureGameState();

        if (!action.AreRequirementsMet(
            gameState))
        {
            RefreshActionAvailability();

            return;
        }

        /*
         * Apply the action's game-state effects before creating its
         * follow-up choices.
         *
         * This means a flag changed by this action can immediately
         * affect which follow-up actions are available.
         */
        action.ApplyEffects(
            gameState);

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

    private void RefreshActionAvailability()
    {
        EnsureGameState();

        bool selectedActionBecameUnavailable =
            false;

        for (int i = 0;
             i < actionButtons.Count;
             i++)
        {
            LocationActionButton actionButton =
                actionButtons[i];

            if (actionButton == null ||
                actionButton.Action == null)
            {
                continue;
            }

            bool isAvailable =
                actionButton.Action.AreRequirementsMet(
                    gameState);

            actionButton.SetAvailable(
                isAvailable);

            if (i == selectedActionIndex &&
                !isAvailable)
            {
                selectedActionBecameUnavailable =
                    true;
            }
        }

        if (selectedActionBecameUnavailable)
        {
            selectedActionIndex =
                -1;
        }

        ActionsChanged?.Invoke();
        SelectionChanged?.Invoke();
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

        float worldDifference =
            viewportTop -
            entryTop;

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

        contentPosition.y +=
            localDifference;

        contentContainer.anchoredPosition =
            contentPosition;

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

    private void EnsureGameState()
    {
        if (gameState != null)
        {
            return;
        }

        gameState =
            FindFirstObjectByType<PlayerGameState>();

        if (gameState == null)
        {
            Debug.LogError(
                "LocationPanel could not find a PlayerGameState in the scene.",
                this);
        }
    }

    private int Mod(
        int value,
        int modulus)
    {
        if (modulus <= 0)
        {
            return 0;
        }

        int result =
            value % modulus;

        if (result < 0)
        {
            result +=
                modulus;
        }

        return result;
    }
}