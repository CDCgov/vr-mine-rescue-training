using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIContextControlBase : UIContextBase
{
    public string ContextVariable = "UNKNOWN";

    private object _lastValue;


    protected override void Start()
    {
        base.Start();

        _context.ContextDataChanged += OnContextDataChanged;
    }

    protected void SetContextVariable(object val)
    {
        if (ContextVariable == null || ContextVariable.Length <= 0)
            return;

        _lastValue = val;
        _context.SetVariable(ContextVariable, val);
    }

    protected virtual void OnContextDataChanged(string variableKey)
    {
        if (variableKey == ContextVariable)
        {
            var val = _context.GetVariable(ContextVariable);
            if (val != _lastValue)
            {
                _lastValue = val;
                OnVariableChanged(val);
            }
        }
    }

    protected float GetVariableAsFloat()
    {
        return _context.GetFloatVariable(ContextVariable);
        //var val = _context.GetVariable(ContextVariable);
        //if (val == null)
        //    return float.NaN;

        //float floatVal = float.NaN;

        //if (val is float || val is double || val is int)
        //    floatVal = (float)val;
        //else if (val is string)
        //{
        //    if (!float.TryParse((string)val, out floatVal))
        //        return float.NaN;
        //}

        //return floatVal;
    }

    protected int GetVariableAsInt()
    {
        return _context.GetIntVariable(ContextVariable);
        //var val = _context.GetVariable(ContextVariable);
        //if (val == null)
        //    return -1;

        //int intVal = -1;

        //if (val is int)
        //    intVal = (int)val;
        //else if (val is float || val is double)
        //    intVal = Mathf.RoundToInt((float)val);
        //else if (val is string)
        //{
        //    if (!int.TryParse((string)val, out intVal))
        //        return -1;
        //}

        //return intVal;
    }

    protected string GetVariableAsString()
    {
        return _context.GetStringVariable(ContextVariable);
        //var val = _context.GetVariable(ContextVariable);
        //if (val == null)
        //    return null;

        //return val.ToString();
    }

    protected abstract void OnVariableChanged(object val);
}
