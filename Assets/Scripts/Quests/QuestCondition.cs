using System;
using UnityEngine;

[Serializable]
public class QuestCondition : GameCondition
{
    [SerializeField]
    private QuestDefinition quest;

    [SerializeField]
    private QuestConditionMode condition =
        QuestConditionMode.Active;

    [SerializeField]
    [Tooltip("Used by AtStage and AtOrAfterStage conditions.")]
    private int stageIndex;

    public QuestDefinition Quest =>
        quest;

    public QuestConditionMode Condition =>
        condition;

    public int StageIndex =>
        stageIndex;

    public override bool IsMet(
        PlayerGameState gameState)
    {
        if (gameState == null)
        {
            return false;
        }

        if (quest == null)
        {
            return false;
        }

        QuestStatus status =
            gameState.GetQuestStatus(quest);

        switch (condition)
        {
            case QuestConditionMode.NotStarted:
                return status ==
                       QuestStatus.NotStarted;

            case QuestConditionMode.Active:
                return status ==
                       QuestStatus.Active;

            case QuestConditionMode.Completed:
                return status ==
                       QuestStatus.Completed;

            case QuestConditionMode.Failed:
                return status ==
                       QuestStatus.Failed;

            case QuestConditionMode.AtStage:
                return status == QuestStatus.Active &&
                       gameState.GetQuestStage(quest) ==
                       stageIndex;

            case QuestConditionMode.AtOrAfterStage:
                return status == QuestStatus.Active &&
                       gameState.GetQuestStage(quest) >=
                       stageIndex;

            default:
                return false;
        }
    }

    public override string GetDescription()
    {
        if (quest == null)
        {
            return "Missing Quest";
        }

        string questName =
            string.IsNullOrWhiteSpace(
                quest.DisplayName)
                ? quest.name
                : quest.DisplayName;

        switch (condition)
        {
            case QuestConditionMode.NotStarted:
                return $"{questName}: Not Started";

            case QuestConditionMode.Active:
                return $"{questName}: Active";

            case QuestConditionMode.Completed:
                return $"{questName}: Completed";

            case QuestConditionMode.Failed:
                return $"{questName}: Failed";

            case QuestConditionMode.AtStage:
                return
                    $"{questName}: Stage {GetStageDescription()}";

            case QuestConditionMode.AtOrAfterStage:
                return
                    $"{questName}: Stage >= {GetStageDescription()}";

            default:
                return questName;
        }
    }

    private string GetStageDescription()
    {
        if (quest == null)
        {
            return stageIndex.ToString();
        }

        string stageName =
            quest.GetStageDisplayName(
                stageIndex);

        if (string.IsNullOrWhiteSpace(
            stageName))
        {
            return stageIndex.ToString();
        }

        return $"{stageIndex} ({stageName})";
    }
}