using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_ResizableStopping : ModularComponentInfo, ISaveableComponent, IInspectableComponent, IInspectableFieldEnabled
{
    public ResizableStopping ResizableStopping;

    [InspectableNumericProperty("Meters Per Tile", MinValue = 0.1f, MaxValue = 5, SliderControl = true, 
        Tooltip = "Controls density of block texture tiling")]
    public float MetersPerTile
    {
        get { return _tilesPerMeter; }
        set
        {
            _tilesPerMeter = value;
            if (ResizableStopping != null)
            {
                ResizableStopping.MetersPerTile = value;
                ResizableStopping.ResizeStopping();
            }
        }
    }

    [InspectableNumericProperty("Door Height", MinValue = 0, MaxValue = 2, SliderControl = true, 
        Units = NumericPropertyUnitType.Meters, Tooltip = "Height of door in stopping")]
    public float DoorHeight
    {
        get { return _doorHeight; }
        set
        {
            _doorHeight = value;
            if (ResizableStopping != null)
            {
                ResizableStopping.DoorHeight = value;
                ResizableStopping.ResizeStopping();
            }
        }
    }

    [InspectableBoolProperty("Flip Door", OnText = "Hinge R", OffText = "Hinge L")]
    public bool FlipDoorRotation
    {
        get { return _flipDoorRotation; }
        set
        {
            _flipDoorRotation = value;
            if (ResizableStopping != null)
            {
                ResizableStopping.FlipDoorRotation = value;
                ResizableStopping.ResizeStopping();
            }
        }
    }

    [InspectableBoolProperty("Door Enabled", Tooltip = "Add door to stopping - if no door present, closed resistance is used")]
    public bool DoorEnabled
    {
        get { return _doorEnabled; }
        set
        {
            _doorEnabled = value;
            if (ResizableStopping != null)
            {
                ResizableStopping.DoorEnabled = value;
                ResizableStopping.ResizeStopping();
            }
        }
    }

    public string ComponentName = "Resizable Stopping";
    public bool ShowDoorEnabledInspector = true;

    private float _tilesPerMeter = 2.0f;
    private float _doorHeight = 0.5f;
    private bool _flipDoorRotation = false;
    private bool _doorEnabled = true;
    private Vector3 _size;

    public string ComponentInspectorTitle 
    {
        get
        {
            return ComponentName;
        }
        
    }

    void Awake()
    {
        if (ResizableStopping == null)
            ResizableStopping = GetComponent<ResizableStopping>();

        if (ResizableStopping != null)
        {
            _doorHeight = ResizableStopping.DoorHeight;
            _tilesPerMeter = ResizableStopping.MetersPerTile;
            _flipDoorRotation = ResizableStopping.FlipDoorRotation;
            _doorEnabled = ResizableStopping.DoorEnabled;
        }
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        if (ResizableStopping == null)
        {
            Debug.LogError($"No DynamicVentilationControl on {name}");
            return;
        }

        MetersPerTile = component.GetParamValueFloat("TilesPerMeter", 1.0f);
        DoorHeight = component.GetParamValueFloat("DoorHeight", 0.5f);
        FlipDoorRotation = component.GetParamValueBool("FlipDoorRotation", false);
        DoorEnabled = component.GetParamValueBool("DoorEnabled", true);

        _size.x = component.GetParamValueFloat("Width", 1.0f);
        _size.y = component.GetParamValueFloat("Height", 1.0f);
        _size.z = 1.0f;

        if (ResizableStopping != null)
            ResizableStopping.ResizeStopping(_size);
    }

    public string[] SaveInfo()
    {
        if (ResizableStopping != null)
            _size = ResizableStopping.Size;

        return new string[]
        {
            "TilesPerMeter|" + MetersPerTile,
            "DoorHeight|" + DoorHeight,
            "FlipDoorRotation|" + FlipDoorRotation,
            "DoorEnabled|" + DoorEnabled,
            "Width|" + _size.x,
            "Height|" + _size.y,
        };
    }

    public string SaveName()
    {
        return ComponentName;
    }

    public bool IsFieldInspectorEnabled(string fieldName)
    {
        if (fieldName == nameof(DoorEnabled))
        {
            return ShowDoorEnabledInspector;
        }

        return true;
    }
}
