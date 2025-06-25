using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[System.Serializable]
public class TrialSettings
{
    public class ValidationException : System.Exception 
    { 
        public ValidationException() : base() { }
        public ValidationException(string message) : base(message) {} 
        public ValidationException(string message, System.Exception inner) : base(message, inner) { }
    }

    public Experiment.UpdateMethod ExperimentUpdateMethod { get; set; }
    public float ManualUpdateTimestep { get; set; }
    public int ManualUpdatesPerFrame { get; set; }

    public bool LoadSettings(Dictionary<string, Experiment.ExperimentVal> settings, out string errorMessage)
    {
        try
        {
            LoadSettings(settings);
            errorMessage = null;
            return true;
        }
        catch (System.Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    public virtual void LoadSettings(Dictionary<string, Experiment.ExperimentVal> settings)
    {
        var type = GetType();
        
        HashSet<string> validPropNames = new HashSet<string>();

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            string propName = prop.Name;
            Experiment.ExperimentVal expVal;

            validPropNames.Add(propName);

            if (!settings.TryGetValue(propName, out expVal))
            {
                throw new ValidationException($"Trial settings missing property {propName}");
            }

            if (expVal.NumValues != 1)
            {
                throw new ValidationException("Tried to load a trial with settings that have multiple possible values");
            }

            try
            {
                var propType = prop.PropertyType;
                if (propType == typeof(double))
                {
                    prop.SetValue(this, expVal.GetValue<double>());
                }
                else if (propType == typeof(float))
                {
                    prop.SetValue(this, expVal.GetValue<float>());
                }
                else if (propType == typeof(int))
                {
                    prop.SetValue(this, expVal.GetValue<int>());
                }
                else if (propType == typeof(string))
                {
                    prop.SetValue(this, expVal.GetValue<string>());
                }
                else if (propType == typeof(Experiment.UpdateMethod))
                {
                    prop.SetValue(this,  System.Enum.Parse(typeof(Experiment.UpdateMethod), expVal.GetValue<string>(), true));
                }
                else
                {
                    throw new ValidationException("Unsupported trial setting data type");
                }
                
            }
            catch (System.Exception ex)
            {
                throw new ValidationException(ex.Message);
            }
        }

        foreach (var key in settings.Keys)
        {
            if (!validPropNames.Contains(key))
            {
                throw new ValidationException("Trial settings has extra / unknown data");
            }
        }
    }

}