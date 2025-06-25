using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;
using YamlDotNet.Serialization;


public enum InputTargetType
{
    Unknown,
    Action
}


[System.Serializable]
public class InputMapping
{
    public InputMapping()
    {

    }
    public InputMapping(string actionName, string device, string opts, KeyCode keyCode, bool ctrl, bool alt, bool shift)
    {
        ActionName = actionName;
        Device = device;
        Options = opts;

        KeyCode = keyCode;
        Ctrl = ctrl;
        Alt = alt;
        Shift = shift;

        Delegate = null;
    }

    public string ActionName { get; set; }
    public string Device { get; set; }
    public KeyCode KeyCode { get; set; }
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public string Options { get; set; }

    public UnityAction Delegate;
/* 
    public InputMappingKB GetKeyboardMap()
    {
        //TODO: fix to check if this is actually a keyboard mapping
        InputMappingKB map = new InputMappingKB();
        map.ActionName = ActionName;
        map.KeyCode = KeyCode;
        map.Ctrl = Ctrl;
        map.Alt = Alt;
        map.Shift = Shift;
        map.Delegate = 

        return null;
    } */
}

public class InputConfig : YAMLConfig
{
    [Description("Keyboard action mapping")]
    public Dictionary<string, InputMapping> KeyboardMap { get; set; }

    public InputMapping LookupAction(string name)
    {
        InputMapping mapping;
        if (KeyboardMap.TryGetValue(name, out mapping))
        {
            return mapping;
        }

        //for now reload defaults if the action isn't in the mapping table - FIXME
        // Debug.LogWarning($"WARNING Reloading default input mapping because {name} isn't in the map!");
        // LoadDefaults();
        // SaveConfig();
        return null;

        // mapping = new InputMapping(name, "KB", "", KeyCode.Joystick1Button10, false, false, false);
        // KeyboardMap.Add(name, mapping);
        // return mapping;
    }



    public override void LoadDefaults()
    {
        KeyboardMap = new Dictionary<string, InputMapping>();

        AddKeyboardMapping("SpawnTestSphere", KeyCode.F3, true, false, false);
        AddKeyboardMapping("SpawnTestCube", KeyCode.F1, false, false, false);
        AddKeyboardMapping("SpawnTestCapsule", KeyCode.F2, false, false, false);
        AddKeyboardMapping("KeyboardTest2", KeyCode.F2, true, false, false);
        AddKeyboardMapping("InputBindingSceneLoadTest", KeyCode.Backspace, false, false, true);

        AddKeyboardMapping("LoadProxExperiment", KeyCode.X, false, false, false);
        AddKeyboardMapping("LoadSurfaceMine", KeyCode.F8, false, false, false);
        AddKeyboardMapping("LoadUndergroundMine", KeyCode.F7, false, false, false);
        AddKeyboardMapping("LoadBH20Garage", KeyCode.F9, false, false, false);

        AddKeyboardMapping("VRNavigateMode", KeyCode.N, false, false, false);
        AddKeyboardMapping("VRGrabMode", KeyCode.G, false, false, false);
        AddKeyboardMapping("VRActionMode", KeyCode.A, false, false, false);
        AddKeyboardMapping("VRMoveToOverview", KeyCode.O, false, false, false);
        AddKeyboardMapping("VRToggleHelp", KeyCode.H, false, false, false);
        AddKeyboardMapping("VRMoveToHaulTruck", KeyCode.T, false, false, false);
        AddKeyboardMapping("VRResetHaulTrucks", KeyCode.R, false, false, false);
    }


    public void AddKeyboardMapping(string actionName, KeyCode keyCode, bool ctrl, bool alt, bool shift)
    {
        KeyboardMap.Add(actionName, new InputMapping(actionName, "KB", "", keyCode, ctrl, alt, shift));
    }
}
