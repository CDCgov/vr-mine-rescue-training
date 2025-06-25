using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineSegmentMaterials : MonoBehaviour
{
    public List<Renderer> RockDustRenderers;

    private MaterialPropertyBlock _rockDustMPB;

    // Start is called before the first frame update
    void Start()
    {
        if (ScenarioSaveLoad.Instance == null || ScenarioSaveLoad.Settings == null)
            return;        

        SetupRockdustMPB();
    }

    private void OnEnable()
    {
        if (ScenarioSaveLoad.Instance != null)
            ScenarioSaveLoad.Instance.MineSettingsChanged += OnMineSettingsChanged;
    }

    private void OnDisable()
    {
        if (ScenarioSaveLoad.Instance != null)
            ScenarioSaveLoad.Instance.MineSettingsChanged -= OnMineSettingsChanged;
    }

    private void OnMineSettingsChanged()
    {
        SetRockDust(ScenarioSaveLoad.Settings.RockDustLevel);
    }

    void SetupRockdustMPB()
    {
        if (_rockDustMPB == null)
        {
            _rockDustMPB = new MaterialPropertyBlock();
        }      

        SetRockDust(ScenarioSaveLoad.Settings.RockDustLevel);

    }

    void SetRockDust(float rockDustLevel)
    {
        if (_rockDustMPB == null || RockDustRenderers == null || RockDustRenderers.Count <= 0)
            return;

        _rockDustMPB.SetFloat("_Rockdust", rockDustLevel);

        foreach (var rend in RockDustRenderers)
        {
            if (rend == null)
                continue;

            rend.SetPropertyBlock(_rockDustMPB);
        }
    }
}
