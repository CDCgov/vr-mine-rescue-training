using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContextData : MonoBehaviour
{
    public event Action<string> ContextDataChanged;


    private Dictionary<string, object> _genericData;

    //void Awake()
    //{
    //    _genericData = new Dictionary<string, object>();
    //}

    public UIContextData()
    {
        _genericData = new Dictionary<string, object>();
    }

    private void Start()
    {
        foreach (var key in _genericData.Keys)
        {
            ContextDataChanged?.Invoke(key);
        }
    }

    public void SetVariable(string key, object val)
    {
        object oldval;
        if (_genericData.TryGetValue(key, out oldval))
        {
            if (oldval == val)
                return;
        }

        _genericData[key] = val;
        try
        {
            ContextDataChanged?.Invoke(key);
        }
        catch (Exception ex)
        {
            Debug.LogError($"UIContextData: Error in ContextDataChanged Handler {ex.Message}");
        }
    }

    public object GetVariable(string key)
    {
        object val;
        if (_genericData.TryGetValue(key, out val))
            return val;

        return null;
    }

    public string GetStringVariable(string key)
    {
        try
        {
            var val = GetVariable(key);
            return Convert.ToString(val);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public float GetFloatVariable(string key)
    {
        try
        {
            var val = GetVariable(key);
            return Convert.ToSingle(val);
        }
        catch (Exception)
        {
            return float.NaN;
        }
    }

    public int GetIntVariable(string key)
    {
        try
        {
            var val = GetVariable(key);
            return Convert.ToInt32(val);
        }
        catch (Exception)
        {
            return -1;
        }
    }
}
