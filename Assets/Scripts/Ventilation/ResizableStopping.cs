using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using UnityEngine.Serialization;

public class ResizableStopping : MonoBehaviour, IScenarioEditorResizable
{
    //public Mesh StoppingMesh;
    //public Mesh CutMesh;

    public GameObject DoorPrefab;
    public GameObject FoamPrefab;
    public Bounds CutBounds;

    [FormerlySerializedAs("TilesPerMeter")]
    public float MetersPerTile = 2.0f;
    public float DoorHeight = 0.5f;
    public float CutOffset = -0.035f;
    public bool FlipDoorRotation = false;
    //public bool OvercastStopping = false;

    public bool DoorEnabled = true;
    public MineMapSymbol SymbolWithDoor;
    public MineMapSymbol SymbolWithoutDoor;

    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private List<Vector3> _meshVertices, _newMeshVertices;
    private List<int> _meshIndices, _newMeshIndices;
    private List<Vector2> _uvCoords;

    private Vector3 _size;
    private float _doorZOffset;
    private Bounds _stoppingMeshBounds;

    private GameObject _doorInstance;
    private ComponentInfo_Door _compInfoDoor;
    private ComponentInfo_NetworkedObject _compInfoNetObj;

    private GameObject _foam1;
    private GameObject _foam2;

    private bool _initialized = false;
    private RoofBoltBlocker[] _roofBoltBlockers = null;

    //private List<NavMeshObstacle> _navMeshObstacles = new List<NavMeshObstacle>();
    private NavMeshObstacle _rightObstacle;

    public Vector3 Size 
    {
        get 
        {
            return _size;
        }
        set
        {
            _size = value;
        }
    }

    public Vector3 LocalCenter => new Vector3(0, _size.y / 2, 0);

    void Awake()
    {
        _meshFilter = transform.GetComponentInChildren<MeshFilter>();
        _size = _meshFilter.sharedMesh.bounds.size;

        //if (DoorEnabled)
        //    SpawnDoor();

        SpawnDoor();
        //SetupComponentInfo();
    }

    // Start is called before the first frame update
    void Start()
    {
        _meshVertices = new List<Vector3>();
        _newMeshVertices = new List<Vector3>();
        _meshIndices = new List<int>();
        _newMeshIndices = new List<int>();
        _uvCoords = new List<Vector2>();
        _mesh = _meshFilter.mesh;

        _initialized = true;

        _roofBoltBlockers = GetComponentsInChildren<RoofBoltBlocker>();

        //_meshFilter.sharedMesh = ProcGeometry.CutMesh(StoppingMesh, CutMesh);

        //ProcGeometry.FastBoxCutXY(_mesh, CutBounds, _meshVertices, _meshIndices, 
        //    _newMeshVertices, _newMeshIndices, _uvCoords, TilesPerMeter);

        ResizeStopping(_size);

        if (SymbolWithDoor != null && SymbolWithoutDoor != null && !TryGetComponent<MineMapSymbolRenderer>(out var symbolRenderer))
        {
            symbolRenderer = gameObject.AddComponent<MineMapSymbolRenderer>();
            symbolRenderer.ShowOnMapMan = false;
            symbolRenderer.SymbolAsset = null;
            symbolRenderer.ConstantUpdate = false;

            if (DoorEnabled)
                symbolRenderer.Symbol = SymbolWithDoor;
            else
                symbolRenderer.Symbol = SymbolWithoutDoor;
        }
    }

    public void ManualResize()
    {
        _size = _meshFilter.sharedMesh.bounds.size;
        ResizeStopping(_size);
    }

    public void ResizeStopping()
    {
        ResizeStopping(_size);
    }

    public void ResizeStopping(Vector3 size)
    {
        if (DoorEnabled)
            SpawnDoor();
        else
            DespawnDoor();

        _size = size;

        if (!_initialized)
            return;

        
        PositionDoor();

        CutStoppingMesh();
        UpdateVentResistancePlane();
        UpdateColliders();
        UpdateRoofBoltBlockers();

        SpawnFoam();
        PositionFoam();
    }

    private void SpawnDoor()
    {
        if (_doorInstance != null)
        {
            _doorInstance.SetActive(true);
            return;
        }

        _doorInstance = Instantiate(DoorPrefab);
        _doorInstance.transform.parent = transform;

        var compInfoNetObj = _doorInstance.GetComponentInChildren<ComponentInfo_NetworkedObject>();
        var compInfoDoor = _doorInstance.GetComponentInChildren<ComponentInfo_Door>();

        if (compInfoNetObj != null)
            Destroy(compInfoNetObj);
        if (compInfoDoor != null)
            Destroy(compInfoDoor);

        SetupComponentInfo();
    }

    private void DespawnDoor()
    {
        if (_doorInstance == null)
            return;

        //Destroy(_doorInstance);
        _doorInstance.SetActive(false);

        SetupComponentInfo();
    }

