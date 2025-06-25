using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;
using YamlDotNet.Serialization;
using System;

public enum SystemType
{
    Unknown,
    Desktop,
    CAVE,
    HMD
}

public enum WandType
{
    Unknown,
    CapLamp,
    Hand
}

public enum PlatformType
{
    Collocation,
    Standing,
    Seated,
    Desktop
}

public enum DebriefScreenActivationState
{
    ScreenOneActive,
    ScreenTwoActive,
    BothActive
}

public enum MeasurementUnits
{
    Unknown = 0,
    Metric = 1,
    Imperial = 2,
}

public struct YAMLVec3
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    public YAMLVec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static YAMLVec3 FromVector3(Vector3 v)
    {
        return new YAMLVec3(v.x, v.y, v.z);
    }

    public Vector3 ToVector3()
    {
        return new Vector3(this.x, this.y, this.z);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is YAMLVec3))
            return false;

        var other = (YAMLVec3)obj;

        if (x == other.x &&
            y == other.y &&
            z == other.z)
        {
            return true;
        }

        return false;
    }

    public static bool operator == (YAMLVec3 a, YAMLVec3 b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(YAMLVec3 a, YAMLVec3 b)
    {
        return !a.Equals(b);
    }
}

public struct YAMLQuaternion
{
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }
    public float w { get; set; }

    public YAMLQuaternion(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static YAMLQuaternion FromQuaternion(Quaternion q)
    {
        return new YAMLQuaternion(q.x, q.y, q.z, q.w);
    }

    public Quaternion ToQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }
}

public struct YAMLPlayerRoleConfig
{
    public string PlayerName { get; set; }
    public VRNPlayerRole PlayerRole { get; set; }
}

[System.Serializable]
public struct SavedPosition
{
    public SavedPosition(Vector3 position, Quaternion rotation, float lightLevel)
    {
        pos = YAMLVec3.FromVector3(position);
        rot = YAMLQuaternion.FromQuaternion(rotation);
        researcherLightLevel = lightLevel;
    }

    public YAMLVec3 pos { get; set; }
    public YAMLQuaternion rot { get; set; }
    public float researcherLightLevel { get; set; }
}

public class SystemConfig : YAMLConfig
{
    public class WandConfig
    {
        public string SegmentName { get; set; }
        public WandType WandType { get; set; }
    }

    //Values for other build types e.g. CAVE (ignored in YAML)

    [YamlIgnore]
    [Description("Valid values are Desktop, CAVE, or HMD")]
    public SystemType SystemType { get; set; }

    [YamlIgnore]
    [Description("Number of screens in CAVE configuration")]
    public int NumScreens { get; set; }

    [YamlIgnore]
    [Description("Overlap in degrees between screens")]
    public float Overlap { get; set; }

    [YamlIgnore]
    public float CAVEUIScale { get; set; }

    [YamlIgnore]
    public RGBColor CAVEBackgroundColor { get; set; }

    [YamlIgnore]
    [Description("IP Address of the computer running the Vicon data server")]
    public string ViconIP { get; set; }


    [YamlIgnore]
    public List<WandConfig> WandConfiguration { get; set; }

    [YamlIgnore]
    [Description("Offset in meters applied to motion tracked objects")]
    public YAMLVec3 MocapPositionOffset { get; set; }

    [YamlIgnore]
    [Description("Segment name of the tracker used to determine movement direction")]
    public string MovementMocapSegment { get; set; }


    [YamlIgnore]
    public Dictionary<int, SavedPosition> SavedPointsOfInterest { get; set; }

    [YamlIgnore]
    [Description("Default platform type (i.e. Collocation, Standing, Seated, Desktop")]
    public PlatformType PlatformType { get; set; }

    [YamlIgnore]
    public DebriefScreenActivationState DebriefScreenActivationState { get; set; }


    //TODO: Old save file path - should not be used & should be removed
    [YamlIgnore]
    public string DebriefFilePathOverride { get; set; }


    // Software settings

    [Description("Boundary grid distances (m) - Positive Z is inby, Positive X to the right facing inby")]
    public float PositiveXExtentDistance { get; set; }
    public float NegativeXExtentDistance { get; set; }
    public float PositiveZExtentDistance { get; set; }
    public float NegativeZExtentDistance { get; set; }

    [Description("Distance to boundary (m) when grid appears")]
    public float ExtentWarningDistance { get; set; }

    [Description("Root folder for VR-MRT folder structure")]
    public string RootDataFolderOverride { get; set; }

    [Description("Default units displayed in scenario editor (i.e. Imperial, Metric)")]
    public MeasurementUnits DisplayUnits { get; set; }

    [Description("Default UI Scale in director mode")]
    public float UIScale { get; set; }

    [Description("Default grid settings for scenario editor")]
    public float GridWidth { get; set; }
    public float GridLength { get; set; }
    public float GridSpacing { get; set; }
    public RGBColor GridColor { get; set; }

    [Description("Default multiplayer server address")]
    public string MultiplayerServer { get; set; }

