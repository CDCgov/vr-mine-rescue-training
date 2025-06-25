using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using UnityEngine.XR;

#pragma warning disable 0219

public enum ProxShell
{
    YellowShell = 0,
    RedShell = 1
}

public class DeformableProxSystem : ProxSystem, ISerializationCallbackReceiver
{
    public const int PROX_VIS_HORZ_DENSITY = 4; //shell marker density horizontally, lower is denser
    public const int PROX_VIS_VERT_DENSITY = 4; //shell marker density vertically, lower is denser
    public const float PROX_VIS_CUTOFF_DIST = 100.0f;
    public const float PROX_VIS_FADE_DIST = 85.0f;

    public const float PROX_SCALE_START = 30.0f;
    public const float PROX_SCALE_RATE = 0.04f;

    public const int MAP_WIDTH = 256;
    public const int MAP_HEIGHT = 128;

    [Tooltip("Magnetic Flux Density - Yellow Zone")]
    [Range(0.00005f, 100)]
    public float B_Yellow;

    [Tooltip("Magnetic Flux Density - Red Zone")]
    [Range(0.00005f, 100)]
    public float B_Red;

    [Tooltip("Shell Base Shape Constant")]
    public float Ca_ShellBaseShapeConst;
    [Tooltip("Shell Shape Changing Constant")]
    public float Da_ShellShapeChangeConst;
    [Tooltip("Shell Base Size Constant")]
    public float Cb_ShellBaseSizeConst;
    [Tooltip("Shell Size Changing Constant")]
    public float Db_ShellSizeChangeConst;

    [HideInInspector]
    public int NumGenerators = 0;

    [HideInInspector]
    public List<DeformableFieldGenerator> FieldGenerators;

    private ProxZone _currentProxVisualization;

    //public Texture2D DebugYellowTex;
    //public Texture2D DebugRedTex;

    public Gradient TextureColorGradient;

    public Material VisMaterial;

//	public float[] StateBlendWeights;

    //public bool ShowVisualization;
    public bool ShowYellowZone = false;
    public bool ShowRedZone = false;
    public bool OverrideVisCameraDistance = false;
    public bool RemoveInteriorPoints = true;
    public int ActiveState = -1;

    private MaterialPropertyBlock _mpbYellow;
    private MaterialPropertyBlock _mpbRed;


    private DynamicMesh _yellowVisMesh;
    private DynamicMesh _redVisMesh;

    private List<float> _visStateBlend;

    private bool _shellsInitialized = false;

    private InstancedMeshShell _redShell;
    private InstancedMeshShell _yellowShell;

    private InstancedMeshShell _redShellTemp;
    private InstancedMeshShell _yellowShellTemp;

    private bool _shellGenerationRunning = false;
    private bool _shellGenerationComplete = false;

    //private float[] _blendWeights;
    //private float[] _lastShellWeights;
    //private int _lastShellState;

    private int _numStates = 2; //fixed at 2 states for now

    public ProxZone CurrentVisualization
    {
        get
        {
            return _currentProxVisualization;
        }
    }

    public override void DisableZoneVisualization()
    {
        ShowRedZone = false;
        ShowYellowZone = false;
    }

    public override void EnableZoneVisualization(VisOptions opt)
    {
        /*if ((int)zone > (int)ProxZone.GreenZone)
        {
            ShowVisualization = true;
        }	
        else
        {
            ShowVisualization = false;
        } */

        //ShowVisualization = true;
        ShowRedZone = opt.ShowRedShell;
        ShowYellowZone = opt.ShowYellowShell;
    }

    public override IEnumerator<GameObject> GetObjectsInZone(ProxZone zone)
    {
        yield return null;
    }

    public void AddGenerator()
    {
        DeformableFieldGenerator gen = new DeformableFieldGenerator();
        //gen.RedZoneMap = new Texture2D(MAP_WIDTH, MAP_HEIGHT, TextureFormat.RGBA32, false);
        //gen.YellowZoneMap = new Texture2D(MAP_WIDTH, MAP_HEIGHT, TextureFormat.RGBA32, false); 

        //gen.Rotation = Quaternion.identity;

        //gen.RedShell = new DeformableShell(MAP_WIDTH, MAP_HEIGHT);
        //gen.YellowShell = new DeformableShell(MAP_WIDTH, MAP_HEIGHT);

        if (FieldGenerators == null)
            FieldGenerators = new List<DeformableFieldGenerator>();

        gen.SetStateCount(2);
        gen.ParentTransform = transform;
        FieldGenerators.Add(gen); 
    }

