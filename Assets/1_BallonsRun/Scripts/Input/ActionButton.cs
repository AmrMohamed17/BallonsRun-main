using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public ButtonAction Battle_Action;

    public void OnPointerDown(PointerEventData eventData)
    {
        InputManager.Instance.onTouchScreenInputEvent(Battle_Action, ButtonState.Down);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        InputManager.Instance.onTouchScreenInputEvent(Battle_Action, ButtonState.Up);
    }
}
