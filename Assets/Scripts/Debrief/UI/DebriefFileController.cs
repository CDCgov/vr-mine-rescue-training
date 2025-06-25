using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using SFB;

enum ScenarioSortMethod
{
    Filename,
    SessionName,
    ScenarioName,
    DateTime,
    Duration,
    FilenameReverse,
    SessionNameReverse,
    ScenarioNameReverse,
    DateTimeReverse,
    DurationReverse
}
public class DebriefFileController : MonoBehaviour
{
    public SystemManager SystemManager;
    public string DebriefPath;
    public GameObject ContentBox;
    public GameObject DebiefFileButtonPrefab;
    public GameObject DebriefLabelPrefab;
    public GameObject SelectedFileLabel;
    public GameObject DebriefSelectorPrefab;
    public GameObject DeleteConfirmationPanel;
    public GameObject RenameDialogPanel;
    public GameObject LaunchButton;
    public SceneConfiguration SceneConfig;
    public UnityEvent SaveFolderChange;
    public TMP_InputField RenameInputField;
    public TextMeshProUGUI PlaceholderFilename;
    public TextMeshProUGUI RenameLabel;
    public ScrollRect FileScrollContainer;
    public Transform FileNameSortDirection;
    public Transform SessionNameSortDirection;
    public Transform ScenarioSortDirection;
    public Transform DurationSortDirection;
    public Transform DateTimeSortDirection;

    private string _selectedFileName;
    private FileInfo _selectedFileInfo;
    private TextMeshProUGUI _priorSelectedText;
    private float _fileButtonVertPosition = 40;
    private List<TextMeshProUGUI> _priorSelectedTexts;
    private List<GameObject> _spawnedDebiefObjects;
    private bool _loaded = false;
    private string _SaveDataPath = null;
    private ScenarioSortMethod ScenarioSortMethod;
    private DebriefSelectionHandler _priorSelectedHandler;


