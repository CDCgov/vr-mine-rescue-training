using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class OptionsMenuController : MonoBehaviour 
{
    public InputField PlayerNameInput;
    public InputField ServerAddressInput;
    public InputField RoomNameInput;


    public void OnSaveSettings()
    {
        MasterControl.Settings.PlayerName = PlayerNameInput.text;
        MasterControl.Settings.MasterServerAddress = ServerAddressInput.text;
        MasterControl.Settings.RoomName = RoomNameInput.text;
        
        MasterControl.SaveSettings();

        MasterControl.ShowOptionsMenu(false);
        MasterControl.ShowMainMenu(true);
    }

    public void LoadSettings()
    {
        PlayerNameInput.text = MasterControl.Settings.PlayerName;
        ServerAddressInput.text = MasterControl.Settings.MasterServerAddress;
        RoomNameInput.text = MasterControl.Settings.RoomName;
    }

    void Start () 
    {
        
    }
    
    void Update () 
    {
    
    }
}