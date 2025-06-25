using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ExperimentPlayModeTests
{

    [Test]
    public void BasicTestsSimplePasses()
    {
        // Use the Assert class to test conditions.
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator BasicTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }

    
    [UnityTest]
    public IEnumerator TestExperimentExecution()
    {
    string simpleConfig = @"
ExperimentType: TestExperiment
StaticValues:
  TestStatic: !string I'm a static string #test comment

TrialBlocks:
  - BlockName: TestBlock1
    BlockVariables:
      BlockVarStatic: !double 1234
      BlockVar1To4: !range { Min: 1, Max: 4, Count: 4 }
      BlockVar88to99: !range { Min: 88, Max: 99, Count: 5 }

  - BlockName: TestBlock2
    BlockVariables:
      BlockVarStatic: !string Hello_World
      BlockVarSet: !set
        - item1
        - item2
";

        var expManager = ExperimentManager.GetDefault();
        TestExperiment experiment;
        ExperimentConfig config;
        
        using (var reader = new StringReader(simpleConfig))
        {
            config = ExperimentConfig.LoadConfigFile(reader);
        }		

        experiment = (TestExperiment)ScriptableObject.CreateInstance(typeof(TestExperiment));
        //experiment = new TestExperiment();

        expManager.StartExperiment(experiment, config, "TestSession");
        int count = 0;
        do
        {
            yield return new WaitForSecondsRealtime(0.20f);
            count++;
        } while (expManager.IsExperimentRunning && count < 150);

        //last trial run should be total trials - 1
        //Assert.That(experiment.TrialNum, Is.EqualTo(21));

        yield return null;
    }

}


