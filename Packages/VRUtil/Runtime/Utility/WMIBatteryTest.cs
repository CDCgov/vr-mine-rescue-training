using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Management;


public class WMIBatteryTest : MonoBehaviour
{
    private System.Diagnostics.Process _proc;

    // Start is called before the first frame update
    void Start()
    {
        string cmd = "wmic path Win32_Battery get EstimatedChargeRemaining";
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo("wmic", "path Win32_Battery get EstimatedChargeRemaining");
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        _proc = new System.Diagnostics.Process();
        _proc.StartInfo = startInfo;


        InvokeRepeating(nameof(CheckBattery), 0, 2.0f);
    }

    void CheckBattery()
    {
        _proc.Start();
        var output = _proc.StandardOutput.ReadToEnd();

        output = output.Replace("\n", "");
        output = output.Replace("EstimatedChargeRemaining", "");

        Debug.Log(output);

    }

}
