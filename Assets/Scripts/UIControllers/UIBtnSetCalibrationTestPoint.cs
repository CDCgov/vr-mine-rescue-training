using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class UIBtnSetCalibrationTestPoint : MonoBehaviour, ISelectedPlayerView
{
    public SystemManager SystemManager;

    public enum SetCalTestPointAction
    {
        SetTestPoint,
        AverageTestPoint,
    }

    public SetCalTestPointAction Action;

    private Button _button;
    private PlayerRepresentation _player;

    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_player == null)
            return;

        var poiSpace = _player.GetRightControllerPOISpace();

        switch (Action)
        {
            case SetCalTestPointAction.SetTestPoint:
                //SystemManager.SystemConfig.CalibrationTestPoint = new YAMLVec3(poiSpace.x, poiSpace.y, poiSpace.z);
                SystemManager.SystemConfig.CalibrationTestPointVec3 = poiSpace;
                break;

            case SetCalTestPointAction.AverageTestPoint:
                //var pt = SystemManager.SystemConfig.CalibrationTestPoint.ToVector3();
                var pt = SystemManager.SystemConfig.CalibrationTestPointVec3;
                pt = pt + poiSpace;
                pt *= 0.5f;
                //SystemManager.SystemConfig.CalibrationTestPoint = new YAMLVec3(pt.x, pt.y, pt.z);
                SystemManager.SystemConfig.CalibrationTestPointVec3 = pt;
                break;
        }

        SystemManager.SystemConfig.SaveConfig();

    }

    public void SetPlayer(PlayerRepresentation player)
    {
        _player = player;
    }
}
