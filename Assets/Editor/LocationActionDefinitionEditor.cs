using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocationActionDefinition))]
public class LocationActionDefinitionEditor : Editor
{
    private SerializedProperty actionId;
    private SerializedProperty displayName;
    private SerializedProperty approach;
    private SerializedProperty description;
    private SerializedProperty move;

    private SerializedProperty strongHitText;
    private SerializedProperty weakHitText;
    private SerializedProperty missText;

    private SerializedProperty followUpActions;
    private SerializedProperty strongHitFollowUpActions;
    private SerializedProperty weakHitFollowUpActions;
    private SerializedProperty missFollowUpActions;

    private void OnEnable()
    {
        actionId =
            serializedObject.FindProperty("actionId");

        displayName =
            serializedObject.FindProperty("displayName");

        approach =
            serializedObject.FindProperty("approach");

        description =
            serializedObject.FindProperty("description");

        move =
            serializedObject.FindProperty("move");

        strongHitText =
            serializedObject.FindProperty("strongHitText");

        weakHitText =
            serializedObject.FindProperty("weakHitText");

        missText =
            serializedObject.FindProperty("missText");

        followUpActions =
            serializedObject.FindProperty("followUpActions");

        strongHitFollowUpActions =
            serializedObject.FindProperty("strongHitFollowUpActions");

        weakHitFollowUpActions =
            serializedObject.FindProperty("weakHitFollowUpActions");

        missFollowUpActions =
            serializedObject.FindProperty("missFollowUpActions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawIdentitySection();
        DrawNarrativeSection();
        DrawMoveSection();

        if (move.objectReferenceValue == null)
        {
            DrawActionArray(
                followUpActions,
                "Standard Follow-Up Actions");
        }
        else
        {
            DrawActionArray(
                strongHitFollowUpActions,
                "Strong Hit Follow-Up Actions");

            DrawActionArray(
                weakHitFollowUpActions,
                "Weak Hit Follow-Up Actions");

            DrawActionArray(
                missFollowUpActions,
                "Miss Follow-Up Actions");
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawIdentitySection()
    {
        EditorGUILayout.LabelField(
            "Identity",
            EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            actionId);

        EditorGUILayout.PropertyField(
            displayName);

        EditorGUILayout.PropertyField(
            approach);

        EditorGUILayout.Space();
    }

    private void DrawNarrativeSection()
    {
        EditorGUILayout.LabelField(
            "Narrative",
            EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            description);

        EditorGUILayout.Space();
    }

    private void DrawMoveSection()
    {
        EditorGUILayout.LabelField(
            "Move",
            EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(
            move);

        if (move.objectReferenceValue != null)
        {
            EditorGUILayout.PropertyField(
                strongHitText);

            EditorGUILayout.PropertyField(
                weakHitText);

            EditorGUILayout.PropertyField(
                missText);
        }

        EditorGUILayout.Space();
    }

    private void DrawActionArray(
        SerializedProperty arrayProperty,
        string label)
    {
        EditorGUILayout.LabelField(
            label,
            EditorStyles.boldLabel);

        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            SerializedProperty element =
                arrayProperty.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(
                element,
                new GUIContent($"Element {i}"));

            if (GUILayout.Button(
                "New",
                GUILayout.Width(50f)))
            {
                CreateAndAssignAction(
                    element);
            }

            if (GUILayout.Button(
                "-",
                GUILayout.Width(25f)))
            {
                arrayProperty.DeleteArrayElementAtIndex(i);

                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Slot"))
        {
            int newIndex =
                arrayProperty.arraySize;

            arrayProperty.InsertArrayElementAtIndex(
                newIndex);

            SerializedProperty newElement =
                arrayProperty.GetArrayElementAtIndex(
                    newIndex);

            newElement.objectReferenceValue =
                null;
        }

        EditorGUILayout.Space();
    }

    private void CreateAndAssignAction(
        SerializedProperty element)
    {
        LocationActionDefinition parent =
            (LocationActionDefinition)target;

        string parentPath =
            AssetDatabase.GetAssetPath(
                parent);

        string folderPath =
            System.IO.Path.GetDirectoryName(
                parentPath);

        string parentFileName =
            System.IO.Path.GetFileNameWithoutExtension(
                parentPath);

        string newPath =
            AssetDatabase.GenerateUniqueAssetPath(
                $"{folderPath}/{parentFileName}_Action.asset");

        LocationActionDefinition newAction =
            CreateInstance<LocationActionDefinition>();

        AssetDatabase.CreateAsset(
            newAction,
            newPath);

        AssetDatabase.SaveAssets();

        element.objectReferenceValue =
            newAction;

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            parent);

        Selection.activeObject =
            newAction;

        EditorGUIUtility.PingObject(
            newAction);
    }
}