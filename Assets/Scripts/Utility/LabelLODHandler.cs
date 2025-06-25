using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelLODHandler : MonoBehaviour
{
    public GameObject[] LabelsToControl;

    private void OnBecameInvisible()
    {
        if (LabelsToControl != null)
        {
            foreach (GameObject label in LabelsToControl)
            {
                label.SetActive(false);
            }
        }
    }

    private void OnBecameVisible()
    {
        if(LabelsToControl != null)
        {
            foreach(GameObject label in LabelsToControl)
            {
                label.SetActive(true);
            }
        }
    }
}
