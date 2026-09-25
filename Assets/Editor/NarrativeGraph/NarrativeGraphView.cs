using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class NarrativeGraphView : GraphView
{
    private const float NodeWidth =
        400f;

    private const float HorizontalSpacing =
        420f;

    private const float VerticalSpacing =
        360f;

    private const string LocationActionsProperty =
        "actions";

    private const string StandardFollowUpProperty =
        "followUpActions";

    private const string StrongHitFollowUpProperty =
        "strongHitFollowUpActions";

    private const string WeakHitFollowUpProperty =
        "weakHitFollowUpActions";

    private const string MissFollowUpProperty =
        "missFollowUpActions";

    private const string HideWhenRequirementsNotMetProperty =
        "hideWhenRequirementsNotMet";

    private const string RequirementsProperty =
        "requirements";

    private const string EffectsProperty =
        "effects";

    private const string StrongHitEffectsProperty =
        "strongHitEffects";

    private const string WeakHitEffectsProperty =
        "weakHitEffects";

    private const string MissEffectsProperty =
        "missEffects";

    private LocationDefinition currentLocation;
    private GameUITheme currentTheme;

    private LocationNode locationNode;

    private readonly Dictionary<
        LocationActionDefinition,
        NarrativeActionNode> actionNodes =
            new Dictionary<
                LocationActionDefinition,
                NarrativeActionNode>();

    private readonly Dictionary<
        int,
        int> rowsPerDepth =
            new Dictionary<int, int>();

    private bool rebuildingGraph;

    public NarrativeGraphView()
    {
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

        GridBackground grid =
            new GridBackground();

        Insert(
            0,
            grid);

        grid.StretchToParentSize();

        this.AddManipulator(
            new ContextualMenuManipulator(
                BuildContextMenu));

        graphViewChanged =
            HandleGraphViewChanged;
    }

    public void SetTheme(
        GameUITheme theme)
    {
        currentTheme =
            theme;

        ApplyThemeToExistingNodes();
    }

    public void LoadLocation(
        LocationDefinition location)
    {
        currentLocation =
            location;

        rebuildingGraph =
            true;

        ClearGraph();

        if (currentLocation == null)
        {
            rebuildingGraph =
                false;

            return;
        }

        rowsPerDepth.Clear();

        locationNode =
            CreateLocationNode();

        LocationActionDefinition[] startingActions =
            currentLocation.Actions;

        if (startingActions != null)
        {
            foreach (LocationActionDefinition action in startingActions)
            {
                if (action == null)
                {
                    continue;
                }

                NarrativeActionNode actionNode =
                    DiscoverAction(
                        action,
                        1);

                if (actionNode == null)
                {
                    continue;
                }

                ConnectVisualEdge(
                    locationNode.ActionsPort,
                    actionNode.InputPort);
            }
        }

        rebuildingGraph =
            false;

        schedule.Execute(
            () =>
            {
                FrameAll();
            })
            .ExecuteLater(
                100);
    }

    public override List<Port> GetCompatiblePorts(
        Port startPort,
        NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts =
            new List<Port>();

        ports.ForEach(
            port =>
            {
                if (port == startPort)
                {
                    return;
                }

                if (port.node == startPort.node)
                {
                    return;
                }

                if (port.direction ==
                    startPort.direction)
                {
                    return;
                }

                if (startPort.direction ==
                    Direction.Output &&
                    port.direction ==
                    Direction.Input)
                {
                    compatiblePorts.Add(
                        port);

                    return;
                }

                if (startPort.direction ==
                    Direction.Input &&
                    port.direction ==
                    Direction.Output)
                {
                    compatiblePorts.Add(
                        port);
                }
            });

        return compatiblePorts;
    }

    private GraphViewChange HandleGraphViewChanged(
        GraphViewChange change)
    {
        if (rebuildingGraph)
        {
            return change;
        }

        if (change.edgesToCreate != null)
        {
            foreach (Edge edge in change.edgesToCreate)
            {
                AddDataConnection(
                    edge);
            }
        }

        if (change.elementsToRemove != null)
        {
            foreach (GraphElement element in change.elementsToRemove)
            {
                if (element is Edge edge)
                {
                    RemoveDataConnection(
                        edge);
                }
            }
        }

        return change;
    }

    private void AddDataConnection(
        Edge edge)
    {
        if (edge == null ||
            edge.output == null ||
            edge.input == null)
        {
            return;
        }

        NarrativeActionNode childNode =
            edge.input.node as NarrativeActionNode;

        if (childNode == null ||
            childNode.Action == null)
        {
            return;
        }

        if (edge.output.node is LocationNode)
        {
            AddObjectToArray(
                currentLocation,
                LocationActionsProperty,
                childNode.Action,
                "Add Location Action");

            return;
        }

        NarrativeActionNode parentNode =
            edge.output.node as NarrativeActionNode;

        if (parentNode == null ||
            parentNode.Action == null)
        {
            return;
        }

        string propertyName =
            GetPropertyNameForPort(
                parentNode,
                edge.output);

        if (string.IsNullOrWhiteSpace(
            propertyName))
        {
            return;
        }

        AddObjectToArray(
            parentNode.Action,
            propertyName,
            childNode.Action,
            "Add Narrative Link");
    }

    private void RemoveDataConnection(
        Edge edge)
    {
        if (edge == null ||
            edge.output == null ||
            edge.input == null)
        {
            return;
        }

        NarrativeActionNode childNode =
            edge.input.node as NarrativeActionNode;

        if (childNode == null ||
            childNode.Action == null)
        {
            return;
        }

        if (edge.output.node is LocationNode)
        {
            RemoveObjectFromArray(
                currentLocation,
                LocationActionsProperty,
                childNode.Action,
                "Remove Location Action");

            return;
        }

        NarrativeActionNode parentNode =
            edge.output.node as NarrativeActionNode;

        if (parentNode == null ||
            parentNode.Action == null)
        {
            return;
        }

        string propertyName =
            GetPropertyNameForPort(
                parentNode,
                edge.output);

        if (string.IsNullOrWhiteSpace(
            propertyName))
        {
            return;
        }

        RemoveObjectFromArray(
            parentNode.Action,
            propertyName,
            childNode.Action,
            "Remove Narrative Link");
    }

    private string GetPropertyNameForPort(
        NarrativeActionNode node,
        Port port)
    {
        if (node == null ||
            port == null)
        {
            return null;
        }

        if (port == node.FollowUpPort)
        {
            return StandardFollowUpProperty;
        }

        if (port == node.StrongHitPort)
        {
            return StrongHitFollowUpProperty;
        }

        if (port == node.WeakHitPort)
        {
            return WeakHitFollowUpProperty;
        }

        if (port == node.MissPort)
        {
            return MissFollowUpProperty;
        }

        return null;
    }

    private void AddObjectToArray(
        Object owner,
        string propertyName,
        Object value,
        string undoName)
    {
        if (owner == null ||
            value == null)
        {
            return;
        }

        SerializedObject serializedObject =
            new SerializedObject(
                owner);

        SerializedProperty array =
            serializedObject.FindProperty(
                propertyName);

        if (array == null ||
            !array.isArray)
        {
            Debug.LogError(
                $"Narrative Graph could not find array '{propertyName}' on '{owner.name}'.");

            return;
        }

        for (int i = 0;
             i < array.arraySize;
             i++)
        {
            SerializedProperty element =
                array.GetArrayElementAtIndex(
                    i);

            if (element.objectReferenceValue ==
                value)
            {
                return;
            }
        }

        Undo.RecordObject(
            owner,
            undoName);

        int newIndex =
            array.arraySize;

        array.InsertArrayElementAtIndex(
            newIndex);

        SerializedProperty newElement =
            array.GetArrayElementAtIndex(
                newIndex);

        newElement.objectReferenceValue =
            value;

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            owner);

        AssetDatabase.SaveAssets();
    }

    private void RemoveObjectFromArray(
        Object owner,
        string propertyName,
        Object value,
        string undoName)
    {
        if (owner == null ||
            value == null)
        {
            return;
        }

        SerializedObject serializedObject =
            new SerializedObject(
                owner);

        SerializedProperty array =
            serializedObject.FindProperty(
                propertyName);

        if (array == null ||
            !array.isArray)
        {
            Debug.LogError(
                $"Narrative Graph could not find array '{propertyName}' on '{owner.name}'.");

            return;
        }

        for (int i = array.arraySize - 1;
             i >= 0;
             i--)
        {
            SerializedProperty element =
                array.GetArrayElementAtIndex(
                    i);

            if (element.objectReferenceValue !=
                value)
            {
                continue;
            }

            Undo.RecordObject(
                owner,
                undoName);

            element.objectReferenceValue =
                null;

            array.DeleteArrayElementAtIndex(
                i);

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                owner);

            AssetDatabase.SaveAssets();

            return;
        }
    }

    private void ClearGraph()
    {
        List<GraphElement> elements =
            graphElements.ToList();

        if (elements.Count > 0)
        {
            DeleteElements(
                elements);
        }

        actionNodes.Clear();
        rowsPerDepth.Clear();

        locationNode =
            null;
    }

    private LocationNode CreateLocationNode()
    {
        LocationNode node =
            new LocationNode(
                currentLocation);

        node.SetPosition(
            new Rect(
                0f,
                0f,
                NodeWidth,
                150f));

        node.capabilities &=
            ~Capabilities.Deletable;

        node.RegisterCallback<MouseDownEvent>(
            evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                Selection.activeObject =
                    currentLocation;

                EditorGUIUtility.PingObject(
                    currentLocation);
            });

        AddElement(
            node);

        return node;
    }

    private NarrativeActionNode DiscoverAction(
        LocationActionDefinition action,
        int depth)
    {
        if (action == null)
        {
            return null;
        }

        if (actionNodes.TryGetValue(
            action,
            out NarrativeActionNode existingNode))
        {
            return existingNode;
        }

        Vector2 position =
            GetPositionForDepth(
                depth);

        NarrativeActionNode node =
            CreateActionNode(
                action,
                position);

        actionNodes.Add(
            action,
            node);

        if (action.RequiresMove)
        {
            DiscoverMoveBranches(
                action,
                node,
                depth + 1);
        }
        else
        {
            DiscoverStandardBranches(
                action,
                node,
                depth + 1);
        }

        return node;
    }

    private void DiscoverStandardBranches(
        LocationActionDefinition action,
        NarrativeActionNode parentNode,
        int childDepth)
    {
        LocationActionDefinition[] followUps =
            action.FollowUpActions;

        if (followUps == null)
        {
            return;
        }

        foreach (LocationActionDefinition followUp in followUps)
        {
            if (followUp == null)
            {
                continue;
            }

            NarrativeActionNode childNode =
                DiscoverAction(
                    followUp,
                    childDepth);

            if (childNode == null)
            {
                continue;
            }

            ConnectVisualEdge(
                parentNode.FollowUpPort,
                childNode.InputPort);
        }
    }

    private void DiscoverMoveBranches(
        LocationActionDefinition action,
        NarrativeActionNode parentNode,
        int childDepth)
    {
        DiscoverResultBranch(
            action.GetFollowUpActions(
                MoveResult.StrongHit),
            parentNode.StrongHitPort,
            childDepth);

        DiscoverResultBranch(
            action.GetFollowUpActions(
                MoveResult.WeakHit),
            parentNode.WeakHitPort,
            childDepth);

        DiscoverResultBranch(
            action.GetFollowUpActions(
                MoveResult.Miss),
            parentNode.MissPort,
            childDepth);
    }

    private void DiscoverResultBranch(
        LocationActionDefinition[] actions,
        Port outputPort,
        int childDepth)
    {
        if (actions == null ||
            outputPort == null)
        {
            return;
        }

        foreach (LocationActionDefinition action in actions)
        {
            if (action == null)
            {
                continue;
            }

            NarrativeActionNode childNode =
                DiscoverAction(
                    action,
                    childDepth);

            if (childNode == null)
            {
                continue;
            }

            ConnectVisualEdge(
                outputPort,
                childNode.InputPort);
        }
    }

    private void ConnectVisualEdge(
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

        AddElement(
            edge);
    }

    private Vector2 GetPositionForDepth(
        int depth)
    {
        if (!rowsPerDepth.ContainsKey(
            depth))
        {
            rowsPerDepth.Add(
                depth,
                0);
        }

        int row =
            rowsPerDepth[depth];

        rowsPerDepth[depth] =
            row + 1;

        return new Vector2(
            depth * HorizontalSpacing,
            row * VerticalSpacing);
    }

    private NarrativeActionNode CreateActionNode(
        LocationActionDefinition action,
        Vector2 position)
    {
        NarrativeActionNode node =
            new NarrativeActionNode(
                action,
                RefreshActionNode);

        /*
         * We only establish width here.
         * Height is allowed to grow naturally as requirements
         * and effects are added.
         */
        node.SetPosition(
            new Rect(
                position,
                new Vector2(
                    NodeWidth,
                    220f)));

        node.style.width =
            NodeWidth;

        ApplyThemeToNode(
            node);

        node.RegisterCallback<MouseDownEvent>(
            evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                /*
                 * Do not steal focus from controls inside the node.
                 */
                if (evt.target is Button ||
                    evt.target is ObjectField ||
                    evt.target is Toggle ||
                    evt.target is IntegerField ||
                    evt.target is EnumField)
                {
                    return;
                }

                Selection.activeObject =
                    action;

                EditorGUIUtility.PingObject(
                    action);
            });

        AddElement(
            node);

        return node;
    }

    private void RefreshActionNode(
        LocationActionDefinition action)
    {
        if (action == null)
        {
            return;
        }

        if (!actionNodes.TryGetValue(
            action,
            out NarrativeActionNode oldNode))
        {
            return;
        }

        Rect oldPosition =
            oldNode.GetPosition();

        List<Edge> connectedEdges =
            edges
                .Where(
                    edge =>
                        edge.input != null &&
                        edge.output != null &&
                        (edge.input.node == oldNode ||
                         edge.output.node == oldNode))
                .ToList();

        rebuildingGraph =
            true;

        foreach (Edge edge in connectedEdges)
        {
            RemoveElement(
                edge);
        }

        RemoveElement(
            oldNode);

        NarrativeActionNode newNode =
            new NarrativeActionNode(
                action,
                RefreshActionNode);

        newNode.SetPosition(
            new Rect(
                oldPosition.position,
                new Vector2(
                    NodeWidth,
                    220f)));

        newNode.style.width =
            NodeWidth;

        ApplyThemeToNode(
            newNode);

        newNode.RegisterCallback<MouseDownEvent>(
            evt =>
            {
                if (evt.button != 0)
                {
                    return;
                }

                if (evt.target is Button ||
                    evt.target is ObjectField ||
                    evt.target is Toggle ||
                    evt.target is IntegerField ||
                    evt.target is EnumField)
                {
                    return;
                }

                Selection.activeObject =
                    action;

                EditorGUIUtility.PingObject(
                    action);
            });

        actionNodes[action] =
            newNode;

        AddElement(
            newNode);

        /*
         * Rebuild only the visual graph connections.
         * The serialized data has not been changed.
         */
        ReconnectAllVisualEdges();

        rebuildingGraph =
            false;
    }

    private void ReconnectAllVisualEdges()
    {
        List<Edge> existingEdges =
            edges.ToList();

        foreach (Edge edge in existingEdges)
        {
            RemoveElement(
                edge);
        }

        if (currentLocation == null ||
            locationNode == null)
        {
            return;
        }

        LocationActionDefinition[] startingActions =
            currentLocation.Actions;

        if (startingActions != null)
        {
            foreach (LocationActionDefinition action in startingActions)
            {
                if (action == null)
                {
                    continue;
                }

                if (!actionNodes.TryGetValue(
                    action,
                    out NarrativeActionNode node))
                {
                    continue;
                }

                ConnectVisualEdge(
                    locationNode.ActionsPort,
                    node.InputPort);
            }
        }

        foreach (KeyValuePair<
                     LocationActionDefinition,
                     NarrativeActionNode> pair in actionNodes)
        {
            LocationActionDefinition action =
                pair.Key;

            NarrativeActionNode node =
                pair.Value;

            if (action == null ||
                node == null)
            {
                continue;
            }

            if (action.RequiresMove)
            {
                ReconnectResultBranch(
                    action.GetFollowUpActions(
                        MoveResult.StrongHit),
                    node.StrongHitPort);

                ReconnectResultBranch(
                    action.GetFollowUpActions(
                        MoveResult.WeakHit),
                    node.WeakHitPort);

                ReconnectResultBranch(
                    action.GetFollowUpActions(
                        MoveResult.Miss),
                    node.MissPort);
            }
            else
            {
                ReconnectResultBranch(
                    action.FollowUpActions,
                    node.FollowUpPort);
            }
        }
    }

    private void ReconnectResultBranch(
        LocationActionDefinition[] actions,
        Port outputPort)
    {
        if (actions == null ||
            outputPort == null)
        {
            return;
        }

        foreach (LocationActionDefinition action in actions)
        {
            if (action == null)
            {
                continue;
            }

            if (!actionNodes.TryGetValue(
                action,
                out NarrativeActionNode childNode))
            {
                continue;
            }

            ConnectVisualEdge(
                outputPort,
                childNode.InputPort);
        }
    }

    private void ApplyThemeToExistingNodes()
    {
        foreach (NarrativeActionNode node in
                 actionNodes.Values)
        {
            ApplyThemeToNode(
                node);
        }
    }

    private void ApplyThemeToNode(
        NarrativeActionNode node)
    {
        if (node == null)
        {
            return;
        }

        if (currentTheme == null)
        {
            node.SetTheme(
                new Color(
                    0.18f,
                    0.18f,
                    0.18f,
                    1f),
                Color.white);

            return;
        }

        GameUITheme.ApproachStyle style =
            currentTheme.GetApproachStyle(
                node.Action.Approach);

        if (style == null)
        {
            return;
        }

        node.SetTheme(
            style.NormalColor,
            style.TextColor);
    }

    private void BuildContextMenu(
        ContextualMenuPopulateEvent evt)
    {
        if (currentLocation == null)
        {
            evt.menu.AppendAction(
                "Select a Location first",
                _ =>
                {
                },
                DropdownMenuAction.Status.Disabled);

            return;
        }

        Vector2 mousePosition =
            contentViewContainer.WorldToLocal(
                evt.mousePosition);

        evt.menu.AppendSeparator();

        evt.menu.AppendAction(
            "Create/Unaligned Action",
            _ =>
                CreateNewAction(
                    ActionApproach.None,
                    mousePosition));

        evt.menu.AppendAction(
            "Create/Valor Action",
            _ =>
                CreateNewAction(
                    ActionApproach.Valor,
                    mousePosition));

        evt.menu.AppendAction(
            "Create/Wit Action",
            _ =>
                CreateNewAction(
                    ActionApproach.Wit,
                    mousePosition));

        evt.menu.AppendAction(
            "Create/Soul Action",
            _ =>
                CreateNewAction(
                    ActionApproach.Soul,
                    mousePosition));

        evt.menu.AppendAction(
            "Create/Shadow Action",
            _ =>
                CreateNewAction(
                    ActionApproach.Shadow,
                    mousePosition));

        evt.menu.AppendAction(
            "Create/Fortune Action",
            _ =>
                CreateNewAction(
                    ActionApproach.Fortune,
                    mousePosition));
    }

    private void CreateNewAction(
        ActionApproach approach,
        Vector2 position)
    {
        if (currentLocation == null)
        {
            return;
        }

        string locationPath =
            AssetDatabase.GetAssetPath(
                currentLocation);

        string folderPath =
            Path.GetDirectoryName(
                locationPath);

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

        string assetPath =
            AssetDatabase.GenerateUniqueAssetPath(
                $"{folderPath}/{approachName}Action.asset");

        LocationActionDefinition action =
            ScriptableObject.CreateInstance<
                LocationActionDefinition>();

        Undo.RegisterCreatedObjectUndo(
            action,
            "Create Narrative Action");

        AssetDatabase.CreateAsset(
            action,
            assetPath);

        SetActionApproach(
            action,
            approach);

        AssetDatabase.SaveAssets();

        NarrativeActionNode node =
            CreateActionNode(
                action,
                position);

        actionNodes.Add(
            action,
            node);

        Selection.activeObject =
            action;

        EditorGUIUtility.PingObject(
            action);
    }

    private void SetActionApproach(
        LocationActionDefinition action,
        ActionApproach approach)
    {
        if (action == null)
        {
            return;
        }

        SerializedObject serializedAction =
            new SerializedObject(
                action);

        SerializedProperty approachProperty =
            serializedAction.FindProperty(
                "approach");

        if (approachProperty == null)
        {
            Debug.LogError(
                "Narrative Graph could not find the serialized 'approach' field on LocationActionDefinition.");

            return;
        }

        approachProperty.enumValueIndex =
            (int)approach;

        serializedAction.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            action);
    }

    private class LocationNode : Node
    {
        public LocationDefinition Location
        {
            get;
        }

        public Port ActionsPort
        {
            get;
        }

        public LocationNode(
            LocationDefinition location)
        {
            Location =
                location;

            title =
                string.IsNullOrWhiteSpace(
                    location.DisplayName)
                    ? location.name
                    : location.DisplayName;

            Label typeLabel =
                new Label(
                    "LOCATION");

            typeLabel.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            extensionContainer.Add(
                typeLabel);

            if (!string.IsNullOrWhiteSpace(
                location.Description))
            {
                Label descriptionLabel =
                    new Label(
                        location.Description);

                descriptionLabel.style.whiteSpace =
                    WhiteSpace.Normal;

                descriptionLabel.style.maxWidth =
                    NodeWidth - 30f;

                extensionContainer.Add(
                    descriptionLabel);
            }

            ActionsPort =
                InstantiatePort(
                    Orientation.Horizontal,
                    Direction.Output,
                    Port.Capacity.Multi,
                    typeof(bool));

            ActionsPort.portName =
                "Starting Actions";

            outputContainer.Add(
                ActionsPort);

            RefreshExpandedState();
            RefreshPorts();
        }
    }

    private class NarrativeActionNode : Node
    {
        private readonly Label approachLabel;

        private readonly VisualElement requirementsContainer;
        private readonly VisualElement effectsContainer;
        private readonly VisualElement strongHitEffectsContainer;
        private readonly VisualElement weakHitEffectsContainer;
        private readonly VisualElement missEffectsContainer;

        private readonly System.Action<
            LocationActionDefinition> refreshRequested;

        public LocationActionDefinition Action
        {
            get;
        }

        public Port InputPort
        {
            get;
        }

        public Port FollowUpPort
        {
            get;
            private set;
        }

        public Port StrongHitPort
        {
            get;
            private set;
        }

        public Port WeakHitPort
        {
            get;
            private set;
        }

        public Port MissPort
        {
            get;
            private set;
        }

        public NarrativeActionNode(
            LocationActionDefinition action,
            System.Action<LocationActionDefinition> refreshRequested)
        {
            Action =
                action;

            this.refreshRequested =
                refreshRequested;

            title =
                string.IsNullOrWhiteSpace(
                    action.DisplayName)
                    ? action.name
                    : action.DisplayName;

            style.minWidth =
                NodeWidth;

            InputPort =
                InstantiatePort(
                    Orientation.Horizontal,
                    Direction.Input,
                    Port.Capacity.Multi,
                    typeof(bool));

            InputPort.portName =
                "In";

            inputContainer.Add(
                InputPort);

            approachLabel =
                new Label(
                    GetApproachName(
                        action.Approach));

            approachLabel.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            extensionContainer.Add(
                approachLabel);

            if (action.RequiresMove &&
                action.Move != null)
            {
                Label moveLabel =
                    new Label(
                        $"Move: {action.Move.DisplayName}");

                extensionContainer.Add(
                    moveLabel);

                StrongHitPort =
                    CreateOutputPort(
                        "Strong Hit");

                WeakHitPort =
                    CreateOutputPort(
                        "Weak Hit");

                MissPort =
                    CreateOutputPort(
                        "Miss");
            }
            else
            {
                FollowUpPort =
                    CreateOutputPort(
                        "Follow Up");
            }

            AddDivider();

            requirementsContainer =
                new VisualElement();

            extensionContainer.Add(
                requirementsContainer);

            BuildRequirementsUI();

            AddDivider();

            effectsContainer =
                new VisualElement();

            extensionContainer.Add(
                effectsContainer);

            BuildEffectsUI();

            if (action.RequiresMove &&
                action.Move != null)
            {
                AddDivider();

                strongHitEffectsContainer =
                    new VisualElement();

                extensionContainer.Add(
                    strongHitEffectsContainer);

                BuildEffectListUI(
                    strongHitEffectsContainer,
                    StrongHitEffectsProperty,
                    "STRONG HIT EFFECTS");

                AddDivider();

                weakHitEffectsContainer =
                    new VisualElement();

                extensionContainer.Add(
                    weakHitEffectsContainer);

                BuildEffectListUI(
                    weakHitEffectsContainer,
                    WeakHitEffectsProperty,
                    "WEAK HIT EFFECTS");

                AddDivider();

                missEffectsContainer =
                    new VisualElement();

                extensionContainer.Add(
                    missEffectsContainer);

                BuildEffectListUI(
                    missEffectsContainer,
                    MissEffectsProperty,
                    "MISS EFFECTS");
            }

            AddDivider();

            Button inspectButton =
                new Button(
                    () =>
                    {
                        Selection.activeObject =
                            action;

                        EditorGUIUtility.PingObject(
                            action);
                    })
                {
                    text =
                        "Inspect"
                };

            extensionContainer.Add(
                inspectButton);

            RefreshExpandedState();
            RefreshPorts();
        }

        private void BuildRequirementsUI()
        {
            requirementsContainer.Clear();

            SerializedObject visibilitySerializedAction =
                new SerializedObject(
                    Action);

            visibilitySerializedAction.Update();

            SerializedProperty hideProperty =
                visibilitySerializedAction.FindProperty(
                    HideWhenRequirementsNotMetProperty);

            if (hideProperty != null)
            {
                Toggle hideToggle =
                    new Toggle(
                        "Hide If Requirements Not Met")
                    {
                        value =
                            hideProperty.boolValue
                    };

                hideToggle.RegisterValueChangedCallback(
                    evt =>
                    {
                        SerializedObject serializedAction =
                            new SerializedObject(
                                Action);

                        serializedAction.Update();

                        SerializedProperty property =
                            serializedAction.FindProperty(
                                HideWhenRequirementsNotMetProperty);

                        if (property == null)
                        {
                            return;
                        }

                        Undo.RecordObject(
                            Action,
                            "Change Action Visibility Requirement");

                        property.boolValue =
                            evt.newValue;

                        serializedAction.ApplyModifiedProperties();

                        EditorUtility.SetDirty(
                            Action);

                        AssetDatabase.SaveAssets();
                    });

                requirementsContainer.Add(
                    hideToggle);
            }

            Label heading =
                CreateSectionHeading(
                    "REQUIREMENTS");

            requirementsContainer.Add(
                heading);

            SerializedObject serializedAction =
                new SerializedObject(
                    Action);

            serializedAction.Update();

            SerializedProperty requirements =
                serializedAction.FindProperty(
                    RequirementsProperty);

            if (requirements == null)
            {
                requirementsContainer.Add(
                    new Label(
                        "Missing requirements property"));

                return;
            }

            for (int i = 0;
                 i < requirements.arraySize;
                 i++)
            {
                int index =
                    i;

                SerializedProperty element =
                    requirements.GetArrayElementAtIndex(
                        index);

                if (element.managedReferenceValue is FlagCondition)
                {
                    VisualElement row =
                        CreateFlagConditionRow(
                            serializedAction,
                            requirements,
                            element,
                            index);

                    requirementsContainer.Add(
                        row);
                }
                else if (element.managedReferenceValue is ResourceCondition)
                {
                    VisualElement row =
                        CreateResourceConditionRow(
                            element,
                            index);

                    requirementsContainer.Add(
                        row);
                }
                else if (element.managedReferenceValue is QuestCondition)
                {
                    VisualElement row =
                        CreateQuestConditionRow(
                            element,
                            index);

                    requirementsContainer.Add(
                        row);
                }
                else
                {
                    VisualElement row =
                        CreateUnsupportedRow(
                            "Unknown Condition",
                            () =>
                            {
                                RemoveManagedReference(
                                    RequirementsProperty,
                                    index,
                                    "Remove Requirement");
                            });

                    requirementsContainer.Add(
                        row);
                }
            }

            VisualElement addButtons =
                new VisualElement();

            addButtons.style.flexDirection =
                FlexDirection.Row;

            addButtons.style.marginTop =
                4f;

            Button addFlagButton =
                new Button(
                    AddFlagRequirement)
                {
                    text =
                        "+ Flag Requirement"
                };

            addFlagButton.style.flexGrow =
                1f;

            Button addResourceButton =
                new Button(
                    AddResourceRequirement)
                {
                    text =
                        "+ Resource Requirement"
                };

            addResourceButton.style.flexGrow =
                1f;

            addButtons.Add(
                addFlagButton);

            addButtons.Add(
                addResourceButton);

            Button addQuestButton =
                new Button(
                    AddQuestRequirement)
                {
                    text =
                        "+ Quest Requirement"
                };

            addQuestButton.style.flexGrow =
                1f;

            addButtons.Add(
                addQuestButton);

            requirementsContainer.Add(
                addButtons);
        }

        private VisualElement CreateFlagConditionRow(
            SerializedObject serializedAction,
            SerializedProperty array,
            SerializedProperty element,
            int index)
        {
            VisualElement row =
                CreateRuleRow();

            SerializedProperty flagProperty =
                element.FindPropertyRelative(
                    "flag");

            SerializedProperty valueProperty =
                element.FindPropertyRelative(
                    "requiredValue");

            ObjectField flagField =
                new ObjectField
                {
                    objectType =
                        typeof(GameFlagDefinition),

                    allowSceneObjects =
                        false
                };

            flagField.style.flexGrow =
                1f;

            flagField.style.minWidth =
                120f;

            flagField.SetValueWithoutNotify(
                flagProperty != null
                    ? flagProperty.objectReferenceValue
                    : null);

            flagField.RegisterValueChangedCallback(
                evt =>
                {
                    SerializedObject currentObject =
                        new SerializedObject(
                            Action);

                    SerializedProperty currentArray =
                        currentObject.FindProperty(
                            RequirementsProperty);

                    if (currentArray == null ||
                        index < 0 ||
                        index >= currentArray.arraySize)
                    {
                        return;
                    }

                    SerializedProperty currentElement =
                        currentArray.GetArrayElementAtIndex(
                            index);

                    SerializedProperty currentFlag =
                        currentElement.FindPropertyRelative(
                            "flag");

                    if (currentFlag == null)
                    {
                        return;
                    }

                    Undo.RecordObject(
                        Action,
                        "Change Flag Requirement");

                    currentFlag.objectReferenceValue =
                        evt.newValue;

                    currentObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(
                        Action);

                    AssetDatabase.SaveAssets();
                });

            Toggle valueToggle =
                new Toggle();

            valueToggle.tooltip =
                "Required flag value";

            valueToggle.style.width =
                35f;

            valueToggle.SetValueWithoutNotify(
                valueProperty != null &&
                valueProperty.boolValue);

            valueToggle.RegisterValueChangedCallback(
                evt =>
                {
                    SerializedObject currentObject =
                        new SerializedObject(
                            Action);

                    SerializedProperty currentArray =
                        currentObject.FindProperty(
                            RequirementsProperty);

                    if (currentArray == null ||
                        index < 0 ||
                        index >= currentArray.arraySize)
                    {
                        return;
                    }

                    SerializedProperty currentElement =
                        currentArray.GetArrayElementAtIndex(
                            index);

                    SerializedProperty currentValue =
                        currentElement.FindPropertyRelative(
                            "requiredValue");

                    if (currentValue == null)
                    {
                        return;
                    }

                    Undo.RecordObject(
                        Action,
                        "Change Flag Requirement");

                    currentValue.boolValue =
                        evt.newValue;

                    currentObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(
                        Action);

                    AssetDatabase.SaveAssets();
                });

            Label valueLabel =
                new Label(
                    valueToggle.value
                        ? "True"
                        : "False");

            valueLabel.style.width =
                36f;

            valueToggle.RegisterValueChangedCallback(
                evt =>
                {
                    valueLabel.text =
                        evt.newValue
                            ? "True"
                            : "False";
                });

            Button removeButton =
                new Button(
                    () =>
                    {
                        RemoveManagedReference(
                            RequirementsProperty,
                            index,
                            "Remove Flag Requirement");
                    })
                {
                    text =
                        "×"
                };

            removeButton.style.width =
                24f;

            row.Add(
                flagField);

            row.Add(
                valueToggle);

            row.Add(
                valueLabel);

            row.Add(
                removeButton);

            return row;
        }

        private VisualElement CreateResourceConditionRow(
            SerializedProperty element,
            int index)
        {
            VisualElement row =
                CreateRuleRow();

            SerializedProperty resourceProperty =
                element.FindPropertyRelative(
                    "resource");

            SerializedProperty comparisonProperty =
                element.FindPropertyRelative(
                    "comparison");

            SerializedProperty valueProperty =
                element.FindPropertyRelative(
                    "value");

            ObjectField resourceField =
                new ObjectField
                {
                    objectType =
                        typeof(GameResourceDefinition),

                    allowSceneObjects =
                        false
                };

            resourceField.style.flexGrow =
                1f;

            resourceField.style.minWidth =
                130f;

            resourceField.SetValueWithoutNotify(
                resourceProperty != null
                    ? resourceProperty.objectReferenceValue
                    : null);

            resourceField.RegisterValueChangedCallback(
                evt =>
                {
                    SerializedObject currentObject =
                        new SerializedObject(
                            Action);

                    currentObject.Update();

                    SerializedProperty currentArray =
                        currentObject.FindProperty(
                            RequirementsProperty);

                    if (currentArray == null ||
                        index < 0 ||
                        index >= currentArray.arraySize)
                    {
                        return;
                    }

                    SerializedProperty currentElement =
                        currentArray.GetArrayElementAtIndex(
                            index);

                    SerializedProperty currentResource =
                        currentElement.FindPropertyRelative(
                            "resource");

                    if (currentResource == null)
                    {
                        return;
                    }

                    Undo.RecordObject(
                        Action,
                        "Change Resource Requirement");

                    currentResource.objectReferenceValue =
                        evt.newValue;

                    currentObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(
                        Action);

                    AssetDatabase.SaveAssets();
                });

            ResourceComparison initialComparison =
                comparisonProperty != null
                    ? (ResourceComparison)comparisonProperty.enumValueIndex
                    : ResourceComparison.GreaterThanOrEqual;

            EnumField comparisonField =
                new EnumField(
                    initialComparison);

            comparisonField.tooltip =
                "Comparison";

            comparisonField.style.width =
                90f;

            comparisonField.RegisterValueChangedCallback(
                evt =>
                {
                    SerializedObject currentObject =
                        new SerializedObject(
                            Action);

                    currentObject.Update();

                    SerializedProperty currentArray =
                        currentObject.FindProperty(
                            RequirementsProperty);

                    if (currentArray == null ||
                        index < 0 ||
                        index >= currentArray.arraySize)
                    {
                        return;
                    }

                    SerializedProperty currentElement =
                        currentArray.GetArrayElementAtIndex(
                            index);

                    SerializedProperty currentComparison =
                        currentElement.FindPropertyRelative(
                            "comparison");

                    if (currentComparison == null)
                    {
                        return;
                    }

                    Undo.RecordObject(
                        Action,
                        "Change Resource Comparison");

                    currentComparison.enumValueIndex =
                        (int)(ResourceComparison)evt.newValue;

                    currentObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(
                        Action);

                    AssetDatabase.SaveAssets();
                });

            IntegerField valueField =
                new IntegerField();

            valueField.tooltip =
                "Required value";

            valueField.style.width =
                60f;

            valueField.SetValueWithoutNotify(
                valueProperty != null
                    ? valueProperty.intValue
                    : 0);

            valueField.RegisterValueChangedCallback(
                evt =>
                {
                    SerializedObject currentObject =
                        new SerializedObject(
                            Action);

                    currentObject.Update();

                    SerializedProperty currentArray =
                        currentObject.FindProperty(
                            RequirementsProperty);

                    if (currentArray == null ||
                        index < 0 ||
                        index >= currentArray.arraySize)
                    {
                        return;
                    }

                    SerializedProperty currentElement =
                        currentArray.GetArrayElementAtIndex(
                            index);

                    SerializedProperty currentValue =
                        currentElement.FindPropertyRelative(
                            "value");

                    if (currentValue == null)
                    {
                        return;
                    }

                    Undo.RecordObject(
                        Action,
                        "Change Resource Requirement Value");

                    currentValue.intValue =
                        evt.newValue;

                    currentObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(
                        Action);

                    AssetDatabase.SaveAssets();
                });

            Button removeButton =
                new Button(
                    () =>
                    {
                        RemoveManagedReference(
                            RequirementsProperty,
                            index,
                            "Remove Resource Requirement");
                    })
                {
                    text =
                        "×"
                };

            removeButton.style.width =
                24f;

            row.Add(
                resourceField);

            row.Add(
                comparisonField);

            row.Add(
                valueField);

            row.Add(
                removeButton);

            return row;
        }

        private VisualElement CreateQuestConditionRow(
            SerializedProperty element,
            int index)
        {
            VisualElement row =
                CreateRuleRow();

            SerializedProperty questProperty =
                element.FindPropertyRelative(
                    "quest");

            SerializedProperty conditionProperty =
                element.FindPropertyRelative(
                    "condition");

            SerializedProperty stageProperty =
                element.FindPropertyRelative(
                    "stageIndex");

            ObjectField questField =
                new ObjectField
                {
                    objectType =
                        typeof(QuestDefinition),

                    allowSceneObjects =
                        false
                };

            questField.style.flexGrow =
                1f;

            questField.style.minWidth =
                120f;

            questField.SetValueWithoutNotify(
                questProperty != null
                    ? questProperty.objectReferenceValue
                    : null);

            questField.RegisterValueChangedCallback(
                evt =>
                {
                    UpdateEffectObjectReference(
                        RequirementsProperty,
                        index,
                        "quest",
                        evt.newValue,
                        "Change Quest Requirement");
                });

            QuestConditionMode initialCondition =
                conditionProperty != null
                    ? (QuestConditionMode)conditionProperty.enumValueIndex
                    : QuestConditionMode.Active;

            EnumField conditionField =
                new EnumField(
                    initialCondition);

            conditionField.tooltip =
                "Required quest state.";

            conditionField.style.width =
                110f;

            conditionField.RegisterValueChangedCallback(
                evt =>
                {
                    UpdateEffectEnum(
                        RequirementsProperty,
                        index,
                        "condition",
                        (int)(QuestConditionMode)evt.newValue,
                        "Change Quest Requirement");

                    refreshRequested?.Invoke(
                        Action);
                });

            row.Add(
                questField);

            row.Add(
                conditionField);

            if (initialCondition == QuestConditionMode.AtStage ||
                initialCondition == QuestConditionMode.AtOrAfterStage)
            {
                IntegerField stageField =
                    new IntegerField();

                stageField.tooltip =
                    "Quest stage index.";

                stageField.style.width =
                    55f;

                stageField.SetValueWithoutNotify(
                    stageProperty != null
                        ? stageProperty.intValue
                        : 0);

                stageField.RegisterValueChangedCallback(
                    evt =>
                    {
                        UpdateEffectInteger(
                            RequirementsProperty,
                            index,
                            "stageIndex",
                            evt.newValue,
                            "Change Quest Requirement Stage");
                    });

                row.Add(
                    stageField);
            }

            Button removeButton =
                new Button(
                    () =>
                    {
                        RemoveManagedReference(
                            RequirementsProperty,
                            index,
                            "Remove Quest Requirement");
                    })
                {
                    text =
                        "×"
                };

            removeButton.style.width =
                24f;

            row.Add(
                removeButton);

            return row;
        }

        private void BuildEffectsUI()
        {
            BuildEffectListUI(
                effectsContainer,
                EffectsProperty,
                "EFFECTS");
        }

        private void BuildEffectListUI(
            VisualElement container,
            string propertyName,
            string headingText)
        {
            if (container == null)
            {
                return;
            }

            container.Clear();

            container.Add(
                CreateSectionHeading(
                    headingText));

            SerializedObject serializedAction =
                new SerializedObject(
                    Action);

            serializedAction.Update();

            SerializedProperty effects =
                serializedAction.FindProperty(
                    propertyName);

            if (effects == null)
            {
                container.Add(
                    new Label(
                        $"Missing {propertyName} property"));

                return;
            }

            for (int i = 0;
                 i < effects.arraySize;
                 i++)
            {
                int index = i;

                SerializedProperty element =
                    effects.GetArrayElementAtIndex(
                        index);

                if (element.managedReferenceValue is SetFlagEffect)
                {
                    container.Add(
                        CreateSetFlagEffectRow(
                            element,
                            index,
                            propertyName));
                }
                else if (element.managedReferenceValue is ModifyResourceEffect)
                {
                    container.Add(
                        CreateModifyResourceEffectRow(
                            element,
                            index,
                            propertyName));
                }
                else if (element.managedReferenceValue is ModifyQuestEffect)
                {
                    container.Add(
                        CreateModifyQuestEffectRow(
                            element,
                            index,
                            propertyName));
                }
                else
                {
                    container.Add(
                        CreateUnsupportedRow(
                            "Unknown Effect",
                            () =>
                            {
                                RemoveManagedReference(
                                    propertyName,
                                    index,
                                    $"Remove {headingText} Effect");
                            }));
                }
            }

            VisualElement addButtons =
                new VisualElement();

            addButtons.style.flexDirection =
                FlexDirection.Row;

            addButtons.style.marginTop =
                4f;

            Button addFlagButton =
                new Button(
                    () =>
                    {
                        AddEffect(
                            propertyName,
                            new SetFlagEffect(),
                            $"Add {headingText} Set Flag Effect");
                    })
                {
                    text =
                        "+ Set Flag"
                };

            addFlagButton.style.flexGrow =
                1f;

            Button addResourceButton =
                new Button(
                    () =>
                    {
                        AddEffect(
                            propertyName,
                            new ModifyResourceEffect(),
                            $"Add {headingText} Resource Effect");
                    })
                {
                    text =
                        "+ Modify Resource"
                };

            addResourceButton.style.flexGrow =
                1f;

            addButtons.Add(
                addFlagButton);

            addButtons.Add(
                addResourceButton);

            Button addQuestButton =
                new Button(
                    () =>
                    {
                        AddEffect(
                            propertyName,
                            new ModifyQuestEffect(),
                            $"Add {headingText} Quest Effect");
                    })
                {
                    text =
                        "+ Modify Quest"
                };

            addQuestButton.style.flexGrow =
                1f;

            addButtons.Add(
                addQuestButton);

            container.Add(
                addButtons);
        }

        private VisualElement CreateSetFlagEffectRow(
            SerializedProperty element,
            int index,
            string propertyName)
        {
            VisualElement row =
                CreateRuleRow();

            SerializedProperty flagProperty =
                element.FindPropertyRelative(
                    "flag");

            SerializedProperty valueProperty =
                element.FindPropertyRelative(
                    "value");

            ObjectField flagField =
                new ObjectField
                {
                    objectType =
                        typeof(GameFlagDefinition),

                    allowSceneObjects =
                        false
                };

            flagField.style.flexGrow =
                1f;

            flagField.style.minWidth =
                120f;

            flagField.SetValueWithoutNotify(
                flagProperty != null
                    ? flagProperty.objectReferenceValue
                    : null);

            flagField.RegisterValueChangedCallback(
                evt =>
                {
                    SerializedObject currentObject =
                        new SerializedObject(
                            Action);

                    currentObject.Update();

                    SerializedProperty currentArray =
                        currentObject.FindProperty(
                            propertyName);

                    if (currentArray == null ||
                        index < 0 ||
                        index >= currentArray.arraySize)
                    {
                        return;
                    }

                    SerializedProperty currentElement =
                        currentArray.GetArrayElementAtIndex(
                            index);

                    SerializedProperty currentFlag =
                        currentElement.FindPropertyRelative(
                            "flag");

                    if (currentFlag == null)
                    {
                        return;
                    }

                    Undo.RecordObject(
                        Action,
                        "Change Set Flag Effect");

                    currentFlag.objectReferenceValue =
                        evt.newValue;

                    currentObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(
                        Action);

                    AssetDatabase.SaveAssets();
                });

            Toggle valueToggle =
                new Toggle();

            valueToggle.tooltip =
                "Value assigned to the flag";

            valueToggle.style.width =
                35f;

            valueToggle.SetValueWithoutNotify(
                valueProperty != null &&
                valueProperty.boolValue);

            Label valueLabel =
                new Label(
                    valueToggle.value
                        ? "True"
                        : "False");

            valueLabel.style.width =
                36f;

            valueToggle.RegisterValueChangedCallback(
                evt =>
                {
                    SerializedObject currentObject =
                        new SerializedObject(
                            Action);

                    currentObject.Update();

                    SerializedProperty currentArray =
                        currentObject.FindProperty(
                            propertyName);

                    if (currentArray == null ||
                        index < 0 ||
                        index >= currentArray.arraySize)
                    {
                        return;
                    }

                    SerializedProperty currentElement =
                        currentArray.GetArrayElementAtIndex(
                            index);

                    SerializedProperty currentValue =
                        currentElement.FindPropertyRelative(
                            "value");

                    if (currentValue == null)
                    {
                        return;
                    }

                    Undo.RecordObject(
                        Action,
                        "Change Set Flag Effect");

                    currentValue.boolValue =
                        evt.newValue;

                    currentObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(
                        Action);

                    AssetDatabase.SaveAssets();

                    valueLabel.text =
                        evt.newValue
                            ? "True"
                            : "False";
                });

            Button removeButton =
                new Button(
                    () =>
                    {
                        RemoveManagedReference(
                            propertyName,
                            index,
                            "Remove Set Flag Effect");
                    })
                {
                    text =
                        "×"
                };

            removeButton.style.width =
                24f;

            row.Add(
                flagField);

            row.Add(
                valueToggle);

            row.Add(
                valueLabel);

            row.Add(
                removeButton);

            return row;
        }

        private VisualElement CreateModifyResourceEffectRow(
            SerializedProperty element,
            int index,
            string propertyName)
        {
            VisualElement row =
                CreateRuleRow();

            SerializedProperty resourceProperty =
                element.FindPropertyRelative(
                    "resource");

            SerializedProperty modificationProperty =
                element.FindPropertyRelative(
                    "modification");

            SerializedProperty valueProperty =
                element.FindPropertyRelative(
                    "value");

            ObjectField resourceField =
                new ObjectField
                {
                    objectType =
                        typeof(GameResourceDefinition),

                    allowSceneObjects =
                        false
                };

            resourceField.style.flexGrow =
                1f;

            resourceField.style.minWidth =
                130f;

            resourceField.SetValueWithoutNotify(
                resourceProperty != null
                    ? resourceProperty.objectReferenceValue
                    : null);

            resourceField.RegisterValueChangedCallback(
                evt =>
                {
                    UpdateEffectObjectReference(
                        propertyName,
                        index,
                        "resource",
                        evt.newValue,
                        "Change Resource Effect");
                });

            ResourceModification initialModification =
                modificationProperty != null
                    ? (ResourceModification)modificationProperty.enumValueIndex
                    : ResourceModification.Increase;

            EnumField modificationField =
                new EnumField(
                    initialModification);

            modificationField.tooltip =
                "How this effect changes the resource.";

            modificationField.style.width =
                100f;

            modificationField.RegisterValueChangedCallback(
                evt =>
                {
                    UpdateEffectEnum(
                        propertyName,
                        index,
                        "modification",
                        (int)(ResourceModification)evt.newValue,
                        "Change Resource Modification");
                });

            IntegerField valueField =
                new IntegerField();

            valueField.tooltip =
                "Value used by the resource modification.";

            valueField.style.width =
                60f;

            valueField.SetValueWithoutNotify(
                valueProperty != null
                    ? valueProperty.intValue
                    : 1);

            valueField.RegisterValueChangedCallback(
                evt =>
                {
                    UpdateEffectInteger(
                        propertyName,
                        index,
                        "value",
                        evt.newValue,
                        "Change Resource Effect Value");
                });

            Button removeButton =
                new Button(
                    () =>
                    {
                        RemoveManagedReference(
                            propertyName,
                            index,
                            "Remove Resource Effect");
                    })
                {
                    text =
                        "×"
                };

            removeButton.style.width =
                24f;

            row.Add(
                resourceField);

            row.Add(
                modificationField);

            row.Add(
                valueField);

            row.Add(
                removeButton);

            return row;
        }

        private VisualElement CreateModifyQuestEffectRow(
            SerializedProperty element,
            int index,
            string propertyName)
        {
            VisualElement row =
                CreateRuleRow();

            SerializedProperty questProperty =
                element.FindPropertyRelative(
                    "quest");

            SerializedProperty modificationProperty =
                element.FindPropertyRelative(
                    "modification");

            SerializedProperty stageProperty =
                element.FindPropertyRelative(
                    "stageIndex");

            ObjectField questField =
                new ObjectField
                {
                    objectType =
                        typeof(QuestDefinition),

                    allowSceneObjects =
                        false
                };

            questField.style.flexGrow =
                1f;

            questField.style.minWidth =
                120f;

            questField.SetValueWithoutNotify(
                questProperty != null
                    ? questProperty.objectReferenceValue
                    : null);

            questField.RegisterValueChangedCallback(
                evt =>
                {
                    UpdateEffectObjectReference(
                        propertyName,
                        index,
                        "quest",
                        evt.newValue,
                        "Change Quest Effect");
                });

            QuestModification initialModification =
                modificationProperty != null
                    ? (QuestModification)modificationProperty.enumValueIndex
                    : QuestModification.Start;

            EnumField modificationField =
                new EnumField(
                    initialModification);

            modificationField.tooltip =
                "How this effect changes the quest.";

            modificationField.style.width =
                100f;

            modificationField.RegisterValueChangedCallback(
                evt =>
                {
                    UpdateEffectEnum(
                        propertyName,
                        index,
                        "modification",
                        (int)(QuestModification)evt.newValue,
                        "Change Quest Modification");

                    refreshRequested?.Invoke(
                        Action);
                });

            row.Add(
                questField);

            row.Add(
                modificationField);

            if (initialModification == QuestModification.SetStage)
            {
                IntegerField stageField =
                    new IntegerField();

                stageField.tooltip =
                    "Quest stage index.";

                stageField.style.width =
                    55f;

                stageField.SetValueWithoutNotify(
                    stageProperty != null
                        ? stageProperty.intValue
                        : 0);

                stageField.RegisterValueChangedCallback(
                    evt =>
                    {
                        UpdateEffectInteger(
                            propertyName,
                            index,
                            "stageIndex",
                            evt.newValue,
                            "Change Quest Effect Stage");
                    });

                row.Add(
                    stageField);
            }

            Button removeButton =
                new Button(
                    () =>
                    {
                        RemoveManagedReference(
                            propertyName,
                            index,
                            "Remove Quest Effect");
                    })
                {
                    text =
                        "×"
                };

            removeButton.style.width =
                24f;

            row.Add(
                removeButton);

            return row;
        }

        private void UpdateEffectObjectReference(
            string propertyName,
            int index,
            string relativePropertyName,
            Object value,
            string undoName)
        {
            SerializedObject currentObject =
                new SerializedObject(
                    Action);

            currentObject.Update();

            SerializedProperty currentArray =
                currentObject.FindProperty(
                    propertyName);

            if (currentArray == null ||
                index < 0 ||
                index >= currentArray.arraySize)
            {
                return;
            }

            SerializedProperty currentElement =
                currentArray.GetArrayElementAtIndex(
                    index);

            SerializedProperty currentProperty =
                currentElement.FindPropertyRelative(
                    relativePropertyName);

            if (currentProperty == null)
            {
                return;
            }

            Undo.RecordObject(
                Action,
                undoName);

            currentProperty.objectReferenceValue =
                value;

            currentObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();
        }

        private void UpdateEffectEnum(
            string propertyName,
            int index,
            string relativePropertyName,
            int value,
            string undoName)
        {
            SerializedObject currentObject =
                new SerializedObject(
                    Action);

            currentObject.Update();

            SerializedProperty currentArray =
                currentObject.FindProperty(
                    propertyName);

            if (currentArray == null ||
                index < 0 ||
                index >= currentArray.arraySize)
            {
                return;
            }

            SerializedProperty currentElement =
                currentArray.GetArrayElementAtIndex(
                    index);

            SerializedProperty currentProperty =
                currentElement.FindPropertyRelative(
                    relativePropertyName);

            if (currentProperty == null)
            {
                return;
            }

            Undo.RecordObject(
                Action,
                undoName);

            currentProperty.enumValueIndex =
                value;

            currentObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();
        }

        private void UpdateEffectInteger(
            string propertyName,
            int index,
            string relativePropertyName,
            int value,
            string undoName)
        {
            SerializedObject currentObject =
                new SerializedObject(
                    Action);

            currentObject.Update();

            SerializedProperty currentArray =
                currentObject.FindProperty(
                    propertyName);

            if (currentArray == null ||
                index < 0 ||
                index >= currentArray.arraySize)
            {
                return;
            }

            SerializedProperty currentElement =
                currentArray.GetArrayElementAtIndex(
                    index);

            SerializedProperty currentProperty =
                currentElement.FindPropertyRelative(
                    relativePropertyName);

            if (currentProperty == null)
            {
                return;
            }

            Undo.RecordObject(
                Action,
                undoName);

            currentProperty.intValue =
                value;

            currentObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();
        }

        private void AddFlagRequirement()
        {
            SerializedObject serializedAction =
                new SerializedObject(
                    Action);

            serializedAction.Update();

            SerializedProperty requirements =
                serializedAction.FindProperty(
                    RequirementsProperty);

            if (requirements == null)
            {
                Debug.LogError(
                    $"Narrative Graph could not find '{RequirementsProperty}' on '{Action.name}'.");

                return;
            }

            Undo.RecordObject(
                Action,
                "Add Flag Requirement");

            int index =
                requirements.arraySize;

            requirements.InsertArrayElementAtIndex(
                index);

            SerializedProperty element =
                requirements.GetArrayElementAtIndex(
                    index);

            element.managedReferenceValue =
                new FlagCondition();

            serializedAction.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();

            refreshRequested?.Invoke(
                Action);
        }

        private void AddResourceRequirement()
        {
            SerializedObject serializedAction =
                new SerializedObject(
                    Action);

            serializedAction.Update();

            SerializedProperty requirements =
                serializedAction.FindProperty(
                    RequirementsProperty);

            if (requirements == null)
            {
                Debug.LogError(
                    $"Narrative Graph could not find '{RequirementsProperty}' on '{Action.name}'.");

                return;
            }

            Undo.RecordObject(
                Action,
                "Add Resource Requirement");

            int index =
                requirements.arraySize;

            requirements.InsertArrayElementAtIndex(
                index);

            SerializedProperty element =
                requirements.GetArrayElementAtIndex(
                    index);

            element.managedReferenceValue =
                new ResourceCondition();

            serializedAction.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();

            refreshRequested?.Invoke(
                Action);
        }

        private void AddQuestRequirement()
        {
            SerializedObject serializedAction =
                new SerializedObject(
                    Action);

            serializedAction.Update();

            SerializedProperty requirements =
                serializedAction.FindProperty(
                    RequirementsProperty);

            if (requirements == null)
            {
                Debug.LogError(
                    $"Narrative Graph could not find '{RequirementsProperty}' on '{Action.name}'.");

                return;
            }

            Undo.RecordObject(
                Action,
                "Add Quest Requirement");

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

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();

            refreshRequested?.Invoke(
                Action);
        }

        private void AddEffect(
            string propertyName,
            GameEffect effect,
            string undoName)
        {
            SerializedObject serializedAction =
                new SerializedObject(
                    Action);

            serializedAction.Update();

            SerializedProperty effects =
                serializedAction.FindProperty(
                    propertyName);

            if (effects == null)
            {
                Debug.LogError(
                    $"Narrative Graph could not find '{propertyName}' on '{Action.name}'.");

                return;
            }

            Undo.RecordObject(
                Action,
                undoName);

            int index =
                effects.arraySize;

            effects.InsertArrayElementAtIndex(
                index);

            SerializedProperty element =
                effects.GetArrayElementAtIndex(
                    index);

            element.managedReferenceValue =
                effect;

            serializedAction.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();

            refreshRequested?.Invoke(
                Action);
        }

        private void RemoveManagedReference(
            string propertyName,
            int index,
            string undoName)
        {
            SerializedObject serializedAction =
                new SerializedObject(
                    Action);

            serializedAction.Update();

            SerializedProperty array =
                serializedAction.FindProperty(
                    propertyName);

            if (array == null ||
                index < 0 ||
                index >= array.arraySize)
            {
                return;
            }

            Undo.RecordObject(
                Action,
                undoName);

            array.DeleteArrayElementAtIndex(
                index);

            serializedAction.ApplyModifiedProperties();

            EditorUtility.SetDirty(
                Action);

            AssetDatabase.SaveAssets();

            refreshRequested?.Invoke(
                Action);
        }

        private VisualElement CreateRuleRow()
        {
            VisualElement row =
                new VisualElement();

            row.style.flexDirection =
                FlexDirection.Row;

            row.style.alignItems =
                Align.Center;

            row.style.marginBottom =
                2f;

            return row;
        }

        private VisualElement CreateUnsupportedRow(
            string label,
            System.Action removeAction)
        {
            VisualElement row =
                CreateRuleRow();

            Label typeLabel =
                new Label(
                    label);

            typeLabel.style.flexGrow =
                1f;

            Button removeButton =
                new Button(
                    removeAction)
                {
                    text =
                        "×"
                };

            removeButton.style.width =
                24f;

            row.Add(
                typeLabel);

            row.Add(
                removeButton);

            return row;
        }

        private Label CreateSectionHeading(
            string text)
        {
            Label label =
                new Label(
                    text);

            label.style.unityFontStyleAndWeight =
                FontStyle.Bold;

            label.style.marginTop =
                3f;

            label.style.marginBottom =
                3f;

            return label;
        }

        private void AddDivider()
        {
            VisualElement divider =
                new VisualElement();

            divider.style.height =
                1f;

            divider.style.backgroundColor =
                new Color(
                    1f,
                    1f,
                    1f,
                    0.15f);

            divider.style.marginTop =
                5f;

            divider.style.marginBottom =
                5f;

            extensionContainer.Add(
                divider);
        }

        public void SetTheme(
            Color backgroundColor,
            Color textColor)
        {
            VisualElement titleContainer =
                this.Q<VisualElement>(
                    "title");

            if (titleContainer != null)
            {
                titleContainer.style.backgroundColor =
                    backgroundColor;
            }

            Label titleLabel =
                this.Q<Label>(
                    "title-label");

            if (titleLabel != null)
            {
                titleLabel.style.color =
                    textColor;
            }

            if (approachLabel != null)
            {
                approachLabel.style.color =
                    textColor;

                approachLabel.style.backgroundColor =
                    backgroundColor;

                approachLabel.style.paddingLeft =
                    6f;

                approachLabel.style.paddingRight =
                    6f;

                approachLabel.style.paddingTop =
                    3f;

                approachLabel.style.paddingBottom =
                    3f;

                approachLabel.style.marginBottom =
                    4f;
            }
        }

        private Port CreateOutputPort(
            string portName)
        {
            Port port =
                InstantiatePort(
                    Orientation.Horizontal,
                    Direction.Output,
                    Port.Capacity.Multi,
                    typeof(bool));

            port.portName =
                portName;

            outputContainer.Add(
                port);

            return port;
        }

        private static string GetApproachName(
            ActionApproach approach)
        {
            switch (approach)
            {
                case ActionApproach.Valor:
                    return "VALOR";

                case ActionApproach.Wit:
                    return "WIT";

                case ActionApproach.Soul:
                    return "SOUL";

                case ActionApproach.Shadow:
                    return "SHADOW";

                case ActionApproach.Fortune:
                    return "FORTUNE";

                case ActionApproach.None:
                    return "UNALIGNED";

                default:
                    return approach
                        .ToString()
                        .ToUpperInvariant();
            }
        }
    }
}