using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFrameRate : MonoBehaviour
{
    public int TargetFrameRate = 30;


    void Start()
    {
        Application.targetFrameRate = TargetFrameRate;
    }

}
