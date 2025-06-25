using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIOSH_MineCreation;
using NIOSH_EditorLayers;

[RequireComponent(typeof(StaticVentilationZoneStratified))]
public class ComponentInfo_StaticGasZone : ModularComponentInfo, ISaveableComponent, IScenarioEditorResizable, IScenarioEditorFocusTarget
{
    [Tooltip("The name of the component as it appears in the inspector, also used to reference this information so the name should be unique for the prefab. This should be assigned in both the editor and scenario prefab")]
    public string componentName = "Static Gas Zone";
    //public ObjectInfo ObjectInfo;

    public GameObject FalloffObject;
    Renderer _falloffRenderer;

    public GameObject ZoneRenderPrefab;

    public IndexedColorList GasZoneColors;
    public GasZonePresetList GasZonePresets;

    private NetworkedObject _netObj = null;

    public Vector3 Size
    {
        get { return _size; }
        set
        {
            if (_size != value)
            {
                _size = value;
                UpdateZoneBounds();
                SizeChanged?.Invoke();
            }
        }
    }

    public Vector3 LocalCenter
    {
        get
        {
            return new Vector3(0, _size.y / 2, 0);
        }
    }

    public event System.Action SizeChanged;

    public List<SubGasZoneData> SubGasZones = new List<SubGasZoneData>();
    public float Falloff = 0.5f;
    //private float _zoneHeight = 1.6f;
    private Vector3 _size = Vector3.one;

    private StaticVentilationZoneStratified _staticVentZone;


    public delegate void OnLoad(int i);
    public OnLoad onLoad;
    BoxCollider _boxCollider;

    bool initialized;


    private void Awake()
    {

        //if (ObjectInfo != null) 
        //{ 
        //    ObjectInfo.componentInfo_StaticGasZone = this; 
        //}

        if (SubGasZones.Count <= 0)
        {
            CreateDefaultSubZone();
        }

        var settings = ScenarioSaveLoad.Settings.MineSettings;
        if (settings != null)
        {
            _size.y = settings.seamHeight;
        }

        //Initialize();

    }

    void Start()
    {
        _staticVentZone = GetComponent<StaticVentilationZoneStratified>();

        if (LayerManager.Instance != null)
            LayerManager.Instance.layerChanged += OnLayerChanged;

        TryGetComponent<BoxCollider>(out _boxCollider);
        TryGetComponent<NetworkedObject>(out _netObj);

        if (FalloffObject != null)
        {
            _falloffRenderer = FalloffObject.GetComponent<Renderer>();
            FalloffObject.transform.SetParent(null, false);
        }

        if (ZoneRenderPrefab != null && 
            (_netObj == null || _netObj.HasAuthority))
        {
            CreateSubZoneRenderers();
            UpdateZoneBounds();

            if (LayerManager.Instance != null)
            {
                if (LayerManager.GetCurrentLayer() != LayerManager.EditorLayer.Ventilation)
                    SetVisibility(false);
            }
        }
    }

    void OnDestroy()
    {
        if (FalloffObject != null)
            Destroy(FalloffObject);

        if (LayerManager.Instance != null)
            LayerManager.Instance.layerChanged -= OnLayerChanged;
    }


    private void UpdateZoneBounds()
    {
        var center = Vector3.zero;
        //var extents = transform.localScale / 2.0f;
        var extents = _size / 2.0f;

        center.y = extents.y;

        if (_boxCollider != null)
        {
            _boxCollider.center = center;
            _boxCollider.size = extents * 2.0f;
        }

        if (_staticVentZone != null)
        {
            _staticVentZone.ZoneBounds.center = center;
            _staticVentZone.ZoneBounds.extents = extents;
        }


        UpdateZoneHeights();
    }
    
    private void CreateDefaultSubZone()
    {
        var zone = new SubGasZoneData();

        zone.Color = 0;
        zone.UpperBoundHeight = -1;

        SubGasZones.Add(zone);
    }

