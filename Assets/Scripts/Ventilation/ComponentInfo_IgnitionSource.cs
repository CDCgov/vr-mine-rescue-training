using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_IgnitionSource : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public IgnitionSource IgnitionSource;

    [InspectableBoolProperty("Source Enabled", OffText = "Off", OnText = "On")]
    public bool IgnitionSourceEnabled
    {
        get { return _ignitionSourceEnabled; }
        set
        {
            _ignitionSourceEnabled = value;
            if (IgnitionSource != null)
                IgnitionSource.IgnitionSourceEnabled = value;
        }
    }

    [InspectableBoolProperty("Explosion Visual", OffText = "Off", OnText = "On")]
    public bool EnableExplosionVisual
    {
        get { return _enableExplosionVisual; }
        set
        {
            _enableExplosionVisual = value;
            if (IgnitionSource != null)
                IgnitionSource.EnableExplosionVisual = value;
        }
    }

    [InspectableBoolProperty("Explosion Audio", OffText = "Off", OnText = "On")]
    public bool EnableExplosionAudio
    {
        get { return _enableExplosionAudio; }
        set
        {
            _enableExplosionAudio = value;
            if (IgnitionSource != null)
                IgnitionSource.EnableExplosionAudio = value;
        }
    }

    //[InspectableBoolProperty("Explosive Range Only", OffText = "Off", OnText = "On")]
    public bool ExplosionExplosiveRangeOnly
    {
        get { return _explosionExplosiveRangeOnly; }
        set
        {
            _explosionExplosiveRangeOnly = value;
            if (IgnitionSource != null)
                IgnitionSource.ExplosionExplosiveRangeOnly = value;
        }
    }

    private bool _ignitionSourceEnabled = true;
    private bool _enableExplosionVisual = true;
    private bool _enableExplosionAudio = true;
    private bool _explosionExplosiveRangeOnly = false;

    public string ComponentInspectorTitle => "Ignition Source";
    

    public void Awake()
    {
        if (IgnitionSource == null)
            TryGetComponent<IgnitionSource>(out IgnitionSource);
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        //AddedResistance = component.GetParamValueFloat("AddedResistance", 0.0f);
        IgnitionSourceEnabled = component.GetParamValueBool("IgnitionSourceEnabled", true);
        EnableExplosionVisual = component.GetParamValueBool("EnableExplosionVisual", true);
        EnableExplosionAudio = component.GetParamValueBool("EnableExplosionAudio", true);
        ExplosionExplosiveRangeOnly = component.GetParamValueBool("ExplosiveRangeOnly", false);
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "IgnitionSourceEnabled|" + IgnitionSourceEnabled,
            "EnableExplosionVisual|" + EnableExplosionVisual,
            "EnableExplosionAudio|" + EnableExplosionAudio,
            "ExplosiveRangeOnly|" + ExplosionExplosiveRangeOnly,
        };
    }

    public string SaveName()
    {
        return "IgnitionSource";
    }
}
