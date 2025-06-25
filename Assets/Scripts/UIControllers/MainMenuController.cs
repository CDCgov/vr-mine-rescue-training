using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour 
{

    public GameObject RootMenu;
    public GameObject MultiplayerMenu;

    public Text StatusText;
    public InputField RoomNameInput;
    public InputField MasterServerAddressInput;
    public Transform LogMessageParent;

    public Button ConnectToCloudBtn;
    public Button ConnectToMasterBtn;
    public Button JoinRoomBtn;
    public Button JoinAsPlayerBtn;
    public Button JoinAsResearcherBtn;
    public Button JoinAs360Btn;

    //quick hack to connect to the server & room
    private bool _connectMultiplayer = false;


    void Start () 
    {	
        //register to receieve network state change events
        //NetworkManager.NetworkStateChanged += OnNetworkStateChanged;

        //OnNetworkStateChanged();
    }	

    void Update()
    {
        /*
        float aspect = (float)Screen.width / (float)Screen.height;
        
        if (aspect > 3)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }*/

        /*

        if (_connectMultiplayer)
        {
            if (NetworkManager.ConnectionReady)
            {
                StatusText.text = "Connected";
                _connectMultiplayer = false;
            }
            else if (NetworkManager.ConnectedToMaster && !NetworkManager.JoiningRoom && !NetworkManager.JoinedRoom)
            {
                //join room
                NetworkManager.ConnectToRoom(MasterControl.Settings.RoomName);
                StatusText.text = "Joining Room " + MasterControl.Settings.RoomName;
            }
            else if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
            {
                //connect to master
                if (MasterControl.Settings.MasterServerAddress.Equals("cloud", System.StringComparison.InvariantCultureIgnoreCase))
                {
                    //connect to cloud
                    //NetworkManager.ConnectToMaster(MasterControl.Settings.MasterServerAddress);
                    NetworkManager.ConnectToCloud();
                    StatusText.text = "Connecting to cloud";
                }
                else
                {
                    NetworkManager.ConnectToMaster(MasterControl.Settings.MasterServerAddress);
                    StatusText.text = "Connecting to " + MasterControl.Settings.MasterServerAddress;
                }
            }
        }
        */
    }

    private void OnNetworkStateChanged()
    {
        //enable/disable buttons
        //ConnectToCloudBtn.interactable = !NetworkManager.ConnectedToMaster;
        //ConnectToMasterBtn.interactable = !NetworkManager.ConnectedToMaster;
        //JoinRoomBtn.interactable = NetworkManager.ConnectedToMaster && !NetworkManager.JoinedRoom;
        //JoinAsPlayerBtn.interactable = NetworkManager.JoinedRoom;
        //JoinAsResearcherBtn.interactable = NetworkManager.JoinedRoom;
        //JoinAs360Btn.interactable = NetworkManager.JoinedRoom;
    }

    public void SetStatus(string text, params object[] parameters)
    {
        string msg = string.Format(text, parameters);
        StatusText.text = msg;
    }

    public void AddLogMessage(string text, params object[] parameters)
    {
        string msg = string.Format(text, parameters);

        GameObject newLog = new GameObject();
        Text txt = newLog.AddComponent<Text>();
        txt.color = Color.white;
        txt.text = msg;
        txt.font = StatusText.font;

        txt.transform.SetParent(LogMessageParent, false);
    }

    public void OnSinglePlayer()
    {
        //RootMenu.SetActive(false);

        MasterControl.ShowMainMenu(false);

        MasterControl.ShowChooseMine(true, (string mapName) =>
        {
            MasterControl.LoadSceneSinglePlayer(mapName);
            MasterControl.ShowChooseMine(false, null);
        });

        //MasterControl.LoadSceneSinglePlayer("TestMine1");
    }

    public void OnSinglePlayerResearcher()
    {
        MasterControl.ShowMainMenu(false);

        MasterControl.ShowChooseMine(true, (string mapName) =>
        {
            MasterControl.LoadSceneSinglePlayerResearcher(mapName);
            MasterControl.ShowChooseMine(false, null);
        });
    }

    public void OnSinglePlayer360()
    {
        MasterControl.ShowMainMenu(false);

        MasterControl.ShowChooseMine(true, (string mapName) =>
        {
            MasterControl.LoadScene(mapName, ClientRole.MultiUser);
            MasterControl.ShowChooseMine(false, null);
        });
    }

    public void OnMultiplayer()
    {
        _connectMultiplayer = true;
        StatusText.text = "";
        RootMenu.SetActive(false);
        MultiplayerMenu.SetActive(true);
    }

    public void OnTestButtonClicked()
    {
        Debug.Log("Test button clicked");

        MasterControl.LoadScene("TestMine1");
    }

    public void OnConnectToCloud()
    {
        //NetworkManager.ConnectToCloud();
    }

    public void OnConnectToMaster()
    {
        //NetworkManager.ConnectToMaster(MasterServerAddressInput.text);
    }

    public void OnJoinRoom()
    {
        //NetworkManager.ConnectToRoom(RoomNameInput.text);
    }

    public void OnJoinAsPlayer()
    {
        MasterControl.SetClientRole(ClientRole.Player);
    }

    public void OnJoinAsMultiUser()
    {
        MasterControl.SetClientRole(ClientRole.MultiUser);
    }

    public void OnJoinAsResearcher()
    {
        MasterControl.SetClientRole(ClientRole.Researcher);
    }

    public void OnOptionsMenu()
    {
        MasterControl.ShowMainMenu(false);
        MasterControl.ShowOptionsMenu(true);
    }
}