using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Text;
using System.Management.Instrumentation;
using System.Linq;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Collections;

public class ScenarioAssetData
{
    [CreateProperty] public string AssetName { get; set; }
    [CreateProperty] public string AssetID { get; set; }
    [CreateProperty] public int Count { get; set; }
    [CreateProperty] public bool ValidAssetID { 
        get
        {
            return AssetReference != null;
        } 
    }


    public LoadableAsset AssetReference { get; set; }
}

public static class ScenarioBindingConverters
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterConverters()
    {
        ConverterGroups.RegisterGlobalConverter(
            (ref DateTime value) => value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        );
    }
}

public class CustomScenarioDS : INotifyBindablePropertyChanged
{
    [CreateProperty] public string ScenarioName { get; set; }
    [CreateProperty] public DateTime ScenarioDate { get; set; }

    [CreateProperty] public SavedScenarioHeader ScenarioHeader
    {
        get 
        {
            if (FilePath == null)
                return null;

            if (_header == null & !_headerReadFailed)
            {
                _header = SavedScenario.ReadScenarioHeader(FilePath);
                if (_header == null)
                    _headerReadFailed = true;
            }

            return _header;
        }
    }

    [CreateProperty] public List<ScenarioAssetData> ScenarioAssets
    {
        get
        {           
            return _assetData;
        }
    }


    public void LoadScenarioAssetDetails(LoadableAssetManager assetManager)
    {
        ReadScenarioData();
        if (_scenario == null)
            return;

        Dictionary<string, ScenarioAssetData> scenarioAssets = new Dictionary<string, ScenarioAssetData>();

        foreach (var asset in _scenario.GetSavedAssets())
        {
            if (scenarioAssets.TryGetValue(asset.AssetID, out var assetData))
            {
                assetData.Count++;
                //scenarioAssets[asset.AssetID] = assetData;
                continue;
            }

            var assetRef = assetManager.FindAsset(asset.AssetID);

            if (assetRef != null && assetRef.ExternalAssetMetadata == null)
            {
                //internal asset
                continue;
            }

            assetData = new ScenarioAssetData
            {
                AssetID = asset.AssetID,
                AssetName = asset.AssetName,
                Count = 1,
                AssetReference = assetRef,
            };


            scenarioAssets[asset.AssetID] = assetData;
        }

        _assetData = scenarioAssets.Values.ToList<ScenarioAssetData>();
        _assetData.Sort((a, b) => a.AssetID.CompareTo(b.AssetName));
    }

    public void SaveFileList(string filename)
    {
        if (File.Exists(filename) || filename == null || ScenarioAssets == null)
            return;

        StringBuilder sb = new();

        foreach (var asset in ScenarioAssets)
        {
            if (asset.AssetReference == null)
            {
                sb.AppendLine($"Missing Asset: {asset.AssetID}");
            }
        }

        foreach (var assetFile in GetScenarioAssetFilenames())
        {
            sb.AppendLine(assetFile);
        }

        var stringData = sb.ToString();
        Debug.Log(stringData);

        
        File.WriteAllText(filename, stringData);
    }

    public void SaveFilesToZip(string filename)
    {
        if (File.Exists(filename) || filename == null || ScenarioAssets == null)
            return;

        try
        {

            using (var zipArchive = ZipFile.Open(filename, ZipArchiveMode.Create))
            {
                foreach (var assetFile in GetScenarioAssetFilenames())
                {
                    if (!File.Exists(assetFile))
                    {
                        Debug.LogError($"SaveFilesToZip: Missing asset file: {assetFile}");
                        continue;
                    }

                    zipArchive.CreateEntryFromFile(assetFile, Path.GetFileName(assetFile));                    
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating zip file {filename}: {ex.Message}");
        }
    }

    private IEnumerable<string> GetScenarioAssetFilenames()
    {
        if (ScenarioAssets == null)
            yield break;

        foreach (var asset in ScenarioAssets)
        {
            if (asset.AssetReference == null)
                continue;

            if (asset.AssetReference.ExternalAssetMetadata == null)
                continue; //built in asset

            var metadata = asset.AssetReference.ExternalAssetMetadata;
            yield return Path.Combine(metadata.SourceFolder, metadata.SourceFile);
            yield return Path.Combine(metadata.SourceFolder, metadata.GeometryFilename);

            if (metadata.IconFilename != null)
                yield return Path.Combine(metadata.SourceFolder, metadata.IconFilename);
        }

    }


    private void ReadScenarioData()
    {
        if (_scenario != null)
            return;

        if (FilePath == null)
            return;

        _scenario = SavedScenario.LoadScenario(FilePath);

        if (_scenario != null)
            _header = _scenario.Header;
    }

    public string FilePath;

    public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

    private SavedScenarioHeader _header = null;
    private SavedScenario _scenario = null;
    private bool _headerReadFailed = false;

    private List<ScenarioAssetData> _assetData;

    private StringBuilder _sb = new StringBuilder();

    public void Notify([CallerMemberName] string property = "")
    {
        propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
