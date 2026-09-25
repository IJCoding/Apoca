using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class NarrativeGraphWindow : EditorWindow
{
    private NarrativeGraphView graphView;

    private ObjectField locationField;
    private ObjectField themeField;

    private LocationDefinition currentLocation;
    private GameUITheme currentTheme;

    [MenuItem("Window/Game/Narrative Graph")]
    public static void OpenWindow()
    {
        NarrativeGraphWindow window =
            GetWindow<NarrativeGraphWindow>();

        window.titleContent =
            new GUIContent("Narrative Graph");

        window.minSize =
            new Vector2(
                800f,
                500f);
    }

    private void OnEnable()
    {
        BuildWindow();

        Undo.undoRedoPerformed +=
            HandleUndoRedo;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -=
            HandleUndoRedo;
    }

    private void BuildWindow()
    {
        rootVisualElement.Clear();

        CreateToolbar();
        CreateGraphView();
    }

    private void CreateToolbar()
    {
        UnityEditor.UIElements.Toolbar toolbar =
            new UnityEditor.UIElements.Toolbar();

        locationField =
            new ObjectField("Location")
            {
                objectType =
                    typeof(LocationDefinition),

                allowSceneObjects =
                    false
            };

        locationField.style.minWidth =
            300f;

        locationField.RegisterValueChangedCallback(
            HandleLocationFieldChanged);

        toolbar.Add(
            locationField);

        themeField =
            new ObjectField("Theme")
            {
                objectType =
                    typeof(GameUITheme),

                allowSceneObjects =
                    false
            };

        themeField.style.minWidth =
            260f;

        themeField.RegisterValueChangedCallback(
            HandleThemeFieldChanged);

        toolbar.Add(
            themeField);

        ToolbarButton refreshButton =
            new ToolbarButton(
                RefreshGraph)
            {
                text =
                    "Refresh"
            };

        toolbar.Add(
            refreshButton);

        ToolbarButton frameAllButton =
            new ToolbarButton(
                FrameGraph)
            {
                text =
                    "Frame All"
            };

        toolbar.Add(
            frameAllButton);

        rootVisualElement.Add(
            toolbar);
    }

    private void CreateGraphView()
    {
        graphView =
            new NarrativeGraphView();

        graphView.style.flexGrow =
            1f;

        rootVisualElement.Add(
            graphView);

        graphView.SetTheme(
            currentTheme);

        if (currentLocation != null)
        {
            graphView.LoadLocation(
                currentLocation);
        }
    }

    private void HandleLocationFieldChanged(
        ChangeEvent<Object> changeEvent)
    {
        currentLocation =
            changeEvent.newValue as LocationDefinition;

        if (graphView == null)
        {
            return;
        }

        graphView.LoadLocation(
            currentLocation);
    }

    private void HandleThemeFieldChanged(
        ChangeEvent<Object> changeEvent)
    {
        currentTheme =
            changeEvent.newValue as GameUITheme;

        if (graphView == null)
        {
            return;
        }

        graphView.SetTheme(
            currentTheme);
    }

    private void RefreshGraph()
    {
        if (graphView == null)
        {
            return;
        }

        graphView.LoadLocation(
            currentLocation);
    }

    private void FrameGraph()
    {
        if (graphView == null)
        {
            return;
        }

        graphView.FrameAll();
    }

    private void HandleUndoRedo()
    {
        if (graphView == null)
        {
            return;
        }

        graphView.LoadLocation(
            currentLocation);

        Repaint();
    }
}