using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using TMPro;


public class SceneConfiguration : MonoBehaviour
{
    private bool _isDM = false;
    private bool _isSinglePlayer = false;
    private bool _isDesktop = false;
    private bool _isSpectator = false;
    private FileInfo _debriefFile = null;
    private string _dbFilename = "";
    private int _debriefMessageCount = 0;

    public TMP_Dropdown DropdownMenu;
    public SystemManager SystemManager;
    public GameObject DebriefInfoPrefab;
    public ClearManagersOnLoad ClearManagers;
    public Toggle[] PlatformToggles;

    public bool IsDM
    {
        get { return _isDM; }
        set
        {
            _isDM = value;
        }
    }

    public bool IsSinglePlayer
    {
        get { return _isSinglePlayer; }
        set
        {
            _isSinglePlayer = value;
        }
    }

    public bool IsDesktop
    {
        get { return _isDesktop; }
        set
        {
            _isDesktop = value;
        }
    }

    public bool IsSpectator
    {
        get { return _isSpectator; }
        set
        {
            _isSpectator = value;
        }
    }

    public FileInfo DebriefFile
    {
        get { return DebriefFile; }
        set
        {
            _debriefFile = value;
        }
    }

    public string DebriefFilename
    {
        get { return _dbFilename; }
        set
        {
            _dbFilename = value;
        }
    }

    public int DebriefLength
    {
        get { return _debriefMessageCount; }
        set
        {
            _debriefMessageCount = value;
        }
    }


    private void Start()
    {
        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
        if(DropdownMenu != null)
            DropdownMenu.SetValueWithoutNotify((int)SystemManager.SystemConfig.PlatformType);
        
    }

    public void LaunchVRMine()
    {
        ClearManagers.DestroyAllManagers();
        if (_isSinglePlayer)
        {
            SceneManager.LoadScene("VRSinglePlayerTest");
            return;
        }
        if (_isDM)
        {
            SceneManager.LoadScene("DMMainScene");
            return;
        }
        else
        {
            if (_isSpectator)
            {
                SceneManager.LoadScene("SpectatorMainScene3");
            }
            else
            {
                SceneManager.LoadScene("VRMainScene");
            }
        }
    }

    public void LaunchSinglePlayer()
    {
        if (_isDesktop)
        {
            //launch desktop version
        }
        else
        {
            ClearManagers.DestroyAllManagers();
            SceneManager.LoadScene("VRSinglePlayerTest");
        }
    }

    public void LaunchSpectator()
    {
        ClearManagers.DestroyAllManagers();
        SceneManager.LoadScene("SpectatorMainScene3");
    }

    public void LaunchDebrief()
    {
        if(_debriefFile == null)
        {
            return;
        }
        ClearManagers.DestroyAllManagers();
        GameObject dbFileInfo = Instantiate(DebriefInfoPrefab);
        DontDestroyOnLoad(dbFileInfo);
        DebriefLoadInfo dbLoadInfo = dbFileInfo.GetComponent<DebriefLoadInfo>();
        dbLoadInfo.DebriefScenePath = _dbFilename;
        dbLoadInfo.NumberOfLogMessages = _debriefMessageCount;
        SceneManager.LoadScene("DebriefMainScene");
    }

    public void LaunchSimpleSceneLoader()
    {
        ClearManagers.DestroyAllManagers();
        SceneManager.LoadScene("SimpleSceneLoader");
    }

    public void SetVRModeToggle(bool isVR)
    {
        IsDesktop = !isVR;
    }

    public void UpdatePlatform()
    {
        PlatformType platformType = (PlatformType)DropdownMenu.value;
        SystemManager.SystemConfig.PlatformType = platformType;
        SystemManager.SystemConfig.SaveConfig();
    }

    public void SetPlatform(int platformtype)
    {
        //Collocation, 0
        //Standing, 1
        //Seated, 2
        //Desktop 3
        SystemManager.SystemConfig.PlatformType = (PlatformType)platformtype;
        SystemManager.SystemConfig.SaveConfig();
    }

    public void LoadPlatform()
    {
        int platform = (int)SystemManager.SystemConfig.PlatformType;
        
        if(platform >= PlatformToggles.Length)
        {
            return;
        }
        PlatformToggles[platform].isOn = true;
        PlatformToggles[platform].group.EnsureValidState();
    }
}
