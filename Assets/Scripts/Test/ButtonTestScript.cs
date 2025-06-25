using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTestScript : MonoBehaviour
{
    public void OnClick()
    {
        Debug.Log("~~~~~~~ Button clicked! ~~~~~~~");
    }

    public void OnPointerDown()
    {
        Debug.Log($"Pointer down: {Time.frameCount}");
    }
}
