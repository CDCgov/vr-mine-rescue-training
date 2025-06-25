using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPhone : GrabBehavior {

    public AudioSource PhoneSource;
    private bool _isGrabbed = false;

    public override void Grabbed()
    {
        base.Grabbed();
        _isGrabbed = true;
        PhoneSource.loop = true;
        PhoneSource.Play();
    }

    public override void Released()
    {
        base.Grabbed();
        _isGrabbed = false;
        PhoneSource.loop = false;
        PhoneSource.Stop();
    }
}