    private void SetupComponentInfo()
    {
        if (!TryGetComponent<ComponentInfo_ResizableStopping>(out var compInfo))
            return;

        if (_compInfoDoor == null)
            _compInfoDoor = gameObject.AddComponent<ComponentInfo_Door>();
        if (_compInfoNetObj == null)
            _compInfoNetObj = gameObject.AddComponent<ComponentInfo_NetworkedObject>();


        if (_doorInstance != null)
        {
            var netObj = _doorInstance.GetComponentInChildren<NetworkedObject>();

            _compInfoNetObj.NetworkedObject = netObj;
            _compInfoNetObj.NetworkedObjectID = compInfo.ComponentName + "DoorNetObj";

            var ventControl = _doorInstance.GetComponentInChildren<DynamicVentControl>();

            _compInfoDoor.DynamicVentControl = ventControl;
            _compInfoDoor.ComponentName = compInfo.ComponentName + " Door";
        }
        
        //else if (!ScenarioSaveLoad.IsScenarioEditor)
        //{
        //    if (_compInfoDoor != null)
        //    {
        //        Destroy(_compInfoDoor);
        //        _compInfoDoor = null;
        //    }

        //    if (_compInfoNetObj != null)
        //    {
        //        Destroy(_compInfoNetObj);
        //        _compInfoNetObj = null;
        //    }    
        //}

    }

    private void PositionDoor()
    {
        if (!DoorEnabled)
            return;

        var doorMesh = _doorInstance.GetComponent<MeshFilter>();
        var doorBounds = doorMesh.sharedMesh.bounds;

        _doorZOffset = doorBounds.center.z * -1.0f;

        if (FlipDoorRotation)
            _doorZOffset *= -1.0f;

        var doorPos = new Vector3(0, DoorHeight + doorBounds.center.y + doorBounds.extents.y, _doorZOffset);

        CutBounds = doorBounds;

        //if (OvercastStopping)
        //{
        //    Vector3 extent = CutBounds.extents;
        //    //Extent correction due to overcast inheriting scale from the tile
        //    extent.y *= _doorInstance.transform.localScale.y;
        //    extent.x *= _doorInstance.transform.localScale.x;
        //    CutBounds.extents = extent;
        //}

        CutBounds.center += doorPos;
        CutBounds.extents = Vector3.Scale(CutBounds.extents, _doorInstance.transform.localScale) + new Vector3(CutOffset, CutOffset, 5);
        
        _doorInstance.transform.localPosition = doorPos;

        if (FlipDoorRotation)
            _doorInstance.transform.localRotation = Quaternion.Euler(0, 180, 0);
        else
            _doorInstance.transform.localRotation = Quaternion.identity;
    }

    private void SpawnFoam()
    {
        if (_foam1 == null)
        {
            _foam1 = Instantiate(FoamPrefab, transform);
        }

        if (_foam2 == null)
        {
            _foam2 = Instantiate(FoamPrefab, transform);
        }
    }

