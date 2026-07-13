using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Composites;


#if UNITY_EDITOR
[InitializeOnLoad] // Automatically register in editor.
#endif
[DisplayStringFormat("{Modifier1}+{Modifier2}+{Modifier3}+{Hotkey}")]
public class ThreeModifierKeyComposite : InputBindingComposite<float>
{
    [InputControl(layout = "Button")] public int Modifier1;
    [InputControl(layout = "Button")] public int Modifier2;
    [InputControl(layout = "Button")] public int Modifier3;
    [InputControl(layout = "Button")] public int Hotkey;

    public override float ReadValue(ref InputBindingCompositeContext context)
    {
        if (!context.ReadValueAsButton(Modifier1))
            return 0;
        if (!context.ReadValueAsButton(Modifier2))
            return 0;
        if (!context.ReadValueAsButton(Modifier3))
            return 0;

        return context.ReadValue<float>(Hotkey);
    }

    public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
    {
        var result = ReadValue(ref context);
        return result > 0 ? 1.0f : 0.0f;
    }

    static ThreeModifierKeyComposite()
    {
        Debug.Log($"Three Modifier Keybind Registered");
        InputSystem.RegisterBindingComposite<ThreeModifierKeyComposite>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() { } // Trigger static constructor.
}
