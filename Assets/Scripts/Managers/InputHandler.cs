using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    private InputBindingManager _bindingManager;
    private Dictionary<string, InputMapping> _kbMap;

    public void SetInputBindingManager(InputBindingManager manager)
    {
        _bindingManager = manager;
    }

    void Start()
    {
        _bindingManager.ActionListChanged += OnActionListChanged;
        _kbMap = _bindingManager.GetKeyboardMap();
    }

    void OnActionListChanged()
    {
        _kbMap = _bindingManager.GetKeyboardMap();
    }

    void Update()
    {
        foreach (var map in _kbMap.Values)
        {
            if (map.Delegate == null)
                continue;
                
            if (Input.GetKeyDown(map.KeyCode))
            {
                if (map.Shift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    continue;
                if (map.Ctrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    continue;
                if (map.Alt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                    continue;

                Debug.Log($"Invoking {map.ActionName}");
                if (map.Delegate != null)
                    map.Delegate();
            }
        }
    }
}