    // Start is called before the first frame update
    void Start()
    {
        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }
        if (!_loaded)
        {
            _priorSelectedTexts = new List<TextMeshProUGUI>();
            _spawnedDebiefObjects = new List<GameObject>();

            ScenarioSortMethod = ScenarioSortMethod.DateTime;
            PopulateContentBox();
            _loaded = true;
        }        
    }
    

    void PopulateContentBox()
    {
        if (_SaveDataPath == null)
        {
            //_SaveDataPath = Application.dataPath + "/../" + DebriefPath;
            
            //if (SystemManager.SystemConfig.DebriefFilePathOverride != null)
            //{
            //    SystemManager.SystemConfig.DebriefFilePathOverride = SystemConfig.GetDefaultSessionLogPath();
            //    _SaveDataPath = SystemManager.SystemConfig.DebriefFilePathOverride;
            //}
            //else
            //{
            //    //string savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "VRMineSessionLogs");
            //    string savePath = SystemConfig.GetDefaultSessionLogPath();
            //    SystemManager.SystemConfig.DebriefFilePathOverride = savePath;
            //    SystemManager.SystemConfig.SaveConfig();
            //    _SaveDataPath = savePath;
            //}

            if (!string.IsNullOrEmpty(SystemManager.SystemConfig.DebriefFilePathOverride) && 
                File.Exists(SystemManager.SystemConfig.DebriefFilePathOverride))
            {
                _SaveDataPath = SystemManager.SystemConfig.DebriefFilePathOverride;
            }
            else
            {
                _SaveDataPath = SystemManager.SystemConfig.SessionLogsFolder;
            }

            SaveFolderChange?.Invoke();

        }
        
        //string[] files = Directory.GetFiles(path);
        DirectoryInfo info = new DirectoryInfo(_SaveDataPath);
        try
        {
            if (!info.Exists)
            {
                Directory.CreateDirectory(_SaveDataPath);
            }
            List<SessionLog.SessionLogMetadata> sessionMeta = SessionLog.ScanFolder(_SaveDataPath);
            //FileInfo[] files = info.GetFiles().OrderByDescending(p => p.CreationTime).ToArray();
            //foreach (FileInfo file in files)
            List<SessionLog.SessionLogMetadata> sortedMetaData = new List<SessionLog.SessionLogMetadata>();
            switch (ScenarioSortMethod)
            {
                case ScenarioSortMethod.Filename:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderBy(l => l.Filename))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.SessionName:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderBy(l => l.SessionName))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.ScenarioName:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderBy(l => l.SceneName))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.DateTime:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderByDescending(l => l.LogStartTime))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.Duration:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderByDescending(l => l.Duration))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.FilenameReverse:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderByDescending(l => l.Filename))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.SessionNameReverse:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderByDescending(l => l.SessionName))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.ScenarioNameReverse:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderByDescending(l => l.SceneName))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.DateTimeReverse:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderBy(l => l.LogStartTime))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                case ScenarioSortMethod.DurationReverse:
                    foreach (SessionLog.SessionLogMetadata meta in sessionMeta.OrderBy(l => l.Duration))
                    {
                        sortedMetaData.Add(meta);
                    }
                    break;
                default:
                    break;
            }
            
            foreach(SessionLog.SessionLogMetadata session in sortedMetaData)
            {
                // DO Something...

                //string[] delimited = Path.GetFileNameWithoutExtension(file.Name).Split('_');
                //if(delimited.Length < 4)
                //{
                //    continue;
                //}

                //RectTransform buttonTransform = debiefButton.GetComponent<RectTransform>();
                //Vector3 pos = buttonTransform.anchoredPosition3D;
                //pos.y = _fileButtonVertPosition - 40;
                //_fileButtonVertPosition = pos.y;
                //buttonTransform.anchoredPosition3D = pos;
                GameObject selector = Instantiate(DebriefSelectorPrefab, ContentBox.transform);
                DebriefSelectionHandler handle = selector.GetComponent<DebriefSelectionHandler>();
                
                //GameObject labelFN = Instantiate(DebriefLabelPrefab, ContentBox.transform);
                //GameObject label0 = Instantiate(DebriefLabelPrefab, ContentBox.transform);
                //GameObject label1 = Instantiate(DebriefLabelPrefab, ContentBox.transform);
                //GameObject label2 = Instantiate(DebriefLabelPrefab, ContentBox.transform);
                //GameObject label3 = Instantiate(DebriefLabelPrefab, ContentBox.transform);
                //GameObject debiefButton = GameObject.Instantiate(DebiefFileButtonPrefab, ContentBox.transform);

                //_spawnedDebiefObjects.Add(selector);
                //_spawnedDebiefObjects.Add(labelFN);
                //_spawnedDebiefObjects.Add(label0);
                //_spawnedDebiefObjects.Add(label1);
                //_spawnedDebiefObjects.Add(label2);
                //_spawnedDebiefObjects.Add(label3);
                //_spawnedDebiefObjects.Add(debiefButton);
                //TextMeshProUGUI label = debiefButton.GetComponentInChildren<TextMeshProUGUI>();
                _spawnedDebiefObjects.Add(selector);
                
                //TextMeshProUGUI lFN = labelFN.GetComponent<TextMeshProUGUI>();
                //TextMeshProUGUI l0 = label0.GetComponent<TextMeshProUGUI>();//.text = delimited[0]
                //TextMeshProUGUI l1 = label1.GetComponent<TextMeshProUGUI>();
                //TextMeshProUGUI l2 = label2.GetComponent<TextMeshProUGUI>();
                //TextMeshProUGUI l3 = label3.GetComponent<TextMeshProUGUI>();
                List<TextMeshProUGUI> labelList = new List<TextMeshProUGUI>();
                //l0.text = delimited[0];
                //l1.text = delimited[1];
                //l2.text = delimited[2];
                //l3.text = delimited[3];
                //lFN.text = session.Filename;
                //l0                                                                                                                                                                                                                                                          .text = session.SessionName;
                //l1.text = session.SceneName;
                //l2.text = session.LogStartTime.Value.ToString("d");
                //l3.text = session.LogStartTime.Value.ToString("t");

                var sceneValid = SceneLoadManager.IsSceneNameValid(session.SceneName);

                if (!sceneValid)
                {
                    handle.FileNameLabel.text = $"<color=\"red\">{session.Filename}</color>";
                }
                else
                {
                    handle.FileNameLabel.text = session.Filename;
                }

                
                if (handle.FileNameLabel.TryGetComponent<MenuTooltip>(out var fileName))
                {
                    fileName.SetTooltipText(session.Filename);
                }
                handle.SessionNameLabel.text = session.SessionName;
                if (handle.SessionNameLabel.TryGetComponent<MenuTooltip>(out var sessionName))
                {
                    sessionName.SetTooltipText(session.SessionName);
                }
                string output = session.SceneName.Replace("CustomScenario:", "");
                output = output.Replace(".json", "");
                handle.ScenarioNameLabel.text = output;
                if (handle.ScenarioNameLabel.TryGetComponent<MenuTooltip>(out var sceneName))
                {
                    sceneName.SetTooltipText(output);
                }
                TimeSpan t = TimeSpan.FromSeconds(session.Duration);
                //handle.DurationLabel.text = $"{t.Hours.ToString("00")}:{t.Minutes.ToString("00")}:{t.Minutes.ToString("00")}";
                handle.DurationLabel.text = t.ToString("hh':'mm':'ss");                
                if (session.LogStartTime != null)
                {
                    handle.DateLabel.text = session.LogStartTime.Value.ToString("d");
                    handle.TimeLabel.text = session.LogStartTime.Value.ToString("t");
                }
                handle.SelectedLength = session.NumMessages;
                //Debug.Log("Num Messages: " + session.NumMessages);
                //handle.ScenarioSelected.AddListener(handle);

                if (sceneValid)
                {
                    Button button = handle.SelectButton;
                    Button l0Btn = handle.FileNameLabel.gameObject.AddComponent<Button>();
                    Button l1Btn = handle.SessionNameLabel.gameObject.AddComponent<Button>();
                    Button l2Btn = handle.ScenarioNameLabel.gameObject.AddComponent<Button>();
                    Button l3Btn = handle.DateLabel.gameObject.AddComponent<Button>();
                    Button lFNBtn = handle.TimeLabel.gameObject.AddComponent<Button>();
                    button.onClick.AddListener(() => SelectFile(session.Filename, labelList, handle.SelectedLength, handle));
                    l0Btn.onClick.AddListener(() => SelectFile(session.Filename, labelList, handle.SelectedLength, handle));
                    l1Btn.onClick.AddListener(() => SelectFile(session.Filename, labelList, handle.SelectedLength, handle));
                    l2Btn.onClick.AddListener(() => SelectFile(session.Filename, labelList, handle.SelectedLength, handle));
                    l3Btn.onClick.AddListener(() => SelectFile(session.Filename, labelList, handle.SelectedLength, handle));
                    lFNBtn.onClick.AddListener(() => SelectFile(session.Filename, labelList, handle.SelectedLength, handle));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load directory at {_SaveDataPath}, exception thrown: {e.ToString()}");
        }
        //foreach (string file in files)
        //{
        //    GameObject debiefButton = GameObject.Instantiate(DebiefFileButtonPrefab, ContentBox.transform);
        //    TextMeshProUGUI label = debiefButton.GetComponentInChildren<TextMeshProUGUI>();
        //    string[] delimited = file.Split('_');
        //}
    }

    private void ScenarioButtonPressed(DebriefSelectionHandler handler)
    {

    }

    public void SelectFile(string val, FileInfo selFileInfo, List<TextMeshProUGUI> buttonText)
    {
        if (!SelectedFileLabel.activeSelf)
        {
            SelectedFileLabel.SetActive(true);
        }
        _selectedFileName = val;
        _selectedFileInfo = selFileInfo;
        SceneConfig.DebriefFile = selFileInfo;
        SelectedFileLabel.GetComponent<TextMeshProUGUI>().text = $"Selected file: {_selectedFileName}";
        foreach (TextMeshProUGUI content in buttonText)
        {
            content.color = Color.yellow;
        }
        if (_priorSelectedTexts != null && buttonText != _priorSelectedTexts)
        {
            foreach (TextMeshProUGUI content in _priorSelectedTexts)
            {
                content.color = Color.white;
            }
        }
        _priorSelectedTexts = buttonText;
        Debug.Log(val);
    }

    public void SelectFile(string selectedPath, List<TextMeshProUGUI> buttonText, int length, DebriefSelectionHandler debriefSelectionHandler)
    {
        if (!SelectedFileLabel.activeSelf)
        {
            SelectedFileLabel.SetActive(true);
        }
        _selectedFileInfo = new FileInfo(selectedPath);
        _selectedFileName = selectedPath;
        if(SceneConfig == null)
        {
            SceneConfig = new SceneConfiguration();
        }
        SceneConfig.DebriefFile = _selectedFileInfo;
        SceneConfig.DebriefLength = length;
        SceneConfig.DebriefFilename = Path.Combine(_SaveDataPath, selectedPath);
        SelectedFileLabel.GetComponent<TextMeshProUGUI>().text = $"Selected file: {_selectedFileName}";
        if (_priorSelectedHandler != null)
        {
            _priorSelectedHandler.UnSelected();
        }
        debriefSelectionHandler.Selected();        
        _priorSelectedHandler = debriefSelectionHandler;
        foreach (TextMeshProUGUI content in buttonText)
        {
            content.color = Color.yellow;
        }
        if (_priorSelectedTexts != null && buttonText != _priorSelectedTexts)
        {
            foreach (TextMeshProUGUI content in _priorSelectedTexts)
            {
                content.color = Color.white;
            }
        }
        _priorSelectedTexts = buttonText;
        Debug.Log(Path.Combine(_SaveDataPath,selectedPath) + ", " + length);
        if(LaunchButton != null)
        {
            if (!LaunchButton.activeSelf)
            {
                LaunchButton.SetActive(true);
            }
        }
    }

    public void DeleteFile()
    {
        if(_selectedFileInfo == null)
        {
            CloseDeleteDialog();
            return;
        }
        File.Delete(_selectedFileInfo.FullName);
        RefreshDebriefContent();
        CloseDeleteDialog();
    }

    public void OpenDeleteDialog()
    {
        DeleteConfirmationPanel.SetActive(true);
    }

    public void CloseDeleteDialog()
    {
        DeleteConfirmationPanel.SetActive(false);
    }

    public void SelectFolderDialog()
    {
        var path = StandaloneFileBrowser.OpenFolderPanel("Select save file folder", _SaveDataPath, false);
        if(path.Length > 0)
        {
            _SaveDataPath = path[0];
            SystemManager.SystemConfig.DebriefFilePathOverride = _SaveDataPath;
            //SystemManager.SystemConfig.SaveConfig();
            SaveFolderChange?.Invoke();
            RefreshDebriefContent();
        }
    }

    public void SelectFolderSettingsMenu()
    {
        var path = StandaloneFileBrowser.OpenFolderPanel("Select save file folder", _SaveDataPath, false);
        if (path.Length > 0)
        {
            _SaveDataPath = path[0];
            SystemManager.SystemConfig.DebriefFilePathOverride = _SaveDataPath;
            //SystemManager.SystemConfig.SaveConfig();
            SaveFolderChange?.Invoke();
        }
    }

    public string GetSaveFolder()
    {
        return _SaveDataPath;
        //if(SystemManager == null)
        //{
        //    SystemManager = SystemManager.GetDefault();
        //}
        //if (SystemManager == null)
        //    Debug.Log("sys manager was null!");
        //return SystemManager.SystemConfig.DebriefFilePathOverride;
    }

    public void RenameDialogEnable()
    {
        PlaceholderFilename.text = Path.GetFileNameWithoutExtension(_selectedFileName);
        RenameLabel.text = "Rename";
        RenameInputField.text = "";
        RenameDialogPanel.SetActive(true);
    }

    public void CloseRenameDialog()
    {
        RenameDialogPanel.SetActive(false);
    }

    public void ConfirmRename()
    {
        if(RenameInputField.text != "" && !RenameInputField.text.Any(Path.GetInvalidPathChars().Contains) && !RenameInputField.text.Contains("\\"))
        {
            //Debug.Log(Path.Combine(_SaveDataPath, selectedPath));
            string name = $"{RenameInputField.text}.vrminelog";
            if (!File.Exists(Path.Combine(_SaveDataPath, name)))
            {
                File.Move(Path.Combine(_SaveDataPath, _selectedFileName), Path.Combine(_SaveDataPath, name));
                //File.Delete(Path.Combine(_SaveDataPath, _selectedFileName));
                _selectedFileName = name;
                RefreshDebriefContent();
                CloseRenameDialog();
            }
            else
            {
                RenameLabel.text = "A save file with that name already exists.";
            }
        }
        else
        {
            RenameLabel.text = "Please enter a valid name.";
        }
    }

    void RefreshDebriefContent()
    {
        foreach (GameObject spawnedObject in _spawnedDebiefObjects)
        {
            Destroy(spawnedObject);
        }
        SelectedFileLabel.SetActive(false);
        if (_priorSelectedTexts != null)
        {
            foreach (TextMeshProUGUI content in _priorSelectedTexts)
            {
                content.color = Color.white;
            }
        }
        _spawnedDebiefObjects.Clear();
        _selectedFileName = "";
        _selectedFileInfo = null;
        if(SceneConfig != null)
            SceneConfig.DebriefFile = null;

        if(LaunchButton != null)
            LaunchButton.SetActive(false);
        PopulateContentBox();
    }

    private void OnDisable()
    {
        //foreach(GameObject spawnedObject in _spawnedDebiefObjects)
        //{
        //    Destroy(spawnedObject);
        //}
        SelectedFileLabel.SetActive(false);
        if (_priorSelectedTexts != null)
        {
            foreach (TextMeshProUGUI content in _priorSelectedTexts)
            {
                content.color = Color.white;
            }
        }
        //_spawnedDebiefObjects.Clear();
        _selectedFileName = "";
        _selectedFileInfo = null;
        if (SceneConfig != null)
        {
            SceneConfig.DebriefFile = null;
        }
    }


    private void OnEnable()
    {
        if(_loaded)
            RefreshDebriefContent();
    }

    public void SetFileNameSort()
    {
        if (ScenarioSortMethod == ScenarioSortMethod.Filename)
        {
            ScenarioSortMethod = ScenarioSortMethod.FilenameReverse;
            FileNameSortDirection.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else
        {
            ScenarioSortMethod = ScenarioSortMethod.Filename;
            FileNameSortDirection.transform.localEulerAngles = Vector3.zero;
        }
        RefreshDebriefContent();
    }

    public void SetSessionNameSort()
    {
        if (ScenarioSortMethod == ScenarioSortMethod.SessionName)
        {
            ScenarioSortMethod=ScenarioSortMethod.SessionNameReverse;
            SessionNameSortDirection.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else
        {
            ScenarioSortMethod = ScenarioSortMethod.SessionName;
            SessionNameSortDirection.transform.localEulerAngles = Vector3.zero;
        }
        RefreshDebriefContent();
    }

    public void SetSceneNameSort() 
    {
        if(ScenarioSortMethod == ScenarioSortMethod.ScenarioName)
        {
            ScenarioSortMethod=ScenarioSortMethod.ScenarioNameReverse;
            ScenarioSortDirection.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else
        {
            ScenarioSortMethod = ScenarioSortMethod.ScenarioName;
            ScenarioSortDirection.transform.localEulerAngles = Vector3.zero;
        }
        RefreshDebriefContent();
    }

    public void SetDateTimeSort()
    {
        if(ScenarioSortMethod == ScenarioSortMethod.DateTime)
        {
            ScenarioSortMethod=ScenarioSortMethod.DateTimeReverse;
            DateTimeSortDirection.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else
        {
            ScenarioSortMethod = ScenarioSortMethod.DateTime;
            DateTimeSortDirection.transform.localEulerAngles = Vector3.zero;
        }
        RefreshDebriefContent();
    }

    public void SetDurationSort()
    {
        if (ScenarioSortMethod == ScenarioSortMethod.Duration)
        {
            ScenarioSortMethod = ScenarioSortMethod.DurationReverse;
            DurationSortDirection.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
        else
        {
            ScenarioSortMethod = ScenarioSortMethod.Duration;
            DurationSortDirection.transform.localEulerAngles = Vector3.zero;
        }
        RefreshDebriefContent();
    }

    public void DirectLoad()
    {
        //if (_debriefFile == null)
        //{
        //    return;
        //}
        //GameObject dbFileInfo = FindObjectOfType<(DebriefInfoPrefab);
        //DontDestroyOnLoad(dbFileInfo);
        DebriefLoadInfo dbLoadInfo = FindObjectOfType<DebriefLoadInfo>();
        if (SceneConfig == null || SceneConfig.DebriefFilename == null)
        {
            if(SceneConfig == null)
            {
                Debug.LogError("Scene config was null");
                return;
            }
            if (SceneConfig.DebriefFilename == null)
            {
                Debug.LogError("Debrief file name was null");
            }
            return;
        }
        dbLoadInfo.DebriefScenePath = SceneConfig.DebriefFilename;
        dbLoadInfo.NumberOfLogMessages = SceneConfig.DebriefLength;
        
        DebriefSceneLoader debriefSceneLoader = FindObjectOfType<DebriefSceneLoader>();
        debriefSceneLoader.LoadNewScene();
    }
}
