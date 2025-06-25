using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamstopVisualDestroyHandle : MonoBehaviour
{
    public GameObject TextToDestroy;
    private void OnDestroy()
    {
        if(TextToDestroy != null)
        {
            Destroy(TextToDestroy);
        }
    }
}
