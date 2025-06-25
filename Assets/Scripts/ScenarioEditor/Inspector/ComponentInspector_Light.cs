using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using System;
public class ComponentInspector_Light : ComponentInspector<ComponentInfo_Light>
{
    //Inspector inspector;
    //public int index;
    public TMP_Text headerText;
    [SerializeField] ToggleSwitch lightToggle;
    [SerializeField] SliderField intensitySliderField;
    [SerializeField] TMP_InputField intensityInputField;
    [SerializeField] TMP_InputField rangeInputField;
    [SerializeField] SliderField rangeSliderField;
    [SerializeField] SliderField colorSliderField;
    [SerializeField] UIColorPalette colorPallette;
    //[SerializeField] ComponentInfo_Light TargetComponentInfo;
    //[SerializeField] HDAdditionalLightData targetLightComponent;
    //[SerializeField] Light targetBaseLightComponent;
    //[SerializeField] List<Light> targetBaseLightComponents;
    //[SerializeField] List<HDAdditionalLightData> targetLightComponents;
    //[SerializeField] List<Material> targetEmissiveMaterials;
    //[SerializeField] List<MeshRenderer> targetEmissiveMeshRenderers;
    [SerializeField] Image colorImageDisplay;
    [SerializeField] Image colorImageDisplay2;
    [SerializeField] SliderField redSliderField;
    [SerializeField] SliderField greenSliderField;
    [SerializeField] SliderField blueSliderField;
    [SerializeField] GameObject AdvancedColorPanel;
    [SerializeField] ToggleSwitch CastShadowsToggle;

    public SliderField VolumetricIntensitySlider;

    //private float _cachedIntensityValue = 0;
    //private float _cachedRangeValue = 0;
    //private float _red = 255;
    //private float _green = 255;
    //private float _blue = 255;
    private Color _savedColor;
    private float _savedTemperature = 6700;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_Lights[index];
        //targetLightComponent = TargetComponentInfo.m_component;
        //if (targetLightComponent != null)
        //{
        //    targetBaseLightComponent = targetLightComponent.GetComponent<Light>();
        //}


        //targetLightComponents = new List<HDAdditionalLightData>();
        //targetBaseLightComponents = new List<Light>();
        //targetEmissiveMaterials = new List<Material>();
        //targetEmissiveMeshRenderers = new List<MeshRenderer>();
        //targetLightComponents = TargetComponentInfo.m_AdditionalLights;
        //targetBaseLightComponents = TargetComponentInfo.m_Lights;
        //targetEmissiveMeshRenderers = TargetComponentInfo.emissiveMeshRenderers;
        ////targetEmissiveMaterials = TargetComponentInfo.emissiveMaterials;
        //foreach (var mr in targetEmissiveMeshRenderers)
        //{
        //    targetEmissiveMaterials.Add(mr.material);
        //}
        InitializeValues();

        //float.TryParse(intensityInputField.text, out _cachedIntensityValue);
        //float.TryParse(rangeInputField.text, out _cachedRangeValue);

        intensitySliderField.onSubmitValue.AddListener(SetLightIntensity);
        //intensityInputField.onSubmit.AddListener(SetLightIntensity);
        intensityInputField.onEndEdit.AddListener(SetLightIntensity);
        //intensityInputField.onValueChanged.AddListener(SetLightIntensity);
        rangeSliderField.onSubmitValue.AddListener(SetLightRange);
        //rangeInputField.onSubmit.AddListener(SetLightRange);
        rangeInputField.onEndEdit.AddListener(SetLightRange);
        //rangeInputField.onValueChanged.AddListener(SetLightRange);
        colorSliderField.onSubmitValue.AddListener(SetTemperature);
        lightToggle.onToggleComplete.AddListener(SetIsOn);
        redSliderField.onSubmitValue.AddListener(SetRed);
        greenSliderField.onSubmitValue.AddListener(SetGreen);
        blueSliderField.onSubmitValue.AddListener(SetBlue);
        CastShadowsToggle.ToggleControl.onValueChanged.AddListener(OnCastShadowsChanged);

        VolumetricIntensitySlider.onSubmitValue.AddListener(SetVolumetricIntensity);

