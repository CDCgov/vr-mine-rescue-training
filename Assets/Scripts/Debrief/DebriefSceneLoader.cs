using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Threading;

public class DebriefSceneLoader : MonoBehaviour
{
    public string SceneToLoad;
    public GameObject CameraScreenOne;
    public GameObject CameraScreenTwo;
    public POIManager POIManager;
    public MineMapSymbolManager SymbolManager;
    public SessionPlaybackControl SessionPlaybackControl;
    public Action SceneLoaded;
    public Action InitiateSceneLoad;
    public Slider ProgressSlider;
    public GameObject ProgressPanel;
    public Slider ProgressSliderScrn2;
    public GameObject ProgressPanelScrn2;
    public GameObject DebriefFilePanel;

    private bool _sceneLoaded = false;
    private bool _debriefReady = false;
    private DebriefLoadInfo _dbLoadInfo;
    private SessionPlaybackControl _sessionPlaybackControl;
    private CancellationTokenSource _cancelSource;
    private float _progress = 0;
    private DebriefOverviewUIController debriefOverviewUIController;

    public bool DebriefReady
    {
        get { return _debriefReady; }        
    }

    async void Start()
    {
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);

        if(SessionPlaybackControl == null)
        {
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        }

        _dbLoadInfo = FindObjectOfType<DebriefLoadInfo>();
        _sessionPlaybackControl = FindObjectOfType<SessionPlaybackControl>();
        debriefOverviewUIController = FindObjectOfType<DebriefOverviewUIController>();
        if (!ProgressPanel.activeSelf)
        {
            ProgressPanel.SetActive(true);
        }
        if (!ProgressPanelScrn2.activeSelf)
        {
            ProgressPanelScrn2.SetActive(true);
        }
        if(_dbLoadInfo != null)
        {
            ProgressSlider.maxValue = 1;
            ProgressSlider.wholeNumbers = false;
            StartCoroutine(PeekAtProgress());

            _cancelSource = new CancellationTokenSource();
            var success = await _sessionPlaybackControl.LoadSession(_dbLoadInfo.DebriefScenePath, 
                (progress) =>
                {
                    _progress = progress;
                }, _cancelSource.Token);

            CompleteSceneLoad();
        }
        
        
    }

    private void OnDestroy()
    {
        if (_cancelSource != null)
        {
            _cancelSource.Cancel();
            _cancelSource.Dispose();
            _cancelSource = null;
        }
    }

    private void CompleteSceneLoad()
    {
        
        Vector3 pos = POIManager.GetFirstOfType(POIType.SpawnPoint).transform.position;
        pos.y += 60;
        Vector3 rotEuler = new Vector3(90, 0, 0);
        
        CameraScreenOne.transform.position = pos;
        CameraScreenOne.transform.eulerAngles = rotEuler;
        ProgressPanel.SetActive(false);
        ProgressPanelScrn2.SetActive(false);
        _sceneLoaded = true;
       
        SymbolManager.AddSceneSymbols(false);

        _debriefReady = true;
        SceneLoaded?.Invoke();
        StopCoroutine(PeekAtProgress());
        //SceneManager.sceneLoaded -= CompleteSceneLoad;
    }

    IEnumerator PeekAtProgress()
    {
        Debug.Log("In Peek at Progress");
        while (!_sceneLoaded)
        {
           
            ProgressSlider.value = _progress;
            ProgressSliderScrn2.value = _progress;
            yield return new WaitForSeconds(0.02f);
        }
    }

    public async void LoadNewScene()
    {
        _progress = 0;
        _sceneLoaded = false;
        
        InitiateSceneLoad?.Invoke();
        if (!ProgressPanel.activeSelf)
        {
            ProgressPanel.SetActive(true);
        }
        if (!ProgressPanelScrn2.activeSelf)
        {
            ProgressPanelScrn2.SetActive(true);
        }
        if (SessionPlaybackControl != null)
        {
            Debug.Log("Scrubbing at new scene load");
            SessionPlaybackControl.Scrub(0);
        }
        if (_dbLoadInfo != null)
        {
           
            ProgressSlider.maxValue = 1;
            ProgressSlider.wholeNumbers = false;
            StartCoroutine(PeekAtProgress());

            if (SymbolManager != null)
                SymbolManager.RemoveAllSymbols();

            _cancelSource = new CancellationTokenSource();
            var success = await _sessionPlaybackControl.LoadSession(_dbLoadInfo.DebriefScenePath,
                (progress) =>
                {
                    _progress = progress;
                }, _cancelSource.Token);
            //SceneManager.sceneLoaded += CompleteSceneLoad;
            Debug.Log("Loading new scene");
            CompleteSceneLoad();
        }
    }


    public void OpenDebriefFilePanel()
    {
        DebriefFilePanel.SetActive(true);
    }

    public void CloseDebriefFilePanel()
    {
        DebriefFilePanel.SetActive(false);
    }
}
