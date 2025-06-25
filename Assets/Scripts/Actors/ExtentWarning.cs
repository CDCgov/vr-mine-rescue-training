using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.HighDefinition;


public class ExtentWarning : MonoBehaviour
{
    private struct PerimeterPlaneData
    {
        public GameObject Instance;
        public Plane WarningPlane;
        public MeshRenderer Renderer;
        public DecalProjector Projector;

        public MaterialPropertyBlock MeshProps;
        //public MaterialPropertyBlock ProjectorProps;
        //public Material GridMaterial;
        public Material DecalMaterial;

        public Vector2 UVScale;
        //public Vector3 GridScale;
        //public Mesh Mesh;
        //public Vector3[] Vertices;
        //public int[] Triangles;
        //public Vector2[] UV;
    }

    private const float GridMeshWidth = 3.0f;
    private const float GridMeshHeight = 3.0f;

    public SystemManager SystemManager;
    public AssetReference PerimeterWarningPrefab;
    public Transform HeadColliderTransform;

    public Color WarningColor = Color.green;
    public Color GridColor = Color.yellow;

    private float _startWarnDist;
    private Vector3 _posBounds;
    private Vector3 _negBounds;
    private Mesh _gridMesh;
    //private Material _gridMat;
    //private Material _projMat;

    private GameObject _perimeterWarningPrefab;

    private List<PerimeterPlaneData> _warningPlanes;

    //public float StartWarnDistanceMeters = 4;
    //public GameObject XPosLine;
    //public GameObject ZPosLine;
    //public GameObject XNegLine;
    //public GameObject ZNegLine;
    //public Transform XPosGrid;
    //public Transform ZPosGrid;
    //public Transform XNegGrid;
    //public Transform ZNegGrid;

    //public Color WarningColor;

    //private Renderer _XPosRen;
    //private Renderer _XNegRen;
    //private Renderer _ZPosRen;
    //private Renderer _ZNegRen;

    //private DecalProjector _XPosDecal;
    //private DecalProjector _XNegDecal;
    //private DecalProjector _ZPosDecal;
    //private DecalProjector _ZNegDecal;


    //private float _xPosT = 0;
    //private float _xNegT = 0;
    //private float _zPosT = 0;
    //private float _zNegT = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        _warningPlanes = new List<PerimeterPlaneData>();

        Vector3[] vertices = null;
        Vector2[] uvs = null;
        int[] triangles = null;

        ProcGeometry.GeneratePlane(new Vector3(-1.0f * GridMeshWidth / 2.0f, GridMeshHeight, 0),
                                   new Vector3(1.0f * GridMeshWidth / 2.0f, 0, 0), 3, 3,
                                   ref vertices, ref triangles, ref uvs);

        _gridMesh = new Mesh();
        _gridMesh.vertices = vertices;
        _gridMesh.uv = uvs;
        _gridMesh.triangles = triangles;

        _gridMesh.RecalculateNormals();
        _gridMesh.RecalculateBounds();

        _startWarnDist = SystemManager.SystemConfig.ExtentWarningDistance;
        _posBounds.x = SystemManager.SystemConfig.PositiveXExtentDistance;
        _negBounds.x = -SystemManager.SystemConfig.NegativeXExtentDistance;
        _posBounds.z = SystemManager.SystemConfig.PositiveZExtentDistance;
        _negBounds.z = -SystemManager.SystemConfig.NegativeZExtentDistance;

        Initialize();

        //_XPosDecal = XPosLine.GetComponent<DecalProjector>();
        //_XNegDecal = XNegLine.GetComponent<DecalProjector>();
        //_ZPosDecal = ZPosLine.GetComponent<DecalProjector>();
        //_ZNegDecal = ZNegLine.GetComponent<DecalProjector>();