        //colorPallette.InputSelectionColor.AddListener(SetAudioSourceMaxDistance);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        intensitySliderField.onSubmitValue.RemoveListener(SetLightIntensity);
        //intensityInputField.onSubmit.RemoveListener(SetLightIntensity);
        //intensityInputField.onValueChanged.RemoveListener(SetLightIntensity);
        intensityInputField.onEndEdit.RemoveListener(SetLightIntensity);
        rangeSliderField.onSubmitValue.RemoveListener(SetLightRange);
        //rangeInputField.onSubmit.RemoveListener(SetLightRange);
        rangeInputField.onEndEdit.RemoveListener(SetLightRange);
        //rangeInputField.onValueChanged.RemoveListener(SetLightRange);
        colorSliderField.onSubmitValue.RemoveListener(SetTemperature);
        lightToggle.onToggleComplete.RemoveListener(SetIsOn);
        redSliderField.onSubmitValue.RemoveListener(SetRed);
        greenSliderField.onSubmitValue.RemoveListener(SetGreen);
        blueSliderField.onSubmitValue.RemoveListener(SetBlue);
        CastShadowsToggle.ToggleControl.onValueChanged.RemoveListener(OnCastShadowsChanged);
        //colorPallette.InputSelectionColor.RemoveListener(SetAudioSourceMaxDistance);
    }
    public void InitializeValues()
    {
        InitializeValues(TargetComponentInfo);
        //if (TargetComponentInfo != null && TargetComponentInfo.m_Lights != null && TargetComponentInfo.m_Lights.Count > 0)
        //{
        //    InitializeValues(TargetComponentInfo);
        //}

        //if (targetLightComponent != null) 
        //{
        //    //targetLightInfo.intensity = targetLightComponent.intensity;
        //    //if (targetLightComponent != null) targetLightComponent.intensity = targetLightInfo.intensity;
        //    //intensitySliderField.startValue = targetLightInfo.intensity;
        //    //intensitySliderField.ResetValues();
        //    AdvancedColorPanel.SetActive(true);

        //    intensityInputField.SetTextWithoutNotify(TargetComponentInfo.intensity.ToString());

        //    //targetLightInfo.range = targetLightComponent.range;
        //    //if (targetLightComponent != null) targetLightComponent.range = targetLightInfo.range;
        //    //rangeSliderField.startValue = targetLightInfo.range;
        //    //rangeSliderField.ResetValues();
        //    rangeInputField.SetTextWithoutNotify(TargetComponentInfo.range.ToString());

        //    colorSliderField.startValue = targetBaseLightComponent.colorTemperature;
        //    colorSliderField.ResetValues();
        //    //Debug.Log($"Light color? {targetLightComponent.color.ToString()}");
        //    redSliderField.startValue = targetLightComponent.color.r * 255;
        //    redSliderField.ResetValues();

        //    greenSliderField.startValue = targetLightComponent.color.g * 255;
        //    greenSliderField.ResetValues();

        //    blueSliderField.startValue = targetLightComponent.color.b * 255;
        //    blueSliderField.ResetValues();

        //    colorImageDisplay.color = targetLightComponent.color;
        //    colorImageDisplay2.color = targetLightComponent.color;
        //    _savedColor = targetLightComponent.color;
        //    _savedTemperature = targetBaseLightComponent.colorTemperature;

        //    AdvancedColorPanel.SetActive(false);

        //    headerText.text = TargetComponentInfo.componentName;
        //    foreach (var mr in targetEmissiveMeshRenderers)
        //    {
        //        mr.material.color = _savedColor;
        //        mr.material.SetColor("_EmissiveColor",_savedColor);
        //    }
        //}

        //if(targetLightComponents.Count > 0)
        //{
        //    AdvancedColorPanel.SetActive(true);

        //    intensityInputField.SetTextWithoutNotify(targetLightComponents[0].intensity.ToString());

        //    //targetLightInfo.range = targetLightComponent.range;
        //    //if (targetLightComponent != null) targetLightComponent.range = targetLightInfo.range;
        //    //rangeSliderField.startValue = targetLightInfo.range;
        //    //rangeSliderField.ResetValues();
        //    rangeInputField.SetTextWithoutNotify(targetLightComponents[0].range.ToString());

        //    colorSliderField.startValue = targetBaseLightComponents[0].colorTemperature;
        //    colorSliderField.ResetValues();
        //    //Debug.Log($"Light color? {targetLightComponents[0].color.ToString()}");
        //    redSliderField.startValue = targetLightComponents[0].color.r * 255;
        //    redSliderField.ResetValues();

        //    greenSliderField.startValue = targetLightComponents[0].color.g * 255;
        //    greenSliderField.ResetValues();

        //    blueSliderField.startValue = targetLightComponents[0].color.b * 255;
        //    blueSliderField.ResetValues();

        //    colorImageDisplay.color = targetLightComponents[0].color;
        //    colorImageDisplay2.color = targetLightComponents[0].color;
        //    _savedColor = targetLightComponents[0].color;
        //    _savedTemperature = targetBaseLightComponents[0].colorTemperature;

        //    AdvancedColorPanel.SetActive(false);

        //    headerText.text = TargetComponentInfo.componentName;
        //    foreach (var mr in targetEmissiveMeshRenderers)
        //    {
        //        mr.material.color = _savedColor;
        //        mr.material.SetColor("_EmissiveColor", _savedColor);
        //    }
        //    //Debug.Log($"Color: {_savedColor.ToString()}");
        //}
    }


    private IEnumerable<MeshRenderer> GetTargetEmissiveRenderers()
    {
        if (TargetComponentInfo == null || TargetComponentInfo.emissiveMeshRenderers == null || TargetComponentInfo.emissiveMeshRenderers.Count <= 0)
            yield break;

        foreach (var renderer in TargetComponentInfo.emissiveMeshRenderers)
            yield return renderer;
    }

    private void InitializeValues(ComponentInfo_Light lightComponent)
    {
        if (lightComponent == null)
            return;

        //var hdrpLight = light.m_Lights[0].GetComponent<HDAdditionalLightData>();

        AdvancedColorPanel.SetActive(true);

        //if (hdrpLight != null)
        //{
        //    hdrpLight.lightUnit = LightUnit.Lumen;
        //    intensityInputField.SetTextWithoutNotify(hdrpLight.intensity.ToString());
        //}
        //else
        //    intensityInputField.SetTextWithoutNotify(light.intensity.ToString());
        lightToggle.ToggleInstantly(lightComponent.isOn);//BUG FIX: this would appear as on if the light was saved as off in Scenario Editor
        intensityInputField.SetTextWithoutNotify(lightComponent.intensity.ToString());

        //targetLightInfo.range = targetLightComponent.range;
        //if (targetLightComponent != null) targetLightComponent.range = targetLightInfo.range;
        //rangeSliderField.startValue = targetLightInfo.range;
        //rangeSliderField.ResetValues();
        rangeInputField.SetTextWithoutNotify(lightComponent.range.ToString());

        colorSliderField.startValue = lightComponent.Temperature;
        colorSliderField.ResetValues();
        //Debug.Log($"Light color? {targetLightComponent.color.ToString()}");
        redSliderField.startValue = lightComponent.color.r * 255;
        redSliderField.ResetValues();

        greenSliderField.startValue = lightComponent.color.g * 255;
        greenSliderField.ResetValues();

        blueSliderField.startValue = lightComponent.color.b * 255;
        blueSliderField.ResetValues();

        colorImageDisplay.color = lightComponent.color;
        colorImageDisplay2.color = lightComponent.color;
        _savedColor = lightComponent.color;
        _savedTemperature = lightComponent.Temperature;

        AdvancedColorPanel.SetActive(false);

        headerText.text = lightComponent.componentName;
        foreach (var mr in lightComponent.emissiveMeshRenderers)
        {
            mr.material.color = _savedColor;
            mr.material.SetColor("_EmissiveColor", _savedColor);
        }

        CastShadowsToggle.ToggleInstantly(lightComponent.CastShadows);

        VolumetricIntensitySlider.startValue = lightComponent.VolumetricIntensity;
        VolumetricIntensitySlider.SetCurrentValue(lightComponent.VolumetricIntensity, false);
    }

    public void SetTemperature(float colorTemp, bool enabled)
    {
        if (TargetComponentInfo == null)
            return;

        TargetComponentInfo.Temperature = colorTemp;
        _savedTemperature = colorTemp;
        //if (targetLightComponent != null) targetLightComponent.color = Color.HSVToRGB(value, 1, 1);
        //colorImageDisplay.color = Color.HSVToRGB(value, 1, 1);

        //targetBaseLightComponent.colorTemperature = value;

        ////colorImageDisplay.color = Color.HSVToRGB(value, 1, 1);
        //Light light = null;
        //if (targetBaseLightComponents != null)
        //{
        //    foreach (var lightComponent in targetBaseLightComponents)
        //    {
        //        lightComponent.colorTemperature = value;
        //        light = lightComponent;
        //    }
        //    //if (targetEmissiveMaterials != null)
        //    //{
        //    //    Color lightColor;
        //    //    lightColor = Mathf.CorrelatedColorTemperatureToRGB(value);
        //    //    //Color col = Mathf.CorrelatedColorTemperatureToRGB(value);
        //    //    foreach (var mat in targetEmissiveMaterials)
        //    //    {
        //    //        mat.color = lightColor;
        //    //        mat.SetColor("_EmissiveColor", lightColor);
        //    //    }
        //    //}

        //}

        foreach (var light in TargetComponentInfo.GetLightsHDRP())
        {
            //light.useColorTemperature = true;
            //light.colorTemperature = value;
            light.EnableColorTemperature(true);
            light.SetColor(light.color, colorTemp);
        }


        Color lightColor;
        lightColor = TargetComponentInfo.color * Mathf.CorrelatedColorTemperatureToRGB(colorTemp);
        //Color col = Mathf.CorrelatedColorTemperatureToRGB(value);
        foreach (var mr in GetTargetEmissiveRenderers())
        {
            if (mr.materials.Length > 1)
            {
                for (int i = 0; i < mr.materials.Length; i++)
                {
                    mr.materials[i].color = lightColor;
                    mr.materials[i].SetColor("_EmissiveColor", lightColor);
                }
            }
            else
            {
                mr.material.color = lightColor;
                mr.material.SetColor("_EmissiveColor", lightColor);
            }
            
        }
        //Debug.Log($"Setting color temperature color: {lightColor.ToString()}");

    }


    private void OnCastShadowsChanged(bool castShadows)
    {
        if (TargetComponentInfo == null)
            return;

        TargetComponentInfo.CastShadows = castShadows;

        foreach (var light in TargetComponentInfo.GetLightsHDRP())
        {
            light.EnableShadows(castShadows);
        }
    }

    public void SetIsOn(bool value)
    {
        if (TargetComponentInfo == null)
            return;

        TargetComponentInfo.isOn = value;

        foreach (var light in TargetComponentInfo.GetLights())
            light.enabled = value;

        //Change from disabling the renderer to changing the emission color to black/not black
        Color col = value ? TargetComponentInfo.color : Color.black;
        col = col * Mathf.CorrelatedColorTemperatureToRGB(TargetComponentInfo.Temperature);
        foreach (var renderer in GetTargetEmissiveRenderers())
        {
            //renderer.enabled = value;
            if (TargetComponentInfo.UsesSeperateModelWhenOff)
            {
                renderer.enabled = value;
            }
            else
            {
                renderer.material.SetColor("_EmissiveColor", col);
            }
        }


        //TargetComponentInfo.isOn = value;
        //if (targetBaseLightComponent != null) targetBaseLightComponent.enabled = value;

        //if (targetBaseLightComponents != null)
        //{
        //    foreach (var lightComponent in targetBaseLightComponents)
        //    {
        //        lightComponent.enabled = value;
        //    }
        //}

        //if (targetEmissiveMeshRenderers != null)
        //{
        //    foreach (var mr in targetEmissiveMeshRenderers)
        //    {
        //        mr.enabled = value;
        //    }
        //}
    }

    public void SetRed(float value, bool enabled)
    {
        Color color = TargetComponentInfo.color;
        color.r = value / 255.0f;

        SetColor(color, true);
        //Color col = new Color(value/255, _green / 255, _blue / 255);
        //_red = value;
        //TargetComponentInfo.color = col;
        ////Debug.Log($"Color: {col.ToString()}");
        //colorImageDisplay.color = col;
        //colorImageDisplay2.color = col;
        //_savedColor = col;
        //if (targetLightComponent != null) targetLightComponent.color = col;

        //if (targetLightComponents != null)
        //{
        //    foreach (var lightComponent in targetLightComponents)
        //    {
        //        lightComponent.color = col;
        //    }
        //}

        //if(targetEmissiveMaterials != null)
        //{
        //    col = col * Mathf.CorrelatedColorTemperatureToRGB(_savedTemperature);
        //    foreach (var mat in targetEmissiveMaterials)
        //    {
        //        mat.color = col;
        //        mat.SetColor("_EmissiveColor", col);
        //    }
        //}
    }
    public void SetGreen(float value, bool enabled)
    {
        Color color = TargetComponentInfo.color;
        color.g = value / 255.0f;

        SetColor(color, true);
        //Color col = new Color(_red / 255, value / 255, _blue / 255);
        //_green = value;
        //TargetComponentInfo.color = col;
        ////Debug.Log($"Color: {col.ToString()}");
        //colorImageDisplay.color = col;
        //colorImageDisplay2.color = col;
        //_savedColor = col;
        //if (targetLightComponent != null) targetLightComponent.color = col;

        //if (targetLightComponents != null)
        //{
        //    foreach (var lightComponent in targetLightComponents)
        //    {
        //        lightComponent.color = col;
        //    }
        //}
        //if (targetEmissiveMaterials != null)
        //{
        //    col = col * Mathf.CorrelatedColorTemperatureToRGB(_savedTemperature);
        //    foreach (var mat in targetEmissiveMaterials)
        //    {
        //        mat.color = col;
        //        mat.SetColor("_EmissiveColor", col);
        //    }
        //}
    }
    public void SetBlue(float value, bool enabled)
    {
        Color color = TargetComponentInfo.color;
        color.b = value / 255.0f;

        SetColor(color, true);
        //_blue = value;
        //Color col = new Color(_red / 255, _green / 255, value / 255);
        //TargetComponentInfo.color = col;
        ////Debug.Log($"Color: {col.ToString()}");
        //colorImageDisplay.color = col;
        //colorImageDisplay2.color = col;
        //_savedColor = col;
        //if (targetLightComponent != null) targetLightComponent.color = col;

        //if (targetLightComponents != null)
        //{
        //    foreach (var lightComponent in targetLightComponents)
        //    {
        //        lightComponent.color = col;
        //    }
        //}
        //if (targetEmissiveMaterials != null)
        //{
        //    col = col * Mathf.CorrelatedColorTemperatureToRGB(_savedTemperature);
        //    foreach (var mat in targetEmissiveMaterials)
        //    {
        //        mat.color = col;
        //        mat.SetColor("_EmissiveColor", col);
        //    }
        //}
    }

    public void SetColor(Color color, bool enabled)
    {
        if (TargetComponentInfo == null)
            return;

        TargetComponentInfo.color = color;
        //Debug.Log($"Color: {col.ToString()}");
        colorImageDisplay.color = color;
        colorImageDisplay2.color = color;
        _savedColor = color;

        //redSliderField.ForceValue((int)(color.r * 255));
        //greenSliderField.ForceValue((int)(color.g * 255));
        //blueSliderField.ForceValue((int)(color.b * 255));
        redSliderField.SetSliderValues(0, 255, color.r * 255.0f);
        greenSliderField.SetSliderValues(0, 255, color.g * 255.0f);
        blueSliderField.SetSliderValues(0, 255, color.b * 255.0f);


        foreach (var light in TargetComponentInfo.GetLights())
        {
            light.color = color;
            light.enabled = true;
        }

        //if (targetLightComponent != null) targetLightComponent.color = col;

        //if (targetLightComponents != null)
        //{
        //    foreach (var lightComponent in targetLightComponents)
        //    {
        //        lightComponent.color = col;
        //    }
        //}

        if (TargetComponentInfo.emissiveMaterials != null)
        {
            color = color * Mathf.CorrelatedColorTemperatureToRGB(_savedTemperature);
            foreach (var mat in TargetComponentInfo.emissiveMaterials)
            {
                mat.color = color;
                mat.SetColor("_EmissiveColor", color);
            }
            
        }
        color = color * Mathf.CorrelatedColorTemperatureToRGB(_savedTemperature);
        foreach (var mr in GetTargetEmissiveRenderers())
        {
            if(mr.materials.Length > 1)
            {
                for (int i = 0; i < mr.materials.Length; i++)
                {
                    mr.materials[i].color = color;
                    mr.materials[i].SetColor("_EmissiveColor", color);
                }
            }
            else
            {
                mr.material.color = color;
                mr.material.SetColor("_EmissiveColor", color);
            }
        }
    }

    //public void SetLightColorPreset(float r, float g, float b)
    //{
    //    redSliderField.ForceValue((int)(r * 255));
    //    greenSliderField.ForceValue((int)(g * 255));
    //    blueSliderField.ForceValue((int)(b * 255));
    //    colorImageDisplay.color = new Color(r, g, b);
    //    _savedColor = colorImageDisplay.color;
    //    colorImageDisplay2.color = _savedColor;
    //}


    private void SetVolumetricIntensity(float intensity, bool enabled)
    {
        TargetComponentInfo.VolumetricIntensity = intensity;

        bool volumetricEnabled = intensity > 0 ? true : false;

        foreach (var hdrpLight in TargetComponentInfo.GetLightsHDRP())
        {
            hdrpLight.affectsVolumetric = volumetricEnabled;
            hdrpLight.volumetricDimmer = intensity;
        }
    }


    public void SetLightIntensity(float intensity, bool enabled)
    {
        TargetComponentInfo.intensity = intensity;

        bool lightEnabled = intensity > 0 ? true : false;

        foreach (var light in TargetComponentInfo.GetLights())
        {
            //light.intensity = value;
            light.enabled = lightEnabled;
        }

        foreach (var hdrpLight in TargetComponentInfo.GetLightsHDRP())
        {
            hdrpLight.lightUnit = LightUnit.Lumen;
            hdrpLight.SetIntensity(intensity, LightUnit.Lumen);
        }
        

        foreach (var renderer in GetTargetEmissiveRenderers())
            renderer.enabled = lightEnabled;

        //if (targetLightComponent != null) targetLightComponent.intensity = value;

        //if (targetLightComponents != null)
        //{
        //    foreach (var lightComponent in targetLightComponents)
        //    {
        //        lightComponent.intensity = value;
        //    }
        //}

        //if (value <= 0)
        //{
        //    if (targetEmissiveMeshRenderers != null)
        //    {
        //        foreach (var mr in targetEmissiveMeshRenderers)
        //        {
        //            mr.enabled = false;
        //        }
        //    }
        //}
        //else
        //{
        //    if (TargetComponentInfo.isOn)
        //    {
        //        if (targetEmissiveMeshRenderers != null)
        //        {
        //            foreach (var mr in targetEmissiveMeshRenderers)
        //            {
        //                mr.enabled = true;
        //            }
        //        }
        //    }
        //}
    }

    public void SetLightIntensity(string value)
    {
        if (float.TryParse(value, out var fVal))
        {
            SetLightIntensity(fVal, true);
        }
        else if (TargetComponentInfo != null)
        {
            intensityInputField.SetTextWithoutNotify(TargetComponentInfo.intensity.ToString("F2"));
        }

        //float fVal;
        //if (float.TryParse(value, out fVal))
        //{
        //    TargetComponentInfo.intensity = fVal;
        //    _cachedIntensityValue = fVal;
        //    if (targetLightComponent != null) targetLightComponent.intensity = fVal;

        //    if (targetLightComponents != null)
        //    {
        //        foreach (var lightComponent in targetLightComponents)
        //        {
        //            lightComponent.intensity = fVal;
        //        }
        //    }
        //    if (fVal <= 0)
        //    {
        //        if (targetEmissiveMeshRenderers != null)
        //        {
        //            foreach (var mr in targetEmissiveMeshRenderers)
        //            {
        //                mr.enabled = false;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (TargetComponentInfo.isOn)
        //        {
        //            if (targetEmissiveMeshRenderers != null)
        //            {
        //                foreach (var mr in targetEmissiveMeshRenderers)
        //                {
        //                    mr.enabled = true;
        //                }
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    intensityInputField.SetTextWithoutNotify(_cachedIntensityValue.ToString());
        //}
    }
    public void SetLightRange(float value, bool enabled)
    {
        TargetComponentInfo.range = value;

        foreach (var light in TargetComponentInfo.GetLights())
            light.range = value;

        //if (targetLightComponent != null) targetLightComponent.range = value;

        //if (targetLightComponents != null)
        //{
        //    foreach (var lightComponent in targetLightComponents)
        //    {
        //        lightComponent.range = value;
        //    }
        //}
    }
    public void SetLightRange(string value)
    {
        if (float.TryParse(value, out var fVal))
        {
            SetLightRange(fVal, true);
        }
        else if (TargetComponentInfo != null)
        {
            rangeInputField.SetTextWithoutNotify(TargetComponentInfo.range.ToString("F2"));
        }

        //float fVal;
        //if (float.TryParse(value, out fVal))
        //{
        //    TargetComponentInfo.range = fVal;
        //    _cachedRangeValue = fVal;
        //    if (targetLightComponent != null) targetLightComponent.range = fVal;

        //    if (targetLightComponents != null)
        //    {
        //        foreach (var lightComponent in targetLightComponents)
        //        {
        //            lightComponent.range = fVal;
        //        }
        //    }
        //}
        //else
        //{
        //    rangeInputField.SetTextWithoutNotify(_cachedRangeValue.ToString());
        //}
    }


}


