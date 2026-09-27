using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class TypewriterSkipTarget : MonoBehaviour, IPointerClickHandler
{
    public event Action Clicked;

    public void OnPointerClick(
        PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
