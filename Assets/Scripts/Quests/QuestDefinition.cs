using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "QuestDefinition",
    menuName = "Game/Quests/Quest Definition")]
public class QuestDefinition : ScriptableObject
{
    [Serializable]
    public class QuestStage
    {
        [SerializeField]
        [Tooltip("The human-readable name of this quest stage.")]
        private string displayName;

        [SerializeField]
        [TextArea(2, 6)]
        [Tooltip("Editor-facing description of what this stage represents.")]
        private string description;

        public string DisplayName =>
            displayName;

        public string Description =>
            description;
    }

    [Header("Identity")]

    [SerializeField]
    [Tooltip("A stable identifier used to distinguish this quest from every other quest.")]
    private string questId;

    [SerializeField]
    [Tooltip("The human-readable name of this quest.")]
    private string displayName;

    [SerializeField]
    [TextArea(2, 6)]
    [Tooltip("A description of the quest.")]
    private string description;

    [Header("Stages")]

    [SerializeField]
    [Tooltip("The ordered stages of this quest.")]
    private QuestStage[] stages =
        Array.Empty<QuestStage>();

    public string QuestId =>
        questId;

    public string DisplayName =>
        displayName;

    public string Description =>
        description;

    public QuestStage[] Stages =>
        stages;

    public int StageCount =>
        stages != null
            ? stages.Length
            : 0;

    public QuestStage GetStage(
        int index)
    {
        if (stages == null ||
            index < 0 ||
            index >= stages.Length)
        {
            return null;
        }

        return stages[index];
    }

    public string GetStageDisplayName(
        int index)
    {
        QuestStage stage =
            GetStage(
                index);

        if (stage == null)
        {
            return string.Empty;
        }

        return stage.DisplayName;
    }
}