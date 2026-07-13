using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public static class InputSystemExtensions
{
    public static bool TryFindAction(this InputActionAsset actions, string actionNameOrId, out InputAction inputAction)
    {
        inputAction = actions.FindAction(actionNameOrId);
        if (inputAction != null)
            return true;

        return false;
    }

    //get mouse position as Vector3 to match Input.mousePosition
    public static Vector3 GetPositionVec3(this Mouse mouse)
    {
        var pos = mouse.position.ReadValue();
        return new Vector3(pos.x, pos.y, 0);
    }
}
