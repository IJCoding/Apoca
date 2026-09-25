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

    public event Action<GameFlagDefinition, bool> FlagChanged;

    public event Action<GameResourceDefinition, int> ResourceChanged;

    private void Awake()
    {
        InitialiseFlags();
        InitialiseResources();
    }

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

    public void ResetAllGameState()
    {
        ResetAllFlags();
        ResetAllResources();
    }

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
}