        //Vector3 xPos = XPosLine.transform.localPosition;
        //Vector3 xNeg = XNegLine.transform.localPosition;
        //Vector3 zPos = ZPosLine.transform.localPosition;
        //Vector3 zNeg = ZNegLine.transform.localPosition;

        //xPos.x = SystemManager.SystemConfig.PositiveXExtentDistance;
        //xNeg.x = -SystemManager.SystemConfig.NegativeXExtentDistance;
        //zPos.z = SystemManager.SystemConfig.PositiveZExtentDistance;
        //zNeg.z = -SystemManager.SystemConfig.NegativeZExtentDistance;

        //XPosLine.transform.localPosition = xPos;
        //XNegLine.transform.localPosition = xNeg;
        //ZPosLine.transform.localPosition = zPos;
        //ZNegLine.transform.localPosition = zNeg;

        //Vector3 gridPosX = XPosGrid.position;
        //Vector3 gridPozZ = ZPosGrid.position;
        //Vector3 gridNegX = XNegGrid.position;
        //Vector3 gridNegZ = ZNegGrid.position;

        //gridPosX.x = XPosLine.transform.position.x;
        //gridPosX.z = XPosLine.transform.position.z;
        //gridPozZ.x = ZPosLine.transform.position.x;
        //gridPozZ.z = ZPosLine.transform.position.z;
        //gridNegX.x = XNegLine.transform.position.x;
        //gridNegX.z = XNegLine.transform.position.z;
        //gridNegZ.x = ZNegLine.transform.position.x;
        //gridNegZ.z = ZNegLine.transform.position.z;

        //XPosGrid.position = gridPosX;
        //ZPosGrid.position = gridPozZ;
        //XNegGrid.position = gridNegX;
        //ZNegGrid.position = gridNegZ;

