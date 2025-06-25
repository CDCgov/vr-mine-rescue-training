using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ConfigSettings 
{
    public string MasterServerAddress;
    public string RoomName;
    public string PlayerName;

    public ConfigSettings()
    {
        LoadDefaultSettings();
    }

    public void LoadDefaultSettings()
    {
        MasterServerAddress = "127.0.0.1";
        RoomName = "TestRoom1";
        PlayerName = System.Environment.MachineName;
    }

    public void LoadSettings(string filename)
    {
        string json = File.ReadAllText(filename, System.Text.Encoding.UTF8);
        JsonUtility.FromJsonOverwrite(json, this);
    }

    public void SaveSettings(string filename)
    {
        string json = JsonUtility.ToJson(this);
        File.WriteAllText(filename, json, System.Text.Encoding.UTF8);
    }	

}