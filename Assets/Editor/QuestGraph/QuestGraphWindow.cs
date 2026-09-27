using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestGraphWindow : EditorWindow
{
    private QuestGraphView graphView;
    private ObjectField questField;
    private QuestDefinition selectedQuest;
    private Label validationLabel;
    private ToolbarSearchField actionSearchField;
    private string actionSearchText = string.Empty;

    [MenuItem("Window/Narrative/Quest Graph")]
    public static void OpenWindow()
    {
        QuestGraphWindow window =
            GetWindow<QuestGraphWindow>();

        window.titleContent =
            new GUIContent("Quest Graph");

        window.minSize =
            new Vector2(
                900f,
                550f);
    }

    private void OnEnable()
    {
        BuildWindow();
    }

    private void OnDisable()
    {
        if (graphView != null &&
            graphView.parent != null)
        {
            graphView.RemoveFromHierarchy();
        }

        graphView = null;
    }

    private void BuildWindow()
    {
        rootVisualElement.Clear();

        BuildToolbar();
        BuildGraph();
    }

    private void BuildToolbar()
    {
        UnityEditor.UIElements.Toolbar toolbar =
            new UnityEditor.UIElements.Toolbar();

        questField =
            new ObjectField("Quest")
            {
                objectType =
                    typeof(QuestDefinition),

                allowSceneObjects =
                    false,

                value =
                    selectedQuest
            };

        questField.style.minWidth =
            300f;

        questField.RegisterValueChangedCallback(
            evt =>
            {
                selectedQuest =
                    evt.newValue as QuestDefinition;

                RefreshGraph();
            });

        toolbar.Add(
            questField);

        Button refreshButton =
            new Button(
                RefreshGraph)
            {
                text =
                    "Refresh"
            };

        toolbar.Add(
            refreshButton);

        Button frameAllButton =
            new Button(
                FrameAll)
            {
                text =
                    "Frame All"
            };

        toolbar.Add(
            frameAllButton);

        Button addStageButton =
            new Button(
                AddStage)
            {
                text =
                    "Add Stage"
            };

        toolbar.Add(
            addStageButton);

        Button addStartActionButton =
            new Button(
                AddStartAction)
            {
                text =
                    "Add Start Action"
            };

        toolbar.Add(
            addStartActionButton);

        actionSearchField =
            new ToolbarSearchField();

        actionSearchField.value =
            actionSearchText;

        actionSearchField.style.width =
            220f;

        actionSearchField.RegisterValueChangedCallback(
            evt =>
            {
                actionSearchText =
                    evt.newValue ?? string.Empty;

                RefreshGraph();
            });

        toolbar.Add(
            actionSearchField);

        validationLabel =
            new Label(
                "Validation: select a quest");

        validationLabel.style.marginLeft =
            12f;

        validationLabel.style.unityFontStyleAndWeight =
            FontStyle.Bold;

        toolbar.Add(
            validationLabel);

        rootVisualElement.Add(
            toolbar);
    }

    private void BuildGraph()
    {
        graphView =
            new QuestGraphView(
                RefreshGraph);

        graphView.style.flexGrow =
            1f;

        rootVisualElement.Add(
            graphView);

        RefreshGraph();
    }

    private void RefreshGraph()
    {
        if (graphView == null)
        {
            return;
        }

        graphView.BuildQuest(
            selectedQuest,
            actionSearchText);

        UpdateValidationLabel();
    }

    private void UpdateValidationLabel()
    {
        if (validationLabel == null)
        {
            return;
        }

        if (selectedQuest == null)
        {
            validationLabel.text =
                "Validation: select a quest";

            return;
        }

        int issueCount =
            graphView != null
                ? graphView.ValidationIssueCount
                : 0;

        validationLabel.text =
            issueCount == 0
                ? "Validation: OK"
                : $"Validation: {issueCount} issue(s)";
    }

    private void AddStage()
    {
        if (selectedQuest == null)
        {
            EditorUtility.DisplayDialog(
                "Quest Graph",
                "Select a quest first.",
                "OK");

            return;
        }

        QuestGraphAuthoring.AddStage(
            selectedQuest);

        RefreshGraph();
    }

    private void AddStartAction()
    {
        if (selectedQuest == null)
        {
            EditorUtility.DisplayDialog(
                "Quest Graph",
                "Select a quest first.",
                "OK");

            return;
        }

        LocationActionDefinition action =
            QuestGraphAuthoring.CreateQuestAction(
                selectedQuest,
                -1);

        if (action != null)
        {
            Selection.activeObject =
                action;

            EditorGUIUtility.PingObject(
                action);

            RefreshGraph();
        }
    }

    private void FrameAll()
    {
        if (graphView == null)
        {
            return;
        }

        graphView.FrameAll();
    }
}

public class QuestGraphView : GraphView
{
    private const float StateNodeWidth =
        260f;

    private const float StateNodeHeight =
        120f;

    private const float ActionNodeWidth =
        340f;

    private const float ActionNodeHeight =
        180f;

    private const float StateX =
        250f;

    private const float ActionX =
        700f;

    private const float TerminalActionX =
        1120f;

    private const float StartY =
        100f;

    private const float RowSpacing =
        300f;

    private const float ActionVerticalGap =
        30f;

    private readonly List<QuestActionReference>
        actionReferences =
            new List<QuestActionReference>();

    private readonly List<string>
        validationMessages =
            new List<string>();

    public int ValidationIssueCount =>
        validationMessages.Count;

    private readonly System.Action requestRefresh;
    private QuestDefinition currentQuest;

    public QuestGraphView(
        System.Action refreshCallback)
    {
        requestRefresh =
            refreshCallback;
        style.flexGrow =
            1f;

        SetupZoom(
            ContentZoomer.DefaultMinScale,
            ContentZoomer.DefaultMaxScale);

        this.AddManipulator(
            new ContentDragger());

        this.AddManipulator(
            new SelectionDragger());

        this.AddManipulator(
            new RectangleSelector());

        this.AddManipulator(
            new ContextualMenuManipulator(
                BuildContextMenu));

        GridBackground grid =
            new GridBackground();

        Insert(
            0,
            grid);

        grid.StretchToParentSize();
    }

    private void BuildContextMenu(
        ContextualMenuPopulateEvent evt)
    {
        if (currentQuest == null)
        {
            evt.menu.AppendAction(
                "Select a Quest first",
                _ => { },
                DropdownMenuAction.Status.Disabled);

            return;
        }

        Vector2 mousePosition =
            contentViewContainer.WorldToLocal(
                evt.mousePosition);

        int sourceStageIndex =
            FindNearestSourceStage(
                mousePosition);

        evt.menu.AppendSeparator();

        AppendCreateAction(
            evt,
            "Create/Unaligned Action",
            ActionApproach.None,
            sourceStageIndex);

        AppendCreateAction(
            evt,
            "Create/Valor Action",
            ActionApproach.Valor,
            sourceStageIndex);

        AppendCreateAction(
            evt,
            "Create/Wit Action",
            ActionApproach.Wit,
            sourceStageIndex);

        AppendCreateAction(
            evt,
            "Create/Soul Action",
            ActionApproach.Soul,
            sourceStageIndex);

        AppendCreateAction(
            evt,
            "Create/Shadow Action",
            ActionApproach.Shadow,
            sourceStageIndex);

        AppendCreateAction(
            evt,
            "Create/Fortune Action",
            ActionApproach.Fortune,
            sourceStageIndex);
    }

    private void AppendCreateAction(
        ContextualMenuPopulateEvent evt,
        string menuName,
        ActionApproach approach,
        int sourceStageIndex)
    {
        evt.menu.AppendAction(
            menuName,
            _ =>
            {
                LocationActionDefinition action =
                    QuestGraphAuthoring.CreateQuestAction(
                        currentQuest,
                        sourceStageIndex,
                        approach);

                if (action == null)
                {
                    return;
                }

                Selection.activeObject =
                    action;

                EditorGUIUtility.PingObject(
                    action);

                requestRefresh?.Invoke();
            });
    }

