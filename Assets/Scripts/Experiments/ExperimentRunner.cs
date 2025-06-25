using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class ExperimentRunner : MonoBehaviour
{
    public ExperimentManager Manager = null;

    public Experiment Experiment;
    public TrialSettings TrialSettings;
    public string Session;
    public string Block;
    public int TrialNum;
    public int BlockTrialNum;

    private bool _initialized = false;
    private bool _completed = false;
    private float _elapsedTime = 0;
    private float _startTime = 0;

    private IEnumerator Start()
    {
        if (Manager == null)
            Manager = ExperimentManager.GetDefault();

        var scenePath = Experiment.GetScenePath(TrialSettings);	
        if (scenePath != null)
        {
            // int sceneCount = SceneManager.sceneCount;
            // for (int i = 0; i < sceneCount; i++)
            // {
            // 	var scene = SceneManager.GetSceneAt(i);

            // 	if (scene.path == scenePath)
            // 	{
            // 		AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(scenePath);
            // 		while (!asyncUnload.isDone)
            // 		{
            // 			yield return null;
            // 		}
            // 	}
            // }

            if (Experiment.ReloadSceneEveryTrial || scenePath != SceneManager.GetActiveScene().path)
            {
                //load the test scene
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath);

                // Wait until the asynchronous scene fully loads
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                // for (int i = 0; i < 500; i++)
                // {
                // 	if (!Physics.autoSimulation)
                // 	{
                // 		for (int j = 0; i < 500; j++)
                // 			Physics.Simulate(TrialSettings.ManualUpdateTimestep);
                // 	}
                // 	yield return null;
                // }
            }
            else
            {
                //skip several frames to allow for scene/object cleanup to finish
                for (int i = 0; i < 4; i++)
                {
                    if (!Physics.autoSimulation)
                        Physics.Simulate(TrialSettings.ManualUpdateTimestep);
                    yield return null;
                }
            }
        }

        if (TrialSettings.ExperimentUpdateMethod == Experiment.UpdateMethod.Manual)
        {
            Physics.autoSimulation = false;
            
            //stabilize physics
            for (int i = 0; i < 100; i++)
                Physics.Simulate(TrialSettings.ManualUpdateTimestep);
            yield return null;

        } 
        else
        {
            Physics.autoSimulation = true;

            //skip two frames to be safe
            yield return null;
            yield return null;
            
        }

        yield return StartCoroutine(Experiment.Initialize(TrialSettings, Session, Block, TrialNum));

        _startTime = Time.time;
        _elapsedTime = 0;
        Experiment.StartExperiment();

        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized || _completed)
                return;

        switch (TrialSettings.ExperimentUpdateMethod)
        {
            case Experiment.UpdateMethod.NormalUpdate:
                _elapsedTime = Time.time - _startTime;
                UpdateExperiment(Time.deltaTime, _elapsedTime);
                break;

            case Experiment.UpdateMethod.Manual:
                for (int i = 0; i < TrialSettings.ManualUpdatesPerFrame; i++)
                {
                    float timestep = TrialSettings.ManualUpdateTimestep;
                    Physics.Simulate(timestep);
                    _elapsedTime += timestep;
                    UpdateExperiment(timestep, _elapsedTime);

                    if (_completed)
                        break;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!_initialized || _completed)
                return;
                
        if (TrialSettings.ExperimentUpdateMethod != Experiment.UpdateMethod.FixedUpdate)
            return;

        _elapsedTime = Time.time - _startTime;
        UpdateExperiment(Time.fixedDeltaTime, _elapsedTime);

    }

    private void UpdateExperiment(float deltaTime, float elapsedTime)
    {
        try
        {
            if (!_initialized || _completed)
                return;

            if (!Experiment.UpdateExperiment(deltaTime, elapsedTime))
            {
                //experiment trial completed, inform the manager and disable
                _completed = true;
                enabled = false;
                Experiment.FinalizeExperiment();
                Manager.TrialCompleted(gameObject);
                //Physics.autoSimulation = true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message + ex.StackTrace);
            Manager.TerminateExperiment(ex.Message);
            //Physics.autoSimulation = true;
        }
    }



}