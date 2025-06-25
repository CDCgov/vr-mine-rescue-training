using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputTargetOptions 
{
    //options
    public bool LookRequiresMousePress = false;
    public bool ToggleMouseCapture = false;

    //state variables
    public bool MouseIsCaptured = false;

    public void ResetState()
    {
        MouseIsCaptured = false;
    }
}