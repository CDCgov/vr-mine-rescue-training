using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFireProtocol;
using System.Threading.Tasks;
using Google.Protobuf;
using System.Buffers;
using System.IO;
using DelaunatorSharp;
using System.Linq;

public class VentilationControl : SceneManagerBase, ISerializationCallbackReceiver
{
    public VentilationManager VentilationManager;
    public NetworkManager NetworkManager;
    public StaticVentilationManager StaticVentilationManager;

    public VentilationProvider VentilationProvider;
    //public MineAtmosphere DefaultAtmosphere;
    public string DefaultVectorFieldFile;
    public string DefaultGasFieldFile;

    public bool AutoInitializeVentilation = true;

    public event Action VentilationReady;
    public event Action VentilationUpdated;

    public Bounds VFXBounds = new Bounds(Vector3.zero, new Vector3(120, 1, 120));

    [System.NonSerialized]
    public VentGraph VentGraph;

    public string VentGraphData;


    private MFCConfigureMFire _mfireConfigParameters;
    private MFireServerControl _serverControl;
    private bool _mfireInitialized = false;
    //private MineNetwork _mineNetwork;


    private bool _mfireUpdated = false;
    private bool _ventGraphInitialized = false;

    private byte[] _graphSerialized;

    private Texture2D VentVectorField;
    private float[] _fieldData;

    private Texture2D VentGasReadingField;
    private byte[] _gasReadingFieldData;

    private int _fieldWidth = 100;
    private int _fieldHeight = 100;

    private bool _fieldUpdateInProgress = false;
    private bool _fieldUpdateSkipped = false;

    private VRNVentGraph _originalVentGraph = null;

    private System.Diagnostics.Stopwatch _timer = new System.Diagnostics.Stopwatch();

    private void Awake()
    {
        if (VentGraph == null)
            VentGraph = new VentGraph();
    }

    void Reset()
    {
        VFXBounds = new Bounds(Vector3.zero, new Vector3(120, 1, 120));
    }

    //public void SetAllFans()

