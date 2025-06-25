using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXSetGroundPlane : MonoBehaviour
{
    public TeleportManager TeleportManager;

    private VisualEffect _vfx;

    public void Start()
    {
        Vector3 groundPlanePos = Vector3.zero;

        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        if (TeleportManager != null && TeleportManager.ActiveTeleportTarget != null)
        {
            groundPlanePos = TeleportManager.ActiveTeleportTarget.position;
        }

        _vfx = GetComponent<VisualEffect>();
        _vfx.SetVector3("GroundPlanePos", groundPlanePos);
    }

}
