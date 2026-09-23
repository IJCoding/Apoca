using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NarrativeInputController : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    [Tooltip("The LocationPanel controlled by narrative input.")]
    private LocationPanel locationPanel;

    [SerializeField]
    [Tooltip("The ScrollRect used by the LocationPanel narrative area.")]
    private ScrollRect scrollRect;

    [Header("Input Actions")]

    [SerializeField]
    private InputActionReference valorAction;

    [SerializeField]
    private InputActionReference witAction;

    [SerializeField]
    private InputActionReference soulAction;

    [SerializeField]
    private InputActionReference shadowAction;

    [SerializeField]
    private InputActionReference fortuneAction;

    [SerializeField]
    private InputActionReference unalignedAction;

    [SerializeField]
    private InputActionReference previousAction;

    [SerializeField]
    private InputActionReference nextAction;

    [SerializeField]
    private InputActionReference confirmAction;

    [SerializeField]
    private InputActionReference scrollDownAction;

    [SerializeField]
    private InputActionReference scrollUpAction;

    [Header("Scrolling")]

    [SerializeField]
    [Range(0.01f, 0.5f)]
    [Tooltip("How far the narrative scrolls for each Scroll Up or Scroll Down input.")]
    private float scrollStep = 0.15f;

    private void Reset()
    {
        locationPanel =
            GetComponent<LocationPanel>();

        scrollRect =
            GetComponentInChildren<ScrollRect>();
    }

    private void Awake()
    {
        if (locationPanel == null)
        {
            locationPanel =
                GetComponent<LocationPanel>();
        }

        if (scrollRect == null)
        {
            scrollRect =
                GetComponentInChildren<ScrollRect>();
        }
    }

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

        Subscribe(
            scrollDownAction,
            HandleScrollDown);

        Subscribe(
            scrollUpAction,
            HandleScrollUp);
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

        Unsubscribe(
            scrollDownAction,
            HandleScrollDown);

        Unsubscribe(
            scrollUpAction,
            HandleScrollUp);
    }

    private void Subscribe(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference == null ||
            actionReference.action == null)
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
        if (actionReference == null ||
            actionReference.action == null)
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
        if (locationPanel == null ||
            !locationPanel.HasSelectedAction)
        {
            return;
        }

        locationPanel.ConfirmSelectedAction();
    }

    private void HandleScrollDown(
        InputAction.CallbackContext context)
    {
        Scroll(
            -scrollStep);
    }

    private void HandleScrollUp(
        InputAction.CallbackContext context)
    {
        Scroll(
            scrollStep);
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

    private void Scroll(
        float amount)
    {
        if (scrollRect == null)
        {
            return;
        }

        scrollRect.StopMovement();

        float newPosition =
            scrollRect.verticalNormalizedPosition +
            amount;

        scrollRect.verticalNormalizedPosition =
            Mathf.Clamp01(
                newPosition);
    }
}