    private void CreateSubZoneRenderers()
    {
        if (SubGasZones == null || SubGasZones.Count <= 0)
            return;

        for (int i = 0; i < SubGasZones.Count; i++)
        {
            SubGasZoneData gasZone = SubGasZones[i];
            if (gasZone != null && gasZone.ZoneRenderInstance == null)
            {
                CreateRendererInstance(gasZone);
            }
        }
    }

    public void SetSize(Vector3 size, Vector3 center)
    {
        transform.position = center;
        Size = size;
    }

    public SubGasZoneData AddNewSubGasZone()
    {
        var gasZone = new SubGasZoneData();
        gasZone.Color = SubGasZones.Count;

        float upperBound = Size.y / 2.0f;
        if (SubGasZones.Count > 1) //gas zone index 0 is the default, check for additional zones
        {
            upperBound = SubGasZones[SubGasZones.Count - 1].UpperBoundHeight / 2.0f;
        }

        gasZone.UpperBoundHeight = upperBound;

        SubGasZones.Add(gasZone);

        if (gasZone != null && gasZone.ZoneRenderInstance == null)
        {
            CreateRendererInstance(gasZone);            
        }

        return gasZone;
    }

    public void RemoveSubGasZone(int index)
    {
        if (index < 0 || index >= SubGasZones.Count)
            return;

        var subGasZone = SubGasZones[index];
        if (subGasZone.ZoneRenderInstance != null)
        {
            Destroy(subGasZone.ZoneRenderInstance);
            subGasZone.ZoneRenderInstance = null;
        }

        SubGasZones.RemoveAt(index);
    }

    private void UpdateVentZone()
    {
        if (_staticVentZone == null)
            _staticVentZone = GetComponent<StaticVentilationZoneStratified>();

        if (_staticVentZone == null)
            return;

        //_staticVentZone.ZoneBounds.extents = CoreRender.transform.localScale / 2;
        _staticVentZone.ZoneBounds.extents = Size / 2.0f;
        _staticVentZone.ZoneBounds.center = new Vector3(0, Size.y / 2.0f, 0);
        _staticVentZone.FalloffSize = Falloff;

        if (_staticVentZone.StaticMineAtmosphere == null)
            _staticVentZone.StaticMineAtmosphere = ScriptableObject.CreateInstance<StaticMineAtmosphere>();

        if (_staticVentZone.AtmosphereStrata == null)
            _staticVentZone.AtmosphereStrata = new List<StaticVentilationZoneStratified.AtmosphereStratumData>();

        //initialize static mine atmosphere scriptable objects if needed
        if (_staticVentZone.AtmosphereStrata.Count != SubGasZones.Count - 1)
        {
            _staticVentZone.AtmosphereStrata.Clear();

            for (int i = 1; i < SubGasZones.Count; i++)
            {
                var stratum = new StaticVentilationZoneStratified.AtmosphereStratumData();
                stratum.StrataAtmosphere = ScriptableObject.CreateInstance<StaticMineAtmosphere>();
                _staticVentZone.AtmosphereStrata.Add(stratum);
            }
        }

        if (SubGasZones.Count > 0)
        {
            //default zone
            var subGasZone = SubGasZones[0];
            _staticVentZone.StaticMineAtmosphere.MineAtmosphere = subGasZone.MineAtmosphere;
        }

        for (int i = 1; i < SubGasZones.Count; i++)
        {
            var subGasZone = SubGasZones[i];
            var stratum = _staticVentZone.AtmosphereStrata[i - 1];

            _staticVentZone.AtmosphereStrata[i - 1] = CopyToStratum(subGasZone, stratum);
        }

        _staticVentZone.RecalculateBounds();
    }

    private StaticVentilationZoneStratified.AtmosphereStratumData CopyToStratum(SubGasZoneData subGasZone, StaticVentilationZoneStratified.AtmosphereStratumData stratum)
    {
        stratum.UpperBoundHeight = subGasZone.UpperBoundHeight;
        stratum.StrataAtmosphere.MineAtmosphere = subGasZone.MineAtmosphere;

        return stratum;
    }

