using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LightGraphicsConfig : MonoBehaviour
{
    public SystemManager SystemManager;

    private Light _light;
    private HDAdditionalLightData _lightData;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        TryGetComponent<HDAdditionalLightData>(out _lightData);
        TryGetComponent<Light>(out _light);

        if (SystemManager.GraphicsConfig == null)
        {
            Debug.LogError($"Couldn't find graphics configuration on {gameObject.name}");
            return;
        }

        ApplyGraphicsConfig(SystemManager.GraphicsConfig);

        SystemManager.GraphicsConfigChanged += OnGraphicsConfigChanged;
    }

    private void OnGraphicsConfigChanged(GraphicsConfig config)
    {
        ApplyGraphicsConfig(config);
    }

    void ApplyGraphicsConfig(GraphicsConfig config)
    {
        if (_light == null || SystemManager == null || config == null)
            return;

        if (_lightData != null)
            SetupLight(_light, _lightData, config);
        else
            SetupLight(_light, config);
    }

    void SetupLight(Light light, GraphicsConfig config)
    {
        
    }

    void SetupLight(Light light, HDAdditionalLightData lightData, GraphicsConfig config)
    {
        if (light.shadows != LightShadows.None)
        {
            lightData.SetShadowResolutionOverride(true);
            lightData.SetShadowResolution(config.DefaultShadowMapResolution);
        }
    }
}
