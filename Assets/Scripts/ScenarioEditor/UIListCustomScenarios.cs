using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIListCustomScenarios : UIContextBase
{
    public enum CustomScenarioSortType
    {
        Unknown,
        Name,
        Date,
    }

    public GameObject ListItemPrefab;

    private const string CONTEXT_VARIABLE = "SCENARIO_FOLDER";
    private List<CustomScenarioData> _scenarios = null;
    private string _scenarioFolder = null;

    private CustomScenarioSortType _lastSortType = CustomScenarioSortType.Unknown;
    private bool _lastSortReversed = false;
    private bool _started = false;

    private void OnEnable()
    {
        if (ListItemPrefab == null || !_started)
            return;

        UpdateScenarioList();
    }

    private void OnDisable()
    {
        
    }

    protected override void Start()
    {
        base.Start();

        if (_context != null)
            _context.ContextDataChanged += OnContextDataChanged;

        if (ListItemPrefab == null)
            return;

        UpdateScenarioList();
        _started = true;
    }

    private void OnDestroy()
    {
        if (_context != null)
            _context.ContextDataChanged -= OnContextDataChanged;
    }

    private void OnContextDataChanged(string obj)
    {
        if (obj == CONTEXT_VARIABLE)
        {
            var newScenarioFolder = _context.GetStringVariable(CONTEXT_VARIABLE);
            //Debug.Log($"Scenario folder changed from {_scenarioFolder} to {newScenarioFolder}");
            if (newScenarioFolder != _scenarioFolder)
                UpdateScenarioList();
        }
    }

    public void Sort(CustomScenarioSortType sortBy, bool reverse)
    {
        switch (sortBy)
        {
            case CustomScenarioSortType.Date:
                SortByDate(reverse);
                break;

            case CustomScenarioSortType.Name:
                SortByName(reverse);
                break;
        }
    }

    public bool WasLastSortReversed
    {
        get { return _lastSortReversed; }
    }

    public CustomScenarioSortType LastSortType
    {
        get { return _lastSortType; }
    }

    public void SortByName(bool reverse)
    {
        if (_scenarios == null)
            return;

        Debug.Log($"UIListCustomScenario: Sort by name, reverse:{reverse}");
        _scenarios.SortByScenarioName(reverse);
        _lastSortType = CustomScenarioSortType.Name;
        _lastSortReversed = reverse;
        AddAllScenarios();
    }

    public void SortByDate(bool reverse)
    {
        if (_scenarios == null)
            return;

        Debug.Log($"UIListCustomScenario: Sort by date, reverse:{reverse}");
        _scenarios.SortByScenarioDate(reverse);
        _lastSortType = CustomScenarioSortType.Date;
        _lastSortReversed = reverse;
        AddAllScenarios();
    }

    public void UpdateScenarioList()
    {        
        if (_context != null)
            _scenarioFolder = _context.GetStringVariable(CONTEXT_VARIABLE);

        if (_scenarioFolder != null && _scenarioFolder.Length > 0)
            ScenarioSaveLoad.Instance.ChangeSaveDestination(_scenarioFolder);
        else
            _scenarioFolder = ScenarioSaveLoad.Instance.GetScenarioFilePath();

        CustomScenarioUtil.GetAllCustomScenarioData(ref _scenarios, _scenarioFolder);

        if (_scenarios == null)
            return;

        SortByDate(true);
        AddAllScenarios();
    }

    private void AddAllScenarios()
    {
        if (_scenarios == null)
            return;

        foreach (Transform obj in transform)
        {
            Destroy(obj.gameObject);
        }

        foreach (var data in _scenarios)
        {
            AddScenario(data);
        }
    }

    private void AddScenario(CustomScenarioData data)
    {
        if (ListItemPrefab == null)
            return;

        var go = Instantiate<GameObject>(ListItemPrefab);
        go.transform.SetParent(transform, false);

        var btn = go.GetComponent<UIBtnSelectCustomScenario>();
        btn.SetScenarioData(data);
        go.SetActive(true);

    }

}