    public void ResetGeneratorToFieldEquation(int genIndex, int stateIndex)
    {
        DeformableFieldGenerator gen = FieldGenerators[genIndex];
        DeformableFieldGeneratorState state = gen.States[stateIndex];

        DeformableShell redShell = state.Shells[(int)ProxShell.RedShell];
        DeformableShell yellowShell = state.Shells[(int)ProxShell.YellowShell];

        redShell.Clear();
        yellowShell.Clear();

        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            for (int x = 0; x < MAP_WIDTH; x++)
            {
                Vector3 dir = CoordinateToVector(new Vector2(x, y));
                float dist;

                //rotate 90 degrees so the default orientation of the field is consistent with what the user expects				
                Vector3 rdir;
                rdir.y = dir.y;
                rdir.x = dir.z;
                rdir.z = -dir.x;

                dist = FieldBasedProxSystem.ComputeShellSurfaceDistance(rdir, B_Yellow, Ca_ShellBaseShapeConst, Da_ShellShapeChangeConst, Cb_ShellBaseSizeConst, Db_ShellSizeChangeConst);
                yellowShell.SetValue(x, y, dist);

                dist = FieldBasedProxSystem.ComputeShellSurfaceDistance(rdir, B_Red, Ca_ShellBaseShapeConst, Da_ShellShapeChangeConst, Cb_ShellBaseSizeConst, Db_ShellSizeChangeConst);
                redShell.SetValue(x, y, dist);

                //Debug.Log(string.Format("red: {0:F2}", dist));
            }
        }

