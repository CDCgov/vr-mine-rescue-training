using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CsvHelper;
using System.IO;
using System.Text;
using System;
using System.Globalization;

[RequireComponent(typeof(Button))]
public class UIHMDCalibrationButton : SelectedPlayerControl
{
    public enum CalibrationCommandEnum
    {
        ResetCal,
        ComputeOffset,
        SetRotation,
        TuneRotation,
        CalibrateFloor,
        CalibrateTwoController,
        SetPlayerHeight,
        RecordRControllerPosition,
        ResetFloorCalibration,
        SetAllPlayerHeights,
    }



    public PlayerManager PlayerManager;
    public CalibrationCommandEnum CalibrationCommand = CalibrationCommandEnum.ComputeOffset;
    public VRNCalibrationSource CalibrationSource = VRNCalibrationSource.CalHead;

    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        var button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (_player == null)
            _player = PlayerManager.CurrentPlayer;

        switch (CalibrationCommand)
        {
            case CalibrationCommandEnum.ResetCal:
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmResetCalibrationOffset, (int)CalibrationSource);
                break;

            case CalibrationCommandEnum.ComputeOffset:
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmRecomputeCalibrationOffset, (int)CalibrationSource);
                break;

            case CalibrationCommandEnum.SetRotation:
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetCalibrationRotation, (int)CalibrationSource);
                break;

            case CalibrationCommandEnum.TuneRotation:
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmTuneCalibrationRotation, (int)CalibrationSource);
                break;

            case CalibrationCommandEnum.CalibrateFloor:
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetFloorCalibration, (int)CalibrationSource);
                break;

            case CalibrationCommandEnum.CalibrateTwoController:
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmTwoControllerCalibration, (int)CalibrationSource);
                break;
            case CalibrationCommandEnum.SetPlayerHeight:
                //float setPlayerHeight = _player.RigTransform.InverseTransformPoint(_player.HeadTransform.position).y;
                //float setPlayerHeight = _player.ComputeHeight();
                //Debug.Log("Player manager updated: " + PlayerManager.CurrentPlayer)
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmSetPlayerHeight, _player.ComputeHeight());
                break;
            case CalibrationCommandEnum.SetAllPlayerHeights:
                Dictionary<int, PlayerRepresentation>.ValueCollection playerRepresentations = PlayerManager.PlayerList.Values;
                foreach(PlayerRepresentation player in playerRepresentations)
                {
                    ///float setHeight = player.RigTransform.InverseTransformPoint(player.HeadTransform.position).y;
                    //Debug.Log("Player manager updated: " + PlayerManager.CurrentPlayer)
                    PlayerManager.SendPlayerMessage(player.PlayerID, VRNPlayerMessageType.PmSetPlayerHeight, player.ComputeHeight());
                }
                break;
            case CalibrationCommandEnum.RecordRControllerPosition:
                SaveControllerPosition();
                break;
            case CalibrationCommandEnum.ResetFloorCalibration:
                PlayerManager.SendPlayerMessage(_player.PlayerID, VRNPlayerMessageType.PmResetFloorCalibration, (int)CalibrationSource);
                break;
        }
    }

    private void SaveControllerPosition()
    {
        if (_player == null)
            return;

        Vector3 poiSpace = _player.GetRightControllerPOISpace();
        _player.SaveCalTestPosition(poiSpace);

        /*
        Directory.CreateDirectory("NetLogs");
        string filename = Path.Combine("NetLogs", "ControllerPosition.csv");

        if (!File.Exists(filename))
        {
            //write header
            using (StreamWriter sw = new StreamWriter(filename, true, Encoding.UTF8))
            {

                using (CsvWriter csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                {
                    csv.WriteField("Name");
                    csv.WriteField("Player ID");
                    csv.WriteField("Date");
                    csv.WriteField($"Timestamp");
                    csv.WriteField("Right Controller Tracked");
                    csv.WriteField("Pos X");
                    csv.WriteField("Pos Y");
                    csv.WriteField("Pos Z");
                    csv.WriteField("Rot X");
                    csv.WriteField("Rot Y");
                    csv.WriteField("Rot Z");
                    csv.WriteField("Rot W");
                    csv.WriteField("POI Space X");
                    csv.WriteField("POI Space Y");
                    csv.WriteField("POI Space Z");
                    csv.NextRecord();

                }
            }
        }

        using (StreamWriter sw = new StreamWriter(filename, true, Encoding.UTF8))
        {
            //append entry
            using (CsvWriter csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
            {

                csv.WriteField(_player.Name);
                csv.WriteField(_player.PlayerID);
                csv.WriteField(DateTime.Now.ToString("s"));
                csv.WriteField(Time.unscaledTime);
                csv.WriteField(_player.RightControllerTracked);
                csv.WriteField(_player.RightController.Object.transform.position.x);
                csv.WriteField(_player.RightController.Object.transform.position.y);
                csv.WriteField(_player.RightController.Object.transform.position.z);
                csv.WriteField(_player.RightController.Object.transform.rotation.x);
                csv.WriteField(_player.RightController.Object.transform.rotation.y);
                csv.WriteField(_player.RightController.Object.transform.rotation.z);
                csv.WriteField(_player.RightController.Object.transform.rotation.w);

                //Vector3 poiSpace = Vector3.zero;
                //if (_player.RigTransform.parent != null)
                //{
                //    var xform = _player.RigTransform.parent;

                //    poiSpace = xform.InverseTransformPoint(_player.RightController.Object.transform.position);
                //}
                //Vector3 poiSpace = _player.GetRightControllerPOISpace();
                csv.WriteField(poiSpace.x);
                csv.WriteField(poiSpace.y);
                csv.WriteField(poiSpace.z);


                //csv.WriteField(_player.RightController.Position.x);
                //csv.WriteField(_player.RightController.Position.y);
                //csv.WriteField(_player.RightController.Position.z);
                //csv.WriteField(_player.RightController.Rotation.x);
                //csv.WriteField(_player.RightController.Rotation.y);
                //csv.WriteField(_player.RightController.Rotation.z);
                //csv.WriteField(_player.RightController.Rotation.w);
                csv.NextRecord();
            }
        }
        */
    }
}