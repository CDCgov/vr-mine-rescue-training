using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLog : MonoBehaviour
{
    [System.NonSerialized]
    public int EnterCount;
    [System.NonSerialized]
    public int ExitCount;
    [System.NonSerialized]
    public int StayCount;

    void Awake()
    {
        EnterCount = 0;
        ExitCount = 0;
        StayCount = 0;
    }

    void OnCollisionEnter()
    {
        EnterCount++;
    }

    void OnCollisionExit()
    {
        ExitCount++;
    }

    void OnCollisionStay()
    {
        StayCount++;
    }
}