        //gen.YellowShell.ConvertToTexture(ref DebugYellowTex, TextureColorGradient);
        //gen.RedShell.ConvertToTexture(ref DebugRedTex, TextureColorGradient);
    }

    public static Vector3 CoordinateToVector(Vector2 coord)
    {
        Vector3 v = Vector3.zero;

        //rescale coordinate to (0 > 2 Pi) X and (-1 to 1 ) Y
        coord.x = (coord.x / (float)MAP_WIDTH) * Mathf.PI * 2.0f;
        coord.y = (coord.y / (float)MAP_HEIGHT) * 2.0f - 1.0f;		

        //rescale Y using an inverse lambert projection
        //https://en.wikipedia.org/wiki/Lambert_cylindrical_equal-area_projection
        //theta = inclination
        //phi = azimuth
        float phi = coord.x;
        float theta = Mathf.Asin(coord.y) + (Mathf.PI / 2.0f);

        //convert to cartesian vector
        //source: https://en.wikipedia.org/wiki/Spherical_coordinate_system#Cartesian_coordinates 
        v.x = Mathf.Sin(theta) * Mathf.Cos(phi);
        v.z = Mathf.Sin(theta) * Mathf.Sin(phi);
        v.y = Mathf.Cos(theta); //y-up

        //Debug.Log(v.magnitude);

        return v;
    }

    public static Vector2 VectorToCoordinate(Vector3 v)
    {
        Vector2 coord = Vector2.zero;

        //normalize to direction vector
        //v.Normalize(); //REQUIRE INPUT TO BE NORMALIZED

        //convert to spherical coordinates
        //source: https://en.wikipedia.org/wiki/Spherical_coordinate_system#Cartesian_coordinates 
        float theta = Mathf.Acos(v.y);
        float phi = Mathf.Atan2(v.z, v.x);

        if (phi < 0)
            phi += Mathf.PI * 2;

        //perform lambert projection
        theta = theta - (Mathf.PI / 2.0f);
        theta = Mathf.Sin(theta);

        //rescale to pixels
        coord.x = (phi / (Mathf.PI * 2)) * (float)MAP_WIDTH;
        coord.y = ((theta + 1.0f) / 2.0f) * (float)MAP_HEIGHT;

        return coord;
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        InitializeShells();


        foreach (DeformableFieldGenerator gen in FieldGenerators)
        {
            if (gen.ParentTransform == null)
                gen.ParentTransform = transform;
        }

        //_lastShellWeights = null;
        //_lastShellState = -1;
    }

    private void InitializeShells()
    {
        _mpbYellow = new MaterialPropertyBlock();
        _mpbYellow.SetColor("_Color", Color.yellow);

        _mpbRed = new MaterialPropertyBlock();
        _mpbRed.SetColor("_Color", Color.red);

        _redShell = new InstancedMeshShell();
        _yellowShell = new InstancedMeshShell();

        _redShellTemp = new InstancedMeshShell();
        _yellowShellTemp = new InstancedMeshShell();

        _shellsInitialized = true;
    }

    

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (ShowRedZone || ShowYellowZone)
        {
            //UpdateVisMesh();
            //DrawVisMesh();

            float distToCamera = 0;

            if (Camera.main != null)
                distToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);
            if (OverrideVisCameraDistance)
                distToCamera = 0;

            if (OverrideVisCameraDistance)
            {
                DrawVisShellMultithreaded();
            }
            else if (distToCamera < PROX_VIS_CUTOFF_DIST)
            {
                bool boundsCheck = true;

                if (Camera.main != null && !XRSettings.enabled)
                {
                    Bounds bounds = new Bounds(transform.position, new Vector3(25, 25, 25));
                    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
                    boundsCheck = GeometryUtility.TestPlanesAABB(planes, bounds);
                }

                if (boundsCheck)
                {

                    DrawVisShellMultithreaded();
                }
            }

        }

    }

    public void DrawVisShellMultithreaded()
    {
        if (!_shellsInitialized)
            InitializeShells();

        if (!_shellGenerationRunning) 
        {
            if (_shellGenerationComplete)
            {
                SwapShells();
            }
            StartBackgroundShellComputation();
        }

        DrawVisShell();
    }

    //swap the temp shell to the primary shell
    private void SwapShells()
    {
        InstancedMeshShell tmpRed = _redShell;
        InstancedMeshShell tmpYellow = _yellowShell;

        _redShell = _redShellTemp;
        _yellowShell = _yellowShellTemp;

        _redShellTemp = tmpRed;
        _yellowShellTemp = tmpYellow;

    }

    private void StartBackgroundShellComputation()
    {		
        _shellGenerationComplete = false;
        _shellGenerationRunning = true;

        float distToCamera = 0;
        if (Camera.main != null)
            distToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);

        CacheGeneratorTransforms();

        ThreadPool.QueueUserWorkItem(BackgroundComputeShells, (object)distToCamera);
    }

    public void ComputeShellsSync()
    {		
        CacheGeneratorTransforms();

        var blendWeights = ComputeVisBlendWeights();
        if (HasShellChanged())
        {

            UpdateVisShell(ref _redShellTemp, ref _yellowShellTemp, blendWeights, PROX_VIS_VERT_DENSITY, PROX_VIS_HORZ_DENSITY);
        }

        SwapShells();
    }

    private void BackgroundComputeShells(object state)
    {
        float distToCamera = (float)state;

        var blendWeights = ComputeVisBlendWeights();
        if (HasShellChanged())
        {

            if (distToCamera < 175)
            {
                UpdateVisShell(ref _redShellTemp, ref _yellowShellTemp, blendWeights, PROX_VIS_VERT_DENSITY, PROX_VIS_HORZ_DENSITY);
            }
            else
            {
                UpdateVisShell(ref _redShellTemp, ref _yellowShellTemp, blendWeights, PROX_VIS_VERT_DENSITY, PROX_VIS_HORZ_DENSITY);
            }

            _shellGenerationComplete = true;
        }

        _shellGenerationRunning = false;
    }

    private void CacheGeneratorTransforms()
    {
        for (int i = 0; i < FieldGenerators.Count; i++)
        {
            DeformableFieldGenerator gen = FieldGenerators[i];
            if (gen.ParentTransform == null)
                continue;

            gen.CacheSystemSpaceTransform(transform);
        }
    }

    /// <summary>
    /// Compute the blend weights needed to visualize the current shells
    /// Default to "null" to use the actual blended shells
    /// Return a custom set of weights to visualize a specific shell
    /// </summary>
    private List<float> ComputeVisBlendWeights()
    {
        //create cached blend weight array if not present
        if (_visStateBlend == null || _visStateBlend.Count != _numStates)
        {
            _visStateBlend = new List<float>(_numStates);
            for (int i = 0; i < _numStates; i++)
                _visStateBlend.Add(0);
        }

        if (ActiveState >= 0 && ActiveState < _numStates)
        {
            //build a custom state blend for just one state
            for (int i = 0; i < _numStates; i++)
            {
                _visStateBlend[i] = 0;
            }

            _visStateBlend[ActiveState] = 1;

            return _visStateBlend;
        }

        //by default return null
        return null;
    }

    private bool HasShellChanged()
    {
        /*
        //check if we actually need to update the shell
        bool shellHasntChanged = false;
        if (_lastShellState == ActiveState && _lastShellWeights != null && _lastShellWeights.Length == _blendWeights.Length)
        {
            shellHasntChanged = true;

            for (int i = 0; i < _blendWeights.Length; i++)
            {
                if (_blendWeights[i] != _lastShellWeights[i])
                {
                    shellHasntChanged = false;
                    break;
                }
            }
        }

        if (shellHasntChanged)
        {
            //Debug.Log("Shell hasn't changed");
        }

        return !shellHasntChanged;
        */
        return true;
    }

    public delegate void ShellMarkerCallback(Vector3 pos, ProxShell shell);

    public void ComputeShellMarkers(List<float> blendWeights, ShellMarkerCallback callback, int vertInc = 4, int horzInc = 5 )
    {
        for (int i = 0; i < FieldGenerators.Count; i++)
        {
            DeformableFieldGenerator gen = FieldGenerators[i];
            //Quaternion rotInverse = Quaternion.Inverse(gen.Rotation);
            if (gen.ParentTransform == null)
                continue;

            //use blended state	
            //start y at 1 to skip strange behaviour at the poles
            for (int y = 1; y < MAP_HEIGHT; y += vertInc)
            {
                for (int x = 0; x < MAP_WIDTH; x += horzInc)
                {
                    Vector2 coord = new Vector2(x, y);
                    Vector3 v = DeformableProxSystem.CoordinateToVector(coord);

                    float yShellDist = gen.GetShellDistXY(ProxShell.YellowShell, coord, blendWeights);
                    float rShellDist = gen.GetShellDistXY(ProxShell.RedShell, coord, blendWeights);

                    //transform the point into the TransformParent's coordinate space by offseting by the generator position
                    //v = gen.Rotation * v;
                    Vector3 ypos = v * yShellDist + gen.Position;
                    Vector3 rpos = v * rShellDist + gen.Position;

                    //compute system space position of the points
                    Vector3 yposSystem = gen.ParentSpaceToSystemSpace(ypos);
                    Vector3 rposSystem = gen.ParentSpaceToSystemSpace(rpos);

                    bool skipYellow = false;
                    bool skipRed = false;

                    //test if the point is inside another generator's shell
                    if (RemoveInteriorPoints)
                    {
                        for (int j = 0; j < FieldGenerators.Count; j++)
                        {
                            if (j == i)
                                continue;

                            DeformableFieldGenerator testGen = FieldGenerators[j];
                            if (testGen.ParentTransform == null)
                                continue;

                            if (gen.ParentTransform == testGen.ParentTransform)
                            {
                                //point is already in the right coordinate space, do a faster parent space test
                                if (testGen.TestPointParentSpace(ypos, ProxShell.YellowShell, blendWeights))
                                {
                                    skipYellow = true;
                                }

                                if (testGen.TestPointParentSpace(rpos, ProxShell.RedShell, blendWeights))
                                {
                                    skipRed = true;
                                }
                            }
                            else
                            {
                                //generator is in a different coordinate space, use world space test
                                if (testGen.TestPointSystemSpace(yposSystem, ProxShell.YellowShell, blendWeights))
                                {
                                    skipYellow = true;
                                }

                                if (testGen.TestPointSystemSpace(rposSystem, ProxShell.RedShell, blendWeights))
                                {
                                    skipRed = true;
                                }
                            }
                        }
                    }

                    if (!skipYellow)
                    {
                        //convert to coordinate space of the prox system's transform
                        //callback(transform.InverseTransformPoint(yposWorld), ProxShell.YellowShell);

                        callback(yposSystem, ProxShell.YellowShell);
                    }

                    if (!skipRed)
                    {
                        //convert to coordinate space of the prox system's transform
                        //callback(transform.InverseTransformPoint(rposWorld), ProxShell.RedShell);

                        callback(rposSystem, ProxShell.RedShell);
                    }

                }//for x
            }//for y

        } //for each field generator		

    }

    public void UpdateVisShell(ref InstancedMeshShell redShell, ref InstancedMeshShell yellowShell, List<float> blendWeights, int vertInc = 4, int horzInc = 5)
    {
        if (redShell == null)
            redShell = new InstancedMeshShell();

        if (yellowShell == null)
            yellowShell = new InstancedMeshShell();

        if (FieldGenerators == null || FieldGenerators.Count <= 0)
            return;		

        redShell.Clear();
        yellowShell.Clear();

        //create local variables to fix anonymous method scope issues
        InstancedMeshShell ys = yellowShell;
        InstancedMeshShell rs = redShell;

        ComputeShellMarkers(blendWeights, (Vector3 pos, ProxShell shell) => {
            if (shell == ProxShell.YellowShell)
                ys.AddMarker(pos);
            else
                rs.AddMarker(pos);
        }, vertInc, horzInc);
                
    }

    public void DrawVisShell()
    {
        Color yellow = Color.yellow;
        Color red = Color.red;

        float distToCamera = 0;
        if (Camera.main != null)
            distToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);

        //compute marker scale 
        float scale = 1.0f;
        scale = Mathf.Clamp((distToCamera - PROX_SCALE_START) * PROX_SCALE_RATE + 1.0f, 1.0f, 5.0f);

        if (OverrideVisCameraDistance)
            scale = 0.3f;

        //compute marker fade
        if (OverrideVisCameraDistance)
        {
            red.a = 1.0f;
            yellow.a = 1.0f;
        }
        else if (distToCamera > PROX_VIS_FADE_DIST)
        {
            distToCamera -= PROX_VIS_FADE_DIST;
            float alpha;
            alpha = distToCamera / (PROX_VIS_CUTOFF_DIST - PROX_VIS_FADE_DIST);
            alpha = 1.0f - alpha;

            alpha = Mathf.Clamp(alpha, 0.0f, 1.0f);

            red.a = alpha;
            yellow.a = alpha;
        }


        if (_mpbRed == null)
            _mpbRed = new MaterialPropertyBlock();

        if (_mpbYellow == null)
            _mpbYellow = new MaterialPropertyBlock();

        _mpbRed.SetColor("_Color", red);
        _mpbYellow.SetColor("_Color", yellow);

        _mpbRed.SetFloat("_Scale", scale);
        _mpbYellow.SetFloat("_Scale", scale);

        if (ShowYellowZone)
            _yellowShell.DrawShell(transform.position, transform.localToWorldMatrix, VisMaterial, _mpbYellow);
        
        if (ShowRedZone)
            _redShell.DrawShell(transform.position, transform.localToWorldMatrix, VisMaterial, _mpbRed);
    }

    public void DrawBoxGizmos()
    {
        if (FieldGenerators == null || FieldGenerators.Count <= 0)
            return;

        CacheGeneratorTransforms();		

        var blendWeights = ComputeVisBlendWeights();

        Matrix4x4 gizMat = Gizmos.matrix;

        Vector3 size = new Vector3(0.04f, 0.04f, 0.04f);		
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.green;
        foreach (DeformableFieldGenerator gen in FieldGenerators)
        {
            if (gen.ParentTransform == null)
                continue;

            Vector3 systemPos = gen.ParentSpaceToSystemSpace(gen.Position);
            Gizmos.DrawCube(systemPos, size * 3);
        }

        Gizmos.color = Color.magenta;
        ComputeShellMarkers(blendWeights, (Vector3 pos, ProxShell shell) => {
            Gizmos.DrawCube(pos, size);
        }, 7, 8);


        Gizmos.matrix = gizMat;
    }



    //private void OnDrawGizmosSelected()
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject != gameObject && !ShowRedZone && !ShowYellowZone)
            return;
