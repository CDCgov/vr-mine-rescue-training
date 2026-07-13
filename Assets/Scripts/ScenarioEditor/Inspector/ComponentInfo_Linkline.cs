using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_Linkline : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public string ComponentInspectorTitle => "Linkline";

    [InspectableNumericProperty("Disconnect Distance", MinValue = 1, MaxValue = 50, SliderControl = true,
        Units = NumericPropertyUnitType.Meters)]
    public float DisconnectDistance { get; set; }

    private LinkLineObject[] _linkLineObjects;

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        DisconnectDistance = component.GetParamValueFloat("DisconnectDistance", 3.0f);


        if (_linkLineObjects == null)
            _linkLineObjects = GetComponentsInChildren<LinkLineObject>();

        foreach (var obj in _linkLineObjects)
        {
            obj.DropLinkDistance = DisconnectDistance;
        }
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "DisconnectDistance|" + DisconnectDistance,
        };
    }

    public string SaveName()
    {
        return "Linkline";
    }

    void Awake()
    {
        DisconnectDistance = 3;
    }

    void Start()
    {
        if (_linkLineObjects == null)
            _linkLineObjects = GetComponentsInChildren<LinkLineObject>();
    }

}
