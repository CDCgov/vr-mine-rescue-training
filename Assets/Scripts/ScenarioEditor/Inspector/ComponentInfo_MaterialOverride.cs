using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_MaterialOverride : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    private struct RendererMatInfo
    {
        public Renderer Renderer;
        public List<Material> SharedMaterials;
    }

    public MaterialManager MaterialManager;

    [InspectableMaterialIDProperty("Material ID", "Name of the material to use in place of the default material for this object")]
    public string MaterialOverrideID
    {
        get
        {
            return _materialOverrideID;
        }
        set
        {
            _materialOverrideID = value;

            OverrideMaterial(_materialOverrideID);
        }
    }

    public string ComponentInspectorTitle => "Material Override";

    private string _materialOverrideID;
    private List<RendererMatInfo> _originalMaterials;

    private List<Material> _tempMaterials = new List<Material>();

    public void Start()
    {
        if (MaterialManager == null)
            MaterialManager = MaterialManager.GetDefault(gameObject);

        if (ScenarioSaveLoad.IsScenarioEditor)
            SaveOriginalMaterials();

        if (_materialOverrideID != null)
            OverrideMaterial(_materialOverrideID);

    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        _materialOverrideID = component.GetParamValueString("MatOverrideID", null);
    }

    public string[] SaveInfo()
    {
        if (_materialOverrideID == null)
            _materialOverrideID = "";

        return new string[]
        {
            "MatOverrideID|" + _materialOverrideID,
        };
    }

    public string SaveName()
    {
        return "MaterialOverride";
    }

    private void SaveOriginalMaterials()
    {
        _originalMaterials = new List<RendererMatInfo>();

        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (var rend in renderers)
        {

            List<Material> sharedMats = new List<Material>();
            rend.GetSharedMaterials(sharedMats);

            //var origMat = new Tuple<Renderer, Material>(rend, rend.sharedMaterial);
            var origMat = new RendererMatInfo
            {
                Renderer = rend,
                SharedMaterials = sharedMats,
            };
            _originalMaterials.Add(origMat);
            
        }

        Debug.Log($"MaterialOverride: Saved {_originalMaterials.Count} original materials");
    }

    private void RestoreOriginalMaterials()
    {
        if (_originalMaterials == null)
            return;

        foreach (var origMat in _originalMaterials)
        {
            if (origMat.Renderer == null || origMat.Renderer.gameObject == null ||
                origMat.SharedMaterials == null || origMat.SharedMaterials.Count <= 0)
                continue;

            //origMat.Item1.sharedMaterial = origMat.Item2;
            origMat.Renderer.SetSharedMaterials(origMat.SharedMaterials);
        }
    }

    private void OverrideMaterial(string matOverrideID)
    {
        if (MaterialManager == null)
            return;

        if (!MaterialManager.TryFindMaterial(matOverrideID, out var overrideMat))
        {
            RestoreOriginalMaterials();
            return;
        }

        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        foreach (var rend in renderers)
        {
            _tempMaterials.Clear();
            rend.GetSharedMaterials(_tempMaterials);

            if (_tempMaterials.Count <= 0)
                continue;

            for (int i = 0; i < _tempMaterials.Count; i++)
            {
                _tempMaterials[i] = overrideMat;
            }

            //rend.sharedMaterial = overrideMat;
            rend.SetSharedMaterials(_tempMaterials);
            _tempMaterials.Clear();
        }
    }
}
