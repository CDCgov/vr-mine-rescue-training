using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum AntiAliasingMode
{
    None,
    FXAA,
    TAA,
    SMAA
}

public enum AntiAliasingQuality
{
    Low,
    Medium,
    High,
}

public enum GraphicsLitShaderMode
{
    Forward,
    Deferred
}

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

    [Description("The anti-aliasing preset to use: FXAA, TAA, SMAA")]
    public AntiAliasingMode AntiAliasingMode  { get;set;}

    [Description("The anti-aliasing quality level: Low, Medium, High")]
    public AntiAliasingQuality AntiAliasingQuality { get; set; }

    [Description("Deferred or Forward rendering")]
    public GraphicsLitShaderMode LitShaderMode { get; set; }

    public override void LoadDefaults()
    {
        LODBias = 1.0f;
        LODBiasVR = 1.5f;

        EnableSSAO = false;
        AntiAliasingMode = AntiAliasingMode.TAA;
        AntiAliasingQuality = AntiAliasingQuality.Medium;

        //ShadowMapResolutionLow = 512;
        //ShadowMapResolutionMedium = 1024;
        //ShadowMapResolutionHigh = 2048;

        DefaultShadowMapResolution = 1024;

        LODLevelDebugView = false;
    }
}
