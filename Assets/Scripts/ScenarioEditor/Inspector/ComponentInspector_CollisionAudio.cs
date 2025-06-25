using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using static NIOSH_EditorLayers.LayerManager;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using System.Linq;

public class ComponentInspector_CollisionAudio : ComponentInspector<ComponentInfo_CollisionAudio>
{
    public TMP_Text headerText;
    [SerializeField] TMP_Dropdown CollisionMatDropdown;
    [SerializeField] TMP_Dropdown CollisionSurfaceDropdown;
    [SerializeField] SliderField PitchSliderField;
    [SerializeField] Button PlayButton;
    [SerializeField] TextMeshProUGUI NowPlayingLabel;
    public AudioSource DemoSoundSource;

    AudioMaterialType _surfaceMaterial;
    private int _surfaceSequence = 1;
    private float _pitchOverride = 1;
    private LoadableAssetManager LoadableAssetManager;
    private AudioMaterialList _audioMaterialList;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        _audioMaterialList = Resources.Load<AudioMaterialList>("Managers/AudioMaterialList");
        InitializeValues();
        CollisionMatDropdown.onValueChanged.AddListener(SetCollisionMaterial);
        CollisionSurfaceDropdown.onValueChanged.AddListener(SetSurfaceMaterial);
        PlayButton.onClick.AddListener(PlaySampleAudio);
        PitchSliderField.onSubmitValue.AddListener(ChangePitch);
        _surfaceMaterial = (AudioMaterialType)0;
    }

    public void InitializeValues()
    {

        //for (int i = 0; i < TargetComponentInfo.AudioMaterialList.AudioMaterials.Count; i++)
        //{
        //    CollisionMatDropdown.options.Add(new TMP_Dropdown.OptionData() { text = TargetComponentInfo.AudioMaterialList.AudioMaterials[i].AudioMaterialName });
        //}
        //Addressables.LoadResourceLocationsAsync("AudioMaterial").Completed += ComponentInspector_CollisionAudio_Completed; ;
        
        foreach (AudioMaterialType type in Enum.GetValues(typeof(AudioMaterialType)))
        {
            CollisionSurfaceDropdown.options.Add(new TMP_Dropdown.OptionData() { text = type.ToString() });
        }
        //CollisionMatDropdown.value = CollisionMatDropdown.options.FindIndex(option => option.text == TargetComponentInfo.CollisionMaterial);
        //CollisionMatDropdown.SetValueWithoutNotify(0);
        CollisionSurfaceDropdown.value = 0;
        NowPlayingLabel.text = "";

        if (NIOSH_EditorLayers.LayerManager.GetCurrentLayer() != EditorLayer.Object)
        {
            CollisionSurfaceDropdown.gameObject.SetActive(false);
            PlayButton.gameObject.SetActive(false);
            NowPlayingLabel.gameObject.SetActive(false);
            PitchSliderField.gameObject.SetActive(false);
        }
        headerText.text = TargetComponentInfo.componentName;

        PopulateDropdown();
    }

    private void ComponentInspector_CollisionAudio_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> obj)
    {
        foreach (var item in obj.Result)
        {            
            string option = item.PrimaryKey.Replace("AudioMaterial/", "");
            CollisionMatDropdown.options.Add(new TMP_Dropdown.OptionData() { text = option });
        }
        ObjectInfo _objInfo = TargetComponentInfo.GetComponentInParent<ObjectInfo>();
        //LoadableAssetManager loadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        string selection = "Stone";//Default material for now
        if (string.IsNullOrEmpty(TargetComponentInfo.CollisionMaterial))
        {
            LoadableAsset la = LoadableAssetManager.FindAsset(_objInfo.AssetID);
            if (la != null)
            {
                if (la.AudioProperties != null)
                {
                    selection = LoadableAssetManager.FindAsset(_objInfo.AssetID).AudioProperties.AudioMaterial.AudioMaterialName;
                    TargetComponentInfo.CollisionMaterial = la.AudioProperties.AudioMaterial.AudioMaterialName;
                }
            }
        }
        else
        {
            selection = TargetComponentInfo.m_CollisionSoundEffect.SoundMaterial.AudioMaterialName;
        }
        Debug.Log($"Audio material name search: {selection}");
        int index = 0;
        for (int i = 0; i < CollisionMatDropdown.options.Count; i++)
        {
            Debug.Log($"Iterating thru list: {CollisionMatDropdown.options[i].text}");
            if (CollisionMatDropdown.options[i].text == selection)
            {
                
                //CollisionMatDropdown.SetValueWithoutNotify(i);
                index = i;
            }
        }
        
        CollisionMatDropdown.value = index;
        //CollisionMatDropdown.RefreshShownValue();
        StartCoroutine("CompleteUIRefresh");
    }

    private void PopulateDropdown()
    {
        foreach (var item in _audioMaterialList.AudioMaterials)
        {
            string option = item.AudioMaterialName;
            CollisionMatDropdown.options.Add(new TMP_Dropdown.OptionData() { text = option });
        }
        ObjectInfo _objInfo = TargetComponentInfo.GetComponentInParent<ObjectInfo>();
        //LoadableAssetManager loadableAssetManager = LoadableAssetManager.GetDefault(gameObject);
        string selection = "Stone";//Default material for now
        if (string.IsNullOrEmpty(TargetComponentInfo.CollisionMaterial))
        {
            LoadableAsset la = LoadableAssetManager.FindAsset(_objInfo.AssetID);
            if (la != null)
            {
                if (la.AudioProperties != null)
                {
                    selection = LoadableAssetManager.FindAsset(_objInfo.AssetID).AudioProperties.AudioMaterial.AudioMaterialName;
                    TargetComponentInfo.CollisionMaterial = selection;
                    TargetComponentInfo.m_CollisionSoundEffect.SoundMaterial = la.AudioProperties.AudioMaterial;
                }
            }
        }
        else
        {
            selection = TargetComponentInfo.m_CollisionSoundEffect.SoundMaterial.AudioMaterialName;
        }
        Debug.Log($"Audio material name search: {selection}");
        int index = 0;
        for (int i = 0; i < CollisionMatDropdown.options.Count; i++)
        {
            //Debug.Log($"Iterating thru list: {CollisionMatDropdown.options[i].text}");
            if (CollisionMatDropdown.options[i].text == selection)
            {

                //CollisionMatDropdown.SetValueWithoutNotify(i);
                index = i;
            }
        }

        CollisionMatDropdown.value = index;
        //CollisionMatDropdown.RefreshShownValue();
        CollisionMatDropdown.RefreshShownValue();
        //StartCoroutine("CompleteUIRefresh");
    }

    private void CompleteUIRefresh()
    {
        CollisionMatDropdown.RefreshShownValue();
    }

    private void SetSurfaceMaterial(int arg0)
    {
        if(arg0 == 0)
        {
            _surfaceMaterial = (AudioMaterialType)arg0;
        }
        else
        {
            _surfaceMaterial = (AudioMaterialType)(arg0 - 1);
        }
    }

    private void ChangePitch(float pitch, bool argument)
    {
        //DemoSoundSource.pitch = pitch;
        _pitchOverride = pitch;
        TargetComponentInfo.Pitch = pitch;
    }

    private void SetCollisionMaterial(int i)
    {
        //TargetComponentInfo.CollisionMaterial = CollisionMatDropdown.captionText.text;
        TargetComponentInfo.SetCollisionMaterial(CollisionMatDropdown.captionText.text);
    }

    private void PlaySampleAudio()
    {
        AudioClip clip;
        float pitchLimited = 1;
        if (CollisionSurfaceDropdown.value == 0)
        {
            //AudioSoundSet soundSet = TargetComponentInfo.AudioMaterialList.AudioMaterials[CollisionMatDropdown.value].GetSoundSet(_surfaceMaterial);
            AudioSoundSet soundSet = TargetComponentInfo.m_CollisionSoundEffect.SoundMaterial.GetSoundSet(_surfaceMaterial);
            CollisionAudioClip collisionAudioClip = soundSet.Sounds[UnityEngine.Random.Range(0, soundSet.Sounds.Count)];
            clip = collisionAudioClip.CollisionClip;
            pitchLimited = collisionAudioClip.PitchRange.Clamp(_pitchOverride);
            _surfaceMaterial++;
            if ((int)_surfaceMaterial >= Enum.GetValues(typeof(AudioMaterialType)).Length)
            {
                _surfaceMaterial = 0;
            }
        }
        else 
        {
            //AudioSoundSet soundSet = TargetComponentInfo.AudioMaterialList.AudioMaterials[CollisionMatDropdown.value].GetSoundSet((AudioMaterialType)(CollisionSurfaceDropdown.value-1));
            AudioSoundSet soundSet = TargetComponentInfo.m_CollisionSoundEffect.SoundMaterial.GetSoundSet((AudioMaterialType)(CollisionSurfaceDropdown.value - 1));
            CollisionAudioClip collisionAudioClip = soundSet.Sounds[UnityEngine.Random.Range(0, soundSet.Sounds.Count)];
            clip = collisionAudioClip.CollisionClip;
            pitchLimited = collisionAudioClip.PitchRange.Clamp(_pitchOverride);

        }
        NowPlayingLabel.text = $"Clip: {clip.name}";
        DemoSoundSource.clip = clip;
        DemoSoundSource.pitch = pitchLimited;
        DemoSoundSource.Play();
    }

    private void OnDestroy()
    {
        CollisionMatDropdown.onValueChanged.RemoveListener(SetCollisionMaterial);
        CollisionSurfaceDropdown.onValueChanged.RemoveListener(SetSurfaceMaterial);
        PlayButton.onClick.RemoveListener(PlaySampleAudio);
    }
}
