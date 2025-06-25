using UnityEngine;
using System.Collections;

public class PlayerLogHandle : LogHandle {

    private PlayerData _PlayerData;
    
    public int PlayerID;
    public LogManager LogManagerRef;
    // Use this for initialization
    void Start () {		
        _PlayerData = new PlayerData();
        _PlayerData.PlayerID = PlayerID;		

        if(LogManagerRef == null)
        {
            LogManagerRef = FindObjectOfType(typeof(LogManager)) as LogManager;
            if(LogManagerRef == null)
            {
                Debug.LogError("You must have a LogManager attached to a GameObject in your scene. : " + gameObject.name);
                Application.Quit();
            }
        }
    }
    
    // Update is called once per frame
    void Update () {
        _PlayerData.Position = transform.position;
        _PlayerData.Rotation = transform.rotation;
        _PlayerData.TimeStamp = (double)Time.time;
        LogManagerRef.AddPacketToQueue(_PlayerData);
    }

    //Only called in Editor mode when the Reset context menu item is selected on a component or when a script is first attached to a game object
    void Reset()
    {
        LogManagerRef = FindObjectOfType(typeof(LogManager)) as LogManager;
        if (LogManagerRef == null)
        {
            GameObject logManager = new GameObject();
            logManager.transform.position = new Vector3(0, 0, 0);
            logManager.name = "Log Manager";
            LogManager lmClass = logManager.AddComponent<LogManager>();
            LogManagerRef = lmClass;
            Debug.LogWarning("You must have a LogManager attached to a GameObject in your scene. One has been generated into your scene.");
        }
        //This is a safeguard to ensure that there are no duplicate ID numbers, the ID numbers can be overridden by the user manuall in the editor
        int highestIndex = 0;
        PlayerLogHandle[] pHandles = GameObject.FindObjectsOfType<PlayerLogHandle>();
        foreach (PlayerLogHandle item in pHandles)
        {
            if (item.PlayerID > highestIndex)
            {
                highestIndex = item.PlayerID;
            }
        }
        PlayerID = highestIndex + 1;
    }
}
