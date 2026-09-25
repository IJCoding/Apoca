using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocationActionDefinition))]
public class LocationActionDefinitionEditor : Editor
{
    private SerializedProperty actionId;
    private SerializedProperty displayName;
    private SerializedProperty approach;
    private SerializedProperty description;

    private SerializedProperty requirements;
    private SerializedProperty effects;

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
            serializedObject.FindProperty(
                "actionId");

        displayName =
            serializedObject.FindProperty(
                "displayName");

        approach =
            serializedObject.FindProperty(
                "approach");

        description =
            serializedObject.FindProperty(
                "description");

        requirements =
            serializedObject.FindProperty(
                "requirements");

        effects =
            serializedObject.FindProperty(
                "effects");

        move =
            serializedObject.FindProperty(
                "move");

        strongHitText =
            serializedObject.FindProperty(
                "strongHitText");

        weakHitText =
            serializedObject.FindProperty(
                "weakHitText");

        missText =
            serializedObject.FindProperty(
                "missText");

        followUpActions =
            serializedObject.FindProperty(
                "followUpActions");

        strongHitFollowUpActions =
            serializedObject.FindProperty(
                "strongHitFollowUpActions");

        weakHitFollowUpActions =
            serializedObject.FindProperty(
                "weakHitFollowUpActions");

        missFollowUpActions =
            serializedObject.FindProperty(
                "missFollowUpActions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawIdentitySection();
        DrawNarrativeSection();
        DrawRequirementsSection();
        DrawEffectsSection();
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

    private void DrawRequirementsSection()
    {
        EditorGUILayout.LabelField(
            "Game State Requirements",
            EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Every requirement must be met for this action to be available.",
            MessageType.Info);

        if (requirements == null)
        {
            EditorGUILayout.HelpBox(
                "The serialized 'requirements' property could not be found.",
                MessageType.Error);

            return;
        }

        for (int i = 0;
             i < requirements.arraySize;
             i++)
        {
            SerializedProperty element =
                requirements.GetArrayElementAtIndex(
                    i);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"Requirement {i + 1}",
                EditorStyles.boldLabel);

            if (GUILayout.Button(
                "Remove",
                GUILayout.Width(65f)))
            {
                requirements.DeleteArrayElementAtIndex(
                    i);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(
                element,
                GUIContent.none,
                true);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "+ Add Flag Requirement"))
        {
            AddManagedReference(
                requirements,
                new FlagCondition());
        }

        if (GUILayout.Button(
            "+ Add Resource Requirement"))
        {
            AddManagedReference(
                requirements,
                new ResourceCondition());
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private void DrawEffectsSection()
    {
        EditorGUILayout.LabelField(
            "Game State Effects",
            EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "These effects are applied when this action is selected.",
            MessageType.Info);

        if (effects == null)
        {
            EditorGUILayout.HelpBox(
                "The serialized 'effects' property could not be found.",
                MessageType.Error);

            return;
        }

        for (int i = 0;
             i < effects.arraySize;
             i++)
        {
            SerializedProperty element =
                effects.GetArrayElementAtIndex(
                    i);

            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                $"Effect {i + 1}",
                EditorStyles.boldLabel);

            if (GUILayout.Button(
                "Remove",
                GUILayout.Width(65f)))
            {
                effects.DeleteArrayElementAtIndex(
                    i);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(
                element,
                GUIContent.none,
                true);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "+ Add Set Flag Effect"))
        {
            AddManagedReference(
                effects,
                new SetFlagEffect());
        }

        if (GUILayout.Button(
            "+ Add Modify Resource Effect"))
        {
            AddManagedReference(
                effects,
                new ModifyResourceEffect());
        }

        EditorGUILayout.EndHorizontal();

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

        if (arrayProperty == null)
        {
            EditorGUILayout.HelpBox(
                $"The serialized '{label}' property could not be found.",
                MessageType.Error);

            EditorGUILayout.Space();

            return;
        }

        for (int i = 0;
             i < arrayProperty.arraySize;
             i++)
        {
            SerializedProperty element =
                arrayProperty.GetArrayElementAtIndex(
                    i);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(
                element,
                new GUIContent(
                    $"Element {i}"));

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
                arrayProperty.DeleteArrayElementAtIndex(
                    i);

                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button(
            "+ Add Slot"))
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

    private void AddManagedReference(
        SerializedProperty arrayProperty,
        object value)
    {
        if (arrayProperty == null)
        {
            return;
        }

        Undo.RecordObject(
            target,
            "Add Game State Rule");

        int newIndex =
            arrayProperty.arraySize;

        arrayProperty.InsertArrayElementAtIndex(
            newIndex);

        SerializedProperty newElement =
            arrayProperty.GetArrayElementAtIndex(
                newIndex);

        newElement.managedReferenceValue =
            value;

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(
            target);

        AssetDatabase.SaveAssets();

        serializedObject.Update();
    }

    private void CreateAndAssignAction(
        SerializedProperty element)
    {
        LocationActionDefinition parent =
            (LocationActionDefinition)target;

        string parentPath =
            AssetDatabase.GetAssetPath(
                parent);

        if (string.IsNullOrWhiteSpace(
            parentPath))
        {
            Debug.LogError(
                "Cannot create a follow-up action because the parent action is not saved as an asset.",
                parent);

            return;
        }

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