    private int FindNearestSourceStage(
        Vector2 graphPosition)
    {
        QuestStateNode nearest =
            null;

        float nearestDistance =
            float.MaxValue;

        foreach (QuestStateNode node
                 in nodes.ToList().OfType<QuestStateNode>())
        {
            if (!node.CanCreateActions)
            {
                continue;
            }

            Vector2 center =
                node.GetPosition().center;

            float distance =
                Vector2.SqrMagnitude(
                    center - graphPosition);

            if (distance >= nearestDistance)
            {
                continue;
            }

            nearestDistance =
                distance;

            nearest =
                node;
        }

        if (nearest != null)
        {
            return nearest.StageIndex;
        }

        return currentQuest.StageCount > 0
            ? 0
            : -1;
    }

    public void BuildQuest(
        QuestDefinition quest,
        string actionSearchText = "")
    {
        ClearGraph();

        currentQuest =
            quest;

        actionReferences.Clear();
        validationMessages.Clear();

        if (quest == null)
        {
            AddEmptyMessage();
            return;
        }

        Dictionary<int, QuestStateNode>
            stageNodes =
                new Dictionary<int, QuestStateNode>();

        QuestStateNode notStartedNode =
            CreateStateNode(
                "NOT STARTED",
                "Quest has not started.",
                new Vector2(
                    StateX,
                    StartY),
                -1,
                true);

        for (int i = 0;
             i < quest.StageCount;
             i++)
        {
            QuestDefinition.QuestStage stage =
                quest.GetStage(
                    i);

            string stageName =
                stage != null &&
                !string.IsNullOrWhiteSpace(
                    stage.DisplayName)
                    ? stage.DisplayName
                    : $"Stage {i}";

            string description =
                stage != null
                    ? stage.Description
                    : string.Empty;

            QuestStateNode stageNode =
                CreateStateNode(
                    $"{i} — {stageName}",
                    description,
                    new Vector2(
                        StateX,
                        GetStageY(i)),
                    i,
                    true);

            stageNodes.Add(
                i,
                stageNode);
        }

        float endingY =
            GetEndingY(
                quest);

        QuestStateNode completedNode =
            CreateStateNode(
                "COMPLETED",
                "Quest has been completed.",
                new Vector2(
                    StateX - 190f,
                    endingY),
                -2,
                false);

        QuestStateNode failedNode =
            CreateStateNode(
                "FAILED",
                "Quest has failed.",
                new Vector2(
                    StateX + 190f,
                    endingY),
                -3,
                false);

        CreateBackgroundStateConnections(
            quest,
            notStartedNode,
            stageNodes,
            completedNode,
            failedNode);

        ScanActions(
            quest);

        ValidateQuest(
            quest);

        List<QuestActionReference> visibleActionReferences =
            GetVisibleActionReferences(
                actionSearchText);

        CreateActionNodes(
            quest,
            visibleActionReferences,
            notStartedNode,
            stageNodes,
            completedNode,
            failedNode);

        AddValidationNode();
        AddLegendNode();

        schedule.Execute(
                () =>
                {
                    FrameAll();
                })
            .ExecuteLater(
                50);
    }

    private void CreateBackgroundStateConnections(
        QuestDefinition quest,
        QuestStateNode notStartedNode,
        Dictionary<int, QuestStateNode> stageNodes,
        QuestStateNode completedNode,
        QuestStateNode failedNode)
    {
        if (quest.StageCount == 0)
        {
            ConnectBackgroundNodes(
                notStartedNode.Output,
                completedNode.Input);

            ConnectBackgroundNodes(
                notStartedNode.Output,
                failedNode.Input);

            return;
        }

        QuestStateNode firstStage =
            GetStageNode(
                stageNodes,
                0);

        if (firstStage != null)
        {
            ConnectBackgroundNodes(
                notStartedNode.Output,
                firstStage.Input);
        }

        for (int i = 0;
             i < quest.StageCount - 1;
             i++)
        {
            QuestStateNode current =
                GetStageNode(
                    stageNodes,
                    i);

            QuestStateNode next =
                GetStageNode(
                    stageNodes,
                    i + 1);

            if (current == null ||
                next == null)
            {
                continue;
            }

            ConnectBackgroundNodes(
                current.Output,
                next.Input);
        }

        QuestStateNode lastStage =
            GetStageNode(
                stageNodes,
                quest.StageCount - 1);

        if (lastStage != null)
        {
            ConnectBackgroundNodes(
                lastStage.Output,
                completedNode.Input);

            ConnectBackgroundNodes(
                lastStage.Output,
                failedNode.Input);
        }
    }

    private void ScanActions(
        QuestDefinition quest)
    {
        string[] actionGuids =
            AssetDatabase.FindAssets(
                "t:LocationActionDefinition");

        foreach (string guid in actionGuids)
        {
            string assetPath =
                AssetDatabase.GUIDToAssetPath(
                    guid);

            LocationActionDefinition action =
                AssetDatabase.LoadAssetAtPath
                    <LocationActionDefinition>(
                        assetPath);

            if (action == null)
            {
                continue;
            }

            ScanRequirements(
                action,
                quest);

            ScanEffects(
                action,
                action.Effects,
                quest,
                QuestActionEffectSource.General);

            ScanEffects(
                action,
                action.StrongHitEffects,
                quest,
                QuestActionEffectSource.StrongHit);

            ScanEffects(
                action,
                action.WeakHitEffects,
                quest,
                QuestActionEffectSource.WeakHit);

            ScanEffects(
                action,
                action.MissEffects,
                quest,
                QuestActionEffectSource.Miss);
        }
    }

    private void ScanRequirements(
        LocationActionDefinition action,
        QuestDefinition quest)
    {
        if (action.Requirements == null)
        {
            return;
        }

        foreach (GameCondition condition
                 in action.Requirements)
        {
            QuestCondition questCondition =
                condition as QuestCondition;

            if (questCondition == null)
            {
                continue;
            }

            if (questCondition.Quest != quest)
            {
                continue;
            }

            QuestActionReference reference =
                GetOrCreateReference(
                    action);

            reference.Requirements.Add(
                questCondition);
        }
    }

    private void ScanEffects(
        LocationActionDefinition action,
        IReadOnlyList<GameEffect> effects,
        QuestDefinition quest,
        QuestActionEffectSource source)
    {
        if (effects == null)
        {
            return;
        }

        foreach (GameEffect effect in effects)
        {
            ModifyQuestEffect questEffect =
                effect as ModifyQuestEffect;

            if (questEffect == null)
            {
                continue;
            }

            if (questEffect.Quest != quest)
            {
                continue;
            }

            QuestActionReference reference =
                GetOrCreateReference(
                    action);

            reference.Effects.Add(
                new QuestEffectReference(
                    questEffect,
                    source));
        }
    }

    private QuestActionReference GetOrCreateReference(
        LocationActionDefinition action)
    {
        QuestActionReference existing =
            actionReferences.FirstOrDefault(
                reference =>
                    reference.Action == action);

        if (existing != null)
        {
            return existing;
        }

        QuestActionReference created =
            new QuestActionReference(
                action);

        actionReferences.Add(
            created);

        return created;
    }

    private void ValidateQuest(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            return;
        }

        if (quest.StageCount == 0)
        {
            AddValidationIssue(
                "Quest has no stages.");

            return;
        }

        foreach (QuestActionReference reference
                 in actionReferences)
        {
            ValidateActionReference(
                reference,
                quest);
        }

        ValidateQuestStart(
            quest);

        ValidateStageProgression(
            quest);

