using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Obsolete]
public class ComponentInfo_MineSceneConfig : MonoBehaviour//, ISaveableComponent
{
    //public MineSceneConfiguration MineSceneConfig;
    //public MineProfiles MineProfiles;
    //string _componenentName = "Mine Scene Config";
    //public float BG4Duration = 20;
    //public float MasterVolume = 100;
    //public int ProfileSelection = 0;
    //public List<VRNPlayerEquipmentType> DisabledEquipment = new List<VRNPlayerEquipmentType>();
    //public bool AllowSelfCalibration = false;
    //public bool SilenceAlarms = true;

    //private NetBroadcastSpawner _netBroadcastSpawner;

    //public void DefaultLoad()
    //{
    //    if (_netBroadcastSpawner == null)
    //    {
    //        _netBroadcastSpawner = FindObjectOfType<NetBroadcastSpawner>();
    //    }
    //    MineSceneConfig.BG4DurationMinutes = BG4Duration;
    //    MineSceneConfig.DisabledEquipmentList = MineProfiles.MineProfileList[ProfileSelection].DisabledEquipment;
    //    MineSceneConfig.AddEquipmentList = MineProfiles.MineProfileList[ProfileSelection].AddedEquipment;
    //    MineSceneConfig.AllowSelfCalibration = AllowSelfCalibration;
    //    MineSceneConfig.MasterVolume = MasterVolume;
    //    MineSceneConfig.SilenceAlarms = SilenceAlarms;

    //    if (_netBroadcastSpawner != null)
    //    {
    //        _netBroadcastSpawner.PlayerPrefab = MineProfiles.MineProfileList[ProfileSelection].ThirdPersonPrefab;
    //    }
    //}
    //public void LoadInfo(SavedComponent component)
    //{
    //    Debug.Log($"Loading Mine Scene config start");
    //    if (component == null)
    //    {
    //        Debug.Log("Uh oh, Mine Scene Config component was null??");
    //        return;
    //    }
    //    if(MineSceneConfig == null)
    //    {
    //        MineSceneConfig = MineSceneConfiguration.CurrentScene;
    //    }

    //    if(_netBroadcastSpawner == null)
    //    {
    //        _netBroadcastSpawner = FindObjectOfType<NetBroadcastSpawner>();
    //    }

    //    _componenentName = component.GetComponentName();
    //    float.TryParse(component.GetParamValueAsStringByName("BG4Duration"), out BG4Duration);
    //    float.TryParse(component.GetParamValueAsStringByName("MasterVolume"), out MasterVolume);
    //    int.TryParse(component.GetParamValueAsStringByName("Profile"), out ProfileSelection);
    //    //string eqList = component.GetParamValueAsStringByName("DisabledEquipment");
    //    //List<int> equipment = new List<int>();
    //    //if (!String.IsNullOrEmpty(eqList)) {
    //    //    equipment = eqList.Split(',').Select(int.Parse).ToList();
    //    //}
    //    //foreach (var item in equipment)
    //    //{
    //    //    DisabledEquipment.Add((VRNPlayerEquipmentType)item);
    //    //}
    //    bool.TryParse(component.GetParamValueAsStringByName("SelfCalibration"), out AllowSelfCalibration);
    //    if(bool.TryParse(component.GetParamValueAsStringByName("SilenceAlarms"), out SilenceAlarms))
    //    {
    //        MineSceneConfig.SilenceAlarms = SilenceAlarms;
    //    }
    //    MineSceneConfig.BG4DurationMinutes = BG4Duration;
    //    MineSceneConfig.DisabledEquipmentList = MineProfiles.MineProfileList[ProfileSelection].DisabledEquipment;
    //    MineSceneConfig.AddEquipmentList = MineProfiles.MineProfileList[ProfileSelection].AddedEquipment;
    //    MineSceneConfig.AllowSelfCalibration = AllowSelfCalibration;
    //    MineSceneConfig.MasterVolume = MasterVolume;

    //    if(_netBroadcastSpawner != null)
    //    {
    //        _netBroadcastSpawner.PlayerPrefab = MineProfiles.MineProfileList[ProfileSelection].ThirdPersonPrefab;
    //    }

    //    Debug.Log($"Should have configured the following: BG - {MineSceneConfig.BG4DurationMinutes}, Disabled - {MineSceneConfig.DisabledEquipmentList.ToString()}, SelfCal - {MineSceneConfig.AllowSelfCalibration}");
    //}

    //public string[] SaveInfo()
    //{        
    //    return new string[] { "BG4Duration|" + BG4Duration, "Profile|" + ProfileSelection, "SelfCalibration|" + AllowSelfCalibration, "MasterVolume|" + MasterVolume, "SilenceAlarms|" + SilenceAlarms };
    //}

    //public string SaveName()
    //{
    //    return _componenentName;
    //}

    //// Start is called before the first frame update
    //void Start()
    //{
    //    if (MineSceneConfig == null)
    //    {
    //        MineSceneConfig = GetComponentInParent<MineSceneConfiguration>();
    //    }
    //}

    //public void SetBG4(float bg4Duration)
    //{
    //    BG4Duration = bg4Duration;
    //    MineSceneConfig.BG4DurationMinutes = bg4Duration;
    //}

    //public void SetDisabledEquipment(List<VRNPlayerEquipmentType> disabledEq)
    //{
    //    DisabledEquipment = disabledEq;
    //    MineSceneConfig.DisabledEquipmentList = disabledEq;
    //}

    //public void SetAllowCalibration(bool allowCal)
    //{
    //    AllowSelfCalibration = allowCal;
    //    MineSceneConfig.AllowSelfCalibration = allowCal;
    //}

    //public void SetMasterVolume(float vol)
    //{
    //    MasterVolume = vol;
    //    MineSceneConfig.MasterVolume = vol;
    //}

    //public void SetMinerProfile(int selection)
    //{
    //    ProfileSelection = selection;
    //    MineSceneConfig.DisabledEquipmentList = MineProfiles.MineProfileList[selection].DisabledEquipment;
    //    MineSceneConfig.AddEquipmentList = MineProfiles.MineProfileList[selection].AddedEquipment;
    //}

    //public void SetSilenceAlarms(bool silence)
    //{
    //    SilenceAlarms = silence;
    //    MineSceneConfig.SilenceAlarms = silence;
    //}
}
