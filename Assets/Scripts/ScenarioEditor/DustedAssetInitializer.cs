using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DustedAssetInitializer : MonoBehaviour
{
    Material _mat;
    Renderer _ren;
    private MaterialPropertyBlock _mpb;
    // Start is called before the first frame update
    private GlobalMineParameters _settings;
    void Start()
    {
        _mat = GetComponent<MeshRenderer>().material;
        float rdValue = 0;
        
        _settings = ScenarioSaveLoad.Settings;
        ScenarioSaveLoad.Instance.MineSettingsChanged += Instance_MineSettingsChanged;
        if (_settings != null)
        {
            rdValue = _settings.RockDustLevel;
            Debug.Log($"Found rock dust setting: {_settings.RockDustLevel}");
        }
        MaterialPropertyBlock mPropBlock = new MaterialPropertyBlock();
        _mpb = mPropBlock;
        mPropBlock.SetFloat("_Rockdust", rdValue);
        if (TryGetComponent<Renderer>(out _ren))
        {            
            _ren.SetPropertyBlock(mPropBlock);
        }
        //_mat.SetFloat("_Rockdust", rdValue);

    }

    private void Instance_MineSettingsChanged()
    {
        if(_ren == null)
        {
            return;
        }
        float rdValue = 0;
        
        if (_settings != null)
        {
            rdValue = _settings.RockDustLevel;
            Debug.Log($"Found rock dust setting: {_settings.RockDustLevel}");
        }

        //MaterialPropertyBlock mPropBlock = new MaterialPropertyBlock();
        _mpb.SetFloat("_Rockdust", rdValue);
        _ren.SetPropertyBlock(_mpb);
    }
    private void OnDestroy()
    {
        ScenarioSaveLoad.Instance.MineSettingsChanged += Instance_MineSettingsChanged;
    }
}
