using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class LocationActionButton : MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    [Tooltip("Displays the name of the location action.")]
    private TMP_Text actionNameText;

    private Button button;
    private LocationActionDefinition action;

    public LocationActionDefinition Action => action;

    public event Action<LocationActionDefinition> Selected;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(
            HandleButtonClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(
            HandleButtonClicked);
    }

    public void SetAction(
        LocationActionDefinition locationAction)
    {
        action = locationAction;

        if (action == null)
        {
            actionNameText.text = string.Empty;

            return;
        }

        actionNameText.text = action.DisplayName;
    }

    private void HandleButtonClicked()
    {
        if (action == null)
        {
            Debug.LogWarning(
                $"Location action button '{name}' was clicked without an action assigned.",
                this);

            return;
        }

        Selected?.Invoke(
            action);
    }
}