        //StartWarnDistanceMeters = SystemManager.SystemConfig.ExtentWarningDistance;
    }

    private async void Initialize()
    {
        _perimeterWarningPrefab = await Addressables.LoadAssetAsync<GameObject>(PerimeterWarningPrefab).Task;

        //var rot90 = Quaternion.Euler(0, 90, 0);
        float xWidth = _posBounds.x - _negBounds.x;
        float zWidth = _posBounds.z - _negBounds.z;

        float xCenter = _negBounds.x + xWidth / 2.0f;
        float zCenter = _negBounds.z + zWidth / 2.0f;

        SpawnPerimeter(new Vector3(_posBounds.x, 0, zCenter), new Vector3(-1, 0, 0), zWidth);
        SpawnPerimeter(new Vector3(_negBounds.x, 0, zCenter), new Vector3(1, 0, 0), zWidth);
        SpawnPerimeter(new Vector3(xCenter, 0, _posBounds.z), new Vector3(0, 0, -1), xWidth);
        SpawnPerimeter(new Vector3(xCenter, 0, _negBounds.z), new Vector3(0, 0, 1), xWidth);

        UpdatePerimeterVisibility(Vector3.zero);
    }

    private void OnDrawGizmosSelected()
    {

    }

    private void SpawnPerimeter(Vector3 pos, Vector3 normal, float width)
    {
        PerimeterPlaneData data = new PerimeterPlaneData();

        var obj = Instantiate(_perimeterWarningPrefab, transform, false);

        data.Instance = obj;
        data.WarningPlane = new Plane(normal, pos);
        data.Renderer = obj.GetComponentInChildren<MeshRenderer>();
        data.Projector = obj.GetComponentInChildren<DecalProjector>();

        if (data.Renderer != null)
        {
            data.MeshProps = new MaterialPropertyBlock();
            data.Renderer.SetPropertyBlock(data.MeshProps);
            //data.GridMaterial = Instantiate<Material>(data.GridMaterial);
        }

        if (data.Projector != null)
        {
            //clone decal material - current version of decal projector doesn't seem to support property blocks
            data.DecalMaterial = Instantiate<Material>(data.Projector.material);
            data.Projector.material = data.DecalMaterial;
        }

        Quaternion rot = Quaternion.LookRotation(normal, Vector3.up);
        //Quaternion rot = Quaternion.FromToRotation(new Vector3(0, 0, 1), normal);
        UpdatePerimiterPosition(ref data, pos, rot, width);

        _warningPlanes.Add(data);

        //var decalProjectors = obj.GetComponentsInChildren<DecalProjector>();
        //var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();

        //Vector3 normal = new Vector3(1, 0, 0);
        //normal = rot * normal;

        //_warningPlanes.Add(new Plane(normal, pos));

        //if (decalProjectors.Length > 0 && _projMat == null)
        //    _projMat = Instantiate<Material>(decalProjectors[0].material);
        //if (meshRenderers.Length > 0 && _gridMat == null)
        //    _gridMat = Instantiate<Material>(meshRenderers[0].sharedMaterial);

        //foreach (var proj in decalProjectors)
        //    proj.material = _projMat;
        //foreach (var rend in meshRenderers)
        //    rend.material = _gridMat;
    }

    private void UpdatePerimiterPosition(ref PerimeterPlaneData data, Vector3 pos, Quaternion rot, float width)
    {
        data.Instance.transform.localPosition = pos;
        data.Instance.transform.localRotation = rot;

        if (data.Renderer != null)
        {
            var meshFilter = data.Renderer.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.mesh = _gridMesh;
                meshFilter.transform.localPosition = Vector3.zero;
                meshFilter.transform.localRotation = Quaternion.identity;
            }
        }

        ScalePrefab(ref data, width);

        //if (data.Renderer != null)
        //{
        //    var meshFilter = data.Renderer.GetComponent<MeshFilter>();
        //    if (meshFilter != null)
        //    {
        //        BuildMesh(ref data, pos, rot);
        //        meshFilter.mesh = data.Mesh;
        //        meshFilter.transform.localPosition = Vector3.zero;
        //        meshFilter.transform.localRotation = Quaternion.identity;
        //    }
        //}
    }

    private void ScalePrefab(ref PerimeterPlaneData data, float width)
    {
        if (data.Renderer != null)
        {
            data.Renderer.transform.localScale = new Vector3(width/GridMeshWidth, 1, 1);

            float uvScale = Mathf.Round(width / GridMeshWidth);
            if (uvScale <= 0)
                uvScale = 1;
            data.UVScale = new Vector2(uvScale, 1);
        }

        if (data.Projector != null)
        {
            var size = data.Projector.size;
            size.x = width;
            data.Projector.size = size;
        }
    }

    //private void BuildMesh(ref PerimeterPlaneData data, Vector3 pos, Quaternion rot)
    //{
    //    if (data.Mesh == null)
    //        data.Mesh = new Mesh();

    //    var topLeft = pos + new Vector3(-5, 5, 0);
    //    var bottomRight = pos + new Vector3(5, 0, 0);
    //    ProcGeometry.GeneratePlane(topLeft, bottomRight, 3, 3, ref data.Vertices, ref data.Triangles, ref data.UV);

    //    data.Mesh.vertices = data.Vertices;
    //    data.Mesh.triangles = data.Triangles;
    //    data.Mesh.uv = data.UV;

    //    data.Mesh.RecalculateNormals();
    //    data.Mesh.RecalculateBounds();
    //}

    private void UpdatePerimeterVisibility(PerimeterPlaneData data, Vector3 playerPos)
    {
        float alpha = 0;

        var dist = Mathf.Abs(data.WarningPlane.GetDistanceToPoint(playerPos));
        //Debug.Log(dist);

        if (dist < _startWarnDist)
        {
            alpha = 1.0f - (dist / _startWarnDist);
            WarningColor.a = alpha;
            GridColor.a = alpha;

            if (data.DecalMaterial != null)
            {
                data.DecalMaterial.SetColor("_BaseColor", WarningColor);
            }

            if (data.MeshProps != null && data.Renderer != null)
            {
                //data.Renderer.GetPropertyBlock(data.MeshProps);
                data.MeshProps.Clear();
                data.MeshProps.SetColor("_UnlitColor", GridColor);
                //data.MeshProps.SetVector("_UnlitColorMap_ST", new Vector2(15, 15));
                data.MeshProps.SetVector("_UnlitColorMap_ST", data.UVScale);

                data.Renderer.SetPropertyBlock(data.MeshProps);
                //if (!data.Renderer.HasPropertyBlock())
                //    data.Renderer.SetPropertyBlock(data.MeshProps);
            }

            data.Instance.SetActive(true);
        }
        else
        {
            data.Instance.SetActive(false);
        }

    }

    //private float ComputeFadeLevel(Vector3 pos)
    //{
    //    float minDist = float.MaxValue;

    //    for (int i = 0; i < _warningPlanes.Count; i++)
    //    {
    //        var dist = Mathf.Abs(_warningPlanes[i].GetDistanceToPoint(pos));
    //        if (dist < minDist)
    //            minDist = dist;
    //    }

    //    //Debug.Log(minDist);

    //    if (minDist > _startWarnDist)
    //        return 0;

    //    return 1.0f - (minDist / _startWarnDist);

    //}

    void UpdatePerimeterVisibility(Vector3 point)
    {
        foreach (var data in _warningPlanes)
            UpdatePerimeterVisibility(data, point);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 point = transform.InverseTransformPoint(HeadColliderTransform.position);

        UpdatePerimeterVisibility(point);

        //var fade = ComputeFadeLevel(point);

        //WarningColor.a = fade;
        //GridColor.a = fade;

        //_projMat.SetColor("_BaseColor", WarningColor);
        //_gridMat.SetColor("_UnlitColor", GridColor);

        //if (point.x > 0)
        //{
        //    _xPosT = (point.x - (SystemManager.SystemConfig.PositiveXExtentDistance - StartWarnDistanceMeters)) / (SystemManager.SystemConfig.PositiveXExtentDistance - StartWarnDistanceMeters);
        //    _xNegT = 0;
        //}
        //else
        //{
        //    _xNegT = (point.x + (SystemManager.SystemConfig.NegativeXExtentDistance - StartWarnDistanceMeters)) / -(SystemManager.SystemConfig.NegativeXExtentDistance - StartWarnDistanceMeters);
        //    _xPosT = 0;
        //}
        //if (point.z > 0)
        //{
        //    _zPosT = (point.z - (SystemManager.SystemConfig.PositiveZExtentDistance - StartWarnDistanceMeters)) / (SystemManager.SystemConfig.PositiveZExtentDistance - StartWarnDistanceMeters);
        //    _zNegT = 0;
        //}
        //else
        //{
        //    _zNegT = (point.z + (SystemManager.SystemConfig.NegativeZExtentDistance - StartWarnDistanceMeters)) / -(SystemManager.SystemConfig.NegativeZExtentDistance - StartWarnDistanceMeters);
        //    _zPosT = 0;
        //}
        //Color xPos = WarningColor;
        //Color xNeg = WarningColor;
        //Color zPos = WarningColor;
        //Color zNeg = WarningColor;
        //xPos.a = Mathf.Lerp(0, 1, _xPosT);
        //xNeg.a = Mathf.Lerp(0, 1, _xNegT);
        //zPos.a = Mathf.Lerp(0, 1, _zPosT);
        //zNeg.a = Mathf.Lerp(0, 1, _zNegT);
        //_XPosDecal.material.SetColor("_BaseColor", xPos);
        //_XNegDecal.material.SetColor("_BaseColor", xNeg);
        //_ZPosDecal.material.SetColor("_BaseColor", zPos);
        //_ZNegDecal.material.SetColor("_BaseColor", zNeg);
    }
}
