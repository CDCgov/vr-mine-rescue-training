using NIOSH_MineCreation;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;

public class Vector3Converter : JsonConverter<Vector3>
{
    private struct JVector3
    {
        public float x;
        public float y;
        public float z;
    }

    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jvec3 = serializer.Deserialize<JVector3>(reader);
        return new Vector3(jvec3.x, jvec3.y, jvec3.z);
    }

    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        var jvec3 = new JVector3
        {
            x = value.x,
            y = value.y,
            z = value.z,
        };

        serializer.Serialize(writer, jvec3);
    }
}

public class QuaternionConverter : JsonConverter<Quaternion>
{
    private struct JQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jquat = serializer.Deserialize<JQuaternion>(reader);

        return new Quaternion(jquat.x, jquat.y, jquat.z, jquat.w);
    }

    public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
    {

        var jquat = new JQuaternion
        {
            x = value.x,
            y = value.y,
            z = value.z,
            w = value.w,
        };

        serializer.Serialize(writer, jquat);
    }

}

[Serializable]
public class SavedScenario
{
    [NonSerialized]
    public SavedScenarioHeader Header;

    public string ScenarioName;
    public GlobalMineParameters GlobalSettings;
    public string MapData;

    [SerializeField]
    [JsonProperty]
    List<SavedAsset> assets = new List<SavedAsset>();

    //12 character version string
    private const int NumVersionBytes = 12;
    private const string ScenarioFileVersion = "VRMine-v2   ";

    //private byte[] _scenarioFileVersionBytes;

    public SavedScenario()
    {
        GlobalSettings = new GlobalMineParameters();
        Header = new SavedScenarioHeader();
        Header.CreationDateTime = DateTime.Now;        
    }

    public SavedScenario(List<SavedAsset> assets)
    {
        this.assets = assets;
    }

    //public void InitializeGlobalParameters(float dust)
    //{
    //    if(parameters == null)
    //    {
    //        parameters = new GlobalMineParameters(dust);
    //    }
    //    else
    //    {
    //        parameters.SetRockDustLevel(dust);
    //    }
    //}

    //public GlobalMineParameters GetGlobalMineParameters()
    //{
    //    return parameters;
    //}

    public List<SavedAsset> GetSavedAssets()
    {
        return assets;
    }

    public void AddAssetToList(SavedAsset asset)
    {
        if(!assets.Contains(asset))
        {
            assets.Add(asset);
        }
    }

    public string GetScenarioName()
    {
        return ScenarioName;
    }

    public void SetScenarioName(string name)
    {
        ScenarioName = name;
    }

    public void SaveScenario(string filename)
    {
        //JSONSerializer.Serialize<SavedScenario>(this, filename);
        var serializer = CreateSerializer();

        //var versionBytes = ASCIIEncoding.ASCII.GetBytes(ScenarioFileVersion);

        //if (versionBytes.Length != NumVersionBytes)
        //    throw new Exception($"SavedScenario: Error version string length incorrect: {versionBytes.Length} bytes, expected {NumVersionBytes}");        

        using (var streamWriter = new StreamWriter(filename, false, Encoding.UTF8))
        using (var writer = new JsonTextWriter(streamWriter))
        {
            //streamWriter.BaseStream.Write(versionBytes, 0, NumVersionBytes);
            streamWriter.WriteLine(ScenarioFileVersion);

            if (Header == null)
            {
                Header = new SavedScenarioHeader();
                Header.CreationDateTime = DateTime.Now;
            }

            GitVersion.ReadVersion();

            Header.ModifiedDateTime = DateTime.Now;
            Header.ScenarioName = ScenarioName;
            Header.VRMineVersion = GitVersion.Version;


            serializer.Serialize(writer, Header);
            streamWriter.WriteLine();
            serializer.Serialize(writer, this);
        }
    }

