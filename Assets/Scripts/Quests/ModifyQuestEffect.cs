using System;
using UnityEngine;

[Serializable]
public class ModifyQuestEffect : GameEffect
{
    [SerializeField]
    private QuestDefinition quest;

    [SerializeField]
    private QuestModification modification =
        QuestModification.Start;

    [SerializeField]
    [Tooltip("Used when the modification is SetStage.")]
    private int stageIndex;

    public QuestDefinition Quest =>
        quest;

    public QuestModification Modification =>
        modification;

    public int StageIndex =>
        stageIndex;

    public override void Apply(
        PlayerGameState gameState)
    {
        if (gameState == null)
        {
            Debug.LogWarning(
                "ModifyQuestEffect cannot be applied because PlayerGameState is null.");

            return;
        }

        if (quest == null)
        {
            Debug.LogWarning(
                "ModifyQuestEffect has no QuestDefinition assigned.");

            return;
        }

        switch (modification)
        {
            case QuestModification.Start:
                gameState.StartQuest(
                    quest);
                break;

            case QuestModification.Advance:
                gameState.AdvanceQuest(
                    quest);
                break;

            case QuestModification.SetStage:
                gameState.SetQuestStage(
                    quest,
                    stageIndex);
                break;

            case QuestModification.Complete:
                gameState.CompleteQuest(
                    quest);
                break;

            case QuestModification.Fail:
                gameState.FailQuest(
                    quest);
                break;

            case QuestModification.Reset:
                gameState.ResetQuest(
                    quest);
                break;

            default:
                Debug.LogWarning(
                    $"Unsupported QuestModification '{modification}'.");
                break;
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

        switch (modification)
        {
            case QuestModification.Start:
                return $"Start {questName}";

            case QuestModification.Advance:
                return $"Advance {questName}";

            case QuestModification.SetStage:
                return
                    $"Set {questName} to Stage {GetStageDescription()}";

            case QuestModification.Complete:
                return $"Complete {questName}";

            case QuestModification.Fail:
                return $"Fail {questName}";

            case QuestModification.Reset:
                return $"Reset {questName}";

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