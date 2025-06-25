using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFireProtocol;
using System.Text;

public class VentFan : VentObj
{
    public int FanID { get; set; }
    public int MFireID
    {
        get
        {
            if (MFFan == null)
                return -1;

            return MFFan.Number;
        }
        set
        {
            MFFan.Number = value;
        }
    }

    public VentAirway Airway;

    public Vector3 WorldPosition;
    public Quaternion WorldRotation;

    public MFFan MFFan
    {
        get
        {
            if (Airway != null)
                _mfFan.AirwayNo = Airway.MFireID;
            return _mfFan;
        }
        set
        {
            _mfFan = value;
        }
    }

    private MFFan _mfFan;
    private MFireServerControl _serverControl;

    public VentFan()
    {
        ResetToDefaults();
    }

    public void ResetToDefaults()
    {
        _mfFan = new MFFan();
    }

    public void UpdateFan()
    {
        UpdateFan(_serverControl);
    }

    public void UpdateFan(MFireServerControl serverControl)
    {
        if (serverControl == null)
            return;

        Debug.Log($"Updating MFFan in airway {MFFan.AirwayNo}");

        _serverControl = serverControl;
        serverControl.ChangeFan(MFFan);

    }

    public void SetFanData(VentFanData fanData)
    {
        if (fanData.PressureData == null || fanData.AirflowData == null || 
            fanData.AirflowData.Length <= 2 ||
            fanData.AirflowData.Length != fanData.PressureData.Length)
        {
            Debug.LogError($"VentFan: Attempt to use invalid fan data for fan {FanID}");
            return;
        }

        MFFan.AirflowData = new List<double>(fanData.AirflowData);
        MFFan.PressureData = new List<double>(fanData.PressureData);

        UpdateFan();
    }

    public override void AppendText(StringBuilder sb)
    {
        sb.AppendLine($"Fan {FanID}");

        if (MFFan.AirflowData != null && MFFan.PressureData != null)
        {
            int count = Mathf.Min(MFFan.AirflowData.Count, MFFan.PressureData.Count);
            for (int i = 0; i < count; i++)
            {
                sb.AppendLine($"{MFFan.AirflowData[i]:F0} : {MFFan.PressureData[i]:F2}");
            }
        }
    }
}