    private static string ReadVersion(StreamReader reader)
    {
        //TODO: may be slow on V1 files, verify only reads up to buffer length not entire file
        return reader.ReadLine();
    }

    private static JsonSerializer CreateSerializer()
    {
        var serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        serializer.Converters.Add(new Vector3Converter());
        serializer.Converters.Add(new QuaternionConverter());
        
        return serializer;
    }

    public static SavedScenario LoadScenario(string filename)
    {
        SavedScenario savedScenario = null;
        var serializer = CreateSerializer();
        //byte[] version = new byte[NumVersionBytes];
        //var versionBytes = ASCIIEncoding.ASCII.GetBytes(ScenarioFileVersion);

        using (var streamReader = new StreamReader(filename, Encoding.UTF8, false, 512))
        using (var reader = new JsonTextReader(streamReader))
        {
            reader.SupportMultipleContent = true;
            //int bytesRead = streamReader.BaseStream.Read(version, 0, NumVersionBytes);
            //if (bytesRead != NumVersionBytes)
            //    throw new System.ApplicationException($"SavedScenario: Couldn't read scenario file version from {filename}");

            //if (((ReadOnlySpan<byte>)(version)).SequenceCompareTo(versionBytes) != 0)
            //{
            //    reader.Close();
            //    return LoadScenarioV1(filename);
            //}

            var fileVersion = ReadVersion(streamReader);
            if (fileVersion != ScenarioFileVersion)
            {
                reader.Close();
                return LoadScenarioV1(filename);
            }

            var header = serializer.Deserialize<SavedScenarioHeader>(reader);
            if (header == null)
                throw new System.ApplicationException($"SavedScenario: Couldn't read header from {filename}");


            //var nextLine = streamReader.ReadLine();
            //Debug.Log($"Next line: {nextLine}");
            reader.Read();

            savedScenario = serializer.Deserialize<SavedScenario>(reader);
            if (savedScenario == null)
                throw new System.ApplicationException($"SavedScenario: Couldn't read SavedScenario from {filename}");

            savedScenario.Header = header;
        }

        return savedScenario;
    }

    private static SavedScenario LoadScenarioV1(string filename)
    {
        string json = JSONFileManagement.LoadJSONAsString(filename);
        SavedScenario savedScenario = JSONSerializer.Deserialize<SavedScenario>(json);

        return savedScenario;
    }

}


[Serializable]
public class GlobalMineParameters
{
    public MineSettings MineSettings;
    public Vector3 MineScale;
    public float RockDustLevel;    
    public string SkyboxID;
    public float MineMapSymbolScale;
    public float MineMapLineWidthScale;
    public float MineMapGridSize;

    public string MinerProfileID;
    public float BG4DurationMinutes;
    public float MasterVolume;
    public bool AllowSelfCalibration;
    public bool AlarmEnabled;
    public bool AlarmEnabledAllowToggle;
    public bool BlockCameraOutOfBounds;

    public bool UseMFire;
    public MineAtmosphere StaticAtmosphere;

    [System.NonSerialized]
    public string MapDataFile;
    [System.NonSerialized]
    public MinerProfile MinerProfile;

    public GlobalMineParameters()
    {
        ResetSettings();
    }

    public void ResetSettings()
    {
        MineSettings = new MineSettings();
        MineScale = Vector3.one;
        RockDustLevel = 0;
        SkyboxID = "UNDERGROUND";
        MineMapSymbolScale = 1.0f;
        MineMapLineWidthScale = 1.0f;
        MineMapGridSize = 1.0f;

        MinerProfileID = null;
        BG4DurationMinutes = 20;
        MasterVolume = 1.0f;
        AllowSelfCalibration = true;
        AlarmEnabled = false;
        AlarmEnabledAllowToggle = false;
        BlockCameraOutOfBounds = false;

        UseMFire = true;
        StaticAtmosphere = MineAtmosphere.NormalAtmosphere;

        MapDataFile = null;
        MinerProfile = null;
    }

}
