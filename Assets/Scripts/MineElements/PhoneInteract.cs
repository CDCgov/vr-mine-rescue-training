using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneInteract : Interactable {
    public PhoneNode node;
    public override void Interact()
    {
        node.PlaySimpleMessage();
    }
}
