using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGameState : MonoBehaviour
{
    [Serializable]
    private class FlagState
    {
        [SerializeField]
        private GameFlagDefinition flag;

        [SerializeField]
        private bool value;

        public GameFlagDefinition Flag =>
            flag;

        public bool Value
        {
            get => value;
            set => this.value = value;
        }

        public FlagState(
            GameFlagDefinition flag,
            bool value)
        {
            this.flag = flag;
            this.value = value;
        }
    }

    [Serializable]
    private class ResourceState
    {
        [SerializeField]
        private GameResourceDefinition resource;

        [SerializeField]
        private int value;

        public GameResourceDefinition Resource =>
            resource;

        public int Value
        {
            get => value;
            set => this.value = value;
        }

        public ResourceState(
            GameResourceDefinition resource,
            int value)
        {
            this.resource = resource;
            this.value = value;
        }
    }

    [Serializable]
    private class QuestState
    {
        [SerializeField]
        private QuestDefinition quest;

        [SerializeField]
        private QuestStatus status;

        [SerializeField]
        private int stageIndex;

        public QuestDefinition Quest =>
            quest;

        public QuestStatus Status
        {
            get => status;
            set => status = value;
        }

        public int StageIndex
        {
            get => stageIndex;
            set => stageIndex = value;
        }

        public QuestState(
            QuestDefinition quest,
            QuestStatus status,
            int stageIndex)
        {
            this.quest = quest;
            this.status = status;
            this.stageIndex = stageIndex;
        }
    }

    [Header("Known Flags")]

    [SerializeField]
    [Tooltip("Flags explicitly tracked by the current game state.")]
    private List<GameFlagDefinition> knownFlags =
        new List<GameFlagDefinition>();

    [Header("Known Resources")]

    [SerializeField]
    [Tooltip("Resources explicitly tracked by the current game state.")]
    private List<GameResourceDefinition> knownResources =
        new List<GameResourceDefinition>();

    [Header("Known Quests")]

    [SerializeField]
    [Tooltip("Quests explicitly tracked by the current game state.")]
    private List<QuestDefinition> knownQuests =
        new List<QuestDefinition>();

    [Header("Runtime Flag State")]

    [SerializeField]
    [Tooltip("Current runtime flag values. Normally populated when the game starts.")]
    private List<FlagState> flagStates =
        new List<FlagState>();

    [Header("Runtime Resource State")]

    [SerializeField]
    [Tooltip("Current runtime resource values. Normally populated when the game starts.")]
    private List<ResourceState> resourceStates =
        new List<ResourceState>();

    [Header("Runtime Quest State")]

    [SerializeField]
    [Tooltip("Current runtime quest values. Normally populated when the game starts.")]
    private List<QuestState> questStates =
        new List<QuestState>();

    public event Action<GameFlagDefinition, bool> FlagChanged;

    public event Action<GameResourceDefinition, int> ResourceChanged;

    public event Action<QuestDefinition, QuestStatus, int> QuestChanged;

    private void Awake()
    {
        InitialiseFlags();
        InitialiseResources();
        InitialiseQuests();
    }

    // ============================================================
    // Flags
    // ============================================================

    public bool GetFlag(
        GameFlagDefinition flag)
    {
        if (flag == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to read a null GameFlagDefinition.",
                this);

            return false;
        }

        FlagState state =
            FindFlagState(flag);

        if (state != null)
        {
            return state.Value;
        }

        return flag.DefaultValue;
    }

    public void SetFlag(
        GameFlagDefinition flag,
        bool value)
    {
        if (flag == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to set a null GameFlagDefinition.",
                this);

            return;
        }

        FlagState state =
            FindFlagState(flag);

        if (state == null)
        {
            state =
                new FlagState(
                    flag,
                    flag.DefaultValue);

            flagStates.Add(state);
        }

        if (state.Value == value)
        {
            return;
        }

        state.Value = value;

        FlagChanged?.Invoke(
            flag,
            value);
    }

    public void ResetFlag(
        GameFlagDefinition flag)
    {
        if (flag == null)
        {
            return;
        }

        SetFlag(
            flag,
            flag.DefaultValue);
    }

    public void ResetAllFlags()
    {
        flagStates.Clear();

        InitialiseFlags();
    }

    public bool IsFlagKnown(
        GameFlagDefinition flag)
    {
        if (flag == null)
        {
            return false;
        }

        return FindFlagState(flag) != null;
    }

    // ============================================================
    // Resources
    // ============================================================

    public int GetResource(
        GameResourceDefinition resource)
    {
        if (resource == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to read a null GameResourceDefinition.",
                this);

            return 0;
        }

        ResourceState state =
            FindResourceState(resource);

        if (state != null)
        {
            return state.Value;
        }

        return resource.DefaultValue;
    }

    public void SetResource(
        GameResourceDefinition resource,
        int value)
    {
        if (resource == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to set a null GameResourceDefinition.",
                this);

            return;
        }

        int clampedValue =
            resource.ClampValue(value);

        ResourceState state =
            FindResourceState(resource);

        if (state == null)
        {
            state =
                new ResourceState(
                    resource,
                    resource.DefaultValue);

            resourceStates.Add(state);
        }

        if (state.Value == clampedValue)
        {
            return;
        }

        state.Value = clampedValue;

        ResourceChanged?.Invoke(
            resource,
            clampedValue);
    }

    public void ModifyResource(
        GameResourceDefinition resource,
        int amount)
    {
        if (resource == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to modify a null GameResourceDefinition.",
                this);

            return;
        }

        int currentValue =
            GetResource(resource);

        SetResource(
            resource,
            currentValue + amount);
    }

    public void ResetResource(
        GameResourceDefinition resource)
    {
        if (resource == null)
        {
            return;
        }

        SetResource(
            resource,
            resource.DefaultValue);
    }

    public void ResetAllResources()
    {
        resourceStates.Clear();

        InitialiseResources();
    }

    public bool IsResourceKnown(
        GameResourceDefinition resource)
    {
        if (resource == null)
        {
            return false;
        }

        return FindResourceState(resource) != null;
    }

    // ============================================================
    // Quests
    // ============================================================

    public QuestStatus GetQuestStatus(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to read a null QuestDefinition.",
                this);

            return QuestStatus.NotStarted;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null)
        {
            return QuestStatus.NotStarted;
        }

        return state.Status;
    }

    public int GetQuestStage(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to read a null QuestDefinition.",
                this);

            return -1;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null ||
            state.Status == QuestStatus.NotStarted)
        {
            return -1;
        }

        return state.StageIndex;
    }

    public bool IsQuestStarted(
        QuestDefinition quest)
    {
        QuestStatus status =
            GetQuestStatus(quest);

        return status == QuestStatus.Active ||
               status == QuestStatus.Completed ||
               status == QuestStatus.Failed;
    }

    public bool IsQuestActive(
        QuestDefinition quest)
    {
        return GetQuestStatus(quest) ==
               QuestStatus.Active;
    }

    public bool IsQuestCompleted(
        QuestDefinition quest)
    {
        return GetQuestStatus(quest) ==
               QuestStatus.Completed;
    }

    public bool IsQuestFailed(
        QuestDefinition quest)
    {
        return GetQuestStatus(quest) ==
               QuestStatus.Failed;
    }

    public void StartQuest(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to start a null QuestDefinition.",
                this);

            return;
        }

        if (quest.StageCount <= 0)
        {
            Debug.LogWarning(
                $"Quest '{quest.name}' cannot be started because it has no stages.",
                quest);

            return;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null)
        {
            state =
                new QuestState(
                    quest,
                    QuestStatus.NotStarted,
                    -1);

            questStates.Add(state);
        }

        if (state.Status != QuestStatus.NotStarted)
        {
            return;
        }

        state.Status =
            QuestStatus.Active;

        state.StageIndex =
            0;

        QuestChanged?.Invoke(
            quest,
            state.Status,
            state.StageIndex);
    }

    public void SetQuestStage(
        QuestDefinition quest,
        int stageIndex)
    {
        if (quest == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to set the stage of a null QuestDefinition.",
                this);

            return;
        }

        if (quest.StageCount <= 0)
        {
            Debug.LogWarning(
                $"Quest '{quest.name}' has no stages.",
                quest);

            return;
        }

        if (stageIndex < 0 ||
            stageIndex >= quest.StageCount)
        {
            Debug.LogWarning(
                $"Quest '{quest.name}' does not contain stage index {stageIndex}.",
                quest);

            return;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null)
        {
            state =
                new QuestState(
                    quest,
                    QuestStatus.Active,
                    stageIndex);

            questStates.Add(state);
        }
        else
        {
            state.Status =
                QuestStatus.Active;

            state.StageIndex =
                stageIndex;
        }

        QuestChanged?.Invoke(
            quest,
            state.Status,
            state.StageIndex);
    }

    public void AdvanceQuest(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to advance a null QuestDefinition.",
                this);

            return;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null ||
            state.Status == QuestStatus.NotStarted)
        {
            StartQuest(quest);

            return;
        }

        if (state.Status == QuestStatus.Completed ||
            state.Status == QuestStatus.Failed)
        {
            return;
        }

        int nextStage =
            state.StageIndex + 1;

        if (nextStage >= quest.StageCount)
        {
            CompleteQuest(quest);

            return;
        }

        state.StageIndex =
            nextStage;

        QuestChanged?.Invoke(
            quest,
            state.Status,
            state.StageIndex);
    }

    public void CompleteQuest(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to complete a null QuestDefinition.",
                this);

            return;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null)
        {
            state =
                new QuestState(
                    quest,
                    QuestStatus.Completed,
                    GetFinalQuestStageIndex(quest));

            questStates.Add(state);
        }
        else
        {
            state.Status =
                QuestStatus.Completed;

            state.StageIndex =
                GetFinalQuestStageIndex(quest);
        }

        QuestChanged?.Invoke(
            quest,
            state.Status,
            state.StageIndex);
    }

    public void FailQuest(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            Debug.LogWarning(
                "PlayerGameState was asked to fail a null QuestDefinition.",
                this);

            return;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null)
        {
            state =
                new QuestState(
                    quest,
                    QuestStatus.Failed,
                    -1);

            questStates.Add(state);
        }
        else
        {
            if (state.Status == QuestStatus.Failed)
            {
                return;
            }

            state.Status =
                QuestStatus.Failed;
        }

        QuestChanged?.Invoke(
            quest,
            state.Status,
            state.StageIndex);
    }

    public void ResetQuest(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            return;
        }

        QuestState state =
            FindQuestState(quest);

        if (state == null)
        {
            return;
        }

        state.Status =
            QuestStatus.NotStarted;

        state.StageIndex =
            -1;

        QuestChanged?.Invoke(
            quest,
            state.Status,
            state.StageIndex);
    }

    public void ResetAllQuests()
    {
        questStates.Clear();

        InitialiseQuests();
    }

    public void RegisterQuest(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            return;
        }

        if (!knownQuests.Contains(quest))
        {
            knownQuests.Add(quest);
        }

        GetOrCreateQuestState(quest);
    }

    public bool IsQuestKnown(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            return false;
        }

        return FindQuestState(quest) != null;
    }

    // ============================================================
    // Global Reset
    // ============================================================

    public void ResetAllGameState()
    {
        ResetAllFlags();
        ResetAllResources();
        ResetAllQuests();
    }

    // ============================================================
    // Initialisation
    // ============================================================

    private void InitialiseFlags()
    {
        foreach (GameFlagDefinition flag in knownFlags)
        {
            if (flag == null)
            {
                continue;
            }

            if (FindFlagState(flag) != null)
            {
                continue;
            }

            flagStates.Add(
                new FlagState(
                    flag,
                    flag.DefaultValue));
        }
    }

    private void InitialiseResources()
    {
        foreach (GameResourceDefinition resource in knownResources)
        {
            if (resource == null)
            {
                continue;
            }

            if (FindResourceState(resource) != null)
            {
                continue;
            }

            resourceStates.Add(
                new ResourceState(
                    resource,
                    resource.DefaultValue));
        }
    }

    private void InitialiseQuests()
    {
        foreach (QuestDefinition quest in knownQuests)
        {
            if (quest == null)
            {
                continue;
            }

            if (FindQuestState(quest) != null)
            {
                continue;
            }

            questStates.Add(
                new QuestState(
                    quest,
                    QuestStatus.NotStarted,
                    -1));
        }
    }

    // ============================================================
    // State Lookup
    // ============================================================

    private FlagState FindFlagState(
        GameFlagDefinition flag)
    {
        foreach (FlagState state in flagStates)
        {
            if (state == null)
            {
                continue;
            }

            if (state.Flag == flag)
            {
                return state;
            }
        }

        return null;
    }

    private ResourceState FindResourceState(
        GameResourceDefinition resource)
    {
        foreach (ResourceState state in resourceStates)
        {
            if (state == null)
            {
                continue;
            }

            if (state.Resource == resource)
            {
                return state;
            }
        }

        return null;
    }

    private QuestState FindQuestState(
        QuestDefinition quest)
    {
        if (quest != null &&
            !knownQuests.Contains(quest))
        {
            knownQuests.Add(quest);
        }

        foreach (QuestState state in questStates)
        {
            if (state == null)
            {
                continue;
            }

            if (state.Quest == quest)
            {
                return state;
            }
        }

        return null;
    }

    private QuestState GetOrCreateQuestState(
        QuestDefinition quest)
    {
        if (quest == null)
        {
            return null;
        }

        QuestState state =
            FindQuestState(quest);

        if (state != null)
        {
            return state;
        }

        state =
            new QuestState(
                quest,
                QuestStatus.NotStarted,
                -1);

        questStates.Add(state);

        return state;
    }

    private int GetFinalQuestStageIndex(
        QuestDefinition quest)
    {
        if (quest == null ||
            quest.StageCount <= 0)
        {
            return -1;
        }

        return quest.StageCount - 1;
    }
}