        ValidateReachability(
            quest);
    }

    private void ValidateActionReference(
        QuestActionReference reference,
        QuestDefinition quest)
    {
        if (reference == null)
        {
            return;
        }

        foreach (QuestCondition requirement
                 in reference.Requirements)
        {
            if (requirement == null)
            {
                continue;
            }

            if (requirement.Condition !=
                    QuestConditionMode.AtStage &&
                requirement.Condition !=
                    QuestConditionMode.AtOrAfterStage)
            {
                continue;
            }

            if (IsValidStageIndex(
                    quest,
                    requirement.StageIndex))
            {
                continue;
            }

            AddActionValidationIssue(
                reference,
                $"Invalid requirement stage index {requirement.StageIndex}.");
        }

        foreach (QuestEffectReference effectReference
                 in reference.Effects)
        {
            if (effectReference == null ||
                effectReference.Effect == null)
            {
                continue;
            }

            ModifyQuestEffect effect =
                effectReference.Effect;

            if (effect.Modification ==
                    QuestModification.SetStage &&
                !IsValidStageIndex(
                    quest,
                    effect.StageIndex))
            {
                AddActionValidationIssue(
                    reference,
                    $"SetStage points to invalid stage index {effect.StageIndex}.");
            }
        }

        bool hasAdvance =
            reference.Effects.Any(
                effect =>
                    effect != null &&
                    effect.Effect != null &&
                    effect.Effect.Modification ==
                    QuestModification.Advance);

        bool hasExactStageRequirement =
            reference.Requirements.Any(
                requirement =>
                    requirement != null &&
                    requirement.Condition ==
                    QuestConditionMode.AtStage &&
                    IsValidStageIndex(
                        quest,
                        requirement.StageIndex));

        if (hasAdvance &&
            !hasExactStageRequirement)
        {
            AddActionValidationIssue(
                reference,
                "Advance is ambiguous because the action has no valid AtStage requirement.");
        }

        if (reference.Effects.Count > 0 &&
            reference.Requirements.Count == 0)
        {
            AddActionValidationIssue(
                reference,
                "Action modifies this quest but has no requirement for this quest.");
        }
    }

    private void ValidateQuestStart(
        QuestDefinition quest)
    {
        bool hasStart =
            actionReferences.Any(
                reference =>
                    reference.Effects.Any(
                        effect =>
                            effect != null &&
                            effect.Effect != null &&
                            effect.Effect.Modification ==
                            QuestModification.Start));

        bool hasSetStageZero =
            actionReferences.Any(
                reference =>
                    reference.Effects.Any(
                        effect =>
                            effect != null &&
                            effect.Effect != null &&
                            effect.Effect.Modification ==
                            QuestModification.SetStage &&
                            effect.Effect.StageIndex == 0));

        if (!hasStart &&
            !hasSetStageZero)
        {
            AddValidationIssue(
                "No discovered action starts the quest or sets it to Stage 0.");
        }
    }

    private void ValidateStageProgression(
        QuestDefinition quest)
    {
        for (int stageIndex = 0;
             stageIndex < quest.StageCount;
             stageIndex++)
        {
            bool hasProgression =
                actionReferences.Any(
                    reference =>
                        ActionProgressesFromStage(
                            reference,
                            quest,
                            stageIndex));

            if (hasProgression)
            {
                continue;
            }

            string stageName =
                quest.GetStageDisplayName(
                    stageIndex);

            if (string.IsNullOrWhiteSpace(
                    stageName))
            {
                stageName =
                    $"Stage {stageIndex}";
            }

            AddValidationIssue(
                $"{stageName} has no discovered progression action.");
        }
    }

    private bool ActionProgressesFromStage(
        QuestActionReference reference,
        QuestDefinition quest,
        int stageIndex)
    {
        if (reference == null)
        {
            return false;
        }

        bool availableAtStage =
            reference.Requirements.Any(
                requirement =>
                    requirement != null &&
                    requirement.Condition ==
                    QuestConditionMode.AtStage &&
                    requirement.StageIndex ==
                    stageIndex);

        if (!availableAtStage)
        {
            return false;
        }

        foreach (QuestEffectReference effectReference
                 in reference.Effects)
        {
            if (effectReference == null ||
                effectReference.Effect == null)
            {
                continue;
            }

            ModifyQuestEffect effect =
                effectReference.Effect;

            switch (effect.Modification)
            {
                case QuestModification.Advance:
                case QuestModification.Complete:
                    return true;

                case QuestModification.SetStage:
                    if (IsValidStageIndex(
                            quest,
                            effect.StageIndex) &&
                        effect.StageIndex >
                        stageIndex)
                    {
                        return true;
                    }

                    break;
            }
        }

        return false;
    }

    private void ValidateReachability(
        QuestDefinition quest)
    {
        if (quest == null ||
            quest.StageCount == 0)
        {
            return;
        }

        HashSet<int> reachableStages =
            new HashSet<int>();

        bool changed =
            true;

        while (changed)
        {
            changed =
                false;

            foreach (QuestActionReference reference
                     in actionReferences)
            {
                if (reference == null)
                {
                    continue;
                }

                if (!CanActionBeReached(
                        reference,
                        quest,
                        reachableStages))
                {
                    continue;
                }

                foreach (QuestEffectReference effectReference
                         in reference.Effects)
                {
                    if (effectReference == null ||
                        effectReference.Effect == null)
                    {
                        continue;
                    }

                    ModifyQuestEffect effect =
                        effectReference.Effect;

                    switch (effect.Modification)
                    {
                        case QuestModification.Start:
                            if (quest.StageCount > 0 &&
                                reachableStages.Add(
                                    0))
                            {
                                changed =
                                    true;
                            }

                            break;

                        case QuestModification.SetStage:
                            if (IsValidStageIndex(
                                    quest,
                                    effect.StageIndex) &&
                                reachableStages.Add(
                                    effect.StageIndex))
                            {
                                changed =
                                    true;
                            }

                            break;

                        case QuestModification.Advance:
                            foreach (QuestCondition requirement
                                     in reference.Requirements)
                            {
                                if (requirement == null ||
                                    requirement.Condition !=
                                    QuestConditionMode.AtStage ||
                                    !IsValidStageIndex(
                                        quest,
                                        requirement.StageIndex))
                                {
                                    continue;
                                }

                                int nextStage =
                                    requirement.StageIndex + 1;

                                if (nextStage <
                                        quest.StageCount &&
                                    reachableStages.Add(
                                        nextStage))
                                {
                                    changed =
                                        true;
                                }
                            }

                            break;
                    }
                }
            }
        }

        for (int stageIndex = 0;
             stageIndex < quest.StageCount;
             stageIndex++)
        {
            if (reachableStages.Contains(
                    stageIndex))
            {
                continue;
            }

            string stageName =
                quest.GetStageDisplayName(
                    stageIndex);

            if (string.IsNullOrWhiteSpace(
                    stageName))
            {
                stageName =
                    $"Stage {stageIndex}";
            }

            AddValidationIssue(
                $"{stageName} is unreachable from discovered quest actions.");
        }
    }

    private bool CanActionBeReached(
        QuestActionReference reference,
        QuestDefinition quest,
        HashSet<int> reachableStages)
    {
        if (reference == null)
        {
            return false;
        }

        if (reference.Requirements.Count == 0)
        {
            return true;
        }

        foreach (QuestCondition requirement
                 in reference.Requirements)
        {
            if (requirement == null)
            {
                continue;
            }

            switch (requirement.Condition)
            {
                case QuestConditionMode.NotStarted:
                    return true;

                case QuestConditionMode.AtStage:
                    if (IsValidStageIndex(
                            quest,
                            requirement.StageIndex) &&
                        reachableStages.Contains(
                            requirement.StageIndex))
                    {
                        return true;
                    }

                    break;

                case QuestConditionMode.AtOrAfterStage:
                    if (!IsValidStageIndex(
                            quest,
                            requirement.StageIndex))
                    {
                        break;
                    }

                    foreach (int reachableStage
                             in reachableStages)
                    {
                        if (reachableStage >=
                            requirement.StageIndex)
                        {
                            return true;
                        }
                    }

                    break;

                case QuestConditionMode.Active:
                    if (reachableStages.Count > 0)
                    {
                        return true;
                    }

                    break;

                case QuestConditionMode.Completed:
                case QuestConditionMode.Failed:
                    break;
            }
        }

        return false;
    }

    private static bool IsValidStageIndex(
        QuestDefinition quest,
        int stageIndex)
    {
        return quest != null &&
               stageIndex >= 0 &&
               stageIndex < quest.StageCount;
    }

    private void AddActionValidationIssue(
        QuestActionReference reference,
        string message)
    {
        if (reference == null ||
            string.IsNullOrWhiteSpace(
                message))
        {
            return;
        }

        if (!reference.ValidationMessages.Contains(
                message))
        {
            reference.ValidationMessages.Add(
                message);
        }

        string actionName =
            GetActionDisplayName(
                reference.Action);

        AddValidationIssue(
            $"{actionName}: {message}");
    }

    private void AddValidationIssue(
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            return;
        }

        if (!validationMessages.Contains(
                message))
        {
            validationMessages.Add(
                message);
        }
    }

    private void AddLegendNode()
    {
        Node node =
            new Node
            {
                title =
                    "GRAPH KEY"
            };

        node.capabilities &=
            ~Capabilities.Deletable;

        Label backgroundLabel =
            new Label(
                "Faint lines: conceptual quest-state order");

        backgroundLabel.style.whiteSpace =
            WhiteSpace.Normal;

        backgroundLabel.style.maxWidth =
            320f;

        backgroundLabel.style.marginBottom =
            5f;

        node.extensionContainer.Add(
            backgroundLabel);

        Label authoredLabel =
            new Label(
                "Strong lines: authored action requirements/effects");

        authoredLabel.style.whiteSpace =
            WhiteSpace.Normal;

        authoredLabel.style.maxWidth =
            320f;

        node.extensionContainer.Add(
            authoredLabel);

        node.RefreshExpandedState();

        node.SetPosition(
            new Rect(
                new Vector2(
                    -520f,
                    StartY + 300f),
                new Vector2(
                    350f,
                    110f)));

        AddElement(
            node);
    }

    private void AddValidationNode()
    {
        if (validationMessages.Count == 0)
        {
            return;
        }

        Node node =
            new Node
            {
                title =
                    $"VALIDATION — {validationMessages.Count} ISSUE(S)"
            };

        node.capabilities &=
            ~Capabilities.Deletable;

        Label explanation =
            new Label(
                "Validation uses authored quest requirements/effects, not the background state-chain lines.");

        explanation.style.whiteSpace =
            WhiteSpace.Normal;

        explanation.style.maxWidth =
            360f;

        explanation.style.marginBottom =
            8f;

        explanation.style.opacity =
            0.7f;

        node.extensionContainer.Add(
            explanation);

        foreach (string message
                 in validationMessages)
        {
            Label label =
                new Label(
                    "⚠ " +
                    message);

            label.style.whiteSpace =
                WhiteSpace.Normal;

            label.style.maxWidth =
                360f;

            label.style.marginBottom =
                5f;

            node.extensionContainer.Add(
                label);
        }

        node.RefreshExpandedState();

        node.SetPosition(
            new Rect(
                new Vector2(
                    -520f,
                    StartY),
                new Vector2(
                    390f,
                    160f)));

        AddElement(
            node);
    }

    private List<QuestActionReference> GetVisibleActionReferences(
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(
                searchText))
        {
            return new List<QuestActionReference>(
                actionReferences);
        }

        string filter =
            searchText.Trim();

        return actionReferences
            .Where(
                reference =>
                    ActionMatchesSearch(
                        reference,
                        filter))
            .ToList();
    }

    private bool ActionMatchesSearch(
        QuestActionReference reference,
        string filter)
    {
        if (reference == null ||
            string.IsNullOrWhiteSpace(
                filter))
        {
            return true;
        }

        LocationActionDefinition action =
            reference.Action;

        if (action != null)
        {
            if (ContainsIgnoreCase(
                    action.DisplayName,
                    filter) ||
                ContainsIgnoreCase(
                    action.name,
                    filter) ||
                ContainsIgnoreCase(
                    action.Description,
                    filter))
            {
                return true;
            }
        }

        foreach (QuestCondition requirement
                 in reference.Requirements)
        {
            if (requirement != null &&
                ContainsIgnoreCase(
                    requirement.GetDescription(),
                    filter))
            {
                return true;
            }
        }

        foreach (QuestEffectReference effectReference
                 in reference.Effects)
        {
            if (effectReference == null ||
                effectReference.Effect == null)
            {
                continue;
            }

            if (ContainsIgnoreCase(
                    effectReference.Effect.GetDescription(),
                    filter) ||
                ContainsIgnoreCase(
                    effectReference.Source.ToString(),
                    filter))
            {
                return true;
            }
        }

        foreach (string validationMessage
                 in reference.ValidationMessages)
        {
            if (ContainsIgnoreCase(
                    validationMessage,
                    filter))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsIgnoreCase(
        string value,
        string filter)
    {
        if (string.IsNullOrEmpty(
                value) ||
            string.IsNullOrEmpty(
                filter))
        {
            return false;
        }

        return value.IndexOf(
                   filter,
                   System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void CreateActionNodes(
        QuestDefinition quest,
        List<QuestActionReference> visibleActionReferences,
        QuestStateNode notStartedNode,
        Dictionary<int, QuestStateNode> stageNodes,
        QuestStateNode completedNode,
        QuestStateNode failedNode)
    {
        if (actionReferences.Count == 0)
        {
            AddNoReferencesNode();
            return;
        }

        if (visibleActionReferences == null ||
            visibleActionReferences.Count == 0)
        {
            AddNoMatchingActionsNode();
            return;
        }

        Dictionary<int, List<QuestActionReference>>
            rowGroups =
                new Dictionary<int, List<QuestActionReference>>();

        foreach (QuestActionReference reference
                 in visibleActionReferences
                     .OrderBy(
                         item =>
                             GetActionDisplayName(
                                 item.Action)))
        {
            int logicalRow =
                GetActionLogicalRow(
                    reference,
                    quest);

            if (!rowGroups.TryGetValue(
                    logicalRow,
                    out List<QuestActionReference> row))
            {
                row =
                    new List<QuestActionReference>();

                rowGroups.Add(
                    logicalRow,
                    row);
            }

            row.Add(
                reference);
        }

        foreach (KeyValuePair<int, List<QuestActionReference>>
                 rowPair in rowGroups
                     .OrderBy(
                         pair =>
                             pair.Key))
        {
            int logicalRow =
                rowPair.Key;

            List<QuestActionReference> references =
                rowPair.Value;

            float currentY =
                GetActionRowStartY(
                    logicalRow,
                    references.Count,
                    quest);

            foreach (QuestActionReference reference
                     in references)
            {
                float estimatedHeight =
                    EstimateActionNodeHeight(
                        reference);

                float actionX =
                    GetActionX(
                        reference,
                        quest);

                QuestActionNode actionNode =
                    new QuestActionNode(
                        reference,
                        quest,
                        requestRefresh);

                actionNode.SetPosition(
                    new Rect(
                        new Vector2(
                            actionX,
                            currentY),
                        new Vector2(
                            ActionNodeWidth,
                            estimatedHeight)));

                AddElement(
                    actionNode);

                ConnectRequirementsToAction(
                    reference,
                    actionNode,
                    notStartedNode,
                    stageNodes,
                    completedNode,
                    failedNode);

                ConnectActionEffects(
                    reference,
                    actionNode,
                    quest,
                    notStartedNode,
                    stageNodes,
                    completedNode,
                    failedNode);

                currentY +=
                    estimatedHeight +
                    ActionVerticalGap;
            }
        }
    }

    private float GetActionX(
        QuestActionReference reference,
        QuestDefinition quest)
    {
        if (reference == null)
        {
            return ActionX;
        }

        bool terminalOnly =
            reference.Effects.Count > 0 &&
            reference.Effects.All(
                effectReference =>
                {
                    if (effectReference == null ||
                        effectReference.Effect == null)
                    {
                        return false;
                    }

                    QuestModification modification =
                        effectReference.Effect.Modification;

                    return modification ==
                               QuestModification.Complete ||
                           modification ==
                               QuestModification.Fail ||
                           modification ==
                               QuestModification.Reset;
                });

        if (terminalOnly)
        {
            return TerminalActionX;
        }

        return ActionX;
    }

    private float GetActionRowStartY(
        int logicalRow,
        int actionCount,
        QuestDefinition quest)
    {
        float centerY =
            GetLogicalRowY(
                logicalRow,
                quest);

        if (actionCount <= 1)
        {
            return centerY;
        }

        float approximateGroupHeight =
            actionCount *
            (ActionNodeHeight +
             ActionVerticalGap);

        return centerY -
               approximateGroupHeight *
               0.25f;
    }

    private float GetLogicalRowY(
        int logicalRow,
        QuestDefinition quest)
    {
        if (logicalRow < 0)
        {
            return StartY;
        }

        if (logicalRow < quest.StageCount)
        {
            return GetStageY(
                logicalRow);
        }

        return GetEndingY(
            quest);
    }

    private float EstimateActionNodeHeight(
        QuestActionReference reference)
    {
        float height =
            ActionNodeHeight;

        if (reference == null)
        {
            return height;
        }

        if (reference.Action != null &&
            !string.IsNullOrWhiteSpace(
                reference.Action.Description))
        {
            height +=
                45f;
        }

        height +=
            reference.Requirements.Count *
            24f;

        height +=
            reference.Effects.Count *
            24f;

        height +=
            reference.ValidationMessages.Count *
            42f;

        if (reference.Requirements.Count > 0)
        {
            height +=
                25f;
        }

        if (reference.Effects.Count > 0)
        {
            height +=
                25f;
        }

        return Mathf.Max(
            ActionNodeHeight,
            height);
    }

    private int GetActionLogicalRow(
        QuestActionReference reference,
        QuestDefinition quest)
    {
        foreach (QuestCondition requirement
                 in reference.Requirements)
        {
            switch (requirement.Condition)
            {
                case QuestConditionMode.NotStarted:
                    return -1;

                case QuestConditionMode.AtStage:
                case QuestConditionMode.AtOrAfterStage:
                    return Mathf.Clamp(
                        requirement.StageIndex,
                        0,
                        Mathf.Max(
                            0,
                            quest.StageCount - 1));

                case QuestConditionMode.Completed:
                    return quest.StageCount;

                case QuestConditionMode.Failed:
                    return quest.StageCount + 1;
            }
        }

        foreach (QuestEffectReference effectReference
                 in reference.Effects)
        {
            switch (effectReference.Effect.Modification)
            {
                case QuestModification.Start:
                    return -1;

                case QuestModification.SetStage:
                    return Mathf.Clamp(
                        effectReference.Effect.StageIndex,
                        0,
                        Mathf.Max(
                            0,
                            quest.StageCount - 1));

                case QuestModification.Complete:
                    return quest.StageCount;

                case QuestModification.Fail:
                    return quest.StageCount + 1;
            }
        }

        return 0;
    }

    private void ConnectRequirementsToAction(
        QuestActionReference reference,
        QuestActionNode actionNode,
        QuestStateNode notStartedNode,
        Dictionary<int, QuestStateNode> stageNodes,
        QuestStateNode completedNode,
        QuestStateNode failedNode)
    {
        foreach (QuestCondition requirement
                 in reference.Requirements)
        {
            QuestStateNode sourceNode =
                GetRequirementStateNode(
                    requirement,
                    notStartedNode,
                    stageNodes,
                    completedNode,
                    failedNode);

            if (sourceNode == null)
            {
                continue;
            }

            ConnectNodes(
                sourceNode.Output,
                actionNode.Input);
        }
    }

    private void ConnectActionEffects(
        QuestActionReference reference,
        QuestActionNode actionNode,
        QuestDefinition quest,
        QuestStateNode notStartedNode,
        Dictionary<int, QuestStateNode> stageNodes,
        QuestStateNode completedNode,
        QuestStateNode failedNode)
    {
        foreach (QuestEffectReference effectReference
                 in reference.Effects)
        {
            QuestStateNode destinationNode =
                GetEffectDestinationNode(
                    reference,
                    effectReference.Effect,
                    quest,
                    notStartedNode,
                    stageNodes,
                    completedNode,
                    failedNode);

            if (destinationNode == null)
            {
                continue;
            }

            ConnectNodes(
                actionNode.Output,
                destinationNode.Input);
        }
    }

    private QuestStateNode GetRequirementStateNode(
        QuestCondition requirement,
        QuestStateNode notStartedNode,
        Dictionary<int, QuestStateNode> stageNodes,
        QuestStateNode completedNode,
        QuestStateNode failedNode)
    {
        switch (requirement.Condition)
        {
            case QuestConditionMode.NotStarted:
                return notStartedNode;

            case QuestConditionMode.Completed:
                return completedNode;

            case QuestConditionMode.Failed:
                return failedNode;

            case QuestConditionMode.AtStage:
                return GetStageNode(
                    stageNodes,
                    requirement.StageIndex);

            case QuestConditionMode.AtOrAfterStage:
                return GetStageNode(
                    stageNodes,
                    requirement.StageIndex);

            case QuestConditionMode.Active:
                return null;

            default:
                return null;
        }
    }

    private QuestStateNode GetEffectDestinationNode(
        QuestActionReference reference,
        ModifyQuestEffect effect,
        QuestDefinition quest,
        QuestStateNode notStartedNode,
        Dictionary<int, QuestStateNode> stageNodes,
        QuestStateNode completedNode,
        QuestStateNode failedNode)
    {
        switch (effect.Modification)
        {
            case QuestModification.Start:
                return GetStageNode(
                    stageNodes,
                    0);

            case QuestModification.SetStage:
                return GetStageNode(
                    stageNodes,
                    effect.StageIndex);

            case QuestModification.Complete:
                return completedNode;

            case QuestModification.Fail:
                return failedNode;

            case QuestModification.Reset:
                return notStartedNode;

            case QuestModification.Advance:
                return GetAdvanceDestinationNode(
                    reference,
                    quest,
                    stageNodes,
                    completedNode);

            default:
                return null;
        }
    }

    private QuestStateNode GetAdvanceDestinationNode(
        QuestActionReference reference,
        QuestDefinition quest,
        Dictionary<int, QuestStateNode> stageNodes,
        QuestStateNode completedNode)
    {
        QuestCondition stageRequirement =
            reference.Requirements.FirstOrDefault(
                requirement =>
                    requirement.Condition ==
                    QuestConditionMode.AtStage);

        if (stageRequirement == null)
        {
            return null;
        }

        int currentStage =
            stageRequirement.StageIndex;

        if (currentStage < 0 ||
            currentStage >= quest.StageCount)
        {
            return null;
        }

        int nextStage =
            currentStage + 1;

        if (nextStage >= quest.StageCount)
        {
            return completedNode;
        }

        return GetStageNode(
            stageNodes,
            nextStage);
    }

    private QuestStateNode GetStageNode(
        Dictionary<int, QuestStateNode> stageNodes,
        int stageIndex)
    {
        if (stageNodes.TryGetValue(
                stageIndex,
                out QuestStateNode node))
        {
            return node;
        }

        return null;
    }

    private QuestStateNode CreateStateNode(
        string title,
        string description,
        Vector2 position,
        int stageIndex,
        bool allowActionCreation)
    {
        QuestStateNode node =
            new QuestStateNode(
                title,
                description,
                currentQuest,
                stageIndex,
                allowActionCreation,
                requestRefresh);

        node.SetPosition(
            new Rect(
                position,
                new Vector2(
                    StateNodeWidth,
                    StateNodeHeight)));

        AddElement(
            node);

        return node;
    }

    private void ConnectBackgroundNodes(
        Port output,
        Port input)
    {
        if (output == null ||
            input == null)
        {
            return;
        }

        Edge edge =
            output.ConnectTo(
                input);

        edge.pickingMode =
            PickingMode.Ignore;

        edge.style.opacity =
            0.22f;

        edge.style.width =
            1f;

        AddElement(
            edge);

        edge.SendToBack();
    }

    private void ConnectNodes(
        Port output,
        Port input)
    {
        if (output == null ||
            input == null)
        {
            return;
        }

        Edge edge =
            output.ConnectTo(
                input);

        edge.pickingMode =
            PickingMode.Ignore;

        edge.style.opacity =
            0.95f;

        edge.style.width =
            2f;

        AddElement(
            edge);
    }

    private float GetStageY(
        int stageIndex)
    {
        return StartY +
               (stageIndex + 1) *
               RowSpacing;
    }

    private float GetEndingY(
        QuestDefinition quest)
    {
        return StartY +
               (quest.StageCount + 1) *
               RowSpacing;
    }

    private void ClearGraph()
    {
        DeleteElements(
            graphElements.ToList());
    }

    private void AddEmptyMessage()
    {
        Node emptyNode =
            new Node
            {
                title =
                    "Select a Quest"
            };

        Label message =
            new Label(
                "Choose a Quest Definition from the toolbar.");

        message.style.whiteSpace =
            WhiteSpace.Normal;

        message.style.paddingLeft =
            8f;

        message.style.paddingRight =
            8f;

        message.style.paddingTop =
            8f;

        message.style.paddingBottom =
            8f;

        emptyNode.extensionContainer.Add(
            message);

        emptyNode.RefreshExpandedState();

        emptyNode.SetPosition(
            new Rect(
                new Vector2(
                    StateX,
                    StartY),
                new Vector2(
                    StateNodeWidth,
                    100f)));

        AddElement(
            emptyNode);
    }

    private void AddNoMatchingActionsNode()
    {
        Node node =
            new Node
            {
                title =
                    "No Matching Actions"
            };

        Label message =
            new Label(
                "The quest has referenced actions, but none match the current search.");

        message.style.whiteSpace =
            WhiteSpace.Normal;

        message.style.maxWidth =
            280f;

        message.style.paddingLeft =
            8f;

        message.style.paddingRight =
            8f;

        message.style.paddingTop =
            8f;

        message.style.paddingBottom =
            8f;

        node.extensionContainer.Add(
            message);

        node.RefreshExpandedState();

        node.SetPosition(
            new Rect(
                new Vector2(
                    ActionX,
                    StartY),
                new Vector2(
                    300f,
                    110f)));

        AddElement(
            node);
    }

    private void AddNoReferencesNode()
    {
        Node node =
            new Node
            {
                title =
                    "No Action References"
            };

        Label message =
            new Label(
                "No LocationActionDefinition assets currently reference this quest.");

        message.style.whiteSpace =
            WhiteSpace.Normal;

        message.style.maxWidth =
            280f;

        message.style.paddingLeft =
            8f;

        message.style.paddingRight =
            8f;

        message.style.paddingTop =
            8f;

        message.style.paddingBottom =
            8f;

        node.extensionContainer.Add(
            message);

        node.RefreshExpandedState();

        node.SetPosition(
            new Rect(
                new Vector2(
                    ActionX,
                    StartY),
                new Vector2(
                    300f,
                    110f)));

        AddElement(
            node);
    }

    private static string GetActionDisplayName(
        LocationActionDefinition action)
    {
        if (action == null)
        {
            return "Missing Action";
        }

        if (!string.IsNullOrWhiteSpace(
                action.DisplayName))
        {
            return action.DisplayName;
        }

        return action.name;
    }
}

public class QuestStateNode : Node
{
    public Port Input
    {
        get;
        private set;
    }

    public Port Output
    {
        get;
        private set;
    }

    private readonly QuestDefinition quest;
    private readonly int stageIndex;
    private readonly System.Action requestRefresh;

    public int StageIndex => stageIndex;
    public bool CanCreateActions { get; }

    public QuestStateNode(
        string nodeTitle,
        string description,
        QuestDefinition questDefinition,
        int sourceStageIndex,
        bool allowActionCreation,
        System.Action refreshCallback)
    {
        quest =
            questDefinition;

        stageIndex =
            sourceStageIndex;

        requestRefresh =
            refreshCallback;

        CanCreateActions =
            allowActionCreation;

        title =
            nodeTitle;

        tooltip =
            "Quest state. Faint state-to-state lines show conceptual order; authored transitions pass through action nodes.";

        Input =
            InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(bool));

        Input.portName =
            string.Empty;

        inputContainer.Add(
            Input);

        Output =
            InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Multi,
                typeof(bool));

        Output.portName =
            string.Empty;

        outputContainer.Add(
            Output);

        if (!string.IsNullOrWhiteSpace(
                description))
        {
            Label descriptionLabel =
                new Label(
                    description);

            descriptionLabel.style.whiteSpace =
                WhiteSpace.Normal;

            descriptionLabel.style.maxWidth =
                230f;

            descriptionLabel.style.paddingLeft =
                6f;

            descriptionLabel.style.paddingRight =
                6f;

            descriptionLabel.style.paddingTop =
                6f;

            descriptionLabel.style.paddingBottom =
                6f;

            extensionContainer.Add(
                descriptionLabel);
        }

        if (stageIndex >= 0)
        {
            Button editStageButton =
                new Button(
                    EditStage)
                {
                    text =
                        "Edit Stage"
                };

            editStageButton.style.marginTop =
                4f;

            extensionContainer.Add(
                editStageButton);
        }

        RefreshExpandedState();
        RefreshPorts();
    }

    private void EditStage()
    {
        if (quest == null ||
            stageIndex < 0)
        {
            return;
        }

        QuestStageEditorWindow.Open(
            quest,
            stageIndex,
            requestRefresh);
    }
}

public class QuestActionNode : Node
{
    public Port Input
    {
        get;
        private set;
    }

    public Port Output
    {
        get;
        private set;
    }

    private readonly QuestActionReference reference;
    private readonly QuestDefinition quest;
    private readonly System.Action requestRefresh;

    public QuestActionNode(
        QuestActionReference actionReference,
        QuestDefinition questDefinition,
        System.Action refreshCallback)
    {
        reference =
            actionReference;

        quest =
            questDefinition;

        requestRefresh =
            refreshCallback;

        title =
            GetActionName(
                reference.Action);

        Input =
            InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(bool));

        Input.portName =
            "Requires";

        inputContainer.Add(
            Input);

        Output =
            InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Multi,
                typeof(bool));

        Output.portName =
            "Changes";

        outputContainer.Add(
            Output);

        BuildContents();

        RegisterCallback<MouseDownEvent>(
            OnMouseDown);

        RefreshExpandedState();
        RefreshPorts();
    }

    private void BuildContents()
    {
        if (reference.Action != null &&
            !string.IsNullOrWhiteSpace(
                reference.Action.Description))
        {
            Label descriptionLabel =
                new Label(
                    reference.Action.Description);

            descriptionLabel.style.whiteSpace =
                WhiteSpace.Normal;

            descriptionLabel.style.maxWidth =
                310f;

            descriptionLabel.style.marginBottom =
                6f;

            extensionContainer.Add(
                descriptionLabel);
        }

        if (reference.Requirements.Count > 0)
        {
            Label heading =
                new Label(
                    "QUEST REQUIREMENTS");

            heading.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            heading.style.marginTop =
                4f;

            extensionContainer.Add(
                heading);

            foreach (QuestCondition requirement
                     in reference.Requirements)
            {
                Label requirementLabel =
                    new Label(
                        "• " +
                        requirement.GetDescription());

                requirementLabel.style.whiteSpace =
                    WhiteSpace.Normal;

                requirementLabel.style.maxWidth =
                    310f;

                extensionContainer.Add(
                    requirementLabel);
            }
        }

        if (reference.Effects.Count > 0)
        {
            Label heading =
                new Label(
                    "QUEST EFFECTS");

            heading.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            heading.style.marginTop =
                8f;

            extensionContainer.Add(
                heading);

            foreach (QuestEffectReference effectReference
                     in reference.Effects)
            {
                string prefix =
                    GetEffectSourceLabel(
                        effectReference.Source);

                Label effectLabel =
                    new Label(
                        $"• [{prefix}] " +
                        effectReference.Effect
                            .GetDescription());

                effectLabel.style.whiteSpace =
                    WhiteSpace.Normal;

                effectLabel.style.maxWidth =
                    310f;

                extensionContainer.Add(
                    effectLabel);
            }
        }

        if (reference.ValidationMessages.Count > 0)
        {
            Label validationHeading =
                new Label(
                    "VALIDATION");

            validationHeading.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            validationHeading.style.marginTop =
                8f;

            extensionContainer.Add(
                validationHeading);

            foreach (string validationMessage
                     in reference.ValidationMessages)
            {
                Label validationLabel =
                    new Label(
                        "⚠ " +
                        validationMessage);

                validationLabel.style.whiteSpace =
                    WhiteSpace.Normal;

                validationLabel.style.maxWidth =
                    310f;

                extensionContainer.Add(
                    validationLabel);
            }
        }

        if (HasAmbiguousAdvance() &&
            !reference.ValidationMessages.Any(
                message =>
                    message.Contains(
                        "Advance is ambiguous")))
        {
            Label warningLabel =
                new Label(
                    "⚠ Advance destination is ambiguous: add an AtStage quest requirement.");

            warningLabel.style.whiteSpace =
                WhiteSpace.Normal;

            warningLabel.style.maxWidth =
                310f;

            warningLabel.style.marginTop =
                8f;

            warningLabel.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            extensionContainer.Add(
                warningLabel);
        }

        VisualElement buttons =
            new VisualElement();

        buttons.style.flexDirection =
            FlexDirection.Row;

        buttons.style.marginTop =
            8f;

        Button selectButton =
            new Button(
                SelectAction)
            {
                text =
                    "Edit Action"
            };

        buttons.Add(
            selectButton);

        Button unlinkButton =
            new Button(
                RemoveQuestWiring)
            {
                text =
                    "Remove From Quest"
            };

        unlinkButton.style.marginLeft =
            4f;

        buttons.Add(
            unlinkButton);

        extensionContainer.Add(
            buttons);

        InspectorElement inlineInspector =
            new InspectorElement(
                reference.Action);

        inlineInspector.style.marginTop =
            8f;

        inlineInspector.style.maxWidth =
            390f;

        extensionContainer.Add(
            inlineInspector);

        Label hint =
            new Label(
                "Double-click or Edit Action opens the asset in the Inspector.");

        hint.style.fontSize =
            10f;

        hint.style.marginTop =
            5f;

        hint.style.opacity =
            0.65f;

        extensionContainer.Add(
            hint);
    }

    private bool HasAmbiguousAdvance()
    {
        bool hasAdvance =
            reference.Effects.Any(
                effect =>
                    effect.Effect != null &&
                    effect.Effect.Modification ==
                    QuestModification.Advance);

        if (!hasAdvance)
        {
            return false;
        }

        bool hasExactStageRequirement =
            reference.Requirements.Any(
                requirement =>
                    requirement != null &&
                    requirement.Condition ==
                    QuestConditionMode.AtStage);

        return !hasExactStageRequirement;
    }

    private void OnMouseDown(
        MouseDownEvent evt)
    {
        if (evt.button != 0 ||
            evt.clickCount < 2)
        {
            return;
        }

        SelectAction();
    }

    private void SelectAction()
    {
        if (reference.Action == null)
        {
            return;
        }

        Selection.activeObject =
            reference.Action;

        EditorGUIUtility.PingObject(
            reference.Action);
    }

    private void RemoveQuestWiring()
    {
        if (reference.Action == null ||
            quest == null)
        {
            return;
        }

        bool confirmed =
            EditorUtility.DisplayDialog(
                "Remove From Quest",
                $"Remove all requirements and effects for '{quest.DisplayName}' from '{GetActionName(reference.Action)}'?\\n\\nThe action asset itself will not be deleted.",
                "Remove",
                "Cancel");

        if (!confirmed)
        {
            return;
        }

        QuestGraphAuthoring.RemoveQuestWiring(
            reference.Action,
            quest);

        requestRefresh?.Invoke();
    }

    private static string GetActionName(
        LocationActionDefinition action)
    {
        if (action == null)
        {
            return "Missing Action";
        }

        if (!string.IsNullOrWhiteSpace(
                action.DisplayName))
        {
            return action.DisplayName;
        }

        return action.name;
    }

    private static string GetEffectSourceLabel(
        QuestActionEffectSource source)
    {
        switch (source)
        {
            case QuestActionEffectSource.General:
                return "General";

            case QuestActionEffectSource.StrongHit:
                return "Strong Hit";

            case QuestActionEffectSource.WeakHit:
                return "Weak Hit";

            case QuestActionEffectSource.Miss:
                return "Miss";

            default:
                return source.ToString();
        }
    }
}

