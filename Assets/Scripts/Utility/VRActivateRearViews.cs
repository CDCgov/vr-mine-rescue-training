using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRActivateRearViews : MonoBehaviour
{
    public GameObject RearViews;

    public void ActivateRearViews(bool active)
    {
        RearViews.SetActive(active);
    }
}
