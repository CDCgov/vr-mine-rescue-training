using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebriefScreenHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
    }    
}
