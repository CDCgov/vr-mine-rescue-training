using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ComponentInspector_AmbientAudio : ComponentInspector<ComponentInfo_AmbientAudio>
{
    public TMP_Text headerText;
    [SerializeField] TMP_Dropdown AmbientAudioClips;
    [SerializeField] SliderField VolumeSliderField;
    [SerializeField] SliderField PitchSliderField;
    [SerializeField] Button PlayButton;

    public Sprite PlaySprite;
    public Sprite StopSprite;

    private bool _isPlaying = false;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        InitializeValues();
        AmbientAudioClips.onValueChanged.AddListener(UpdateAudioClip);
        PitchSliderField.onSubmitValue.AddListener(UpdatePitchSlider);
        VolumeSliderField.onSubmitValue.AddListener(UpdateVolumeSlider);
        PlayButton.onClick.AddListener(OnPlayButton);
        PlayButton.image.sprite = PlaySprite;
        TargetComponentInfo.PlayOnStart = false;
        TargetComponentInfo.StopSource();
    }

    private void OnDestroy()
    {
        AmbientAudioClips.onValueChanged.RemoveListener(UpdateAudioClip);
        PitchSliderField.onSubmitValue.RemoveListener(UpdatePitchSlider);
        VolumeSliderField.onSubmitValue.RemoveListener(UpdateVolumeSlider);
        PlayButton.onClick.RemoveListener(OnPlayButton);
        TargetComponentInfo.StopSource();
    }

    void InitializeValues()
    {
        for (int i = 0; i < TargetComponentInfo.AudioCollection.AmbientAudios.Length; i++)
        {
            string option = TargetComponentInfo.AudioCollection.AmbientAudios[i].ClipName;
            AmbientAudioClips.options.Add(new TMP_Dropdown.OptionData() { text = option });
        }

        AmbientAudioClips.SetValueWithoutNotify(TargetComponentInfo.ClipIndex);
        PitchSliderField.startValue = TargetComponentInfo.Pitch;
        PitchSliderField.SetSliderValues(TargetComponentInfo.AudioCollection.AmbientAudios[TargetComponentInfo.ClipIndex].PitchRange.Min, TargetComponentInfo.AudioCollection.AmbientAudios[TargetComponentInfo.ClipIndex].PitchRange.Max, 1);
        VolumeSliderField.startValue = TargetComponentInfo.volume;
    }

    void UpdateAudioClip(int i)
    {
        PlayButton.image.sprite = PlaySprite;
        PlayButton.image.color = Color.green;
        TargetComponentInfo.StopSource();
        _isPlaying = false;
        TargetComponentInfo.ClipIndex = i;        
        PitchSliderField.SetSliderValues(TargetComponentInfo.AudioCollection.AmbientAudios[TargetComponentInfo.ClipIndex].PitchRange.Min, TargetComponentInfo.AudioCollection.AmbientAudios[TargetComponentInfo.ClipIndex].PitchRange.Max, 1);
        TargetComponentInfo.InitAudioClip();
    }

    void UpdatePitchSlider(float pitch, bool arg)
    {
        TargetComponentInfo.Pitch = pitch;
        TargetComponentInfo.SetPitch(pitch);
    }

    void UpdateVolumeSlider(float volume, bool arg)
    {
        TargetComponentInfo.volume = volume;
        TargetComponentInfo.SetVolume(volume);
    }

    void OnPlayButton()
    {
        if (_isPlaying)
        {
            PlayButton.image.sprite = PlaySprite;
            PlayButton.image.color = Color.green;
            TargetComponentInfo.StopSource();
            _isPlaying = false;
        }
        else
        {
            PlayButton.image.sprite = StopSprite;
            PlayButton.image.color = Color.red;
            TargetComponentInfo.PlaySource();
            _isPlaying = true;
        }
    }
}
