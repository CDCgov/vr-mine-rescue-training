using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete]
public class ComponentInfo_SceneVentillation : MonoBehaviour//, ISaveableComponent
{
    //public bool UseMFire = true;
    //public MineAtmosphere StaticAtmosphere;

    //string _componenentName = "MineVentilation";
    //private StaticVentilationManager _StaticMineAtmosphere;
    //private VentilationControl _VentControl;
    //public void LoadInfo(SavedComponent component)
    //{
    //    _StaticMineAtmosphere = FindObjectOfType<StaticVentilationManager>();
    //    _VentControl = FindObjectOfType<VentilationControl>();
    //    UseMFire = component.GetParamValueBool("UseMFire");
    //    MineAtmosphere mineAtmo = new MineAtmosphere();
    //    mineAtmo.Oxygen = component.GetParamValueFloat("StaticOxygen");
    //    mineAtmo.CarbonMonoxide = component.GetParamValueFloat("StaticCO");
    //    mineAtmo.Methane = component.GetParamValueFloat("StaticMethane");
    //    mineAtmo.HydrogenSulfide = component.GetParamValueFloat("StaticH2S");
    //    StaticAtmosphere = mineAtmo;

    //    if (!UseMFire)
    //    {
    //        if(_StaticMineAtmosphere != null && _VentControl != null)
    //        {
    //            //_StaticMineAtmosphere.MineAtmosphere = StaticAtmosphere;
    //            _StaticMineAtmosphere.DefaultMineAtmosphere = StaticAtmosphere;
    //            _VentControl.VentilationProvider = VentilationProvider.StaticVentilation;
    //        }
    //        else
    //        {
    //            Debug.Log($"Static mine atmo and/or vent control was null???");
    //        }
    //    }
    //}

    //public string[] SaveInfo()
    //{
    //    return new string[] { "UseMFire|" + UseMFire, "StaticOxygen|" + StaticAtmosphere.Oxygen, "StaticCO|" + StaticAtmosphere.CarbonMonoxide, "StaticMethane|" + StaticAtmosphere.Methane, "StaticH2S|" + StaticAtmosphere.HydrogenSulfide };
    //}

    //public string SaveName()
    //{
    //    return _componenentName;
    //}
}
