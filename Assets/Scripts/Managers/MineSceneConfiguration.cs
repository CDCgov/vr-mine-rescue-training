using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineSceneConfiguration : MonoBehaviour
{
    public static MineSceneConfiguration CurrentScene
    {
        get
        {
            if (_currentScene != null)
                return _currentScene;
            else
            {
                var obj = GameObject.FindGameObjectWithTag("MineSceneConfiguration");
                if (obj != null)
                    _currentScene = obj.GetComponent<MineSceneConfiguration>();

                return _currentScene;
            }
        }
    }
    private static MineSceneConfiguration _currentScene = null;

    public GlobalMineParameters MineParameters;

    public float BG4DurationMinutes = 20;
    public float MasterVolume = 100;
    public List<VRNPlayerEquipmentType> DisabledEquipmentList;
    public List<VRNPlayerEquipmentType> AddEquipmentList;
    public bool AllowSelfCalibration = false;
    public bool SilenceAlarms = true;

    private void OnEnable()
    {
        MineSceneConfiguration._currentScene = this;
    }

    private void OnDisable()
    {
        MineSceneConfiguration._currentScene = null;
    }
}
