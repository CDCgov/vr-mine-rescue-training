using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXEmitOnImpact : MonoBehaviour
{
    public string VFXEventName = "EmitBurst";
    private VisualEffect _vfx;

    // Start is called before the first frame update
    void Start()
    {
        _vfx = GetComponentInChildren<VisualEffect>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_vfx == null)
            return;

        _vfx.SendEvent(VFXEventName);
    }

}
