using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


public class CameraGraphicsConfig : MonoBehaviour
{
    public SystemManager SystemManager;
    public bool IsVRCamera = false;

    private HDAdditionalCameraData _hdData;
    private FrameSettingsOverrideMask _frameMask;
    

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _hdData = GetComponent<HDAdditionalCameraData>();
        if (_hdData == null)
        {
            Debug.LogError($"Couldn't find HDAdditionalCameraData on {gameObject.name}");
            return;
        }

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
        if (_hdData == null)
            return;

        _hdData.customRenderingSettings = true;

        _frameMask = _hdData.renderingPathCustomFrameSettingsOverrideMask;
        _frameMask.mask[(uint)FrameSettingsField.LODBiasMode] = true;
        _frameMask.mask[(uint)FrameSettingsField.LODBias] = true;
        _frameMask.mask[(uint)FrameSettingsField.SSAO] = true;
        _frameMask.mask[(uint)FrameSettingsField.LitShaderMode] = true;
        _hdData.renderingPathCustomFrameSettingsOverrideMask = _frameMask;

        if (config.LitShaderMode == GraphicsLitShaderMode.Forward)
            _hdData.renderingPathCustomFrameSettings.litShaderMode = LitShaderMode.Forward;
        else
            _hdData.renderingPathCustomFrameSettings.litShaderMode = LitShaderMode.Deferred;

        switch (config.AntiAliasingMode)
        {
            case AntiAliasingMode.FXAA:
                _hdData.antialiasing = HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                break;

            case AntiAliasingMode.TAA:
                _hdData.antialiasing = HDAdditionalCameraData.AntialiasingMode.TemporalAntialiasing;
                break;

            case AntiAliasingMode.SMAA:
                _hdData.antialiasing = HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                break;
        }

        switch (config.AntiAliasingQuality)
        {
            case AntiAliasingQuality.Low:
                _hdData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.Low;
                _hdData.TAAQuality = HDAdditionalCameraData.TAAQualityLevel.Low;
                break;

            case AntiAliasingQuality.Medium:
                _hdData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.Medium;
                _hdData.TAAQuality = HDAdditionalCameraData.TAAQualityLevel.Medium;
                break;

            case AntiAliasingQuality.High:
                _hdData.SMAAQuality = HDAdditionalCameraData.SMAAQualityLevel.High;
                _hdData.TAAQuality = HDAdditionalCameraData.TAAQualityLevel.High;
                break;
        }

        


        _hdData.renderingPathCustomFrameSettings.lodBiasMode = LODBiasMode.OverrideQualitySettings;
        if (IsVRCamera)
            _hdData.renderingPathCustomFrameSettings.lodBias = config.LODBiasVR;
        else
            _hdData.renderingPathCustomFrameSettings.lodBias = config.LODBias;

        _hdData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.SSAO, config.EnableSSAO);
    }

}
