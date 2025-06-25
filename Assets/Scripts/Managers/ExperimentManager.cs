using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Collections;
using System.Collections.Generic;

[HasCommandConsoleCommands]
[CreateAssetMenu(fileName = "ExperimentManager", menuName = "VRMine/Managers/ExperimentManager", order = 0)]
public class ExperimentManager : Manager
{
    public const string DefaultResourcePath = "Managers/ExperimentManager";
    public static ExperimentManager GetDefault() { return Resources.Load<ExperimentManager>(DefaultResourcePath); }

    public bool TerminateAfterExperimentComplete = false;

    public List<Experiment> Experiments;

    public UnityEvent TestEvent;
    public event UnityAction<Experiment> ExperimentStarting;
    public event UnityAction<Experiment> ExperimentCompleted;
    public event UnityAction ExperimentTerminated;

    public bool IsExperimentRunning
    {
        get
        {
            return _experimentRunning;
        }
    }

    public int CurrentTrial
    {
        get
        {
            if (!_experimentRunning)
                return -1;
            else
                return _trialCount;
        }
    }

    public Experiment CurrentExperiment
    {
        get
        {
            return _currentExperiment;
        }
    }

    public ExperimentConfig CurrentExperimentConfig
    {
        get
        {
            return _currentConfig;
        }
    }

    private Experiment _experimentPrefab;
    private Experiment _currentExperiment;
    private ExperimentConfig _currentConfig;
    private Dictionary<string, Experiment.ExperimentVal> _trialSettings;

    private bool _experimentRunning = false;
    private ExperimentRunner _runner;

    private int _trialCount;
    private int _totalTrialCount;
    private string _sessionName;

    private TrialBlock _block;
    private int _blockTrialCount;
    private int _blockTotalTrialCount;
    private int _blockIndex;
    private int _numBlockVars;
    private int[] _blockValCount;
    private int[] _blockValIndices;
    private BlockVariable[] _blockVars;

    [CommandConsoleCommand("exp_test_config", "test experiment config loading")]
    public static void CCExpTestConfig()
    {

    }

    [CommandConsoleCommand("exp_load", "load an experiment")]
    public static void CCLoadExperiment(string expName)
    {
        var expManager = GetDefault();

        try
        {
            MasterControl.ShowMainMenu(false);
            CommandConsole.ShowCommandConsole(false);
        }
        catch (System.Exception)
        {

        }

        expManager.StartExperiment(expName, $"{expName}-{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}");
    }

    [CommandConsoleCommand("exp_list", "list experiment configs")]
    public static void CCListExperiments()
    {
        string folder = GetExpConfigFolder();

        //CommandConsole.Print(folder);

        var files = Directory.GetFiles(folder, "*.yaml");

        CommandConsole.Print($"{files.Length} Availabe Experiments:");

        foreach (var file in files)
        {
            //CommandConsole.Print(file);
            CommandConsole.Print(Path.GetFileNameWithoutExtension(file));
        }
    }

    [CommandConsoleCommand("exp_show_log_folder", "display the folder experiment logs are written to")]
    public static void CCShowLogFolder()
    {
        var expManager = GetDefault();

        string folder = expManager.GetLogFolder();

        CommandConsole.Print($"Log folder: {folder}");
    }

    public static string GetExpConfigFolder()
    {
        string path = Application.dataPath;
        DirectoryInfo dir = Directory.GetParent(path);
        path = Path.Combine(dir.FullName, "ExperimentConfig");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public string GetLogFolder()
    {
        string path = Application.dataPath;
        DirectoryInfo dir = Directory.GetParent(path);
        path = Path.Combine(dir.FullName, "ExperimentLogs");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    public int CurrentExperimentTrialCount
    {
        get
        {
            return _totalTrialCount;
        }
    }


    private void OnEnable()
    {
    }

    private void OnDisable()
    {

    }

    public Experiment FindExperimentPrefab(string expType)
    {
        var expManager = GetDefault();

        foreach (var exp in expManager.Experiments)
        {
            if (exp.name == expType)
            {
                return exp;
            }
            //CommandConsole.Print($"Experiment: {exp.name}");
        }

        throw new System.Exception($"Invalid experiment type {expType}");
    }

    public void StartExperiment(string configName, string sessionName)
    {
        string folder = GetExpConfigFolder();
        var files = Directory.GetFiles(folder, "*.yaml");

        foreach (var file in files)
        {
            if (file.Contains(configName))
            {
                var config = ExperimentConfig.LoadConfigFile(file);
                StartExperiment(config, sessionName);
                return;
            }
        }

        throw new System.Exception($"Couldn't find experiment {configName}");
    }

    public void StartExperiment(ExperimentConfig config, string sessionName)
    {
        var experiment = FindExperimentPrefab(config.ExperimentType);
        StartExperiment(experiment, config, sessionName);
    }

    public void StartExperiment(Experiment experiment, ExperimentConfig config, string sessionName)
    {
        try
        {
            _experimentPrefab = experiment;
            _currentConfig = config;

            foreach (var val in _currentConfig.StaticValues)
            {
                if (val.Value.NumValues != 1)
                {
                    throw new System.Exception("Error: only single value entries allowed in static experiment config");
                }
            }

            if (_currentConfig.TrialBlocks == null || _currentConfig.TrialBlocks.Count <= 0)
            {
                throw new System.Exception("Error: experiment config has no trial blocks");
            }

            _trialCount = 0;
            _totalTrialCount = config.ComputeNumTrials();
            _sessionName = sessionName;
            _runner = null;
            _experimentRunning = true;

            BeginExperimentBlock(0);
            StartTrial();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
            TerminateExperiment(ex.Message);
        }
    }

    public void TerminateExperiment(string errorMessage = null)
    {
        _experimentRunning = false;

        if (_runner != null)
        {
            _runner.gameObject.SetActive(false);
            Destroy(_runner.gameObject);
            _runner = null;

            if (_currentExperiment != null)
                _currentExperiment.FinalizeExperiment();
        }

        Physics.autoSimulation = true;

        Debug.LogWarning($"Experiment Terminated, reason: {errorMessage}");
    }

    public void TrialCompleted(GameObject runner)
    {
        runner.SetActive(false);
        Destroy(runner);
        _runner = null;

        RaiseExperimentCompleted(_currentExperiment);

        //check for termination
        if (!_experimentRunning)
            return;

        if (PrepareNextTrial())
        {
            StartTrial();
        }
        else
        {
            //experiment complete
            _experimentRunning = false;
            Physics.autoSimulation = true;

            if (TerminateAfterExperimentComplete)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                #endif
            }
        }
    }

    public void SkipCurrentTrial()
    {
        if (!_experimentRunning || _runner == null)
            return;

        _runner.gameObject.SetActive(false);
        Destroy(_runner.gameObject);
        _runner = null;

        if (_currentExperiment != null)
            _currentExperiment.FinalizeExperiment();

        RaiseExperimentCompleted(_currentExperiment);

        if (PrepareNextTrial())
        {
            StartTrial();
        }
        else
        {
            //experiment complete
            _experimentRunning = false;
            Physics.autoSimulation = true;

            if (TerminateAfterExperimentComplete)
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                #endif
            }
        }
    }

