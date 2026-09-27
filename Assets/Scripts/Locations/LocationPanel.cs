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

    [Header("Speaker Presentation")]

    [SerializeField]
    [Tooltip("Theme used for narration and for characters without their own theme.")]
    private GameUITheme defaultTheme;

    [SerializeField]
    [Tooltip("Optional speaker-name label. Hidden for narration.")]
    private TMP_Text speakerNameText;

    [SerializeField]
    [Tooltip("Optional portrait image. Hidden for narration or speakers without a portrait.")]
    private Image speakerPortraitImage;

    [SerializeField]
    [Tooltip("Images that should use the active theme's background colour.")]
    private Image[] themedBackgroundImages =
        Array.Empty<Image>();

    [SerializeField]
    [Tooltip("Images that should use the active theme's panel colour.")]
    private Image[] themedPanelImages =
        Array.Empty<Image>();

    [SerializeField]
    [Tooltip("Text elements that should use the active theme's primary text colour.")]
    private TMP_Text[] themedPrimaryTexts =
        Array.Empty<TMP_Text>();

    [SerializeField]
    [Tooltip("Text elements that should use the active theme's secondary text colour.")]
    private TMP_Text[] themedSecondaryTexts =
        Array.Empty<TMP_Text>();

    [Header("Typewriter")]

    [SerializeField]
    [Min(1f)]
    private float defaultCharactersPerSecond = 45f;

    [SerializeField]
    [Min(0f)]
    private float defaultCommaPause = 0.18f;

    [SerializeField]
    [Min(0f)]
    private float defaultSentencePause = 0.42f;

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

    private Coroutine typewriterCoroutine;
    private TMP_Text activeTypewriterText;
    private string activeTypewriterFullText;
    private bool isTypewriting;

    public event Action ActionsChanged;
    public event Action SelectionChanged;

    public bool HasActions =>
        HasVisibleActionButtons();

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
        StopTypewriter(
            false);

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

        ApplySpeakerPresentation(
            null);

        AppendNarrative(
            location.Description,
            null);

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
        if (isTypewriting)
        {
            CompleteCurrentTypewriter();
            return;
        }

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
               actionButton.gameObject.activeInHierarchy &&
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
        string text,
        CharacterDefinition speaker = null)
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

        StopTypewriter(
            true);

        ApplySpeakerPresentation(
            speaker);

        NarrativeText narrativeText =
            Instantiate(
                narrativeTextPrefab,
                contentContainer);

        narrativeText.SetText(
            text);

        generatedContent.Add(
            narrativeText.gameObject);

        TMP_Text textComponent =
            narrativeText.GetComponentInChildren<TMP_Text>(
                true);

        if (textComponent != null)
        {
            GameUITheme theme =
                GetThemeForSpeaker(
                    speaker);

            if (theme != null)
            {
                textComponent.color =
                    theme.Narrative.PrimaryTextColor;
            }

            TypewriterSkipTarget skipTarget =
                textComponent.GetComponent<TypewriterSkipTarget>();

            if (skipTarget == null)
            {
                skipTarget =
                    textComponent.gameObject.AddComponent<TypewriterSkipTarget>();
            }

            skipTarget.Clicked +=
                CompleteCurrentTypewriter;

            StartTypewriter(
                textComponent,
                text,
                speaker);
        }

        return narrativeText;
    }

    private void StartTypewriter(
        TMP_Text textComponent,
        string fullText,
        CharacterDefinition speaker)
    {
        if (textComponent == null)
        {
            return;
        }

        activeTypewriterText =
            textComponent;

        activeTypewriterFullText =
            fullText ?? string.Empty;

        textComponent.text =
            activeTypewriterFullText;

        textComponent.maxVisibleCharacters =
            0;

        textComponent.ForceMeshUpdate();

        isTypewriting =
            true;

        float charactersPerSecond =
            speaker != null
                ? speaker.CharactersPerSecond
                : defaultCharactersPerSecond;

        float commaPause =
            speaker != null
                ? speaker.CommaPause
                : defaultCommaPause;

        float sentencePause =
            speaker != null
                ? speaker.SentencePause
                : defaultSentencePause;

        typewriterCoroutine =
            StartCoroutine(
                TypewriterRoutine(
                    textComponent,
                    activeTypewriterFullText,
                    charactersPerSecond,
                    commaPause,
                    sentencePause));
    }

    private IEnumerator TypewriterRoutine(
        TMP_Text textComponent,
        string fullText,
        float charactersPerSecond,
        float commaPause,
        float sentencePause)
    {
        textComponent.ForceMeshUpdate();

        int visibleCharacterCount =
            textComponent.textInfo.characterCount;

        float characterDelay =
            1f / Mathf.Max(
                1f,
                charactersPerSecond);

        for (int visibleCount = 1;
             visibleCount <= visibleCharacterCount;
             visibleCount++)
        {
            if (textComponent == null ||
                textComponent != activeTypewriterText)
            {
                yield break;
            }

            textComponent.maxVisibleCharacters =
                visibleCount;

            KeepTypewriterTextInView(
                textComponent,
                visibleCount - 1);

            char visibleCharacter =
                GetVisibleCharacter(
                    textComponent,
                    fullText,
                    visibleCount - 1);

            float delay =
                characterDelay;

            if (visibleCharacter == ',' ||
                visibleCharacter == ':' ||
                visibleCharacter == ';')
            {
                delay +=
                    commaPause;
            }
            else if (visibleCharacter == '.' ||
                     visibleCharacter == '!' ||
                     visibleCharacter == '?')
            {
                delay +=
                    sentencePause;
            }

            if (delay > 0f)
            {
                yield return new WaitForSecondsRealtime(
                    delay);
            }
            else
            {
                yield return null;
            }
        }

        FinishTypewriterState(
            textComponent);
    }

    private void KeepTypewriterTextInView(
        TMP_Text textComponent,
        int visibleCharacterIndex)
    {
        if (textComponent == null ||
            scrollRect == null ||
            contentContainer == null)
        {
            return;
        }

        RectTransform viewport =
            scrollRect.viewport;

        if (viewport == null)
        {
            return;
        }

        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo =
            textComponent.textInfo;

        if (textInfo == null ||
            textInfo.characterCount == 0)
        {
            return;
        }

        int characterIndex =
            Mathf.Clamp(
                visibleCharacterIndex,
                0,
                textInfo.characterCount - 1);

        while (characterIndex > 0 &&
               !textInfo.characterInfo[characterIndex].isVisible)
        {
            characterIndex--;
        }

        TMP_CharacterInfo characterInfo =
            textInfo.characterInfo[characterIndex];

        Vector3 characterBottomWorld =
            textComponent.transform.TransformPoint(
                characterInfo.bottomLeft);

        Vector3 characterBottomInViewport =
            viewport.InverseTransformPoint(
                characterBottomWorld);

        /*
         * Keep the active line comfortably above the bottom of the narrative
         * viewport. The extra reserve means the text continues upward instead
         * of visually running into the approach / previous / confirm / next
         * controls that sit beneath the scroll area.
         */
        float bottomReserve =
            Mathf.Min(
                110f,
                viewport.rect.height * 0.22f);

        float visibleBottom =
            viewport.rect.yMin +
            bottomReserve;

        if (characterBottomInViewport.y >=
            visibleBottom)
        {
            return;
        }

        float overflow =
            visibleBottom -
            characterBottomInViewport.y;

        RectTransform contentParent =
            contentContainer.parent as RectTransform;

        if (contentParent == null)
        {
            return;
        }

        Vector3 viewportStartWorld =
            viewport.TransformPoint(
                Vector3.zero);

        Vector3 viewportEndWorld =
            viewport.TransformPoint(
                new Vector3(
                    0f,
                    overflow,
                    0f));

        Vector3 parentStart =
            contentParent.InverseTransformPoint(
                viewportStartWorld);

        Vector3 parentEnd =
            contentParent.InverseTransformPoint(
                viewportEndWorld);

        float localOverflow =
            parentEnd.y -
            parentStart.y;

        Vector2 contentPosition =
            contentContainer.anchoredPosition;

        contentPosition.y +=
            localOverflow;

        contentContainer.anchoredPosition =
            contentPosition;

        scrollRect.StopMovement();
    }

    private char GetVisibleCharacter(
        TMP_Text textComponent,
        string fullText,
        int visibleIndex)
    {
        if (textComponent == null ||
            visibleIndex < 0 ||
            visibleIndex >= textComponent.textInfo.characterCount)
        {
            return '\0';
        }

        TMP_CharacterInfo characterInfo =
            textComponent.textInfo.characterInfo[visibleIndex];

        int sourceIndex =
            characterInfo.index;

        if (sourceIndex < 0 ||
            sourceIndex >= fullText.Length)
        {
            return '\0';
        }

        return fullText[sourceIndex];
    }

    public void CompleteCurrentTypewriter()
    {
        if (!isTypewriting)
        {
            return;
        }

        StopTypewriter(
            true);
    }

    private void StopTypewriter(
        bool revealAll)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(
                typewriterCoroutine);

            typewriterCoroutine =
                null;
        }

        if (activeTypewriterText != null &&
            revealAll)
        {
            activeTypewriterText.maxVisibleCharacters =
                int.MaxValue;
        }

        activeTypewriterText =
            null;

        activeTypewriterFullText =
            null;

        bool wasTypewriting =
            isTypewriting;

        isTypewriting =
            false;

        if (revealAll &&
            wasTypewriting)
        {
            RevealActionButtons();
        }
    }

    private void FinishTypewriterState(
        TMP_Text textComponent)
    {
        if (textComponent != null)
        {
            textComponent.maxVisibleCharacters =
                int.MaxValue;
        }

        if (activeTypewriterText == textComponent)
        {
            activeTypewriterText =
                null;

            activeTypewriterFullText =
                null;

            typewriterCoroutine =
                null;

            isTypewriting =
                false;

            RevealActionButtons();
        }
    }

    private void ApplySpeakerPresentation(
        CharacterDefinition speaker)
    {
        GameUITheme theme =
            GetThemeForSpeaker(
                speaker);

        if (speakerNameText != null)
        {
            bool hasSpeaker =
                speaker != null &&
                !string.IsNullOrWhiteSpace(
                    speaker.DisplayName);

            speakerNameText.gameObject.SetActive(
                hasSpeaker);

            if (hasSpeaker)
            {
                speakerNameText.text =
                    speaker.DisplayName;
            }
        }

        if (speakerPortraitImage != null)
        {
            bool hasPortrait =
                speaker != null &&
                speaker.Portrait != null;

            speakerPortraitImage.gameObject.SetActive(
                hasPortrait);

            speakerPortraitImage.sprite =
                hasPortrait
                    ? speaker.Portrait
                    : null;
        }

        if (theme == null)
        {
            return;
        }

        GameUITheme.NarrativePalette palette =
            theme.Narrative;

        ApplyImageColour(
            themedBackgroundImages,
            palette.BackgroundColor);

        ApplyImageColour(
            themedPanelImages,
            palette.PanelColor);

        ApplyTextColour(
            themedPrimaryTexts,
            palette.PrimaryTextColor);

        ApplyTextColour(
            themedSecondaryTexts,
            palette.SecondaryTextColor);

        if (speakerNameText != null)
        {
            speakerNameText.color =
                palette.AccentColor;
        }
    }

    private GameUITheme GetThemeForSpeaker(
        CharacterDefinition speaker)
    {
        if (speaker != null &&
            speaker.Theme != null)
        {
            return speaker.Theme;
        }

        return defaultTheme;
    }

    private void ApplyImageColour(
        Image[] images,
        Color colour)
    {
        if (images == null)
        {
            return;
        }

        foreach (Image image in images)
        {
            if (image != null)
            {
                image.color =
                    colour;
            }
        }
    }

    private void ApplyTextColour(
        TMP_Text[] texts,
        Color colour)
    {
        if (texts == null)
        {
            return;
        }

        foreach (TMP_Text textElement in texts)
        {
            if (textElement != null)
            {
                textElement.color =
                    colour;
            }
        }
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

                bool isAvailable =
                    action.AreRequirementsMet(
                        gameState);

                if (!isAvailable &&
                    action.HideWhenRequirementsNotMet)
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

                actionButton.SetAvailable(
                    isAvailable);

                actionButton.Selected +=
                    HandleActionSelected;

                actionButton.gameObject.SetActive(
                    !isTypewriting);

                actionButtons.Add(
                    actionButton);

                generatedContent.Add(
                    actionButton.gameObject);
            }
        }

        ActionsChanged?.Invoke();
        SelectionChanged?.Invoke();
    }

    private void RevealActionButtons()
    {
        EnsureGameState();

        foreach (LocationActionButton actionButton in actionButtons)
        {
            if (actionButton == null ||
                actionButton.Action == null)
            {
                continue;
            }

            bool isAvailable =
                actionButton.Action.AreRequirementsMet(
                    gameState);

            bool shouldBeVisible =
                isAvailable ||
                !actionButton.Action.HideWhenRequirementsNotMet;

            actionButton.SetAvailable(
                isAvailable);

            actionButton.gameObject.SetActive(
                shouldBeVisible);
        }

        ActionsChanged?.Invoke();
        SelectionChanged?.Invoke();
    }

    private bool HasVisibleActionButtons()
    {
        foreach (LocationActionButton actionButton in actionButtons)
        {
            if (actionButton != null &&
                actionButton.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        return false;
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

        if (isTypewriting)
        {
            CompleteCurrentTypewriter();
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
         * Apply the action's general game-state effects before resolving
         * the action and before creating its follow-up choices.
         *
         * Move-result-specific effects are applied separately after the
         * move result has been determined.
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
            action.Description,
            action.Speaker);

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

        /*
         * Apply the effects belonging specifically to the rolled result.
         *
         * This happens before the result narrative and follow-up actions
         * are created so that those follow-up actions evaluate their
         * requirements against the newly updated game state.
         */
        action.ApplyResultEffects(
            resolution.Result,
            gameState);

        AppendMoveRoll(
            move,
            resolution);

        AppendNarrative(
            action.GetResultText(
                resolution.Result),
            action.GetResultSpeaker(
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

            bool shouldBeVisible =
                isAvailable ||
                !actionButton.Action.HideWhenRequirementsNotMet;

            actionButton.gameObject.SetActive(
                shouldBeVisible);

            actionButton.SetAvailable(
                isAvailable);

            if (i == selectedActionIndex &&
                (!isAvailable ||
                 !shouldBeVisible))
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