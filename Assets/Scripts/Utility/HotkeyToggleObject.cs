using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotkeyToggleObject : MonoBehaviour
{
    public GameObject TargetObject;
    public bool StartActive = false;
    public KeyCode Hotkey;
    public bool Shift;
    public bool Ctrl;
    public bool Alt;

    private void Start()
    {
        if (TargetObject == null)
            return;

        TargetObject.SetActive(StartActive);
    }

    void Update()
    {
        if (TargetObject == null)
            return;

        if (Input.GetKeyDown(Hotkey))
        {
            if (Shift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                return;

            if (Ctrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                return;

            if (Alt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                return;

            TargetObject.SetActive(!TargetObject.activeSelf);
        }
    }


}
