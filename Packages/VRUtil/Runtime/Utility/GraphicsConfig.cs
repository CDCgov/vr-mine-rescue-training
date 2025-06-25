using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsConfig : YAMLConfig
{

    public float LODBias { get; set; }
    public float LODBiasVR { get; set; }

    public bool EnableSSAO { get; set; }
    //public int ShadowMapResolutionLow { get; set; }
    //public int ShadowMapResolutionMedium { get; set; }
    //public int ShadowMapResolutionHigh { get; set; }

    public int DefaultShadowMapResolution { get; set; }

    public bool LODLevelDebugView { get; set; }

    public override void LoadDefaults()
    {
        LODBias = 1.0f;
        LODBiasVR = 1.5f;

        EnableSSAO = false;
        //ShadowMapResolutionLow = 512;
        //ShadowMapResolutionMedium = 1024;
        //ShadowMapResolutionHigh = 2048;

        DefaultShadowMapResolution = 1024;

        LODLevelDebugView = false;
    }
}
