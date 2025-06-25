using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

public class ScenarioEditorEditModeTests
{
    [Test]
    public void TestVariantSetIncrement()
    {
        LoadableVariantSet set = ScriptableObject.CreateInstance<LoadableVariantSet>();
        set.LoadableVariants = new List<LoadableAsset>();
        
        for (int i = 0; i < 5; i++)
        {
            LoadableAsset asset = ScriptableObject.CreateInstance<LoadableAsset>();
            asset.AssetID = i.ToString();
            set.LoadableVariants.Add(asset);
        }

        string assetID = "2";
        int count = set.LoadableVariants.Count;

        for (int j = -100; j < 100; j++)
        {
            var asset = set.GetNext(assetID, j);
            Assert.That(asset, Is.Not.Null, $"Couldn't get asset with increment {j}");

            var index = (2 + j);
            if (index >= count)
                index %= count;

            while (index < 0)
                index += count;

            Assert.That(asset.AssetID, Is.EqualTo(index.ToString()), $"Asset had wrong id {asset.AssetID} for index {index}");
        }
    }

    private SavedScenario CreateSavedScenario(string name)
    {
        SavedScenario scenario = new SavedScenario();
        scenario.ScenarioName = name;

        SavedAsset asset = new SavedAsset
        {
            AssetID = "INVALID_ID",
            AssetName = "TestAsset",
        };

        scenario.AddAssetToList(asset);

        return scenario;
    }

    private void CreateOldScenarioFile(string filename)
    {
        SavedScenario scenario = CreateSavedScenario("Old Scenario Test");

        if (Path.HasExtension(filename))
        {
            filename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
        }
        JSONSerializer.Serialize<SavedScenario>(scenario, filename);
    }

    private void CreateNewScenarioFile(string filename)
    {
        SavedScenario scenario = CreateSavedScenario("New Scenario Test");

        scenario.SaveScenario(filename);
    }

    [Test]
    public void TestScenarioReadWrite()
    {
        const string filename = "C:\\temp\\new-scenario-test.json";

        CreateNewScenarioFile(filename);
        var scenario = SavedScenario.LoadScenario(filename);

        Debug.Log($"Read scenario {scenario.ScenarioName}");
        Debug.Log($"VR Mine version: {scenario.Header.VRMineVersion}");

        Assert.That(scenario.ScenarioName == "New Scenario Test");
        Assert.That(scenario.Header.VRMineVersion == GitVersion.Version);
    }

    [Test]
    public void TestReadOldScenario()
    {
        const string filename = "C:\\temp\\old-scenario-test.json";

        CreateOldScenarioFile(filename);
        var scenario = SavedScenario.LoadScenario(filename);

        Debug.Log($"Read scenario {scenario.ScenarioName}");

        Assert.That(scenario.ScenarioName == "Old Scenario Test");
    }

    [Test]
    public void TestReadOldScenarioDirect()
    {
        var serializer = new JsonSerializer();

        const string filename = "C:\\temp\\old-scenario-test.json";
        CreateOldScenarioFile(filename);


        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        using (var streamReader = new StreamReader(filename, Encoding.UTF8, false))
        using (var reader = new JsonTextReader(streamReader))
        {
            var obj = serializer.Deserialize(reader, typeof(SavedScenario));
            sw.Stop();

            Assert.That(obj is SavedScenario, $"deserialized object is wrong type: {obj.GetType()}");

            Debug.Log($"Scenario read took {sw.ElapsedMilliseconds}ms");

            var scenario = (SavedScenario)obj;

            Assert.That(scenario.ScenarioName, Is.EqualTo("Old Scenario Test"));

            var assets = scenario.GetSavedAssets();
            Assert.That(assets.Count, Is.EqualTo(1));
            Assert.That(assets[0].AssetName, Is.EqualTo("TestAsset"));

        }
    }

    [Test]
    public void TestScenarioEditorHeaderRead()
    {
        const string filename = "C:\\temp\\scenario-header-test.json";

        int bytesRead = 0;
        int bytesWritten = 0;
        var serializer = new JsonSerializer();
        var testData = new SavedScenarioHeader();

        serializer.Formatting = Formatting.Indented;

        using (var streamWriter = new StreamWriter(filename, false, Encoding.UTF8))
        using (var writer = new JsonTextWriter(streamWriter))
        {
            for (int i = 0; i < 250; i++)
            {
                testData.CreationDateTime = DateTime.Now - TimeSpan.FromDays(1);
                testData.ModifiedDateTime = DateTime.Now;
                testData.ScenarioName = $"Test Data {i}";


                serializer.Serialize(writer, testData);
            }

            streamWriter.Flush();
            bytesWritten = (int)streamWriter.BaseStream.Position;
            Debug.Log($"Wrote test data, {bytesWritten} bytes");
        }

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        using (var streamReader = new StreamReader(filename, Encoding.UTF8, false, 256))
        using (var reader = new JsonTextReader(streamReader))
        {
            
            var obj = serializer.Deserialize(reader, typeof(SavedScenarioHeader));
            sw.Stop();

            Assert.That(obj is SavedScenarioHeader, $"deserialized object is wrong type: {obj.GetType()}");

            Debug.Log($"Header read took {sw.ElapsedMilliseconds}ms");

            var header = (SavedScenarioHeader)obj;
            Assert.That(header.ScenarioName, Is.EqualTo("Test Data 0"), $"Read wrong scenario name from header {header.ScenarioName}");

            bytesRead = (int)streamReader.BaseStream.Position;
            Debug.Log($"Read {bytesRead} bytes");

            Assert.That(bytesRead, Is.LessThan(bytesWritten / 20), $"Read too many bytes when reading header - {bytesRead}");
        }

        Debug.Log("Complete");
    }

    [Test]
    public void TestStaticGasZoneOverrides()
    {
        MineAtmosphere test1 = new MineAtmosphere(0.1f, 0.1f, 0.1f, 0.1f);
        MineAtmosphere test2 = new MineAtmosphere(1.0f, 1.0f, 1.0f, 1.0f);

        test1.SetStrength(1.0f);
        test2.SetStrength(0.0f);

        MineAtmosphere atm = new MineAtmosphere(0, 0, 0, 0);
        atm.SetStrength(0.0f);

        atm.Combine(test1);
        atm.Combine(test2);

        Debug.Log(atm.ToString());

        test1.SetStrength(0.0f);
        test2.SetStrength(0.5f);

        atm = new MineAtmosphere(0, 0, 0, 0);
        atm.SetStrength(0.0f);

        atm.Combine(test1);
        atm.Combine(test2);

        Debug.Log(atm.ToString());

    }

}
