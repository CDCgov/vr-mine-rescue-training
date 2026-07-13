using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class VolumeGraphicsConfig : MonoBehaviour
{
    public SystemManager SystemManager;

    private Volume _volume;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _volume = gameObject.AddComponent<Volume>();
        _volume.profile = new VolumeProfile();
        _volume.priority = 1000;

        ApplyGraphicsConfig(SystemManager.GraphicsConfig);

        SystemManager.GraphicsConfigChanged += OnGraphicsConfigChanged;
    }

    private void OnGraphicsConfigChanged(GraphicsConfig config)
    {
        ApplyGraphicsConfig(config);
    }

    void ApplyGraphicsConfig(GraphicsConfig config)
    {
        if (!_volume.profile.TryGet<HDShadowSettings>(out var shadowSettings))
        {
            shadowSettings = _volume.profile.Add<HDShadowSettings>();
        }

        if (config.ShadowQuality == GraphicsQuality.Low)
        {
            shadowSettings.active = true;
            shadowSettings.maxShadowDistance.Override(125);
            shadowSettings.cascadeShadowSplitCount.Override(2);
            shadowSettings.cascadeShadowSplit0.Override(0.3f);
            shadowSettings.cascadeShadowBorder0.Override(0.2f);
            shadowSettings.cascadeShadowBorder1.Override(0.3f);
        }
        else
        {
            shadowSettings.active = false;
        }
    }
}
