using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class NarrativeInputController : MonoBehaviour
{
    [Header("UI")]

    [SerializeField]
    [Tooltip("The location panel controlled by narrative input.")]
    private LocationPanel locationPanel;

    [Header("Input")]

    [SerializeField]
    [Tooltip("Input action used to select/cycle Valor actions.")]
    private InputActionReference valorAction;

    [SerializeField]
    [Tooltip("Input action used to select/cycle Wit actions.")]
    private InputActionReference witAction;

    [SerializeField]
    [Tooltip("Input action used to select/cycle Soul actions.")]
    private InputActionReference soulAction;

    [SerializeField]
    [Tooltip("Input action used to select/cycle Shadow actions.")]
    private InputActionReference shadowAction;

    [SerializeField]
    [Tooltip("Input action used to select/cycle Fortune actions.")]
    private InputActionReference fortuneAction;

    [SerializeField]
    [Tooltip("Input action used to select/cycle unaligned actions.")]
    private InputActionReference unalignedAction;

    [SerializeField]
    [Tooltip("Input action used to select the previous action.")]
    private InputActionReference previousAction;

    [SerializeField]
    [Tooltip("Input action used to select the next action.")]
    private InputActionReference nextAction;

    [SerializeField]
    [Tooltip("Input action used to confirm the selected action.")]
    private InputActionReference confirmAction;

    private void OnEnable()
    {
        Subscribe(
            valorAction,
            HandleValor);

        Subscribe(
            witAction,
            HandleWit);

        Subscribe(
            soulAction,
            HandleSoul);

        Subscribe(
            shadowAction,
            HandleShadow);

        Subscribe(
            fortuneAction,
            HandleFortune);

        Subscribe(
            unalignedAction,
            HandleUnaligned);

        Subscribe(
            previousAction,
            HandlePrevious);

        Subscribe(
            nextAction,
            HandleNext);

        Subscribe(
            confirmAction,
            HandleConfirm);
    }

    private void OnDisable()
    {
        Unsubscribe(
            valorAction,
            HandleValor);

        Unsubscribe(
            witAction,
            HandleWit);

        Unsubscribe(
            soulAction,
            HandleSoul);

        Unsubscribe(
            shadowAction,
            HandleShadow);

        Unsubscribe(
            fortuneAction,
            HandleFortune);

        Unsubscribe(
            unalignedAction,
            HandleUnaligned);

        Unsubscribe(
            previousAction,
            HandlePrevious);

        Unsubscribe(
            nextAction,
            HandleNext);

        Unsubscribe(
            confirmAction,
            HandleConfirm);
    }

    private void Subscribe(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null)
        {
            return;
        }

        actionReference.action.performed +=
            callback;

        actionReference.action.Enable();
    }

    private void Unsubscribe(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null)
        {
            return;
        }

        actionReference.action.performed -=
            callback;

        actionReference.action.Disable();
    }

    private void HandleValor(
        InputAction.CallbackContext context)
    {
        SelectApproach(
            ActionApproach.Valor);
    }

    private void HandleWit(
        InputAction.CallbackContext context)
    {
        SelectApproach(
            ActionApproach.Wit);
    }

    private void HandleSoul(
        InputAction.CallbackContext context)
    {
        SelectApproach(
            ActionApproach.Soul);
    }

    private void HandleShadow(
        InputAction.CallbackContext context)
    {
        SelectApproach(
            ActionApproach.Shadow);
    }

    private void HandleFortune(
        InputAction.CallbackContext context)
    {
        SelectApproach(
            ActionApproach.Fortune);
    }

    private void HandleUnaligned(
        InputAction.CallbackContext context)
    {
        SelectApproach(
            ActionApproach.None);
    }

    private void HandlePrevious(
        InputAction.CallbackContext context)
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.SelectPreviousAction();
    }

    private void HandleNext(
        InputAction.CallbackContext context)
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.SelectNextAction();
    }

    private void HandleConfirm(
        InputAction.CallbackContext context)
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.ConfirmSelectedAction();
    }

    private void SelectApproach(
        ActionApproach approach)
    {
        if (locationPanel == null)
        {
            return;
        }

        locationPanel.SelectNextApproach(
            approach);
    }
}