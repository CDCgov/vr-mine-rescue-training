using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FreezeGlobalRotation : MonoBehaviour 
{
    void Start () 
    {
    
    }
    
    void Update () 
    {
        transform.rotation = Quaternion.identity;
    }
}