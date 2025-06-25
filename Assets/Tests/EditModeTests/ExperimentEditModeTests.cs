using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ExperimentEditModeTests
{
/* 
    [Test]
    public void TestDebugMessage()
    {
        // Use the Assert class to test conditions.
        Debug.Log("Hello World");
        TestContext.WriteLine("This is per-test text output");
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator BasicEditModeTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }

    [Test]
    public void TestExperimentValTypes()
    {
        var x = new Experiment.SingleVal<double>();

        var drange = new Experiment.DoubleRange(5.2, 10.5, 10);
        var min = (Experiment.SingleVal<double>) drange.GetValue(0);
        var max = (Experiment.SingleVal<double>) drange.GetValue(10 - 1);

        Assert.That(min.Value, Is.EqualTo(5.2));
        Assert.That(max.Value, Is.EqualTo(10.5));

    }

    [Test]
    public void TestIntRange()
    {
        var x = new Experiment.IntRange(7, 25);

        Assert.That(x.NumValues, Is.EqualTo(19));

        var start = (Experiment.SingleVal<int>) x.GetValue(0);
        var end = (Experiment.SingleVal<int>) x.GetValue(18);

        Assert.That(start.Value, Is.EqualTo(7));
        Assert.That(end.Value, Is.EqualTo(25));
    }

    [Test]
    public void TestExperimentConfigLoad()
    {
        string simpleConfig = @"
ExperimentType: TestExperiment
StaticValues:
  testDouble: !double 42.42 #test comment
  testDouble2: !double 88.8
  testString: !string Hello World
  testInt: !int 42
  testRange: !range { Min: 3.7, Max: 5.2, Count: 15 }
  testSet: !set 
    - Item1
    - Item2
    - Item3

TrialBlocks:
  - BlockName: TestBlock1
    BlockVariables:
      BlockVar1: !double 1234
      BlockVar2: !range { Min: 1, Max: 2, Count: 25 }
      BlockVar3: !range { Min: 88, Max: 99, Count: 5 }

  - BlockName: TestBlock2
    BlockVariables:
      BlockVar1: !string Hello_World
      BlockVar2: !set
        - item1
        - item2
";
        using (var reader = new StringReader(simpleConfig))
        {
            var config = ExperimentConfig.LoadConfigFile(reader);

            Assert.NotNull(config);
            Assert.IsInstanceOf(typeof(ExperimentConfig), config);
            TestContext.WriteLine($"Read experiment type {config.ExperimentType}");
            Assert.IsTrue(config.ExperimentType == "TestExperiment", "Wrong experiment type read from config");

            Assert.NotNull(config.StaticValues);
            TestContext.Write($"StaticValues has {config.StaticValues.Count} values: ");
            foreach (var val in config.StaticValues)
            {
                TestContext.Write($" {val.Key}");
            }
            TestContext.WriteLine();
            Assert.IsTrue(config.StaticValues.ContainsKey("testDouble"), "Static values missing testDouble");
            Assert.IsTrue(config.StaticValues.ContainsKey("testDouble2"), "Static values missing testDouble2");
            Assert.IsTrue(config.StaticValues.ContainsKey("testString"), "Static values missing testString");
            Assert.IsTrue(config.StaticValues.ContainsKey("testInt"), "Static values missing testInt");
            Assert.IsTrue(config.StaticValues.ContainsKey("testRange"), "Static values missing testRange");
            Assert.IsTrue(config.StaticValues.ContainsKey("testSet"), "Static values missing testSet");
            
            Assert.That(config.StaticValues["testDouble"], Is.InstanceOf(typeof(Experiment.SingleVal<double>)));
            Assert.That(config.StaticValues["testDouble2"], Is.InstanceOf(typeof(Experiment.SingleVal<double>)));
            Assert.That(config.StaticValues["testString"], Is.InstanceOf(typeof(Experiment.SingleVal<string>)));
            Assert.That(config.StaticValues["testInt"], Is.InstanceOf(typeof(Experiment.SingleVal<int>)));
            Assert.That(config.StaticValues["testRange"], Is.InstanceOf(typeof(Experiment.FloatRange)));
            Assert.That(config.StaticValues["testSet"], Is.InstanceOf(typeof(Experiment.ExpValSet<string>)));

            var dVal = (Experiment.SingleVal<double>)config.StaticValues["testDouble"];
            TestContext.WriteLine($"testDouble = {dVal.Value}");
            Assert.IsTrue(dVal.Value == 42.42);

            var dVal2 = (Experiment.SingleVal<double>)config.StaticValues["testDouble2"];
            TestContext.WriteLine($"testDouble2 = {dVal2.Value}");
            Assert.IsTrue(dVal2.Value == 88.8);

            var sVal = (Experiment.SingleVal<string>)config.StaticValues["testString"];
            TestContext.WriteLine($"testString = {sVal.Value}");
            Assert.That(sVal.Value, Is.EqualTo("Hello World"));

            var iVal = (Experiment.SingleVal<int>)config.StaticValues["testInt"];
            TestContext.WriteLine($"testInt = {iVal.Value}");
            Assert.That(iVal.Value, Is.EqualTo(42));

            var fRange = (Experiment.FloatRange)config.StaticValues["testRange"];
            TestContext.WriteLine($"testRange = Min: {fRange.Min} Max: {fRange.Max} Count: {fRange.Count}");
            Assert.That(fRange.Min, Is.EqualTo(3.7f));
            Assert.That(fRange.Max, Is.EqualTo(5.2f));
            Assert.That(fRange.Count, Is.EqualTo(15));

            var testSet = (Experiment.ExpValSet<string>)config.StaticValues["testSet"];
            int testSetCount = testSet.NumValues;
            TestContext.Write($"testSet: Count: {testSetCount} Contents:");
            for(int i = 0; i < testSetCount; i++)
            {
                var setVal = testSet.GetValue(i);

                TestContext.Write($" {setVal.GetValue<string>()}");
            }
            TestContext.WriteLine();
            
            Assert.That(testSet.GetValue(0).GetValue<string>(), Is.EqualTo("Item1"));
            Assert.That(testSet.GetValue(1).GetValue<string>(), Is.EqualTo("Item2"));
            Assert.That(testSet.GetValue(2).GetValue<string>(), Is.EqualTo("Item3"));

            Assert.That(config.TrialBlocks, Is.Not.Null);
            Assert.That(config.TrialBlocks.Count, Is.EqualTo(2));

            TrialBlock b1 = config.TrialBlocks[0];
            TrialBlock b2 = config.TrialBlocks[1];
            
            for (int i = 0; i < config.TrialBlocks.Count; i++)
            {
                TrialBlock b = config.TrialBlocks[i];
                TestContext.WriteLine($"Block {i}: {b.BlockName}");
                foreach (var bvar in b.BlockVariables)
                {
                    TestContext.WriteLine($"{bvar.Key}: {bvar.Value}");
                }
            }


        }
    }

    [Test]
    public void TestSetParsing()
    {
            string setTestConfig = @"
ExperimentType: TestExperiment
StaticValues:
  TestStringSet: !set
    - Item 1
    - Item 2
    - Item 3
  TestIntSet: !intset
    - 1
    - 2
    - 3
  TestFloatSet: !floatset
    - 1.1
    - 2.2
    - 3.3
  TestDoubleSet: !doubleset
    - 4.4
    - 5.5
    - 6.6

";
        using (var reader = new StringReader(setTestConfig))
        {
            var config = ExperimentConfig.LoadConfigFile(reader);

            Assert.NotNull(config);
            Assert.IsInstanceOf(typeof(ExperimentConfig), config);
            Assert.NotNull(config.StaticValues);

            foreach (var val in config.StaticValues)
            {
                int numVals = val.Value.NumValues;
                TestContext.WriteLine($"{val.Key}");
                
                for (int i = 0; i < numVals; i++)
                {
                    var expVal = val.Value.GetValue(i);
                    TestContext.WriteLine($"{expVal}");
                }
            }

            var expStringSet = config.StaticValues["TestStringSet"];
            var expIntSet = config.StaticValues["TestIntSet"];
            var expFloatSet = config.StaticValues["TestFloatSet"];
            var expDoubleSet = config.StaticValues["TestDoubleSet"];

            Assert.That(expStringSet, Is.InstanceOf(typeof(Experiment.ExpValSet<string>)));
            Assert.That(expIntSet, Is.InstanceOf(typeof(Experiment.ExpValSet<int>)));
            Assert.That(expFloatSet, Is.InstanceOf(typeof(Experiment.ExpValSet<float>)));
            Assert.That(expDoubleSet, Is.InstanceOf(typeof(Experiment.ExpValSet<double>)));

            var stringSet = (Experiment.ExpValSet<string>)expStringSet;
            var intSet = (Experiment.ExpValSet<int>)expIntSet;
            var floatSet = (Experiment.ExpValSet<float>)expFloatSet;
            var doubleSet = (Experiment.ExpValSet<double>)expDoubleSet;

            Assert.That(stringSet.Values[0], Is.EqualTo("Item 1"));
            Assert.That(stringSet.Values[1], Is.EqualTo("Item 2"));
            Assert.That(stringSet.Values[2], Is.EqualTo("Item 3"));

            Assert.That(intSet.Values[0], Is.EqualTo(1));
            Assert.That(intSet.Values[1], Is.EqualTo(2));
            Assert.That(intSet.Values[2], Is.EqualTo(3));

            Assert.That(floatSet.Values[0], Is.EqualTo(1.1f));
            Assert.That(floatSet.Values[1], Is.EqualTo(2.2f));
            Assert.That(floatSet.Values[2], Is.EqualTo(3.3f));

            Assert.That(doubleSet.Values[0], Is.EqualTo(4.4));
            Assert.That(doubleSet.Values[1], Is.EqualTo(5.5));
            Assert.That(doubleSet.Values[2], Is.EqualTo(6.6));
        }
    }

    [Test]
    public void TrialSettingsLoading()
    {
        using (var reader = new StringReader(TestTrialSettings.SampleConfigGood))
        {
            var config = ExperimentConfig.LoadConfigFile(reader);

            TestTrialSettings settings = new TestTrialSettings();
            settings.LoadSettings(config.StaticValues);

            TestContext.WriteLine($"TestString: {settings.TestString}");
            TestContext.WriteLine($"TestInt: {settings.TestInt}");
            TestContext.WriteLine($"TestDouble: {settings.TestDouble}");
            TestContext.WriteLine($"TestFloat: {settings.TestFloat}");

            Assert.That(settings.TestString, Is.EqualTo("Hello World"));
            Assert.That(settings.TestInt, Is.EqualTo(42));
            Assert.That(settings.TestDouble, Is.EqualTo(42.42));
            Assert.That(settings.TestFloat, Is.EqualTo(42.42f));
        }
    }

    private void TestTrialSettingsError(string yamlContent)
    {
        using (var reader = new StringReader(yamlContent))
        {
            var config = ExperimentConfig.LoadConfigFile(reader);

            TestTrialSettings settings = new TestTrialSettings();
            Assert.That(() => { 
                try
                {
                    settings.LoadSettings(config.StaticValues); 
                }
                catch (System.Exception ex)
                {
                    TestContext.WriteLine($"Caught Exception: {ex.Message}");
                    throw ex;
                }
                }, Throws.Exception.InstanceOf<TrialSettings.ValidationException>());
        }
    }

    [Test]
    public void TrialSettingsMissing()
    {
        TestTrialSettingsError(TestTrialSettings.SampleConfigMissing);
    }

    [Test]
    public void TrialSettingsExtra()
    {
        TestTrialSettingsError(TestTrialSettings.SampleConfigExtra);
    }

    [Test]
    public void TrialSettingsInvalid()
    {
        TestTrialSettingsError(TestTrialSettings.SampleConfigInvalid);
    }

    [Test]
    public void TestVectorParsing()
    {
        var yaml = ExperimentConfig.BuildDeserializer();

        var v = yaml.Deserialize<TestVectorRead>(TestVectorRead.SampleConfigGood);

        TestContext.WriteLine(v.TestString);
        TestContext.WriteLine(v.TestVector.Value);
        TestContext.WriteLine(v.TestVector2.Value);
        TestContext.WriteLine(v.TestVector3.Value);
        TestContext.WriteLine(v.TestFloat);
    }

    [Test]
    public void TestExperimentExecution()
    {
    string simpleConfig = @"
ExperimentType: TestExperiment
StaticValues:
  testDouble: !double 42.42 #test comment
  testDouble2: !double 88.8
  testString: !string Hello World
  testInt: !int 42
  testRange: !range { Min: 3.7, Max: 5.2, Count: 15 }
  testSet: !set 
    - Item1
    - Item2
    - Item3

TrialBlocks:
  - BlockName: TestBlock1
    BlockVariables:
      BlockVar1: !double 1234
      BlockVar2: !range { Min: 1, Max: 2, Count: 25 }
      BlockVar3: !range { Min: 88, Max: 99, Count: 5 }

  - BlockName: TestBlock2
    BlockVariables:
      BlockVar1: !string Hello_World
      BlockVar2: !set
        - item1
        - item2
";

        var expManager = ExperimentManager.GetDefault();
        //Experiment experiment;
        ExperimentConfig config;
        
        using (var reader = new StringReader(simpleConfig))
        {
            config = ExperimentConfig.LoadConfigFile(reader);
        }		

        //experiment = (Experiment)ScriptableObject.CreateInstance(typeof(TestExperiment));
    }
}

// class TestExperiment : Experiment
// {

// }

class TestVectorRead
{
    public static string SampleConfigGood = @"
TestString: 'Hello World'
TestVector:
  x: 1
  y: 2
  z: 3
TestVector2: { x: 4, y: 5, z: 6 }
TestVector3: [ 7.7, 8.8, 9 ]
TestFloat: 42
";

    public string TestString { get; set; }
    public Experiment.SingleVal<Vector3> TestVector { get; set; }
    public Experiment.SingleVal<Vector3> TestVector2 { get; set; }
    public Experiment.SingleVal<Vector3> TestVector3 { get; set; }
    public float TestFloat { get; set; }
}

class TestTrialSettings : TrialSettings
{

    public static string SampleConfigGood = @"
ExperimentType: TestExperiment
StaticValues:
  ExperimentUpdateMethod: !string Manual
  ManualUpdateTimestep: !float 0.02
  ManualUpdatesPerFrame: !int 5
  TestString: !string 'Hello World'
  TestInt: !int 42
  TestDouble: !double 42.42
  TestFloat: !float 42.42
";

    public static string SampleConfigExtra = @"
ExperimentType: TestExperiment
StaticValues:
  ExperimentUpdateMethod: !string Manual
  ManualUpdateTimestep: !float 0.02
  ManualUpdatesPerFrame: !int 5
  TestString: !string 'Hello World'
  TestInt: !int 42
  TestDouble: !double 42.42
  TestDouble2: !double 42.42
  TestFloat: !float 42.42
";

    public static string SampleConfigMissing = @"
ExperimentType: TestExperiment
StaticValues:
  ExperimentUpdateMethod: !string Manual
  ManualUpdateTimestep: !float 0.02
  ManualUpdatesPerFrame: !int 5
  TestString: !string 'Hello World'
  TestDouble: !double 42.42
  TestFloat: !float 42.42
";

    public static string SampleConfigInvalid = @"
ExperimentType: TestExperiment
StaticValues:
  ExperimentUpdateMethod: !string Manual
  ManualUpdateTimestep: !float 0.02
  ManualUpdatesPerFrame: !int 5
  TestString: !string 'Hello World'
  TestInt: !int 42
  Testdouble: !double 42.42
  TestFloat: !float 42.42
";

    public string TestString { get; set; }
    public int TestInt { get; set; }
    public double TestDouble { get; set; }
    public float TestFloat { get; set; }

    */
}