public class QuestActionReference
{
    public LocationActionDefinition Action
    {
        get;
        private set;
    }

    public List<QuestCondition> Requirements
    {
        get;
        private set;
    }

    public List<QuestEffectReference> Effects
    {
        get;
        private set;
    }

    public List<string> ValidationMessages
    {
        get;
        private set;
    }

    public QuestActionReference(
        LocationActionDefinition action)
    {
        Action =
            action;

        Requirements =
            new List<QuestCondition>();

        Effects =
            new List<QuestEffectReference>();

        ValidationMessages =
            new List<string>();
    }
}

public class QuestEffectReference
{
    public ModifyQuestEffect Effect
    {
        get;
        private set;
    }

    public QuestActionEffectSource Source
    {
        get;
        private set;
    }

    public QuestEffectReference(
        ModifyQuestEffect effect,
        QuestActionEffectSource source)
    {
        Effect =
            effect;

        Source =
            source;
    }
}

public enum QuestActionEffectSource
{
    General,
    StrongHit,
    WeakHit,
    Miss
}


public static class QuestGraphAuthoring
{
    public static void AddStage(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            return;
        }

        Undo.RecordObject(
            quest,
            "Add Quest Stage");

        SerializedObject serializedQuest =
            new SerializedObject(
                quest);