    private void PositionFoam()
    {
        var foamMesh = FoamPrefab.GetComponent<MeshFilter>().sharedMesh;
        var foamBounds = foamMesh.bounds;

        var scale = _size;
        scale = scale.ComponentDivide(foamBounds.size);
        scale.z = 1.0f;

        _foam1.transform.localScale = scale;
        _foam2.transform.localScale = scale;

        var foamOffset = _stoppingMeshBounds.extents.z + (foamBounds.center.z + foamBounds.extents.z);

        _foam1.transform.localPosition = new Vector3(0, 0, foamOffset);
        _foam2.transform.localPosition = new Vector3(0, 0, -foamOffset);

        _foam1.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    private void CutStoppingMesh()
    {
        //_meshFilter.sharedMesh = ProcGeometry.BoxCutXY(_mesh, CutBounds, TilesPerMeter);

        var dm = ProcGeometry.ConvertToDMesh(_mesh);

        var scale = _size;
        scale = scale.ComponentDivide(_mesh.bounds.size);
        scale.z = 1.0f;

        ProcGeometry.ApplyScale(dm, scale, Vector3.zero);
        if (DoorEnabled)
            ProcGeometry.BoxCutXY(dm, CutBounds);
        ProcGeometry.PlanarUnwrapXY(dm, MetersPerTile);

        var stoppingMesh = ProcGeometry.ConvertToUnityMesh(dm);

        stoppingMesh.RecalculateBounds();
        stoppingMesh.RecalculateTangents();

        _meshFilter.sharedMesh = stoppingMesh;
        _stoppingMeshBounds = stoppingMesh.bounds;
    }

    private void UpdateVentResistancePlane()
    {
        VentResistancePlane ventPlane;

        if (DoorEnabled)
        {
            if (!_doorInstance.TryGetComponent<VentResistancePlane>(out ventPlane))
                return;

            if (gameObject.TryGetComponent<VentResistancePlane>(out var objResistancePlane))
            {
                Destroy(objResistancePlane);
            }

            //counteract the relative scale of the door transform compared to the stopping
            float scaleInvX = 1.0f / ventPlane.transform.localScale.x;
            float scaleInvY = 1.0f / ventPlane.transform.localScale.y;

            ventPlane.PlaneOffset = (transform.position - _doorInstance.transform.position) + 
                new Vector3(0, _stoppingMeshBounds.size.y * 0.5f * scaleInvY, 0);
            Debug.Log($"ResizableStopping: Vent plane offset {ventPlane.PlaneOffset}");

            ventPlane.PlaneWidth = _stoppingMeshBounds.size.x * 0.5f * scaleInvX;
            ventPlane.PlaneHeight = _stoppingMeshBounds.size.y * 0.5f * scaleInvY;
        }
        else
        {
            if (!gameObject.TryGetComponent<VentResistancePlane>(out ventPlane))
            {
                ventPlane = gameObject.AddComponent<VentResistancePlane>();
            }

            float addedResistance = 2000;

            //if (_doorInstance != null) 
            //{
            //    var compInfoDoor = _doorInstance.GetComponentInChildren<ComponentInfo_Door>(includeInactive:true);
            //    if (compInfoDoor != null) 
            //    {
            //        addedResistance = compInfoDoor.MaxResistance;
            //    }
            //}
            if (_compInfoDoor != null)
                addedResistance = _compInfoDoor.MaxResistance;

            ventPlane.AddedResistance = addedResistance;
            ventPlane.PlaneOffset = new Vector3(0, _stoppingMeshBounds.size.y * 0.5f, 0);

            ventPlane.PlaneWidth = _stoppingMeshBounds.size.x * 0.5f;
            ventPlane.PlaneHeight = _stoppingMeshBounds.size.y * 0.5f;
        }
    }

    private void UpdateRoofBoltBlockers()
    {
        if (_roofBoltBlockers == null || _roofBoltBlockers.Length <= 0)
            return;

        foreach (var blocker in _roofBoltBlockers)
        {
            if (!blocker.TryGetComponent<BoxCollider>(out var collider))
                continue;

            blocker.transform.localScale= Vector3.one;
            blocker.transform.localRotation = Quaternion.identity;

            Vector3 scale = transform.lossyScale;

            blocker.transform.position = transform.position + new Vector3(0, _stoppingMeshBounds.size.y * scale.y, 0);

            collider.center = Vector3.zero;
            collider.size = new Vector3(_stoppingMeshBounds.size.x, 0.5f / scale.y, 0.75f / scale.z);
        }
    }

    private void UpdateColliders()
    {
        if (TryGetComponent<BoxCollider>(out var boxCollider))
        {
            boxCollider.center = _stoppingMeshBounds.center;
            boxCollider.size = _stoppingMeshBounds.size;
        }

        //_navMeshObstacles.Clear();
        //GetComponentsInChildren<NavMeshObstacle>(_navMeshObstacles);

        NavMeshObstacle leftObstacle;
        if (!TryGetComponent<NavMeshObstacle>(out leftObstacle))
            leftObstacle = gameObject.AddComponent<NavMeshObstacle>();

        if (DoorEnabled)
        {
            //check there are two nav mesh obstacles or add them
            //int count = _navMeshObstacles.Count;
            //for (int i = count; i < 2; i++)
            //{
            //    var obstacle = gameObject.AddComponent<NavMeshObstacle>();
            //    if (obstacle == null)
            //    {
            //        Debug.LogError("ResizableStopping: Couldn't add NavMeshObstacle");
            //        return;
            //    }
            //    _navMeshObstacles.Add(obstacle);
            //}

            //var leftObstacle = _navMeshObstacles[0];
            //var rightObstacle = _navMeshObstacles[1];

            float doorHalfWidth = CutBounds.extents.x;
            Vector3 center, size;

            size = _stoppingMeshBounds.size;
            size.x = (size.x / 2.0f) - doorHalfWidth;

            //var leftObstacle = gameObject.AddComponent<NavMeshObstacle>();

            center = _stoppingMeshBounds.center;
            center.x -= _stoppingMeshBounds.extents.x;
            center.x += size.x / 2.0f;

            leftObstacle.center = center;
            leftObstacle.size = size;
            leftObstacle.carving = true;

            //var rightObstacle = gameObject.AddComponent<NavMeshObstacle>();

            //only one NaveMeshObstacle is allowed per object, create a child object for the 2nd half of the door
            if (_rightObstacle == null)
            {
                GameObject obj = new GameObject("NavMeshObstacle");
                obj.transform.SetParent(transform, false);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                _rightObstacle = obj.AddComponent<NavMeshObstacle>();
            }

            center = _stoppingMeshBounds.center;
            center.x += _stoppingMeshBounds.extents.x;
            center.x -= size.x / 2.0f;

            _rightObstacle.center = center;
            _rightObstacle.size = size;
            _rightObstacle.carving = true;
        }
        else
        {
            leftObstacle.center = _stoppingMeshBounds.center;
            leftObstacle.size = _stoppingMeshBounds.size;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        Gizmos.DrawCube(CutBounds.center, CutBounds.size);
        Gizmos.matrix = Matrix4x4.identity;
    }

    public void SetSize(Vector3 size, Vector3 center)
    {
        transform.localScale = Vector3.one;
        transform.position = center;        
        ResizeStopping(size);
    }
}
