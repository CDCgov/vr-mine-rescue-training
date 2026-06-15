using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotkeyToggleObject : MonoBehaviour
{
    public GameObject TargetObject;
    public InputActionReference Action;
    public bool StartActive = false;

    [System.Obsolete]
    public KeyCode Hotkey;
    public bool Shift;
    public bool Ctrl;
    public bool Alt;

    private void Start()
    {
        if (TargetObject == null)
            return;

        TargetObject.SetActive(StartActive);

        if (Action != null && Action.action != null)
            Action.action.performed += OnActionPerformed;
    }

    private void OnDestroy()
    {
        if (Action != null && Action.action != null)
            Action.action.performed -= OnActionPerformed;
    }

    //void Update()
    //{
    //    if (TargetObject == null)
    //        return;

    //    if (Input.GetKeyDown(Hotkey))
    //    {
    //        if (Shift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
    //            return;

    //        if (Ctrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
    //            return;

    //        if (Alt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
    //            return;

    //        TargetObject.SetActive(!TargetObject.activeSelf);
    //    }
    //}

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        TargetObject.SetActive(!TargetObject.activeSelf);
    }
}