    public void UpdateZoneHeights()
    {
        if (_staticVentZone == null || SubGasZones == null || SubGasZones.Count <= 0)
            return;

        //UpdateZoneBounds();
        UpdateVentZone();

        float strataBottom = 0;

        for (int i = SubGasZones.Count - 1; i > 0; i--)
        {
            var subGasZone = SubGasZones[i];
            var stratum = _staticVentZone.AtmosphereStrata[i - 1];

            if (stratum.LocalYUpperBounds > _size.y)
            {
                stratum.LocalYUpperBounds = _size.y;
                _staticVentZone.AtmosphereStrata[i - 1] = stratum;
                subGasZone.UpperBoundHeight = stratum.LocalYUpperBounds;

                //subGasZone.RaiseValuesChanged();
            }

            subGasZone.SetRenderHeight(strataBottom, stratum.LocalYUpperBounds, Size);
            strataBottom = stratum.LocalYUpperBounds;
        }

        //set default zone (top zone, index 0)
        SubGasZones[0].SetRenderHeight(strataBottom, Size.y, Size);
    }

    public string[] SaveInfo()
    {
        List<string> data = SaveSubInfo();
        data.Add("GasZoneCount| " + SubGasZones.Count);
        data.Add("Falloff|" + Falloff);
        data.Add("ZoneHeight|" + Size.y);
        data.Add("ZoneSizeX|" + Size.x);
        data.Add("ZoneSizeZ|" + Size.z);
        return data.ToArray();
    }
    public string SaveName()
    {
        return componentName;
    }

    List<string> SaveSubInfo()
    {
        List<string> zoneDataList = new List<string>();

        //save to info
        for (int i = 0; i < SubGasZones.Count; i++)
        {
            SubGasZoneData zone = SubGasZones[i];

            string co = $"CO_{i}|" + zone.MineAtmosphere.CarbonMonoxide;
            string o2 = $"O2_{i}|" + zone.MineAtmosphere.Oxygen;
            string h2s = $"H2S_{i}|" + zone.MineAtmosphere.HydrogenSulfide;
            string ch4 = $"CH4_{i}|" + zone.MineAtmosphere.Methane;

            string coStr = $"COStr_{i}|" + zone.MineAtmosphere.CarbonMonoxideStrength;
            string o2Str = $"O2Str_{i}|" + zone.MineAtmosphere.OxygenStrength;
            string h2sStr = $"H2SStr_{i}|" + zone.MineAtmosphere.HydrogenSulfideStrength;
            string ch4Str = $"CH4Str_{i}|" + zone.MineAtmosphere.MethaneStrength;

            string upperBounds = $"UpperBounds_{i}|" + zone.UpperBoundHeight;
            string color = $"Color_{i}|" + SubGasZones[i].Color;

            string data = co + o2 + h2s + ch4 + upperBounds;

            zoneDataList.Add(co);
            zoneDataList.Add(o2);
            zoneDataList.Add(h2s);
            zoneDataList.Add(ch4);

            zoneDataList.Add(coStr);
            zoneDataList.Add(o2Str);
            zoneDataList.Add(h2sStr);
            zoneDataList.Add(ch4Str);

            zoneDataList.Add(upperBounds);
            zoneDataList.Add(color);
        }

        return zoneDataList;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load static gas zone component info. Saved component is null for " + gameObject.name);
            return;
        }

        componentName = component.GetComponentName();

        Vector3 size = Vector3.one;
        float.TryParse(component.GetParamValueAsStringByName("ZoneHeight"), out size.y);
        float.TryParse(component.GetParamValueAsStringByName("ZoneSizeX"), out size.x);
        float.TryParse(component.GetParamValueAsStringByName("ZoneSizeZ"), out size.z);

        Size = size;

        if (!float.TryParse(component.GetParamValueAsStringByName("Falloff"), out Falloff))
            Debug.LogError($"Static gas zone {componentName} missing Falloff data");

