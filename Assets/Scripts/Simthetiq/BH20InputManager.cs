using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Override of the Default unity Event to allow two parameters to be sent when the event is fired
/// </summary>
public class ControllerEvent : UnityEvent<float, InputControllerState>
{
    public InputControllerState state = InputControllerState.Idle;

}

public enum EventKey
{
    LeftJoystictEvent = 0,
    RightJoystickEvent = 1,
    TriggersEvent = 2,
    ButtonAEvent = 3,
    ButtonBEvent = 4,
    ButtonXEvent = 5,
    ButtonYEvent = 6, 
    ButtonBumperRightEvent = 7,
    ButtonBumperLeftEvent = 8,
    ButtonLeftJoystickEvent = 9,

}
/// <summary>
///  Detect input and send signal throughout the scripts
/// </summary>
public class BH20InputManager : MonoBehaviour
{
    public ControllerEvent LeftJoystictEvent;
    public ControllerEvent RightJoystickEvent;
    public ControllerEvent TriggersEvent;
    public ControllerEvent ButtonAEvent;
    public ControllerEvent ButtonBEvent;
    public ControllerEvent ButtonXEvent;
    public ControllerEvent ButtonYEvent;
    public ControllerEvent ButtonBumperRightEvent;
    public ControllerEvent ButtonBumperLeftEvent;
    public ControllerEvent ButtonLeftJoystickEvent;
    private void Awake()
    {
        InitEvent();
    }

    
    // Update is called once per frame
    void Update ()
    {
        UpdateIntEvent(Input.GetKey(KeyCode.JoystickButton0), ref ButtonAEvent);
        UpdateIntEvent(Input.GetKey(KeyCode.JoystickButton1) , ref ButtonBEvent);
        UpdateIntEvent(Input.GetKey(KeyCode.JoystickButton2) , ref  ButtonXEvent);
        UpdateIntEvent(Input.GetKey(KeyCode.JoystickButton3) , ref ButtonYEvent);
        UpdateIntEvent(Input.GetKey(KeyCode.JoystickButton4) , ref ButtonBumperLeftEvent);
        UpdateIntEvent(Input.GetKey(KeyCode.JoystickButton5) , ref ButtonBumperRightEvent);
        UpdateIntEvent(Input.GetKey(KeyCode.JoystickButton8), ref ButtonLeftJoystickEvent);
        
         
        UpdateFloatEvent(Input.GetAxis("GPHorizontal"),-0.2f,0.2f,LeftJoystictEvent);
        UpdateFloatEvent(Input.GetAxisRaw("Triggers"),-0.2f,0.2f,TriggersEvent);// must be added manually to the unity input system, see picture TriggerInputSetting;


        //debug
        // if (Input.GetKey(KeyCode.W))
        // {
        //     TriggersEvent.Invoke(-1,InputControllerState.Hold);
        // }

        // if (Input.GetKey(KeyCode.LeftShift))
        // {
        //     ButtonAEvent.Invoke(1,InputControllerState.Hold);
        // }

    }

    /// <summary>
    /// Check if the key is pressed and send the information to the listeners throughout the scripts
    /// </summary>
    /// <param name="isPressed"></param>
    /// <param name="evnt"></param>
    private void UpdateIntEvent(bool isPressed, ref ControllerEvent evnt)
    {  
       if (isPressed && evnt.state == InputControllerState.Idle)
        {
            evnt.Invoke(1, InputControllerState.Pressed);
            evnt.state = InputControllerState.Pressed;
        }
        else if(isPressed)
        {
            evnt.Invoke(1, InputControllerState.Hold);
            evnt.state = InputControllerState.Hold;
        }
        else if (evnt.state != InputControllerState.Idle && evnt.state != InputControllerState.Released)
        {
            evnt.Invoke(0f,InputControllerState.Released);
            evnt.state = InputControllerState.Released;
        }
        else
        {
            evnt.Invoke(0f,InputControllerState.Idle);
            evnt.state = InputControllerState.Idle;
        }
    }

    /// <summary>
    /// Check if an axis type input is active and send the information to the listeners throughout the scripts
    /// </summary>
    /// <param name="value"></param>
    /// <param name="conditionMin"></param>
    /// <param name="conditionMax"></param>
    /// <param name="evnt"></param>
    private void UpdateFloatEvent(float value, float conditionMin, float conditionMax, ControllerEvent evnt)
    {

        if (IsActive(value, conditionMin, conditionMax))
        {
            evnt.state = InputControllerState.Active;
            evnt.Invoke(value, InputControllerState.Active);
        }
        else
        {
            evnt.state = InputControllerState.Idle;
            evnt.Invoke(0f,InputControllerState.Idle);
        }
    }

    /// <summary>
    /// Initiate the events for future use
    /// </summary>
    private void InitEvent()
    {
        LeftJoystictEvent = new ControllerEvent();
        RightJoystickEvent = new ControllerEvent();
        TriggersEvent = new ControllerEvent();
        ButtonAEvent = new ControllerEvent();
        ButtonBEvent = new ControllerEvent();
        ButtonXEvent = new ControllerEvent();
        ButtonYEvent = new ControllerEvent();
        ButtonBumperLeftEvent = new ControllerEvent();
        ButtonBumperRightEvent = new ControllerEvent();
        ButtonLeftJoystickEvent = new ControllerEvent();
    }

    /// <summary>
    /// check if a value is outside of a min max deadzone
    /// </summary>
    /// <param name="value"></param>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <returns></returns>
    private bool IsActive(float value, float minimum, float maximum)
    {
        if (value < minimum || value > maximum) return true;
        return false;
    }

    /// <summary>
    /// Return the event Displayed in the Enum;
    /// </summary>
    /// <param name="eK"></param>
    /// <returns></returns>
    public ControllerEvent GetControllerEvent(EventKey eK)
    {
        ControllerEvent returnEvent = null;
        switch (eK)
        {
            case EventKey.LeftJoystictEvent:
                returnEvent = LeftJoystictEvent;
                break;
            case EventKey.RightJoystickEvent:
                returnEvent = RightJoystickEvent;
                break;
            case EventKey.TriggersEvent:
                returnEvent = TriggersEvent;
                break;
            case EventKey.ButtonAEvent:
                returnEvent = ButtonAEvent;
                break;
            case EventKey.ButtonBEvent:
                returnEvent = ButtonBEvent;
                break;
            case EventKey.ButtonXEvent:
                returnEvent = ButtonXEvent;
                break;
            case EventKey.ButtonYEvent:
                returnEvent = ButtonYEvent;
                break;
            case EventKey.ButtonBumperRightEvent:
                returnEvent = ButtonBumperRightEvent;
                break;
            case EventKey.ButtonBumperLeftEvent:
                returnEvent = ButtonBumperLeftEvent;
                break;
            case EventKey.ButtonLeftJoystickEvent:
                returnEvent = ButtonLeftJoystickEvent;
                break;
            default:
                throw new ArgumentOutOfRangeException("eK", eK, null);
        }

        return returnEvent;
    }
}
/// <summary>
/// enum for the button/joystick state
/// </summary>
 public enum InputControllerState
{
    Released,
    Pressed,
    Hold,
    Active,
    Idle,
}