#endif
        if (!Application.isPlaying)
        {
            /*
            if (_mpbYellow == null)
            {
                _mpbYellow = new MaterialPropertyBlock();
                _mpbYellow.SetColor("_Color", Color.yellow);
            }

            if (_mpbRed == null)
            {
                _mpbRed = new MaterialPropertyBlock();
                _mpbRed.SetColor("_Color", Color.red);
            }
            
            UpdateVisShell(ref _redShell, ref _yellowShell);
            DrawVisShell(); 
            */

            //DrawShellGizmos();

            //DrawBoxGizmos();
        }
        
        
    }
    
    /// <summary>
    /// Test a world space point's prox zone
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public override ProxZone TestPoint(Vector3 position)
    {
        int proxZone = (int)ProxZone.GreenZone;

        foreach (DeformableFieldGenerator gen in FieldGenerators)
        {
            Vector3 parentSpacePos = gen.WorldSpaceToParentSpace(position);
            
            int zone = (int)gen.TestPointParentSpace(parentSpacePos, null);

            proxZone = Mathf.Max(proxZone, zone);

            if (proxZone >= (int)ProxZone.RedZone)
                break;
        }

        return (ProxZone)proxZone;
    }

    public override Bounds ComputeProxSystemBounds()
    {
        Bounds b = new Bounds();

        float maxVal = 0;
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (DeformableFieldGenerator gen in FieldGenerators)
        {
            foreach (DeformableFieldGeneratorState state in gen.States)
            {
                foreach (DeformableShell shell in state.Shells)
                {
                    maxVal = Mathf.Max(shell.MaxValue, maxVal);					
                }
            }

            center += gen.Position;
            count++;
        }

        if (count == 0)
            center = Vector3.zero;
        else
            center = center / (float)count;

        maxVal *= 2;
        b.extents = new Vector3(maxVal, maxVal, maxVal);
        b.center = center;

        return b;
    }

    public void SaveGenerators()
    {
        if (FieldGenerators != null)
        {
            NumGenerators = FieldGenerators.Count;
            foreach (DeformableFieldGenerator gen in FieldGenerators)
            {
                gen.Save();
            }
        }
        else
            NumGenerators = 0;
    }

    public void SaveAsCopy()
    {
        if (FieldGenerators != null)
        {
            NumGenerators = FieldGenerators.Count;
            foreach (DeformableFieldGenerator gen in FieldGenerators)
            {
                gen.GeneratorID = Guid.NewGuid().ToString();
                gen.Save();
            }
        }
        else
            NumGenerators = 0;
    }

    private static Dictionary<string, DeformableFieldGenerator> _genCache;

    public void OnBeforeSerialize()
    {
        if (FieldGenerators == null)
            return;

        if (_genCache == null)
            _genCache = new Dictionary<string, DeformableFieldGenerator>();

        foreach (DeformableFieldGenerator gen in FieldGenerators)
        {
            _genCache[gen.GeneratorID] = gen;
        }
    }

    public void OnAfterDeserialize()
    {
        /*
        if (_genCache != null)
        {
            Debug.LogFormat("AfterDeserialize Cache Contents: {0}", _genCache.Count);
        }
        else
        {
            Debug.LogFormat("AfterDeserialize Cache Is Empty");
        } */

        if (_genCache == null)
            _genCache = new Dictionary<string, DeformableFieldGenerator>();

        if (FieldGenerators == null)
            return;

        for (int i = 0; i < FieldGenerators.Count; i++)
        {
            DeformableFieldGenerator cachedGen;

            if (_genCache.TryGetValue(FieldGenerators[i].GeneratorID, out cachedGen))
            {
                //FieldGenerators[i] = cachedGen;
                FieldGenerators[i].LoadFromCache(cachedGen);
            }
            else
            {
                FieldGenerators[i].Load();
            }
        }

        //foreach (DeformableFieldGenerator gen in FieldGenerators)
        //{

        //	gen.Load();
        //}
    }
}
