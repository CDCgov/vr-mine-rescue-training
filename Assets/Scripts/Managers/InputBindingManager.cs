using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class InputMappingKB
{
    public string ActionName;
    public KeyCode KeyCode;
    public bool Alt;
    public bool Ctrl;
    public bool Shift;
    public UnityAction Delegate;
}


[HasCommandConsoleCommands]
[CreateAssetMenu(fileName = "InputBindingManager", menuName = "VRMine/Managers/InputBindingManager", order = 0)]
public class InputBindingManager : Manager
{
    public const string DefaultResourcePath = "Managers/InputBindingManager";
    public static InputBindingManager GetDefault() { return Resources.Load<InputBindingManager>(DefaultResourcePath); }

    private InputHandler _inputHandler;
    private InputConfig _inputConfig;

    public event UnityAction ActionListChanged;

    public void RegisterAction(string actionName, string actionCategory, UnityAction action)
    {
        LoadConfig();
        LocateInputHandler();

        var mapping = _inputConfig.LookupAction(actionName);
        if (mapping != null)
        {
            mapping.Delegate = action;
            // {
            // 	//new action added, save out the config to include it
            // 	_inputConfig.SaveConfig();
            // 	Debug.Log($"New action detected '{actionName}', saving config");
            // }

            RaiseActionListChanged();
        }
    }

    public void UnregisterAction(string actionName)
    {
        LoadConfig();
        LocateInputHandler();

        var mapping = _inputConfig.LookupAction(actionName);
        if (mapping != null)
        {
            mapping.Delegate = null;
        }
    }

    public Dictionary<string, InputMapping> GetKeyboardMap()
    {
        return _inputConfig.KeyboardMap;
    }

    private void RaiseActionListChanged()
    {
        ActionListChanged?.Invoke();
    }

    private void OnEnable()
    {		
        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadConfig();
    }

    private void LoadConfig()
    {
        if (_inputConfig == null)
        {
            _inputConfig = YAMLConfig.LoadConfig<InputConfig>("input_config.yaml");
            _inputConfig.LoadDefaults(); //FIXME - for now always reset to defaults
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {

    }

    private void OnDisable()
    {
    }

    private void LocateInputHandler()
    {
        if (_inputHandler != null)
            return;

        var obj = GameObject.Find("InputHandler");
        if (obj != null)
        {
            _inputHandler = obj.GetComponent<InputHandler>();
            if (_inputHandler == null)
            {
                Debug.LogError("Error: InputHandler object missing InputHandler component");
                Destroy(obj);
            }
        }
        
        //if haven't found the input handler, create one
        if (_inputHandler == null)
        {
            obj = new GameObject("InputHandler");
            Util.DontDestroyOnLoad(obj);

            _inputHandler = obj.AddComponent<InputHandler>();
            _inputHandler.SetInputBindingManager(this);
        }
    }


}