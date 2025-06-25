using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContextSetFromConfig : UIContextBase
{
    public SystemManager SystemManager;

    protected override void Start()
    {
        base.Start();

        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _context.SetVariable("OBJECT_UPDATE_RATE", SystemManager.SystemConfig.MPObjectUpdateRateHz);
        _context.SetVariable("VR_UPDATE_RATE", SystemManager.SystemConfig.MPVRUpdateRateHz);
    }
}
