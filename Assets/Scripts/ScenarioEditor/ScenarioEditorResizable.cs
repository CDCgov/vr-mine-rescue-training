using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Default implementation of IScenarioEditorResizable using the transform scale
/// </summary>
public class ScenarioEditorResizable : MonoBehaviour, IScenarioEditorResizable
{
    public Vector3 Size
    {
        get
        {
            return transform.localScale;
        }
        set
        {
            transform.localScale = value;
        }
    }

    public Vector3 LocalCenter
    {
        get
        {
            return Vector3.zero;
        }
    }


    public void SetSize(Vector3 size, Vector3 center)
    {
        transform.position = center;
        transform.localScale = size;
    }
}
