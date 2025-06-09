using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public delegate void OnInputEventHandler(ButtonAction action, ButtonState buttonState);
    public static OnInputEventHandler onInputEventHandler;

    public void onTouchScreenInputEvent(ButtonAction action, ButtonState buttonState)
    {
        onInputEventHandler?.Invoke(action, buttonState);
    }
}

public enum ButtonState
{
    Down,
    Up
}

public enum ButtonAction
{
    Jump
}