        SerializedProperty stages =
            serializedQuest.FindProperty(
                "stages");

        int index =
            stages.arraySize;

        stages.InsertArrayElementAtIndex(
            index);

        SerializedProperty stage =
            stages.GetArrayElementAtIndex(
                index);

        SerializedProperty displayName =
            stage.FindPropertyRelative(
                "displayName");

        SerializedProperty description =
            stage.FindPropertyRelative(
                "description");

        if (displayName != null)
        {
            displayName.stringValue =
                $"Stage {index}";
        }

        if (description != null)
        {
            description.stringValue =
                string.Empty;
        }

        serializedQuest.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            quest);

        AssetDatabase.SaveAssets();
    }

    public static LocationActionDefinition CreateQuestAction(
        QuestDefinition quest,
        int sourceStageIndex,
        ActionApproach approach = ActionApproach.None)
    {
        if (quest == null)
        {
            return null;
        }

        string questPath =
            AssetDatabase.GetAssetPath(
                quest);

        string folderPath =
            System.IO.Path.GetDirectoryName(
                questPath);

        if (string.IsNullOrWhiteSpace(
                folderPath))
        {
            folderPath =
                "Assets";
        }

        folderPath =
            folderPath.Replace(
                "\\",
                "/");

        string approachName =
            approach == ActionApproach.None
                ? "Unaligned"
                : approach.ToString();

        string sourceName =
            sourceStageIndex < 0
                ? "Start"
                : $"Stage{sourceStageIndex}";

        string path =
            AssetDatabase.GenerateUniqueAssetPath(
                $"{folderPath}/{SafeName(quest.DisplayName)}_{sourceName}_{approachName}Action.asset");

        LocationActionDefinition action =
            ScriptableObject.CreateInstance
                <LocationActionDefinition>();

        Undo.RegisterCreatedObjectUndo(
            action,
            "Create Quest Action");

        AssetDatabase.CreateAsset(
            action,
            path);

        SerializedObject serializedAction =
            new SerializedObject(
                action);

        SerializedProperty actionId =
            serializedAction.FindProperty(
                "actionId");

        SerializedProperty displayName =
            serializedAction.FindProperty(
                "displayName");

        SerializedProperty approachProperty =
            serializedAction.FindProperty(
                "approach");

        string assetName =
            System.IO.Path.GetFileNameWithoutExtension(
                path);

        if (actionId != null)
        {
            actionId.stringValue =
                assetName
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(" ", "_");
        }

        if (displayName != null)
        {
            displayName.stringValue =
                sourceStageIndex < 0
                    ? $"Start {quest.DisplayName}"
                    : $"Progress {quest.DisplayName}";
        }

        if (approachProperty != null)
        {
            approachProperty.enumValueIndex =
                (int)approach;
        }

        serializedAction.ApplyModifiedProperties();

        if (sourceStageIndex < 0)
        {
            AddQuestCondition(
                action,
                quest,
                QuestConditionMode.NotStarted,
                0);

            AddQuestEffect(
                action,
                quest,
                QuestModification.Start,
                0,
                "effects");
        }
        else
        {
            AddQuestCondition(
                action,
                quest,
                QuestConditionMode.AtStage,
                sourceStageIndex);

            AddQuestEffect(
                action,
                quest,
                QuestModification.Advance,
                0,
                "effects");
        }

        EditorUtility.SetDirty(
            action);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return action;
    }

    public static void RemoveQuestWiring(
        LocationActionDefinition action,
        QuestDefinition quest)
    {
        if (action == null ||
            quest == null)
        {
            return;
        }

        Undo.RecordObject(
            action,
            "Remove Quest Wiring");

        RemoveMatchingManagedReferences(
            action,
            "requirements",
            item =>
            {
                QuestCondition condition =
                    item as QuestCondition;

                return condition != null &&
                       condition.Quest == quest;
            });

        string[] effectProperties =
        {
            "effects",
            "strongHitEffects",
            "weakHitEffects",
            "missEffects"
        };

        foreach (string propertyName
                 in effectProperties)
        {
            RemoveMatchingManagedReferences(
                action,
                propertyName,
                item =>
                {
                    ModifyQuestEffect effect =
                        item as ModifyQuestEffect;

                    return effect != null &&
                           effect.Quest == quest;
                });
        }

        EditorUtility.SetDirty(
            action);

        AssetDatabase.SaveAssets();
    }

    private static void AddQuestCondition(
        LocationActionDefinition action,
        QuestDefinition quest,
        QuestConditionMode mode,
        int stageIndex)
    {
        SerializedObject serializedAction =
            new SerializedObject(
                action);

        SerializedProperty requirements =
            serializedAction.FindProperty(
                "requirements");

        int index =
            requirements.arraySize;

        requirements.InsertArrayElementAtIndex(
            index);

        SerializedProperty element =
            requirements.GetArrayElementAtIndex(
                index);

        element.managedReferenceValue =
            new QuestCondition();

        serializedAction.ApplyModifiedProperties();

        serializedAction.Update();

        element =
            requirements.GetArrayElementAtIndex(
                index);

        SerializedProperty questProperty =
            element.FindPropertyRelative(
                "quest");

        SerializedProperty conditionProperty =
            element.FindPropertyRelative(
                "condition");

        SerializedProperty stageProperty =
            element.FindPropertyRelative(
                "stageIndex");

        questProperty.objectReferenceValue =
            quest;

        conditionProperty.enumValueIndex =
            (int)mode;

        stageProperty.intValue =
            stageIndex;

        serializedAction.ApplyModifiedProperties();
    }

    private static void AddQuestEffect(
        LocationActionDefinition action,
        QuestDefinition quest,
        QuestModification modification,
        int stageIndex,
        string propertyName)
    {
        SerializedObject serializedAction =
            new SerializedObject(
                action);

        SerializedProperty effects =
            serializedAction.FindProperty(
                propertyName);

        int index =
            effects.arraySize;

        effects.InsertArrayElementAtIndex(
            index);

        SerializedProperty element =
            effects.GetArrayElementAtIndex(
                index);

        element.managedReferenceValue =
            new ModifyQuestEffect();

        serializedAction.ApplyModifiedProperties();

        serializedAction.Update();

        element =
            effects.GetArrayElementAtIndex(
                index);

        SerializedProperty questProperty =
            element.FindPropertyRelative(
                "quest");

        SerializedProperty modificationProperty =
            element.FindPropertyRelative(
                "modification");

        SerializedProperty stageProperty =
            element.FindPropertyRelative(
                "stageIndex");

        questProperty.objectReferenceValue =
            quest;

        modificationProperty.enumValueIndex =
            (int)modification;

        stageProperty.intValue =
            stageIndex;

        serializedAction.ApplyModifiedProperties();
    }

    private static void RemoveMatchingManagedReferences(
        LocationActionDefinition action,
        string propertyName,
        System.Func<object, bool> predicate)
    {
        SerializedObject serializedAction =
            new SerializedObject(
                action);

        SerializedProperty list =
            serializedAction.FindProperty(
                propertyName);

        if (list == null ||
            !list.isArray)
        {
            return;
        }

        for (int i = list.arraySize - 1;
             i >= 0;
             i--)
        {
            SerializedProperty element =
                list.GetArrayElementAtIndex(
                    i);

            object value =
                element.managedReferenceValue;

            if (!predicate(
                    value))
            {
                continue;
            }

            list.DeleteArrayElementAtIndex(
                i);
        }

        serializedAction.ApplyModifiedProperties();
    }

    private static string SafeName(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return "Quest";
        }

        foreach (char invalid
                 in System.IO.Path.GetInvalidFileNameChars())
        {
            value =
                value.Replace(
                    invalid,
                    '_');
        }

        return value.Replace(
            " ",
            "_");
    }
}

