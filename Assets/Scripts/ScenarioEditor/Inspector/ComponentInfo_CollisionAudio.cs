using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AddressableAssets;

public class ComponentInfo_CollisionAudio : ModularComponentInfo, ISaveableComponent
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Collision SFx";
    public string CollisionMaterial = "";
    //protected ObjectInfo objectInfo;
    public Inspector.ExposureLevel volumeExposureLevel;
    public CollisionSoundEffect m_CollisionSoundEffect;
    public float Pitch = 1;

    private LoadableAssetManager LoadableAssetManager;
    private ObjectInfo _objInfo;
    private AudioMaterialList _audioMaterialList;

    void Awake()
    {
        if(m_CollisionSoundEffect == null)
            m_CollisionSoundEffect = GetComponentInParent<CollisionSoundEffect>();

        if (LoadableAssetManager == null)
            LoadableAssetManager = LoadableAssetManager.GetDefault(gameObject);

        if (_objInfo == null)
            _objInfo = GetComponentInParent<ObjectInfo>();

        _audioMaterialList = Resources.Load<AudioMaterialList>("Managers/AudioMaterialList");

        
    }

    void Start()
    {
        if(CollisionMaterial != null)
        {
            return;
        }
        if (_objInfo != null)
        {
            LoadableAsset la = LoadableAssetManager.FindAsset(_objInfo.AssetID);
            if (la != null && la.AudioProperties != null && la.AudioProperties.AudioMaterial != null)
            {
                CollisionMaterial = la.AudioProperties.AudioMaterial.AudioMaterialName;
                AudioMaterial am;
                if (_audioMaterialList.TryGetMaterialByName(CollisionMaterial, out am))
                {
                    m_CollisionSoundEffect.SoundMaterial = am;
                }
            }
            else
            {
                CollisionMaterial = m_CollisionSoundEffect.SoundMaterial.AudioMaterialName;
            }
        }
        else
        {
            CollisionMaterial = m_CollisionSoundEffect.SoundMaterial.AudioMaterialName;
        }
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null) return;
        componentName = component.GetComponentName();
        //symbol = symbolList.GetCollectionByString(component.GetParamValueAsStringByName("Label"));
        string text = component.GetParamValueAsStringByName("CollisionMaterial");
        float.TryParse(component.GetParamValueAsStringByName("Pitch"), out Pitch);
        AudioMaterial am;
        //AudioMaterialList.TryGetMaterialByName(text, out am);
        try
        {
            if (text != null && text.Length > 0 && _audioMaterialList.TryGetMaterialByName(text, out am))
            {
                //Addressables.LoadAssetAsync<AudioMaterial>($"AudioMaterial/{text}").Completed += OnLoadDone;
                m_CollisionSoundEffect.SoundMaterial = am;
            }
            else
            {
                m_CollisionSoundEffect.SoundMaterial = _audioMaterialList.FallbackMaterial;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading CollisionMaterial for {componentName} on {gameObject.name}: {ex.Message} {ex.StackTrace}");
            m_CollisionSoundEffect.SoundMaterial = _audioMaterialList.FallbackMaterial;
        }

        //m_CollisionSoundEffect.SoundMaterial = am;
        //m_symbolRenderer.Symbol.SymbolText = text;
        CollisionMaterial = text;
        m_CollisionSoundEffect.DefaultPitch = Pitch;
        //_label.text = text;
    }

    public string[] SaveInfo()
    {
        return new string[] { "CollisionMaterial|" + CollisionMaterial, "Pitch|" + Pitch };
    }

    public string SaveName()
    {
        return componentName;
    }

    private void OnLoadDone(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<AudioMaterial> obj)
    {
        if (obj.Result != null)
        {
            m_CollisionSoundEffect.SoundMaterial = obj.Result;
        }
        else
        {
            m_CollisionSoundEffect.SoundMaterial = LoadableAssetManager.FindAsset(_objInfo.AssetID).AudioProperties.AudioMaterial;
        }
    } 

    public void SetCollisionMaterial(string material)
    {
        CollisionMaterial = material;
        //Addressables.LoadAssetAsync<AudioMaterial>($"AudioMaterial/{material}").Completed += OnLoadDone;
        AudioMaterial am;
        if(_audioMaterialList.TryGetMaterialByName(material, out am))
        {
            m_CollisionSoundEffect.SoundMaterial = am;
        }
    }
}