    [Description("Default multiplayer port")]
    public int MultiplayerPort { get; set; }

    [Description("Name of last recorded session in director mode")]
    public string SessionName { get; set; }

    [YamlIgnore]
    [Description("Start as server by default when launching without a menu")]
    public bool DefaultToServerMode { get; set; }

    [Description("Default name for multiplayer client")]
    public string MultiplayerName { get; set; }

    [Description("Default update rate for VR positions sent to server")]
    public float MPVRUpdateRateHz { get; set; }
    [Description("Default update rate for object data sent to server")]
    public float MPObjectUpdateRateHz { get; set; }


    [Description("Walking speed in m/s for joystick movement in VR")]
    public float VRMovementSpeed { get; set; }

    [Description("If true, shows all map symbols on debug UI map in VR for single player and multiplayer, e.g. bad roof, curtains, RA")]
    public bool ShowAllMapSymbolsInDebugMap { get; set; }

    [Description("Scale multiplier of map board in VR")]
    public YAMLVec3 MapBoardScale { get; set; }


    [Description("Distance to map board to show pencil instead of glove")]
    public float MapPencilActivationDist { get; set; }

    [Description("Maximum distance for the map man to place automatic gas checks (-1 = Infinite)")]
    public float MapManMaxDistance { get; set; }

    [Description("Default resistance value for curtains")]
    public float DefaultCurtainResistance { get; set; }

    [Description("Replace player names with random identifiers in debrief module")]
    public bool DebriefRandomizePlayerNames { get; set; }

    [Description("Forward vector for initial calibration orientation in the coordinate space of the controller. Default 0,0,1 for Quest 3")]
    public YAMLVec3 CalibrationInitialForwardVector { get; set; }

    //debug values

    [Description("Debug: Enable network stats log")]
    public bool MPPacketStatLogEnabled { get; set; }

    [Description("Internal calibration settings")]
    public float TrackingMinStability { get; set; }
    public float TrackingAllowedRange { get; set; }
    public int TrackingNumSamples { get; set; }
    public float TrackingMaxPosError { get; set; }
    public float TrackingMaxAngleError { get; set; }
    public YAMLVec3 CalibrationOffset { get; set; }
    public YAMLQuaternion CalibrationRotation { get; set; }

    [Description("Internal player role assignements")]
    public Dictionary<string, VRNPlayerRole> PlayerRoleConfig { get; set; }

    [Description("Internal settings")]
    public float DistanceUnit { get; set; }
    public List<string> FavoriteScenes { get; set; }
    public bool UseMaterialTextureCache { get; set; }

    //Runtime Settings
    [YamlIgnore]
    public bool GridEnabled { get; set; }


    [YamlIgnore]
    public string ExternalAssetsFolder 
    { 
        get
        {
            return GetSubfolder("ExternalAssets");
        } 
    }


    [YamlIgnore]
    public string SessionLogsFolder
    {
        get
        {
            return GetSubfolder("VRMineSessionLogs");
        }
    }

    [YamlIgnore]
    public string ScenariosFolder
    {
        get
        {
            return GetSubfolder("Scenarios");
        }
    }

    [YamlIgnore]
    public string MaterialsFolder
    {
        get
        {
            return GetSubfolder("CustomMaterials");
        }
    }

    [YamlIgnore]
    public string RootDataFolder
    {
        get
        {
            string folder = null;
            if (RootDataFolderOverride == null || RootDataFolderOverride.Length <= 0 || !Directory.Exists(RootDataFolderOverride))
            {
                var userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                folder = Path.Combine(userprofile, "VRMine");
                Directory.CreateDirectory(folder);
            }
            else
            {
                //folder = Path.Combine(RootDataFolderOverride, "VRMine");
                folder = RootDataFolderOverride;
            }

            return folder;
        }
    }


    private string GetSubfolder(string folderName)
    {
        var folder = Path.Combine(RootDataFolder, folderName);
        Directory.CreateDirectory(folder);

        return folder;
    }


    //private YAMLVec3 _calibrationTestPoint;
    private Vector3 _calibrationTestPoint;
    public YAMLVec3 CalibrationTestPoint
    {
        get 
        {
            //return _calibrationTestPoint; 
            return new YAMLVec3(_calibrationTestPoint.x, _calibrationTestPoint.y, _calibrationTestPoint.z);
        }
        set
        {
            CalibrationTestPointVec3 = value.ToVector3();
            //if (_calibrationTestPoint == value)
            //    return;
            //_calibrationTestPoint = value;
            //try
            //{
            //    CalibrationTestPointChanged?.Invoke();
            //}  catch (System.Exception) { }
        }
    }

    [YamlIgnore]
    public Vector3 CalibrationTestPointVec3
    {
        get { return _calibrationTestPoint; }
        set
        {
            if (_calibrationTestPoint == value)
                return;

            _calibrationTestPoint = value;
            try
            {
                CalibrationTestPointChanged?.Invoke();
            }
            catch (System.Exception) { }
        }
    }


