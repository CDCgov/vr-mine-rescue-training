using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;


class TestExperiment : Experiment
{
    public int TrialNum;

    int _updateCount = 0;	

    private bool _initialized = false;
    private bool _trialComplete = false;

    public override bool Initialized
    {
        get
        {
            return _initialized;
        }
    }

    public override bool Complete
    {
        get
        {
            return _trialComplete;
        }
    }

    public override void FinalizeExperiment()
    {
        
    }

    public override string GetScenePath(TrialSettings settings)
    {
        return null;
    }

    public override TrialSettings ParseTrialSettings(Dictionary<string, ExperimentVal> settings)
    {
        var ts =  new TrialSettings();
        ts.ExperimentUpdateMethod = Experiment.UpdateMethod.Manual;
        ts.ManualUpdatesPerFrame = 1;
        ts.ManualUpdateTimestep = 0.02f;

        return ts;
    }

    public override IEnumerator Initialize(TrialSettings settings, string sessionName, string blockName, int trialNum)
    {
        TrialNum = trialNum;
        TestContext.WriteLine("initializing trial:");
        // foreach (var kvp in settings)
        // {
        // 	TestContext.WriteLine($"{kvp.Key}: {kvp.Value.ToString()}");
        // }
        yield return null;

        _initialized = true;
    }

    public override void StartExperiment()
    {
        _updateCount = 0;
    }

    public override bool UpdateExperiment(float deltaTime, float elapsedTime)
    {
        _updateCount++;
        if (_updateCount > 10)
        {
            _trialComplete = true;
            return false;
        }

        return true;
    }
}