using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class NetSendBatteryStatus : MonoBehaviour
{
    public NetworkManager NetworkManager;

    private System.Diagnostics.Process _proc;
    private VRNBatteryStatus _vrnBattStatus;

    private Regex _whitespaceReg;
    
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        _vrnBattStatus = new VRNBatteryStatus();

        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("wmic", "path Win32_Battery get EstimatedChargeRemaining");
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        _proc = new System.Diagnostics.Process();
        _proc.StartInfo = startInfo;

        _whitespaceReg = new Regex("\\s+", RegexOptions.Multiline);

        //InvokeRepeating(nameof(SendBatteryStatus), 3.0f, 8.0f);
        StartCoroutine(SendBatteryStatus());
    }


    IEnumerator SendBatteryStatus()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(5.0f);

            //start process
            try
            {
                _proc.Start();
            }
            catch (System.Exception) 
            {                
                continue;
            }

            //give process time to run
            yield return new WaitForSecondsRealtime(3.0f);


            //collect results
            try
            { 
                var output = _proc.StandardOutput.ReadToEnd();

                //output = output.Replace("\n", "");
                output = output.Replace("EstimatedChargeRemaining", "");
                output = _whitespaceReg.Replace(output, " ");
                output = output.Trim();

                _vrnBattStatus.ClientID = NetworkManager.ClientID;
                _vrnBattStatus.StatusMessage = output;

                NetworkManager.SendNetMessage(VRNPacketType.SendBatteryStatus, _vrnBattStatus, broadcast: false);
                Debug.Log($"Sent battery status {_vrnBattStatus.StatusMessage}");
            }
            catch (System.Exception) { }
        }
    }
}