    private async void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);
        if (StaticVentilationManager == null)
            StaticVentilationManager = StaticVentilationManager.GetDefault(gameObject);

        //InitializeVectorFields();

        //_mineNetwork = MineNetwork.FindSceneMineNetwork();
        _serverControl = FindObjectOfType<MFireServerControl>();

        if (_serverControl != null)
            _serverControl.MFireSimulationWillUpdate += OnMFireSimulationWillUpdate;

        if (AutoInitializeVentilation)
            await InitializeVentilation();
    }

    private void OnDestroy()
    {
        DestroyVectorFields();
    }


    private void InitializeVectorFields()
    {
        _fieldData = new float[_fieldWidth * _fieldHeight * 2];
        for (int i = 0; i < _fieldData.Length; i++)
            _fieldData[i] = 0;

        VentVectorField = new Texture2D(_fieldWidth, _fieldHeight, TextureFormat.RGFloat, false);
        VentVectorField.SetPixelData<float>(_fieldData, 0);
        VentVectorField.Apply();


        if (DefaultVectorFieldFile != null)
        {
            try
            {
                var filename = Path.Combine(Application.streamingAssetsPath, DefaultVectorFieldFile);
                LoadVectorFieldData(filename);
                VentVectorField.SetPixelData<float>(_fieldData, 0);
                VentVectorField.Apply();
            }
            catch (Exception) { }
        }

        _gasReadingFieldData = new byte[_fieldWidth * _fieldHeight * 4];
        for (int i = 0; i < _gasReadingFieldData.Length; i++)
            _gasReadingFieldData[i] = 0;


        VentGraph.UpdateFieldMineGeometry(_gasReadingFieldData, _fieldWidth, _fieldHeight, VFXBounds);

        VentGasReadingField = new Texture2D(_fieldWidth, _fieldHeight, TextureFormat.RGBA32, false);
        VentGasReadingField.SetPixelData<byte>(_gasReadingFieldData, 0);
        VentGasReadingField.Apply();


        if (DefaultGasFieldFile != null)
        {
            try
            {
                var filename = Path.Combine(Application.streamingAssetsPath, DefaultGasFieldFile);
                LoadGasFieldData(filename);
                VentGasReadingField.SetPixelData<byte>(_gasReadingFieldData, 0);
                VentGasReadingField.Apply();
            }
            catch (Exception) { }
        }
    }

    private void DestroyVectorFields()
    {
        _fieldData = null;
        _gasReadingFieldData = null;
        if (VentVectorField != null)
        {
            Destroy(VentVectorField);
            VentVectorField = null;
        }

        if (VentGasReadingField != null)
        {
            Destroy(VentGasReadingField);
            VentGasReadingField = null;
        }
    }


    public void ClearVentGraph()
    {
        VentGraph = new VentGraph();
    }

    private void OnMFireSimulationWillUpdate(MFireServerControl obj)
    {
        VentGraph.ResetControlResistance();

        if (VentilationManager != null)
            VentilationManager.RaiseVentilationWillUpdate();

        VentGraph.UpdateAirwayResistance();
    }


    public bool IsVentilationReady
    {
        get
        {
            if (VentilationProvider == VentilationProvider.MFIRE)
            {
                return _mfireInitialized;
            }
            else
                return true;
        }
    }


    public bool AutoAdvanceEnabled
    {
        get
        {
            if (!_mfireInitialized || _serverControl == null)
                return false;

            return _serverControl.AutoAdvanceEnabled;
        }
        set
        {
            if (_serverControl != null)
                _serverControl.AutoAdvanceEnabled = value;
        }
    }


    public float GetSimulationTime()
    {
        if (_serverControl == null)
            return 0;

        return (float)_serverControl.MFireElapsedTime;
    }

    public void AdvanceSimulation()
    {
        if (_serverControl == null)
            return;

        Debug.Log("Vent: Advancing Sim");

        _serverControl.AdvanceMFireSimulation();
    }

    public void ResetSimulation()
    {
        if (_serverControl == null)
            return;

        Debug.Log("Vent: Reseting Sim");

        if (_originalVentGraph != null)
        {
            if (VentGraph == null)
                VentGraph = new VentGraph();

            VentGraph.Reset();
            VentGraph.LoadFrom(_originalVentGraph);
        }

        //_serverControl.ResetMFireSimulation();
        _ = InitializeMFIRE();
    }


    public async Task InitializeVentilation()
    {
        if (NetworkManager == null)
            return;

        InitializeVectorFields();

        if (NetworkManager.IsServer && !NetworkManager.IsPlaybackMode)
        {
            if (VentGraph != null && VentGraph.NumAirways > 0 && VentGraph.NumJuncions > 0)
            {
                _originalVentGraph = new VRNVentGraph();
                VentGraph.SaveTo(_originalVentGraph);
            }

            if (VentilationProvider == VentilationProvider.MFIRE)
                await InitializeMFIRE();

            if (_mfireInitialized || VentilationProvider != VentilationProvider.MFIRE)
            {
                Debug.Log("MFIRE: Ventilation Initialized");
                VentilationReady?.Invoke();
            }
        }
        else
        {
            Debug.Log("Ventilation ready as client");
            VentilationReady?.Invoke();
        }

    }

    public VentJunction FindClosestJunction(Vector3 worldPos)
    {
        return VentGraph.FindClosestJunction(worldPos);
    }

    /// <summary>
    /// Retrieve mine atmosphere information at the specified world position
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public bool GetMineAtmosphere(Vector3 worldPos, out MineAtmosphere mineAtmosphere, bool useRaycast)
    {
        MineAtmosphere defaultAtmosphere;

        if (VentilationManager != null)
            defaultAtmosphere = VentilationManager.DefaultAtmosphere;
        else
            defaultAtmosphere = MineAtmosphere.NormalAtmosphere;

        //start with zero atmosphere values
        mineAtmosphere = new MineAtmosphere(0,0,0,0);
        mineAtmosphere.SetStrength(0);

        //float staticAtmosphereStrength = 0;

        if (StaticVentilationManager != null)
        {
            StaticVentilationManager.GetMineAtmosphere(worldPos, out mineAtmosphere);//, out staticAtmosphereStrength);
        }

        if (VentilationProvider == VentilationProvider.MFIRE /*&& staticAtmosphereStrength < 1 */&& VentGraph != null)
        {
            //compute MFIRE calculated atmosphere
            var dynamicAtmosphere = VentGraph.ComputeLocalMineAtmosphere(worldPos, useRaycast);
            var oxygen = 0.21f - (0.22f * dynamicAtmosphere.Methane);
            if (oxygen < 0.10f)
                oxygen = 0.10f;

            dynamicAtmosphere.Oxygen = oxygen;

            //use MFIRE atmosphere as default where not overridden by static

            mineAtmosphere.Normalize(dynamicAtmosphere);
        }
        else
        {
            //normalize static atmosphere usind default
            mineAtmosphere.Normalize(defaultAtmosphere);
        }

        //if (staticAtmosphereStrength < 1 && VentilationProvider == VentilationProvider.MFIRE)
        //{
        //    var oxygen = 0.21f - (0.22f * mineAtmosphere.Methane);
        //    if (oxygen < 0.10f)
        //        oxygen = 0.10f;

        //    mineAtmosphere.Oxygen = oxygen;
        //}

        return true;
    }

    public void LoadVentGraphData(VRNVentGraph vrnGraph)
    {
        if (VentGraph == null || vrnGraph.Airways.Count != VentGraph.NumAirways ||
            vrnGraph.Junctions.Count != VentGraph.NumJuncions)
        {
            VentGraph = new VentGraph();
        }

        VentGraph.LoadFrom(vrnGraph);

        _ventGraphInitialized = true;
        _mfireUpdated = true;
    }


    protected async Task InitializeMFIRE()
    {
        if (_serverControl == null)
        {
            Debug.LogError($"MFIRE: Couldn't find MFIRE Server Control");
            return;
        }

        GameObject mineNetworkObj = GameObject.Find("MineNetwork");
        IVentGraphBuilder graphBuilder = null;
        if (mineNetworkObj != null)
        {
            graphBuilder = mineNetworkObj.GetComponent<IVentGraphBuilder>();
        }

        if (VentGraph == null)
        {
            VentGraph = new VentGraph();
        }

        if (VentGraph.NumAirways <= 0 || VentGraph.NumJuncions <= 0)
        {
            VentGraph.Reset();

            //for now rebuild the ventilation graph from the mine segments
            //RebuildFromMineNetwork();


            if (graphBuilder != null)
            {
                graphBuilder.BuildVentGraph(VentGraph);
            }
        }
        else
        {
            VentGraph.ResetVentilationData();
            if (graphBuilder != null)
            {
                graphBuilder.UpdateVentGraph(VentGraph);
            }

        }

        VentilationManager.DestroyVentUI();
        VentilationManager.RaiseVentGraphReset();

        VentGraph.ResetControlResistance();

        EngineState? engineState = null;
        try
        {
            _serverControl.ServerConnection.SendMFireCmd(new MFCResetSimulation());
            engineState = await _serverControl.GetEngineState();

            if (engineState == null)
            {
                Debug.Log("MFIRE: Couldn't connect to MFIRE server");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error communicating with MFIRE Server {ex.Message}");
            return;
        }

        Debug.Log($"MFIRE: Connected to MFIRE, engine state {engineState.ToString()}");

        ////for now rebuild the ventilation graph from the mine segments
        //RebuildFromMineNetwork();

        //get any updates from ventilation controls
        VentilationManager.RaiseVentilationWillUpdate();

        //build the MFIRE configuration
        if (!VentGraph.BuildMFireConfig(_serverControl))
            return;

        _mfireConfigParameters = new MFCConfigureMFire();

        var startJunc = VentGraph.GetStartJunction();
        if (startJunc != null)
        {
            _mfireConfigParameters.StartJunction = startJunc.MFireID;
        }

        Debug.LogFormat("Sending MFire {0} Junctions, {1} Airways, {2} Fans", _serverControl.GetNumJunctions(),
                _serverControl.GetNumAirways(), _serverControl.GetNumFans());

        if (!_serverControl.ValidateConfig())
        {
            Debug.Log("Failed to build valid MFire Network!");
            return;
        }


        //var date = System.DateTime.Now.ToString("yyyy-dd-M_HH-mm-ss");
        //var filename = $"VentGraph-{date}.json";
        //var json = VentGraph.SaveToJSON();
        //File.WriteAllText(filename, json);
        //Debug.Log($"Saved VentGraph to {filename}");

        //_serverControl.ServerConnection.SendMFireCmd(_mfireConfigParameters);
        _serverControl.SetMFIREConfigParameters(_mfireConfigParameters);
        _serverControl.SendMFireConfig();


        _serverControl.ServerConnection.SendRunSimulation();

        _serverControl.MFireSimulationUpdated += OnMFIREUpdated;
        _mfireInitialized = true;

        //enable auto-advance by default
        AutoAdvanceEnabled = true;
    }

    private void OnMFIREUpdated()
    {
        Debug.Log("MFIRE: Received sim update");

        //copy data into the vent classes
        VentGraph.UpdateFromSimulation(_serverControl);
        _ventGraphInitialized = true;

        //set flag and process in Update() - this call is not on the main thread
        _mfireUpdated = true;



        //SimulationChanged?.Invoke();
    }

    private void ProcessMFIREUpdate()
    {
        UpdateVectorField();

        _mfireUpdated = false;
        VentilationUpdated?.Invoke();
    }

    private async void UpdateVectorField()
    {
        if (_fieldUpdateInProgress)
        {
            _fieldUpdateSkipped = true;
            return;
        }

        if (VentGraph != null)
        {
            _fieldUpdateInProgress = true;
            try
            {

                _timer.Restart();


                await Task.Run(() =>
                {
                    VentGraph.UpdateVectorField2D(_fieldData, _fieldWidth, _fieldHeight, VFXBounds);
                    VentGraph.UpdateFieldGasReadings(_gasReadingFieldData, _fieldWidth, _fieldHeight, VFXBounds);
                });

                _timer.Stop();
                Debug.Log($"Calculating Vent Vector Field took {_timer.ElapsedMilliseconds}ms");

                _timer.Restart();
                VentVectorField.SetPixelData<float>(_fieldData, 0);
                VentVectorField.Apply();

                VentGasReadingField.SetPixelData<byte>(_gasReadingFieldData, 0);
                VentGasReadingField.Apply();

                _timer.Stop();
                Debug.Log($"Updating Vent Vector Field took {_timer.ElapsedMilliseconds}ms");
            }
            finally
            {
                _fieldUpdateInProgress = false;
            }
        }
    }

    public Texture2D GetVectorField()
    {
        return VentVectorField;
    }

    public Texture2D GetGasField()
    {
        return VentGasReadingField;
    }

    public Bounds GetVectorFieldBounds()
    {
        return VFXBounds;
    }

    private void Update()
    {
        if (_mfireUpdated)
            ProcessMFIREUpdate();

        if (_fieldUpdateSkipped && !_fieldUpdateInProgress)
        {
            _fieldUpdateSkipped = false;
            UpdateVectorField();
        }

        if (Input.GetKeyDown(KeyCode.V) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift))
        {
            //SaveFieldData();
        }
    }

    private void SaveFieldData()
    {
        var folder = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string vectorFieldFile = Path.Combine(folder, "VentVectorField.bin");
        string gasFieldFile = Path.Combine(folder, "VentGasField.bin");


        if (!File.Exists(vectorFieldFile) && _fieldData != null)
        {
            using (var fs = new FileStream(vectorFieldFile, FileMode.Create))
            {
                using (var writer = new BinaryWriter(fs))
                {
                    writer.Write((uint)_fieldData.Length);
                    for (int i = 0; i < _fieldData.Length; i++)
                    {
                        writer.Write(_fieldData[i]);
                    }
                }
            }
            Debug.Log($"Saved vector field to {vectorFieldFile}");
        }

        if (!File.Exists(gasFieldFile) && _gasReadingFieldData != null)
        {
            File.WriteAllBytes(gasFieldFile, _gasReadingFieldData);
            Debug.Log($"Saved gas field to {gasFieldFile}");
        }
    }

    private void LoadVectorFieldData(string filename)
    {
        using (var fs = new FileStream(filename, FileMode.Open))
        {
            using (var reader = new BinaryReader(fs))
            {
                var count = reader.ReadUInt32();
                if (count != _fieldData.Length)
                    throw new InvalidDataException("Field data is the wrong size");

                for (int i = 0; i < count; i++)
                {
                    _fieldData[i] = reader.ReadSingle();
                }
            }
        }
        Debug.Log($"Read vector field data from {filename}");
    }

    private void LoadGasFieldData(string filename)
    {
        var data = File.ReadAllBytes(filename);
        if (data.Length != _gasReadingFieldData.Length)
            throw new InvalidDataException("Gas reading data is the wrong size");

        _gasReadingFieldData = data;
        Debug.Log($"Read vector field data from {filename}");

    }

    private void SaveFieldsToPNG()
    {
        var folder = Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        string vectorFieldFile = Path.Combine(folder, "VentVectorField.png");
        string gasFieldFile = Path.Combine(folder, "VentGasField.png");

        var vectorField = GetVectorField();
        var gasField = GetGasField();

        if (!File.Exists(vectorFieldFile) && vectorField != null)
        {
            var pngBytes = vectorField.EncodeToPNG();
            File.WriteAllBytes(vectorFieldFile, pngBytes);
            Debug.Log($"Saved vector field to {vectorFieldFile}");
        }

        if (!File.Exists(gasFieldFile) && gasField != null)
        {
            var pngBytes = gasField.EncodeToPNG();
            File.WriteAllBytes(gasFieldFile, pngBytes);
            Debug.Log($"Saved gas field to {gasFieldFile}");
        }
    }

    public void OnBeforeSerialize()
    {
        if (VentGraph != null)
        {
            VRNVentGraph vrnGraph = new VRNVentGraph();
            VentGraph.SaveTo(vrnGraph);

            int size = vrnGraph.CalculateSize();
            //var data = new Span<byte>();

            //vrnGraph.WriteTo(data);
            //Debug.Log($"Span: {data.Length}, {data[0]} {data[1]}");

            //byte[] data = new byte[size];
            if (_graphSerialized == null || _graphSerialized.Length != size)
                _graphSerialized = new byte[size];

            MemoryStream memStream = new MemoryStream(_graphSerialized);
            vrnGraph.WriteTo(memStream);

            var base64data = Convert.ToBase64String(_graphSerialized);
            //Debug.Log(base64data);
            VentGraphData = base64data;
        }
    }

    public void OnAfterDeserialize()
    {
        if (VentGraphData != null && VentGraphData.Length > 0)
        {
            var data = Convert.FromBase64String(VentGraphData);

            VRNVentGraph vrnGraph = VRNVentGraph.Parser.ParseFrom(data);
            if (vrnGraph != null)
            {
                if (VentGraph == null)
                    VentGraph = new VentGraph();

                VentGraph.LoadFrom(vrnGraph);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (VFXBounds == null)
            return;

        Gizmos.color = new Color(1, 1, 1, 0.15f);

        Gizmos.DrawCube(VFXBounds.center, VFXBounds.size);
    }

    public void TestAutogen()
    {
        Delaunator delaunator = new Delaunator(VentGraph.GetJunctions().ToArray());

        delaunator.ForEachTriangleEdge(edge =>
        {
            VentAirway airway = new VentAirway();
            airway.Start = (VentJunction)edge.P;
            airway.End = (VentJunction)edge.Q;
            VentGraph.AddAirway(airway);
            //VentGraph.AddAirway()
            //CreateLine(TrianglesContainer, $"TriangleEdge - {edge.Index}", new Vector3[] { edge.P.ToVector3(), edge.Q.ToVector3() }, triangleEdgeColor, triangleEdgeWidth, 0);

            /*if (drawTrianglePoints)
            {
                var pointGameObject = Instantiate(trianglePointPrefab, PointsContainer);
                pointGameObject.transform.SetPositionAndRotation(edge.P.ToVector3(), Quaternion.identity);
            }*/
        });
    }
}