public class QuestStageEditorWindow : EditorWindow
{
    private QuestDefinition quest;
    private int stageIndex;
    private System.Action onChanged;

    private TextField nameField;
    private TextField descriptionField;

    public static void Open(
        QuestDefinition quest,
        int stageIndex,
        System.Action onChanged)
    {
        QuestStageEditorWindow window =
            CreateInstance<QuestStageEditorWindow>();

        window.quest =
            quest;

        window.stageIndex =
            stageIndex;

        window.onChanged =
            onChanged;

        window.titleContent =
            new GUIContent(
                $"Quest Stage {stageIndex}");

        window.minSize =
            new Vector2(
                380f,
                260f);

        window.ShowUtility();
    }

    private void CreateGUI()
    {
        if (quest == null ||
            stageIndex < 0 ||
            stageIndex >= quest.StageCount)
        {
            rootVisualElement.Add(
                new Label(
                    "Quest stage is no longer valid."));

            return;
        }

        QuestDefinition.QuestStage stage =
            quest.GetStage(
                stageIndex);

        Label heading =
            new Label(
                $"Stage {stageIndex}");

        heading.style.unityFontStyleAndWeight =
            FontStyle.Bold;

        heading.style.fontSize =
            16f;

        heading.style.marginBottom =
            10f;

        rootVisualElement.Add(
            heading);

        nameField =
            new TextField(
                "Display Name");

        nameField.value =
            stage != null
                ? stage.DisplayName
                : string.Empty;

        rootVisualElement.Add(
            nameField);

        descriptionField =
            new TextField(
                "Description")
            {
                multiline =
                    true
            };

        descriptionField.value =
            stage != null
                ? stage.Description
                : string.Empty;

        descriptionField.style.height =
            120f;

        descriptionField.style.marginTop =
            8f;

        rootVisualElement.Add(
            descriptionField);

        Button saveButton =
            new Button(
                Save)
            {
                text =
                    "Save Stage"
            };

        saveButton.style.marginTop =
            10f;

        rootVisualElement.Add(
            saveButton);
    }

    private void Save()
    {
        if (quest == null ||
            stageIndex < 0)
        {
            return;
        }

        Undo.RecordObject(
            quest,
            "Edit Quest Stage");

        SerializedObject serializedQuest =
            new SerializedObject(
                quest);

        SerializedProperty stages =
            serializedQuest.FindProperty(
                "stages");

        if (stageIndex >= stages.arraySize)
        {
            return;
        }

        SerializedProperty stage =
            stages.GetArrayElementAtIndex(
                stageIndex);

        stage.FindPropertyRelative(
                "displayName").stringValue =
            nameField.value;

        stage.FindPropertyRelative(
                "description").stringValue =
            descriptionField.value;

        serializedQuest.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            quest);

        AssetDatabase.SaveAssets();

        onChanged?.Invoke();

        Close();
    }
}