        int gasZoneCount = 0;
        if (!int.TryParse(component.GetParamValueAsStringByName("GasZoneCount"), out gasZoneCount))
            Debug.LogError($"Static gas zone {componentName} missing GasZoneCount data");

        SubGasZones.Clear();
        for (int i = 0; i < gasZoneCount; i++)
        {
            var subZone = LoadSubZoneData(i, component);
            SubGasZones.Add(subZone);
        }

        UpdateVentZone();

        Debug.Log("Load Static Gas Zone Info With " + gasZoneCount + " Sub Zones");

    }

    private SubGasZoneData LoadSubZoneData(int index, SavedComponent component)
    {
        var zone = new SubGasZoneData();

        try
        {

            //load strengths - only allow exactly 1.0f or 0.0f
            float coStr = component.GetParamValueFloat("COStr_" + index, 1.0f) > 0 ? 1.0f : 0.0f;
            float o2Str = component.GetParamValueFloat("O2Str_" + index, 1.0f) > 0 ? 1.0f : 0.0f;
            float h2sStr = component.GetParamValueFloat("H2SStr_" + index, 1.0f) > 0 ? 1.0f : 0.0f;
            float ch4Str = component.GetParamValueFloat("CH4Str_" + index, 1.0f) > 0 ? 1.0f : 0.0f;

            zone.MineAtmosphere.CarbonMonoxide = float.Parse(component.GetParamValueAsStringByName("CO_" + index));
            zone.MineAtmosphere.Oxygen = float.Parse(component.GetParamValueAsStringByName("O2_" + index));
            zone.MineAtmosphere.HydrogenSulfide = float.Parse(component.GetParamValueAsStringByName("H2S_" + index));
            zone.MineAtmosphere.Methane = float.Parse(component.GetParamValueAsStringByName("CH4_" + index));

            zone.MineAtmosphere.CarbonMonoxideStrength = coStr;
            zone.MineAtmosphere.OxygenStrength = o2Str;
            zone.MineAtmosphere.HydrogenSulfideStrength = h2sStr;
            zone.MineAtmosphere.MethaneStrength = ch4Str;

            zone.UpperBoundHeight = float.Parse(component.GetParamValueAsStringByName("UpperBounds_" + index));
            zone.Color = int.Parse(component.GetParamValueAsStringByName("Color_" + index));
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error parsing atmosphere data for {componentName} zone {index} :: {ex.Message} {ex.StackTrace}");
        }

        return zone;
    }

    public void Update()
    {
        if (transform.hasChanged)
        {
            transform.localScale = Vector3.one;
            transform.hasChanged = false;

        }

        if (_boxCollider == null || FalloffObject == null)
            return;

        //FalloffObject.transform.localScale = transform.lossyScale + (new Vector3(Falloff, Falloff, Falloff) * 2);
        FalloffObject.transform.localScale = Size + (new Vector3(Falloff, Falloff, Falloff) * 2);
        FalloffObject.transform.position = transform.TransformPoint(_boxCollider.center);
        FalloffObject.transform.rotation = transform.rotation;
    }



    public void CreateRendererInstance(SubGasZoneData info)
    {
        if (info == null)
            return;

        if (info.ZoneRenderInstance == null) 
            info.ZoneRenderInstance = Instantiate(ZoneRenderPrefab, transform);

        info.ZoneRenderer = info.ZoneRenderInstance.GetComponent<Renderer>();

        if (GasZoneColors != null)
            info.ChangeColor(GasZoneColors.GetColor(info.Color));

    }

    public void SetVisibility(bool visible)
    {
        if (_falloffRenderer != null) 
            _falloffRenderer.enabled = visible;

        foreach (SubGasZoneData subInfo in SubGasZones)
        {
            subInfo.SetVisibility(visible);
        }
    }

    void OnLayerChanged(LayerManager.EditorLayer layer)
    {
        if (layer == LayerManager.EditorLayer.Ventilation)
            SetVisibility(true);
        else
            SetVisibility(false);
        
    }

    public Vector3 GetFocusTarget()
    {
        return transform.position + new Vector3(0, _size.y / 2, 0);
    }
}