    public event System.Action CalibrationTestPointChanged;


    //public static string GetDefaultSessionLogPath()
    //{
    //    var userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    //    var folder = Path.Combine(userprofile, "VRMine", "VRMineSessionLogs");
    //    Directory.CreateDirectory(folder);

    //    return folder;
    //}

    //public static string GetDefaultScenarioFilePath()
    //{
    //    var userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    //    var folder = Path.Combine(userprofile, "VRMine", "Scenarios");
    //    Directory.CreateDirectory(folder);

    //    return folder;
    //}


    public override void LoadDefaults()
    {
        SystemType = SystemType.Desktop;
        NumScreens = 6;
        Overlap = 40;
        CAVEUIScale = 1.0f;
        CAVEBackgroundColor = new RGBColor();
        ViconIP = "127.0.0.1";
        MocapPositionOffset = new YAMLVec3(0, -1.5f, 0);
        MovementMocapSegment = "Wand1:Wand1";

        TrackingMinStability = 0.3f;
        TrackingAllowedRange = 2.0f;
        TrackingNumSamples = 400;
        TrackingMaxPosError = 0.3f;
        TrackingMaxAngleError = 3.0f;

        CalibrationInitialForwardVector = new YAMLVec3(0, 0, 1);
        CalibrationOffset = new YAMLVec3(0, 0, 0);
        CalibrationRotation = YAMLQuaternion.FromQuaternion(Quaternion.identity);

        //_calibrationTestPoint = new YAMLVec3(2.5f, 0, 2.5f);
        _calibrationTestPoint = new Vector3(2.5f, 0, 2.5f);

        SessionName = "";
        //SessionLogFolder = GetDefaultSessionLogPath();//SessionLog.GetDefaultLogPath();

        MultiplayerServer = "127.0.0.1";
        MultiplayerPort = 9090;
        DefaultToServerMode = false;
        MultiplayerName = SystemInfo.deviceName;

        MPVRUpdateRateHz = 40;
        MPObjectUpdateRateHz = 25;
        MPPacketStatLogEnabled = false;
        VRMovementSpeed = 1.5f;

        ShowAllMapSymbolsInDebugMap = true;

        UIScale = 1.0f;
        DebriefRandomizePlayerNames = false;

        WandConfiguration = new List<WandConfig>();

        WandConfig wand;

        wand = new WandConfig();
        wand.SegmentName = "Wand1:Wand1";
        wand.WandType = WandType.CapLamp;
        WandConfiguration.Add(wand);

        wand = new WandConfig();
        wand.SegmentName = "Wand2:Wand2";
        wand.WandType = WandType.CapLamp;
        WandConfiguration.Add(wand);

        SavedPointsOfInterest = new Dictionary<int, SavedPosition>();
        SavedPointsOfInterest.Add(1, new SavedPosition(Vector3.up, Quaternion.identity, 0));

        PlayerRoleConfig = new Dictionary<string, VRNPlayerRole>();
        PlayerRoleConfig.Add("Captain", VRNPlayerRole.Captain);
        PlayerRoleConfig.Add("MapMan", VRNPlayerRole.MapMan);
        PlayerRoleConfig.Add("GasMan", VRNPlayerRole.GasMan);
        PlayerRoleConfig.Add("SecondGasMan", VRNPlayerRole.SecondGasMan);
        PlayerRoleConfig.Add("TailCaptain", VRNPlayerRole.TailCaptain);
        DebriefFilePathOverride = Application.dataPath + "/../SavedLogs";
        PlatformType = PlatformType.Collocation;
        DebriefScreenActivationState = DebriefScreenActivationState.ScreenOneActive;
        DistanceUnit = 3.28084f;
        PositiveXExtentDistance = 9;
        NegativeXExtentDistance = 9;
        PositiveZExtentDistance = 9;
        NegativeZExtentDistance = 9;
        ExtentWarningDistance = 4;

        FavoriteScenes = new List<string>();
        FavoriteScenes.Add("VRWaitingRoom");

        //ScenarioEditorUIScale = 1;

        DisplayUnits = MeasurementUnits.Imperial;
        RootDataFolderOverride = null;
        GridColor = RGBColor.FromColor(Color.white);
        GridLength = 91.44f;
        GridWidth = 91.44f;
        GridSpacing = 304.8f;

        MapBoardScale = new YAMLVec3(1.25f, 1.25f, 1.25f);
        MapPencilActivationDist = 0.25f;
        DefaultCurtainResistance = 10.0f;
        MapManMaxDistance = 15;

        UseMaterialTextureCache = true;

        GridEnabled = false;
    }
}

public class RGBColor
{
    public float r { get; set; }
    public float g { get; set; }
    public float b { get; set; }

    public Color ToColor()
    {
        return new Color(r, g, b);
    }

    public static RGBColor FromColor(Color c)
    {
        RGBColor rgb = new RGBColor();
        rgb.r = c.r;
        rgb.g = c.g;
        rgb.b = c.b;

        return rgb;
    }
}