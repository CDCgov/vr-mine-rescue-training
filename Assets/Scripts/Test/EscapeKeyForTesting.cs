using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EscapeKeyForTesting : MonoBehaviour 
{
    void Start () 
    {
    
    }
    
    void Update () 
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}