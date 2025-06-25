using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class LogHandleEditor : MonoBehaviour 
{
    [MenuItem("CONTEXT/LogHandle/Foo")]
    public static void Bar()
    {
        Debug.Log("foobar");
    }
}