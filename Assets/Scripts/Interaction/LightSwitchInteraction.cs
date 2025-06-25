using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitchInteraction : Interactable {
    public bool LightOn = false;
    public GameObject LightObject;

    public override void Interact()
    {
        LightOn = !LightOn;
        LightObject.SetActive(LightOn);
    }
}
