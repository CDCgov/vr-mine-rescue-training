using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class GitVersion : MonoBehaviour
{
    public static string Version = "unknown";
    public static void UpdateVersion()
    {
        try
        {
            ProcessStartInfo info = new ProcessStartInfo("git.exe", "describe --abbrev=5 --always --dirty=-MODIFIED");
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            using (Process p = Process.Start(info))
            {
                if (p.WaitForExit(500))
                {
                    string version = p.StandardOutput.ReadToEnd().Trim();
                    Version = version;
                }
                else
                {
                    return;
                }
            }

            if (Version.Contains("-g"))
            {
                Version = Version + "\n" + System.DateTime.Now.ToString();
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(e.ToString());
        }
    }

    public static void SaveVersion()
    {
        string fileName = Application.streamingAssetsPath + "/version.txt";
        if (File.Exists(fileName))
        {
            string version = File.ReadAllText(fileName);
            if (version == Version)
            {
                return; //No need to update, they are identical.
            }

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(Version);
                writer.Close();
            }
        }
        else
        {
            using (StreamWriter writer = File.CreateText(fileName))
            {
                writer.Write(Version);
                writer.Close();
            }
        }
    }

    public static void ReadVersion()
    {
        string fileName = Application.streamingAssetsPath + "/version.txt";
        if (File.Exists(fileName))
        {
            Version = File.ReadAllText(fileName);
        }
    }
}