    private void BeginExperimentBlock(int index)
    {
        _blockIndex = index;
        _block = _currentConfig.TrialBlocks[index];
        _blockTotalTrialCount = _block.ComputeNumTrials();

        if (_block.BlockVariables == null || _block.BlockVariables.Count <= 0)
            throw new System.Exception("Error: block has no variables");

        _numBlockVars = _block.BlockVariables.Count;
        int i = 0;

        //convert block variables to an array and extract counts
        _blockValCount = new int[_numBlockVars];
        _blockValIndices = new int[_numBlockVars];
        _blockVars = new BlockVariable[_numBlockVars];

        i = 0;
        foreach (var kvp in _block.BlockVariables)
        {
            _blockVars[i] = new BlockVariable(kvp.Key, kvp.Value);
            i++;
        }

        if (i != _numBlockVars)
            throw new System.Exception("Error initializing block variables");

        for (i = 0; i < _numBlockVars; i++)
        {
            _blockValCount[i] = _blockVars[i].Value.NumValues;
            _blockValIndices[i] = 0;
        }

        _blockTrialCount = 0;
    }

    private bool IncrementBlockVariables()
    {
        for (int i = _numBlockVars - 1; i >= 0; i--)
        {
            _blockValIndices[i]++;
            if (_blockValIndices[i] < _blockValCount[i])
            {
                //this is a valid new index
                _blockTrialCount++;
                return true;
            }
            else
            {
                //reset this variable and contine to the next
                _blockValIndices[i] = 0;
            }
        }

        //didn't find a new index set, block complete
        return false;
    }

    private Dictionary<string, Experiment.ExperimentVal> BuildTrialSettings()
    {
        Dictionary<string, Experiment.ExperimentVal> settings = new Dictionary<string, Experiment.ExperimentVal>();

        foreach (var kvp in _currentConfig.StaticValues)
        {
            settings.Add(kvp.Key, kvp.Value);
        }

        for (int i = 0; i < _numBlockVars; i++)
        {
            var expVal = _blockVars[i].Value.GetValue(_blockValIndices[i]);
            if (expVal.NumValues != 1)
                throw new System.Exception("Experiment value couldn't be resolved to a single value!");

            settings[_blockVars[i].Name] = expVal;
        }

        return settings;
    }

    private bool PrepareNextTrial()
    {
        if (!IncrementBlockVariables())
        {
            //move to next block if available
            if (_blockIndex + 1 < _currentConfig.TrialBlocks.Count)
            {
                BeginExperimentBlock(_blockIndex + 1);
            }
            else
            {
                //experiment complete
                return false;
            }
        }

        _trialCount++;
        return true;
    }

    private void StartTrial()
    {
        Debug.Log($"Starting trial #{_trialCount + 1} of {_totalTrialCount} (Block {_block.BlockName} Trial {_blockTrialCount + 1} of {_blockTotalTrialCount})");

        _trialSettings = BuildTrialSettings();
        GameObject objRunner = new GameObject("ExperimentRunner");
        var runner = objRunner.AddComponent<ExperimentRunner>();
        GameObject.DontDestroyOnLoad(objRunner);

        _currentExperiment = Instantiate<Experiment>(_experimentPrefab);

        runner.Session = _sessionName;
        runner.Block = _block.BlockName;
        runner.Experiment = _currentExperiment;
        runner.TrialNum = _trialCount;
        runner.BlockTrialNum = _blockTrialCount;
        runner.TrialSettings = _currentExperiment.ParseTrialSettings(_trialSettings);

        RaiseExperimentStarting(_currentExperiment);

        _runner = runner;
    }

    private void RaiseExperimentStarting(Experiment experiment)
    {
        var handler = ExperimentStarting;
        if (handler != null)
            handler(experiment);
    }

    private void RaiseExperimentCompleted(Experiment experiment)
    {
        var handler = ExperimentCompleted;
        if (handler != null)
            handler(experiment);
    }

}