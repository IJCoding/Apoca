#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InteractiveVisualEditorWindow : EditorWindow
{
    private InteractiveVisualDefinition definition;
    private int selectedHotspot = -1;
    private bool drawing;
    private int draggingPoint = -1;
    private Vector2 scroll;
    private Rect imageRect;

    private const float SidebarWidth = 300f;

    [MenuItem("Window/Game/Interactive Visual Editor")]
    public static void Open()
    {
        GetWindow<InteractiveVisualEditorWindow>("Interactive Visual");
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (definition == null)
        {
            EditorGUILayout.HelpBox("Assign or create an Interactive Visual Definition.", MessageType.Info);
            return;
        }

        float toolbarHeight = EditorGUIUtility.singleLineHeight + 8f;
        Rect body = new Rect(0f, toolbarHeight, position.width, position.height - toolbarHeight);
        Rect sidebar = new Rect(0f, body.y, SidebarWidth, body.height);
        Rect canvas = new Rect(SidebarWidth, body.y, Mathf.Max(1f, body.width - SidebarWidth), body.height);

        DrawSidebar(sidebar);
        DrawCanvas(canvas);
        HandleKeyboard();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        InteractiveVisualDefinition next = (InteractiveVisualDefinition)EditorGUILayout.ObjectField(
            definition, typeof(InteractiveVisualDefinition), false, GUILayout.Width(280f));

        if (next != definition)
        {
            definition = next;
            selectedHotspot = -1;
            drawing = false;
        }

        if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(60f)))
            CreateDefinition();

        GUILayout.FlexibleSpace();

        if (definition != null && GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50f)))
        {
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSidebar(Rect rect)
    {
        GUILayout.BeginArea(rect, EditorStyles.helpBox);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
        Sprite newVisual = (Sprite)EditorGUILayout.ObjectField("Image", definition.Visual, typeof(Sprite), false);

        if (newVisual != definition.Visual)
        {
            Undo.RecordObject(definition, "Change Interactive Visual");
            definition.EditorSetVisual(newVisual);
            EditorUtility.SetDirty(definition);
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Hotspots", EditorStyles.boldLabel);

        List<InteractiveVisualDefinition.Hotspot> hotspots = definition.EditorHotspots;

        for (int i = 0; i < hotspots.Count; i++)
        {
            if (GUILayout.Button($"{i + 1}. {hotspots[i].DisplayName}"))
            {
                selectedHotspot = i;
                drawing = false;
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Hotspot")) AddHotspot();

        using (new EditorGUI.DisabledScope(selectedHotspot < 0 || selectedHotspot >= hotspots.Count))
        {
            if (GUILayout.Button("Delete")) DeleteSelectedHotspot();
        }
        EditorGUILayout.EndHorizontal();

        if (selectedHotspot >= 0 && selectedHotspot < hotspots.Count)
            DrawHotspotInspector(hotspots[selectedHotspot]);

        EditorGUILayout.Space(12f);
        EditorGUILayout.HelpBox(
            "Add Hotspot, assign a Location, then click around the image. Click the first point or press Enter to finish. Drag points to edit. Right-click a point to remove it.",
            MessageType.None);

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawHotspotInspector(InteractiveVisualDefinition.Hotspot hotspot)
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Selected Hotspot", EditorStyles.boldLabel);

        string newName = EditorGUILayout.TextField("Name Override", hotspot.EditorDisplayName);
        LocationDefinition newLocation = (LocationDefinition)EditorGUILayout.ObjectField(
            "Location", hotspot.Location, typeof(LocationDefinition), false);

        if (newName != hotspot.EditorDisplayName || newLocation != hotspot.Location)
        {
            Undo.RecordObject(definition, "Edit Hotspot");
            hotspot.EditorSetDisplayName(newName);
            hotspot.EditorSetLocation(newLocation);
            EditorUtility.SetDirty(definition);
        }

        EditorGUILayout.LabelField("Points", hotspot.EditorPoints.Count.ToString());

        if (GUILayout.Button(drawing ? "Finish Drawing" : "Redraw Polygon"))
        {
            if (drawing)
            {
                FinishDrawing();
            }
            else
            {
                Undo.RecordObject(definition, "Redraw Hotspot");
                hotspot.EditorPoints.Clear();
                drawing = true;
                EditorUtility.SetDirty(definition);
            }
        }
    }

    private void DrawCanvas(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f));

        Sprite sprite = definition.Visual;
        if (sprite == null)
        {
            GUI.Label(rect, "Assign an Image in the left panel.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        Texture2D texture = sprite.texture;
        if (texture == null) return;

        Rect sr = sprite.rect;
        float aspect = sr.width / sr.height;
        float availableAspect = rect.width / rect.height;

        if (availableAspect > aspect)
        {
            float width = rect.height * aspect;
            imageRect = new Rect(rect.center.x - width * 0.5f, rect.y, width, rect.height);
        }
        else
        {
            float height = rect.width / aspect;
            imageRect = new Rect(rect.x, rect.center.y - height * 0.5f, rect.width, height);
        }

        Rect uv = new Rect(sr.x / texture.width, sr.y / texture.height, sr.width / texture.width, sr.height / texture.height);
        GUI.DrawTextureWithTexCoords(imageRect, texture, uv, false);

        DrawHotspots();
        HandleCanvasInput();
    }

    private void DrawHotspots()
    {
        List<InteractiveVisualDefinition.Hotspot> hotspots = definition.EditorHotspots;
        Handles.BeginGUI();

        for (int h = 0; h < hotspots.Count; h++)
        {
            var hotspot = hotspots[h];
            List<Vector2> points = hotspot.EditorPoints;
            if (points.Count == 0) continue;

            Vector3[] guiPoints = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 p = NormalizedToGUI(points[i]);
                guiPoints[i] = new Vector3(p.x, p.y, 0f);
            }

            Handles.color = h == selectedHotspot
                ? new Color(1f, 0.75f, 0.1f, 1f)
                : new Color(1f, 1f, 1f, 0.65f);

            for (int i = 0; i < guiPoints.Length - 1; i++)
                Handles.DrawAAPolyLine(3f, guiPoints[i], guiPoints[i + 1]);

            if (points.Count >= 3 && (!drawing || h != selectedHotspot))
                Handles.DrawAAPolyLine(3f, guiPoints[guiPoints.Length - 1], guiPoints[0]);

            if (h == selectedHotspot)
                for (int i = 0; i < guiPoints.Length; i++)
                    Handles.DrawSolidDisc(guiPoints[i], Vector3.forward, 5f);

            GUI.Label(new Rect(guiPoints[0].x + 8f, guiPoints[0].y - 20f, 180f, 20f),
                hotspot.DisplayName, EditorStyles.whiteMiniLabel);
        }

        Handles.EndGUI();
    }

    private void HandleCanvasInput()
    {
        Event e = Event.current;
        if (!imageRect.Contains(e.mousePosition)) return;
        if (selectedHotspot < 0 || selectedHotspot >= definition.EditorHotspots.Count) return;

        var hotspot = definition.EditorHotspots[selectedHotspot];
        List<Vector2> points = hotspot.EditorPoints;

        if (drawing && e.type == EventType.MouseDown && e.button == 0)
        {
            if (points.Count >= 3 && Vector2.Distance(e.mousePosition, NormalizedToGUI(points[0])) <= 12f)
            {
                FinishDrawing();
                e.Use();
                return;
            }

            Undo.RecordObject(definition, "Add Hotspot Point");
            points.Add(GUIToNormalized(e.mousePosition));
            EditorUtility.SetDirty(definition);
            e.Use();
            Repaint();
            return;
        }

        if (!drawing && e.type == EventType.MouseDown && e.button == 1)
        {
            int index = FindPoint(points, e.mousePosition, 10f);
            if (index >= 0)
            {
                Undo.RecordObject(definition, "Remove Hotspot Point");
                points.RemoveAt(index);
                EditorUtility.SetDirty(definition);
                e.Use();
                Repaint();
                return;
            }
        }

        if (!drawing) HandlePointDragging(hotspot, e);
    }

    private void HandlePointDragging(InteractiveVisualDefinition.Hotspot hotspot, Event e)
    {
        List<Vector2> points = hotspot.EditorPoints;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            draggingPoint = FindPoint(points, e.mousePosition, 10f);
            if (draggingPoint >= 0)
            {
                Undo.RecordObject(definition, "Move Hotspot Point");
                e.Use();
            }
        }
        else if (e.type == EventType.MouseDrag && e.button == 0 && draggingPoint >= 0)
        {
            points[draggingPoint] = GUIToNormalized(e.mousePosition);
            EditorUtility.SetDirty(definition);
            e.Use();
            Repaint();
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            draggingPoint = -1;
        }
    }

    private int FindPoint(List<Vector2> points, Vector2 mouse, float radius)
    {
        for (int i = 0; i < points.Count; i++)
            if (Vector2.Distance(NormalizedToGUI(points[i]), mouse) <= radius)
                return i;
        return -1;
    }

    private Vector2 GUIToNormalized(Vector2 gui)
    {
        return new Vector2(
            Mathf.Clamp01(Mathf.InverseLerp(imageRect.xMin, imageRect.xMax, gui.x)),
            Mathf.Clamp01(1f - Mathf.InverseLerp(imageRect.yMin, imageRect.yMax, gui.y)));
    }

    private Vector2 NormalizedToGUI(Vector2 normalized)
    {
        return new Vector2(
            Mathf.Lerp(imageRect.xMin, imageRect.xMax, normalized.x),
            Mathf.Lerp(imageRect.yMax, imageRect.yMin, normalized.y));
    }

    private void HandleKeyboard()
    {
        Event e = Event.current;
        if (!drawing || e.type != EventType.KeyDown) return;

        if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
        {
            FinishDrawing();
            e.Use();
        }
        else if (e.keyCode == KeyCode.Escape)
        {
            drawing = false;
            e.Use();
            Repaint();
        }
    }

    private void AddHotspot()
    {
        Undo.RecordObject(definition, "Add Hotspot");
        definition.EditorHotspots.Add(new InteractiveVisualDefinition.Hotspot());
        selectedHotspot = definition.EditorHotspots.Count - 1;
        drawing = true;
        EditorUtility.SetDirty(definition);
        Repaint();
    }

    private void DeleteSelectedHotspot()
    {
        if (selectedHotspot < 0 || selectedHotspot >= definition.EditorHotspots.Count) return;
        Undo.RecordObject(definition, "Delete Hotspot");
        definition.EditorHotspots.RemoveAt(selectedHotspot);
        selectedHotspot = Mathf.Clamp(selectedHotspot - 1, -1, definition.EditorHotspots.Count - 1);
        drawing = false;
        EditorUtility.SetDirty(definition);
        Repaint();
    }

    private void FinishDrawing()
    {
        drawing = false;
        EditorUtility.SetDirty(definition);
        Repaint();
    }

    private void CreateDefinition()
    {
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/InteractiveVisual.asset");
        InteractiveVisualDefinition asset = CreateInstance<InteractiveVisualDefinition>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();

        definition = asset;
        selectedHotspot = -1;
        drawing = false;
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}
#endif
