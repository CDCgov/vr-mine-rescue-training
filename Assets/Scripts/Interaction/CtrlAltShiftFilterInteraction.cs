using UnityEngine;
using UnityEngine.InputSystem;

public class CtrlAltShiftFilterInteraction : IInputInteraction
{
    public void Process(ref InputInteractionContext context)
    {
        if (context.ControlIsActuated())
        {
            if (Keyboard.current.ctrlKey.IsPressed() &&
                Keyboard.current.altKey.IsPressed() &&
                Keyboard.current.shiftKey.IsPressed())
            {
                context.Performed();
            }
            else
            {
                context.Canceled();
            }
        }
    }

    public void Reset()
    {
        
    }
}
