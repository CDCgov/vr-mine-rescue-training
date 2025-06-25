using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestSlowZone : MonoBehaviour 
{
    void OnTriggerEnter(Collider col)
    {
        MineSegNavTest navTest = col.gameObject.GetComponent<MineSegNavTest>();
        if (navTest != null)
        {
            navTest.SetTargetSpeed(navTest.SlowSpeed);
        }
    }

    void OnTriggerExit(Collider col)
    {
        MineSegNavTest navTest = col.gameObject.GetComponent<MineSegNavTest>();
        if (navTest != null)
        {
            navTest.SetTargetSpeed(navTest.Speed);
        }
    }
}