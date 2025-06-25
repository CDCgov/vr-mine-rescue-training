using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ComponentInspector_AudioSource : ComponentInspector<ComponentInfo_AudioSource>
{
    //Inspector inspector;
    public TMP_Text headerText;
    [SerializeField] SliderField volumeSliderField;
    //[SerializeField] Button PlayButton;
    //[SerializeField] ComponentInfo_AudioSource TargetComponentInfo; 
    [SerializeField] AudioSource targetAudioSourceComponent;

    // Start is called before the first frame update
    public int index;

    private void Awake()
    {
        //volumeSliderField.startValue = targetAudioSourceInfo.volume;
    }
    public override void Start()
    {
        base.Start();
        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_AudioSources[index]; 
        targetAudioSourceComponent = TargetComponentInfo.m_component;
        InitializeValues();
        volumeSliderField.onSubmitValue.AddListener(SetAudioSourceVolume);
        //PlayButton.onClick.AddListener(OnPlayButton);
        headerText.text += $": {TargetComponentInfo.AudioSourceDetail}";
        //rangeMinSliderField.onSubmitValue.AddListener(SetAudioSourceMinDistance);
        //rangeMaxSliderField.onSubmitValue.AddListener(SetAudioSourceMaxDistance);

    }
    private void OnDestroy()
    {
        volumeSliderField.onSubmitValue.RemoveListener(SetAudioSourceVolume);
        //PlayButton.onClick.RemoveListener(OnPlayButton);
        //rangeMinSliderField.onSubmitValue.RemoveListener(SetAudioSourceMinDistance);
        //rangeMaxSliderField.onSubmitValue.RemoveListener(SetAudioSourceMaxDistance);
    }
    public void InitializeValues()
    {
        if (targetAudioSourceComponent != null) targetAudioSourceComponent.volume = TargetComponentInfo.volume; 
        volumeSliderField.startValue = TargetComponentInfo.volume;
        volumeSliderField.ResetValues();
    }
    public void SetAudioSourceVolume(float value, bool enabled)
    {
        TargetComponentInfo.volume = value;
        if (targetAudioSourceComponent != null) 
            targetAudioSourceComponent.volume = value / 100.0f;
    }
   
    

}
