using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Composites;


#if UNITY_EDITOR
[InitializeOnLoad] // Automatically register in editor.
#endif
[DisplayStringFormat("Ctrl+Alt+Shift+{Hotkey}")]
public class CtrlAltShiftKeyComposite : InputBindingComposite<float>
{
    [InputControl(layout = "Button")] public int Hotkey;

    public bool HiddenBinding = false;

    public override float ReadValue(ref InputBindingCompositeContext context)
    {
        if (Keyboard.current.ctrlKey.IsPressed() &&
            Keyboard.current.altKey.IsPressed() &&
            Keyboard.current.shiftKey.IsPressed())
        {
            return context.ReadValue<float>(Hotkey);
        }
        else
        {
            return 0;
        }
    }

    public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
    {
        return ReadValue(ref context);
    }

    static CtrlAltShiftKeyComposite()
    {
        InputSystem.RegisterBindingComposite<CtrlAltShiftKeyComposite>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() { } // Trigger static constructor.
}
