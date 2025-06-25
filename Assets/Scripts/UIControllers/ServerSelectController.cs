using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ServerSelectController : MonoBehaviour
{
    public SystemManager SystemManager;

    public TMP_InputField ServerIPText;
    public TMP_InputField ClientNameText;
    public Toggle StartAsServerToggle;

    public TMP_InputField VRUpdateRate;
    public TMP_InputField ObjUpdateRate;
    public TMP_InputField ServerPortText;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (ServerIPText != null)
        {
            ServerIPText.text = SystemManager.SystemConfig.MultiplayerServer;
            ServerIPText.onValueChanged.AddListener((serverIP) => {
                SystemManager.SystemConfig.MultiplayerServer = serverIP;
                SystemManager.SystemConfig.SaveConfig();
            });
        }

        if(ServerPortText != null)
        {
            ServerPortText.text = SystemManager.SystemConfig.MultiplayerPort.ToString();
            ServerPortText.onValueChanged.AddListener((serverPort) =>
            {
                int val = 0;
                if(int.TryParse(serverPort, out val)){
                    SystemManager.SystemConfig.MultiplayerPort = val;
                    SystemManager.SystemConfig.SaveConfig();
                }
            });
        }

        if (ClientNameText != null)
        {
            ClientNameText.text = SystemManager.SystemConfig.MultiplayerName;
            ClientNameText.onValueChanged.AddListener((clientName) => {
                SystemManager.SystemConfig.MultiplayerName = clientName;
                SystemManager.SystemConfig.SaveConfig();
            });
        }

        if (StartAsServerToggle != null)
        {
            StartAsServerToggle.isOn = SystemManager.SystemConfig.DefaultToServerMode;
            StartAsServerToggle.onValueChanged.AddListener((startAsServer) =>
            {
                SystemManager.SystemConfig.DefaultToServerMode = startAsServer;
                SystemManager.SystemConfig.SaveConfig();
            });
        }

        if (VRUpdateRate != null)
        {
            VRUpdateRate.text = SystemManager.SystemConfig.MPVRUpdateRateHz.ToString("F1");
            VRUpdateRate.onValueChanged.AddListener((vrUpdateRate) =>
            {
                float rate;
                if (float.TryParse(VRUpdateRate.text, out rate))
                {
                    SystemManager.SystemConfig.MPVRUpdateRateHz = rate;
                    SystemManager.SystemConfig.SaveConfig();
                }
            });
        }

        if (ObjUpdateRate != null)
        {
            ObjUpdateRate.text = SystemManager.SystemConfig.MPObjectUpdateRateHz.ToString("F1");
            ObjUpdateRate.onValueChanged.AddListener((objUpdateRate) =>
            {
                float rate;
                if (float.TryParse(ObjUpdateRate.text, out rate))
                {
                    SystemManager.SystemConfig.MPObjectUpdateRateHz = rate;
                    SystemManager.SystemConfig.SaveConfig();
                }
            });
        }
    }

    void OnServerIPChanged(string serverIP)
    {
        
    }

    private void OnEnable()
    {
        if (SystemManager != null)
        {
            if (ServerIPText != null)
            {
                ServerIPText.text = SystemManager.SystemConfig.MultiplayerServer;
            }
            if(ServerPortText != null)
            {
                ServerPortText.text = SystemManager.SystemConfig.MultiplayerPort.ToString();
            }
            if (ClientNameText != null)
            {
                ClientNameText.text = SystemManager.SystemConfig.MultiplayerName;
            }
            if (StartAsServerToggle != null)
            {
                StartAsServerToggle.isOn = SystemManager.SystemConfig.DefaultToServerMode;
            }
            if (VRUpdateRate != null)
            {
                VRUpdateRate.text = SystemManager.SystemConfig.MPVRUpdateRateHz.ToString("F1");
            }
            if (ObjUpdateRate != null)
            {
                ObjUpdateRate.text = SystemManager.SystemConfig.MPObjectUpdateRateHz.ToString("F1");
            }
        }
    }

}
