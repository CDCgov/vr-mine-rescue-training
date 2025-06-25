using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VectorGraphics;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

//using EzySlice;
using DataStructures.ViliWonka.KDTree;
using UnityEngine.SceneManagement;
using Cinemachine.Utility;
using System.IO;
using System.IO.Compression;

//public struct MapGridEntry
//{

//    public MineSegment OwningSegment;
//    public MapGridEntry(MineSegment owningSegment)
//    {
//        OwningSegment = owningSegment;
//    }
//}

public class VectorMineMapData
{
    public LineSpriteGenerator LineSpriteGenerator;
    public MapGrid MapGrid;
    public Vector2 BoundsMax;
    public Vector2 BoundsMin;
}

public class MapGrid
{
    public float GridSize = 2.00f;
    public Vector2 GridOrigin;

    private bool[,] _grid;


    public int Width
    {
        get { return _grid.GetLength(0); }
    }

    public int Height
    {
        get { return _grid.GetLength(1); }
    }

    //public bool[,] GridData
    //{
    //    get { return _grid; }
    //}

    public MapGrid(bool initialize = true)
    {
        GridOrigin = new Vector2(-500, -500);
        _grid = new bool[1000, 1000];

        if (!initialize)
            return;

        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int z = 0; z < _grid.GetLength(1); z++)
            {
                //_grid[x, z] = new MapGridEntry(null);
                _grid[x, z] = false;
            }
        }
    }

    public void ResetOrigin()
    {
        GridOrigin = new Vector2((float)Width * GridSize * -0.5f, 
                                (float)Height * GridSize * -0.5f);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(GridSize);
        writer.WriteVector(GridOrigin);

        if (_grid == null || Width <= 0 || Height <= 0)
        {
            writer.Write((int)0);
            writer.Write((int)0);
            return;
        }

        writer.Write(Width);
        writer.Write(Height);

        for (int x = 0; x < _grid.GetLength(0); x++)
        {
            for (int z = 0; z < _grid.GetLength(1); z++)
            {
                writer.Write(_grid[x, z]);
            }
        }
    }

    public void Read(BinaryReader reader)
    {
        GridSize = reader.ReadSingle();
        GridOrigin = reader.ReadVector2();

        var width = reader.ReadInt32();
        var height = reader.ReadInt32();

        Debug.Log($"MapGrid: Reading grid with size {GridSize:F2} origin {GridOrigin.x:F1}, {GridOrigin.y:F1} dimensions {width}x{height}");
        _grid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                _grid[x, z] = reader.ReadBoolean();
            }
        }
    }

    /// <summary>
    /// Returns the world space position aligned with the grid for the provided world space position
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Vector3 GetGridPosition(Vector3 worldPos)
    {
        int gridx = (int)(worldPos.x / GridSize);
        int gridz = (int)(worldPos.z / GridSize);

        worldPos.x = gridx * GridSize;
        worldPos.z = gridz * GridSize;

        //worldPos += new Vector3(GridSize / 2.0f, 0, GridSize / 2.0f);

        return worldPos;

        //if (!GetGridCoordinate(worldPos, out int gridx, out int gridz))
        //    return Vector3.zero;

        //Vector2 gridPos = GetGridPosition(gridx, gridz);

        //return new Vector3(gridPos.x, worldPos.y, gridPos.y);
    }

    public Vector3 GetGridPositionCeil(Vector3 worldPos)
    {
        int gridx = (int)Mathf.Ceil((worldPos.x / GridSize));
        int gridz = (int)Mathf.Ceil((worldPos.z / GridSize));

        worldPos += new Vector3(GridSize / 2.0f, 0, GridSize / 2.0f);

        worldPos.x = gridx * GridSize;
        worldPos.z = gridz * GridSize;

        return worldPos;
    }

    /// <summary>
    /// Get the coordinate index of the world space position in the grid
    /// </summary>
    public bool GetGridCoordinate(Vector2 pos, out int gridx, out int gridz)
    {
        pos = pos - GridOrigin;
        gridx = (int)(pos.x / GridSize);
        gridz = (int)(pos.y / GridSize);

        if (gridx < 0 || gridx >= _grid.GetLength(0))
            return false;
        if (gridz < 0 || gridz >= _grid.GetLength(1))
            return false;

        return true;
    }

    /// <summary>
    /// Returns bottom left corner position in world space
    /// </summary>
    /// <returns>Bottom left corner position in world space</returns>
    public Vector2 GetGridPosition(int gridx, int gridz)
    {
        Vector2 pos = new Vector2();
        pos.x = gridx * GridSize;
        pos.y = gridz * GridSize;

        pos = pos + GridOrigin;
        return pos;
    }

    //public void SetOwner(Vector2 pos, MineSegment seg)
    //{
    //    var entry = GetEntry(pos);
    //    entry.OwningSegment = seg;
    //    SetEntry(pos, entry);
    //}

    //public MineSegment GetOwner(Vector2 pos)
    //{
    //    var entry = GetEntry(pos);
    //    return entry.OwningSegment;
    //}

    public void SetOccupied(Vector2 pos, bool occupied)
    {
        if (!GetGridCoordinate(pos, out int gridx, out int gridz))
            return;

        SetOccupied(gridx, gridz, occupied);
    }

    public void SetOccupied(int gridx, int gridz, bool occupied)
    {
        _grid[gridx, gridz] = occupied;
    }

    public bool GetOccupied(Vector2 pos)
    {
        if (!GetGridCoordinate(pos, out int gridx, out int gridz))
            return false;

        return GetOccupied(gridx, gridz);
    }

    public bool GetOccupied(int gridx, int gridz)
    {
        return _grid[gridx, gridz];
    }

    //public MapGridEntry GetEntry(Vector2 pos)
    //{
    //    int gridx, gridz;
    //    if (!GetGridCoordinate(pos, out gridx, out gridz))
    //        return false;

    //    return _grid[gridx, gridz];
    //}

    //public MapGridEntry GetEntry(int gridx, int gridz)
    //{
    //    return _grid[gridx, gridz];
    //}

    //public bool SetEntry(Vector2 pos, MapGridEntry entry)
    //{
    //    int gridx, gridz;
    //    if (!GetGridCoordinate(pos, out gridx, out gridz))
    //        return false;

    //    _grid[gridx, gridz] = entry;
    //    return true;
    //}

    //public void SetEntry(int gridx, int gridz, MapGridEntry entry)
    //{
    //    _grid[gridx, gridz] = entry;
    //}

    public Vector2 RightCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(GridSize, GridSize / 2.0f);
        return pos;
    }
    public Vector2 LeftCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(0, GridSize / 2.0f);
        return pos;
    }

    public Vector2 TopCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(GridSize / 2.0f, GridSize);
        return pos;
    }

    public Vector2 BottomCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(GridSize / 2.0f, 0);
        return pos;
    }

    public Vector2 TopLeftCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(0, GridSize);
        return pos;
    }

    public Vector2 BottomLeftCoord(int gridx, int gridz)
    {
        return GetGridPosition(gridx, gridz);
    }

    public Vector2 BottomRightCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(GridSize, 0);
        return pos;
    }

    public Vector2 TopRightCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(GridSize, GridSize);
        return pos;
    }

    public Vector2 CenterCoord(int gridx, int gridz)
    {
        Vector2 pos = GetGridPosition(gridx, gridz);
        pos = pos + new Vector2(GridSize / 2.0f, GridSize / 2.0f);
        return pos;
    }

    public bool RaycastHorizontal(Vector2 pos, float maxDist, out Vector2 start, out Vector2 end)
    {

        return Raycast(pos, maxDist, 1, 0, out start, out end, false);
    }

    public bool RaycastVertical(Vector2 pos, float maxDist, out Vector2 start, out Vector2 end)
    {
        return Raycast(pos, maxDist, 0, -1, out start, out end, false);
    }

    public bool RaycastDiagonal(Vector2 pos, float maxDist, out Vector2 start, out Vector2 end)
    {
        return Raycast(pos, maxDist, 1, 1, out start, out end, false);
    }

    private bool Raycast(Vector2 pos, float maxDist, int stepx, int stepz, out Vector2 start, out Vector2 end, bool inclusive = false)
    {
        start = end = Vector2.zero;

        int gridx, gridz;
        if (!GetGridCoordinate(pos, out gridx, out gridz))
            return false;
        var occupied = GetOccupied(gridx, gridz);
        if (!occupied)
            return false;

        int startx, startz;
        int endx, endz;

        int maxSteps = Mathf.CeilToInt(maxDist / GridSize);

        if (!ScanUntilEmpty(gridx, gridz, stepx * -1, stepz * -1, maxSteps, out startx, out startz, inclusive))
            return false;
        if (!ScanUntilEmpty(gridx, gridz, stepx, stepz, maxSteps, out endx, out endz, inclusive))
            return false;

        if (stepx > 0 && stepz > 0)
        {
            //diagonal
            start = BottomLeftCoord(startx, startz);
            end = TopRightCoord(endx, endz);

            //start = CenterCoord(startx, startz);
            //end = CenterCoord(endx, endz);
        }
        else if (stepx > 0)
        {
            //horizontal
            start = LeftCoord(startx, startz);
            end = RightCoord(endx, endz);

            //start = CenterCoord(startx, startz);
            //end = CenterCoord(endx, endz);
        }
        else
        {
            //vertical
            start = TopCoord(startx, startz);
            end = BottomCoord(endx, endz);

            //start = CenterCoord(startx, startz);
            //end = CenterCoord(endx, endz);
        }
        return true;
    }


    /// <summary>
    /// Inclusive set to true returns first empty cell
    /// Inclusive set to false returns cell before empty cell
    /// </summary>
    /// <param name="gridx"></param>
    /// <param name="gridz"></param>
    /// <param name="entry"></param>
    /// <param name="stepx"></param>
    /// <param name="stepz"></param>
    /// <param name="maxSteps"></param>
    /// <param name="resultx"></param>
    /// <param name="resultz"></param>
    /// <param name="inclusive"></param>
    /// <returns></returns>
    private bool ScanUntilEmpty(int gridx, int gridz, int stepx, int stepz, int maxSteps, out int resultx, out int resultz, bool inclusive = false)
    {
        var x = gridx;
        var z = gridz;
        resultx = gridx;
        resultz = gridz;

        int steps = 0;

        while (true)
        {
            try
            {
                x += stepx;
                z += stepz;
                steps++;

                var nextEntry = GetOccupied(x, z);
                if (!inclusive)
                {
                    if (nextEntry == false)
                        break;
                }

                resultx = x;
                resultz = z;

                if (inclusive)
                {
                    if (nextEntry == false)
                        break;
                }

                if (steps > maxSteps)
                    return false;
            }
            catch (System.Exception) { return false; }
        }

        return true;
    }
}

public class MineMapClickedEventData
{
    public PointerEventData PointerEvent;
    public Vector3 WorldSpacePosition;
}

[RequireComponent(typeof(SVGImage))]
public class VectorMineMap : MonoBehaviour, IPointerClickHandler
{
    private class SymbolInfo
    {
        public MineMapSymbol Symbol;
        public GameObject Object;
        public RectTransform RT;
        public Vector3 LastPosition;
        public Quaternion LastRotation;
        public Vector3 SymbolPosition;
        public Quaternion SymbolRotation;
    }

    // public static List<MineMapSymbol> ActiveSymbols = new List<MineMapSymbol>();

    public MineMapSymbolManager MineMapSymbolManager;
    public SceneLoadManager SceneLoadManager;

    public Color MapWallColor = Color.white;
    public bool OverrideMapSymbolColors = false;
    public Color MapSymbolColor = Color.white;
    public float LineThickness = 0.1f;
    public float BorderSize = 0.1f;

    public GameObject ResearcherCam;
    //public GameObject PlayerArrowPrefab;

    public event System.Action<MineMapClickedEventData> MapClicked;

    [System.NonSerialized]
    public MapGrid MapGrid;

    private KDTree _mineSegmentTree;
    private Dictionary<int, MineSegment> _mineSegmentMap;
    private KDQuery _mineSegmentQuery;
    private List<int> _mineSegmentQueryResults;

    private RectTransform _rect;
    //private MineNetwork _mineNet;
    private SVGImage _svgImage;
    private LineSpriteGenerator _lineSprite;
    Vector2 _mineMin;
    Vector2 _mineMax;
    bool _boundsInitialized = false;

    Matrix4x4 _worldToCanvasMat;
    Matrix4x4 _canvasToWorldMat;
    private float _lastSceneLoadTime = 0;

    private float _lineThickness;


    //private List<SymbolInfo> _symbolInfoList;
    private Dictionary<long, SymbolInfo> _symbolMap = new Dictionary<long, SymbolInfo>();
    private Dictionary<MineMapSymbol, SymbolInfo> _localSymbols = new Dictionary<MineMapSymbol, SymbolInfo>();

    private bool _mapBuildInProgress = false;

    private SymbolInfo _tempSymbolInfo;
    private AsyncOperationHandle<MineMapSymbol> _tempSymbolHandle;
    [HideInInspector]
    public MineMapSymbol SelectedSymbol;

    void Start()
    {

        if (MineMapSymbolManager == null)
            MineMapSymbolManager = MineMapSymbolManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);

        MapGrid = new MapGrid();

        _rect = GetComponent<RectTransform>();
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        if (ScenarioInitializer.Instance != null)
        {
            ScenarioInitializer.Instance.onCustomScenarioLoad += OnCustomSceneChanged;
        }
        else
        {
            GameObject obj = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
            obj.name = "ScenarioInitializer";
            obj.AddComponent<ScenarioInitializer>();
            ScenarioInitializer.Instance.onCustomScenarioLoad += OnCustomSceneChanged;
        }


        _svgImage = gameObject.GetComponent<SVGImage>();
        _lineSprite = new LineSpriteGenerator(); // gameObject.AddComponent<LineSpriteGenerator>();
        //_lineSprite.name = gameObject.name;
        /*
		_lineSprite.AddPath(new Vector2[]
		{
			new Vector2(0,0),
			new Vector2(100,0),
			new Vector2(100,100),
			new Vector2(0,100),
		}, Color.red, 1.0f);
		*/
        _lineSprite.UpdateSprite(gameObject);

        //_symbolInfoList = new List<SymbolInfo>();

        var sw = new System.Diagnostics.Stopwatch();
        //sw.Start();
        //BuildBaseMine();
        //sw.Stop();
        //Debug.Log($"Building 2D Mine Map took {sw.ElapsedMilliseconds}ms");

        //_ = BuildBaseMine();
        //UpdateTransformMatrices();
        _ = RebuildMap();

        //add existing symbols
        //foreach (var symbol in MineMapSymbolManager.ActiveSymbols)
        //{
        //    OnSymbolAdded(symbol);
        //}

        MineMapSymbolManager.SymbolAdded += OnSymbolAdded;
        MineMapSymbolManager.SymbolRemoved += OnSymbolRemoved;
        MineMapSymbolManager.SymbolColorChanged += OnSymbolColorChanged;
    }

    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
        ScenarioInitializer.Instance.onCustomScenarioLoad -= OnCustomSceneChanged;
    }

    private void OnSymbolColorChanged(MineMapSymbol symbol)
    {
        if (_mapBuildInProgress)
            return;

        SymbolInfo info;
        if (!_symbolMap.TryGetValue(symbol.SymbolID, out info) &&
            !_localSymbols.TryGetValue(symbol, out info))
            return;

        if (info.Object == null)
            return;
        if (info.Symbol.AllowColorChange)
        {
            ResetSymbolColor(info);
        }        
        else if (OverrideMapSymbolColors)
        {
            //ResetSymbolColor(info);
            if (SelectedSymbol != null)
            {
                if(SelectedSymbol == symbol)
                {
                    Debug.Log($"Changing the selected symbol color! {symbol.DisplayName}, {symbol.Color}");
                    symbol.SetColor(info.Object, symbol.Color);
                }
                else
                {
                    symbol.SetColor(info.Object, MapSymbolColor);
                }
            }
            else
            {
                symbol.SetColor(info.Object, MapSymbolColor);
            }
        }
        else
        {
            symbol.SetColor(info.Object, symbol.Color);
        }

        
    }

    //public bool PointInBounds(Vector3 worldPos)
    //{
    //    var rt = transform.parent as RectTransform;
    //    if (rt == null)
    //        rt = transform as RectTransform;

    //    if (rt == null)
    //        return true;

    //    Vector2 pos = rt.InverseTransformPoint(worldPos);
    //    return rt.rect.Contains(pos);
    //}

    public void GetSymbolObject(MineMapSymbol symbol, out GameObject go)
    {
        SymbolInfo info;
        if (!_symbolMap.TryGetValue(symbol.SymbolID, out info) &&
            !_localSymbols.TryGetValue(symbol, out info))
        {
            go = null;
            return;
        }

        go = info.Object;
    }

    public void GetSymbolParent(MineMapSymbol symbol, out MineMapSymbol parent)
    {
        if (_mapBuildInProgress)
        {
            parent = null;
        }

        SymbolInfo info;
        if (!_symbolMap.TryGetValue(symbol.SymbolID, out info) &&
            !_localSymbols.TryGetValue(symbol, out info))
        {
            parent = null;
            return;
        }
        parent = info.Symbol.ParentSymbol;
        
    }

    public void GetSymbolRectTransform(MineMapSymbol symbol, out RectTransform rect)
    {
        if (_mapBuildInProgress)
        {
            rect = null;
        }

        SymbolInfo info;
        if (!_symbolMap.TryGetValue(symbol.SymbolID, out info) &&
            !_localSymbols.TryGetValue(symbol, out info))
        {
            rect = null;
            return;
        }

        rect = info.RT;
    }

    public void InstantiateTempSymbol(string addressable, Vector3 pos, Quaternion rot, string text = null)
    {
        //if (_tempSymbolHandle.IsValid())
        //    return;

        if (_tempSymbolInfo != null)
            return;

        _tempSymbolHandle = Addressables.LoadAssetAsync<MineMapSymbol>(addressable);
        _tempSymbolHandle.Completed += (h) =>
        {
            if (_tempSymbolInfo != null)
            {
                Addressables.Release(_tempSymbolHandle);
            }

            var symbol = Instantiate<MineMapSymbol>(h.Result);
            symbol.AddressableKey = addressable;

            _tempSymbolInfo = InstantiateSymbol(symbol);
            //symbol.WorldRotation = rot;
            PositionSymbol(_tempSymbolInfo, pos, rot);
            ResetSymbolColor(_tempSymbolInfo);
        };
    }

    public void PositionTempSymbol(Vector3 worldPos)
    {
        if (_tempSymbolInfo == null)
            return;

        _tempSymbolInfo.Symbol.WorldRotation = Quaternion.identity;
        PositionSymbol(_tempSymbolInfo, worldPos, _tempSymbolInfo.Symbol.WorldRotation);
        
    }

    public void ClearTempSymbol()
    {
        if (_tempSymbolInfo == null)
            return;

        //destroy game object
        DestroySymbol(_tempSymbolInfo);

        //destroy scriptable object copy
        Destroy(_tempSymbolInfo.Symbol);

        _tempSymbolInfo = null;

        Addressables.Release(_tempSymbolHandle);
    }


    private void OnSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        //ActiveSymbols.Clear();
        _lastSceneLoadTime = Time.unscaledTime;
        //StartCoroutine(RebuildMap());
        ClearMap();
        //Debug.Log("Vector mine map scene change called");
    }

    private void OnCustomSceneChanged()
    {
        //ActiveSymbols.Clear();
        _lastSceneLoadTime = Time.unscaledTime;
        //StartCoroutine(RebuildMap());
        ClearMap();
        //Debug.Log("Vector mine map scene change called");
    }

    private void ClearMap()
    {
        ClearSymbols();
        //MapGrid = new MapGrid();

        //clear any untracked symbols
        foreach (Transform xform in transform)
        {
            Destroy(xform.gameObject);
        }

        if (_lineSprite != null)
        {
            _lineSprite.Clear();
            _lineSprite.UpdateSprite(gameObject);
            //Destroy(_lineSprite);
            _lineSprite = null;
        }
    }

    private async Task RebuildMap()
    {
        //in builds, the objects from the scene manager aren't ready for some reason
        //delay a few frames so they are ready
        //yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();

        ClearSymbols();
        MapGrid = new MapGrid();

        await BuildBaseMine();
        //UpdateTransformMatrices();

        foreach (var symbol in MineMapSymbolManager.ActiveSymbols)
        {
            OnSymbolAdded(symbol);
        }

    }

    //private void SliceMesh(MineSegment seg)
    //{
    //    //var hull = seg.gameObject.Slice(new EzySlice.Plane(new Vector3(0, 1, 0), Vector3.up));
    //    //var filters = seg.GetComponentsInChildren<MeshFilter>();
    //    //foreach (var filter in filters)

    //    var colliders = seg.GetComponentsInChildren<MeshCollider>();
    //    foreach (var collider in colliders)
    //    {
    //        //g3.DMesh3 m = new g3.DMesh3()
    //        //Debug.Log($"{seg.name} {filter.gameObject.name} - {filter.sharedMesh.name}");
    //        //if (filter.sharedMesh == null || !filter.gameObject.activeInHierarchy)
    //        //	continue;
    //        //if (!filter.sharedMesh.isReadable)
    //        //	return;

    //        if (collider.sharedMesh == null || !collider.gameObject.activeInHierarchy || !collider.sharedMesh.isReadable)
    //            continue;

    //        //Debug.Log($"Slicing {seg.name} {filter.gameObject.name} - {filter.sharedMesh.name}");

    //        //var hull = Slicer.Slice(filter.sharedMesh, 
    //        //	new EzySlice.Plane(new Vector3(0, 1, 0), Vector3.up), 
    //        //	new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), 0);

    //        //var hull = filter.gameObject.Slice(new EzySlice.Plane(new Vector3(0, 1, 0), Vector3.up));

    //        var hull = Slicer.Slice(collider.sharedMesh,
    //            new EzySlice.Plane(new Vector3(0, 1, 0), Vector3.up),
    //            new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f), 0);

    //        if (hull != null)
    //        {

    //            var obj = hull.CreateLowerHull();
    //            obj.name = seg.name + collider.gameObject.name;
    //            obj.transform.position = collider.transform.position;
    //        }
    //    }

    //}

    private static async Task PixelizeSegment(MapGrid grid, MineSegment seg)
    {
        MeshCollider[] colliders = null;

        if (seg.TryGetComponent<MineMapColliders>(out var mineMapColliders) 
            && mineMapColliders.MeshColliders != null && mineMapColliders.MeshColliders.Length > 0)
        {
            colliders = mineMapColliders.MeshColliders;
        }
        else
        {
            colliders = seg.GetComponentsInChildren<MeshCollider>();
        }

        if (colliders == null || colliders.Length <= 0)
            return;

        foreach (var collider in colliders)
        {

            if (collider.sharedMesh == null || !collider.gameObject.activeInHierarchy || !collider.sharedMesh.isReadable || collider.sharedMesh.vertices == null)
                continue;

            if (collider.TryGetComponent<DoNotMap>(out var _))
                continue;

            var verts = collider.sharedMesh.vertices;
            var triangles = collider.sharedMesh.triangles;
            var matrix = collider.transform.localToWorldMatrix;

            Debug.Log($"VectorMineMap Pixelizing Mesh with {verts.Length} vertices and {triangles.Length} triangle indices");
            //Note: something causes this to run significantly faster on a background thread despite being sequential - further investigation needed
            await Task.Run(() =>
            {
                try
                {
                    PixelizeMesh(grid, verts, triangles, matrix, seg);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error running pixelize mesh {ex.Message} {ex.StackTrace}");
                }
            });
        }
    }

    private static void PixelizeMesh(MapGrid grid, Vector3[] verts, int[] triangles, Matrix4x4 localToWorld, MineSegment seg)
    {
        foreach (var vert in verts)
        {
            //var v = xform.TransformPoint(vert);
            var v = localToWorld.MultiplyPoint(vert);
            //MapGrid.SetOwner(new Vector2(v.x, v.z), seg);
            grid.SetOccupied(new Vector2(v.x, v.z), true);
        }

        for (int i = 0; i < triangles.Length - 2; i += 3)
        {
            try
            {
                var v1 = verts[triangles[i]];
                var v2 = verts[triangles[i + 1]];
                var v3 = verts[triangles[i + 2]];
                PixelizeEdge(grid, v1, v2, seg, localToWorld);
                PixelizeEdge(grid, v2, v3, seg, localToWorld);
                PixelizeEdge(grid, v3, v1, seg, localToWorld);

                //var center = v1 + v2 + v3;
                //center /= 3.0f;

                //center = localToWorld.MultiplyPoint(center);
                //grid.SetOccupied(new Vector2(center.x, center.z), true);

                PixelizeTriangle(grid, v1, v2, v3, localToWorld);
            }
            catch (System.Exception)
            {

            }
        }
    }

    private static void PixelizeTriangle(MapGrid grid, Vector3 v1, Vector3 v2, Vector3 v3, Matrix4x4 localToWorld)
    {
        v1 = localToWorld.MultiplyPoint(v1);
        v2 = localToWorld.MultiplyPoint(v2);
        v3 = localToWorld.MultiplyPoint(v3);

        Bounds bounds = new Bounds(v1, Vector3.zero);
        bounds.Encapsulate(v2);
        bounds.Encapsulate(v3);

        Vector2 min = new Vector2(bounds.min.x, bounds.min.z);
        Vector2 max = new Vector2(bounds.max.x, bounds.max.z);

        grid.GetGridCoordinate(min, out var gridMinX, out var gridMinZ);
        grid.GetGridCoordinate(max, out var gridMaxX, out var gridMaxZ);

        //gridMinX -= 1;
        //gridMaxX += 1;
        //gridMinZ -= 1;
        //gridMaxZ += 1;

        if (gridMinZ > gridMaxZ || gridMinX > gridMaxX)
        {
            Debug.LogError($"Error computing bounds on map triangle {v1} {v2} {v3}");
            return;
        }

        if (gridMinX < 0)
            gridMinX = 0;
        if (gridMinZ < 0)
            gridMinZ = 0;

        if (gridMaxX >= grid.Width)
            gridMaxX = grid.Width - 1;
        if (gridMaxZ >= grid.Height)
            gridMaxZ = grid.Height - 1;

        Vector2 a = new Vector2(v1.x, v1.z);
        Vector2 b = new Vector2(v2.x, v2.z);
        Vector2 c = new Vector2(v3.x, v3.z);

        for (int x = gridMinX; x <= gridMaxX; x++)
        {
            for (int z = gridMinZ; z <= gridMaxZ; z++)
            {
                //grid.SetOccupied(x, z, true);
                //var pt = grid.BottomRightCoord(x, z);
                //var pt2 = grid.BottomLeftCoord(x, z);
                //var pt3 = grid.TopRightCoord(x, z);
                //var pt4 = grid.TopLeftCoord(x, z);
                //if (Util.PointInTriangle(pt, a, b, c) ||
                //    Util.PointInTriangle(pt2, a, b, c) ||
                //    Util.PointInTriangle(pt3, a, b, c) ||
                //    Util.PointInTriangle(pt4, a, b, c))
                //{
                //    grid.SetOccupied(x, z, true);
                //}   

                var pt = grid.CenterCoord(x, z);
               
                if (Util.PointInTriangle(pt, a, b, c))  
                {
                    grid.SetOccupied(x, z, true);
                }
            }
        }
    }

    private static void PixelizeEdge(MapGrid grid, Vector3 v1, Vector3 v2, MineSegment seg, Matrix4x4 localToWorld)
    {
        var pixelizeDist = grid.GridSize * 0.75f;
        if (pixelizeDist <= 0)
            return;

        var dir = v2 - v1;
        var length = dir.magnitude;
        if (length <= pixelizeDist)
            return;

        dir.Normalize();
        var dist = pixelizeDist;
        while (dist < length)
        {
            var v = v1 + dir * dist;
            //v = xform.TransformPoint(v);
            v = localToWorld.MultiplyPoint(v);
            //MapGrid.SetOwner(new Vector2(v.x, v.z), seg);
            grid.SetOccupied(new Vector2(v.x, v.z), true);

            dist += pixelizeDist;
        }
    }

    private void SweepFromPoint(Vector3 pt, float dist, int numSamples, int divisor, Bounds bounds)
    {
        List<Vector2> curSeg = new List<Vector2>(numSamples);
        RaycastHit hit;

        int mask = LayerMask.GetMask("Floor", "Roof", "Walls");
        bounds.Expand(0.5f);

        for (int i = 0; i < numSamples; i++)
        {
            float angle = (float)i / (float)(numSamples - 1);
            angle *= 360.0f;

            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 dir = rot * Vector3.forward;

            bool rayHit = Physics.Raycast(pt, dir, out hit, dist, mask);
            if (rayHit && bounds.Contains(hit.point))
            {
                //continue the segment
                curSeg.Add(hit.point.XZProjection());
            }
            else
            {
                //complete the segment
                if (curSeg.Count >= 2)
                {
                    //var ptArray = curSeg.ToArray();
                    var ptArray = SimplifySegment(curSeg, divisor);
                    _lineSprite.AddPath((ICollection<Vector2>)ptArray, MapWallColor, _lineThickness);
                    curSeg.Clear();

                    foreach (var p in ptArray)
                    {
                        if (!_boundsInitialized)
                        {
                            //_mineBounds = new Bounds(new Vector3(p.x, 0, p.y), Vector3.zero);
                            _mineMin = _mineMax = p;
                            _boundsInitialized = true;
                        }
                        else
                        {
                            //_mineBounds.Encapsulate(p);
                            _mineMin = Vector2.Min(_mineMin, p);
                            _mineMax = Vector2.Max(_mineMax, p);
                        }
                    }

                    //Debug.Log($"Added segment with {ptArray.Length} points");
                }
            }
        }
    }

    private void UpdateMineMapBounds(Vector2 p)
    {
        UpdateMineMapBounds(p, ref _mineMin, ref _mineMax, ref _boundsInitialized);
    }

    private static void UpdateMineMapBounds(Vector2 p, ref Vector2 boundsMin, ref Vector2 boundsMax, ref bool boundsInitialized)
    {
        if (!boundsInitialized)
        {
            //_mineBounds = new Bounds(new Vector3(p.x, 0, p.y), Vector3.zero);
            boundsMin = boundsMax = p;
            boundsInitialized = true;
        }
        else
        {
            //_mineBounds.Encapsulate(p);
            boundsMin = Vector2.Min(boundsMin, p);
            boundsMax = Vector2.Max(boundsMax, p);
        }
    }

    private bool IsPointInBounds(Vector2 p)
    {
        if (p.x < _mineMin.x || p.x > _mineMax.x)
            return false;
        if (p.y < _mineMin.y || p.y > _mineMax.y)
            return false;

        return true;
    }

    private void ResetMineBounds()
    {
        _boundsInitialized = false;
        _mineMin = _mineMax = Vector2.zero;
    }

    private Vector2[] SimplifySegment(List<Vector2> points, int divisor)
    {
        if (points.Count <= divisor)
            return points.ToArray();

        int count = points.Count / divisor;
        var seg = new Vector2[count];

        for (int i = 0; i < count - 1; i++)
        {
            seg[i] = points[i * divisor];
        }

        seg[count - 1] = points[points.Count - 1];

        return seg;
    }

    private void FixedUpdate()
    {
        //for some reason, vector sprites can only be created at certain times 
        //(Start(), FixedUpdate(), possibly others)
        //so rebuild the mine map here when it is needed
        //additionally, a delay is needed due to scene objects not 
        //being ready immediatly in the build, while they are in the editor

        if (SceneLoadManager != null && SceneLoadManager.LoadInProgress)
            return;

        if (!_mapBuildInProgress && _lineSprite == null && (Time.unscaledTime - _lastSceneLoadTime) > 2.0f)
            _ = RebuildMap();
    }

    private MineSegment FindClosestSegment(Vector3 worldPos)
    {
        if (_mineSegmentTree == null || _mineSegmentQuery == null || _mineSegmentQueryResults == null)
            return null;

        _mineSegmentQueryResults.Clear();
        _mineSegmentQuery.KNearest(_mineSegmentTree, worldPos, 1, _mineSegmentQueryResults);

        if (_mineSegmentQueryResults.Count <= 0)
            return null;

        var index = _mineSegmentQueryResults[0];
        if (!_mineSegmentMap.TryGetValue(index, out var seg))
            return null;

        if (Vector3.Distance(worldPos, seg.transform.position) > 6) return null;//For snap to intersection behavior fix
                
        return seg;
    }

    public static async Task<VectorMineMapData> BuildBaseMapData(Color mapWallColor, float lineThickness, float gridSize = -1)
    {
        var mapData = new VectorMineMapData
        {
            LineSpriteGenerator = new LineSpriteGenerator(),
            MapGrid = new MapGrid(),
            BoundsMax = Vector2.zero,
            BoundsMin = Vector2.zero,
        };

        bool boundsInitialized = false;

        try
        {

            //await Task.Delay(10000);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            if (gridSize > 0)
            {
                mapData.MapGrid.GridSize = gridSize;
                mapData.MapGrid.ResetOrigin();
                Debug.Log($"VectorMineMap: Using grid size {gridSize} origin {mapData.MapGrid.GridOrigin}");
            }
            else
            {
                var mineNetwork = FindObjectOfType<MineNetwork>();
                if (mineNetwork != null)
                {
                    mapData.MapGrid.GridSize = 1.0f * mineNetwork.SceneTileScale.x;
                    Debug.Log($"VectorMineMap: Found mine network with scale {mineNetwork.SceneTileScale} using grid size {mapData.MapGrid.GridSize}");
                }
                else
                {
                    mapData.MapGrid.GridSize = 1.0f;
                    Debug.Log($"VectorMineMap: No MineNetwork object, using default grid size {mapData.MapGrid.GridSize}");
                }
            }

            Debug.Log($"VectorMineMap: Building base map with grid size {mapData.MapGrid.GridSize}");

            sw.Start();

            //ResetMineBounds();

            //_lineSprite = new LineSpriteGenerator();// gameObject.AddComponent<LineSpriteGenerator>();

            var activeScene = SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();

            foreach (var obj in rootObjects)
            {
                if (obj == null)
                    continue;

                var segments = obj.GetComponentsInChildren<MineSegment>();
                foreach (var seg in segments)
                {
                    if (!seg.IncludeInMap)
                        continue;

                    await PixelizeSegment(mapData.MapGrid, seg);
                }
            }

            sw.Stop();
            Debug.Log($"VectorMineMap: Pixelize took {sw.ElapsedMilliseconds} ms");

            sw.Reset();
            sw.Start();
            //sweep the grid to build the map   
            int width = mapData.MapGrid.Width;
            int height = mapData.MapGrid.Height;
            int segCount = 0;
            for (int x = 0; x < width - 1; x++)
            {
                if (segCount > 10000)   
                {
                    Debug.LogError($"Error building mine map - too many wall segments");
                    break;
                }

                for (int y = 0; y < height - 1; y++)
                {
                    //var entry = MapGrid.GetEntry(x, y);
                    //bool isNull = entry.OwningSegment == null;
                    bool isOcupied = mapData.MapGrid.GetOccupied(x, y);

                    //check adjacent
                    var right = mapData.MapGrid.GetOccupied(x + 1, y);
                    if (right != isOcupied)
                    {
                        //draw right perimeter line
                        Vector2 pos = mapData.MapGrid.GetGridPosition(x, y);
                        Vector2 p1 = pos + new Vector2(mapData.MapGrid.GridSize, mapData.MapGrid.GridSize);
                        Vector2 p2 = pos + new Vector2(mapData.MapGrid.GridSize, 0);
                        mapData.LineSpriteGenerator.AddSegment(p1, p2, mapWallColor, lineThickness);
                        segCount++;

                        UpdateMineMapBounds(p1, ref mapData.BoundsMin, ref mapData.BoundsMax, ref boundsInitialized);
                        UpdateMineMapBounds(p2, ref mapData.BoundsMin, ref mapData.BoundsMax, ref boundsInitialized);
                    }

                    var above = mapData.MapGrid.GetOccupied(x, y + 1);
                    if (above != isOcupied)
                    {
                        //draw above perimeter line
                        Vector2 pos = mapData.MapGrid.GetGridPosition(x, y);
                        Vector2 p1 = pos + new Vector2(mapData.MapGrid.GridSize, mapData.MapGrid.GridSize);
                        Vector2 p2 = pos + new Vector2(0, mapData.MapGrid.GridSize);
                        mapData.LineSpriteGenerator.AddSegment(p1, p2, mapWallColor, lineThickness);
                        segCount++;

                        UpdateMineMapBounds(p1, ref mapData.BoundsMin, ref mapData.BoundsMax, ref boundsInitialized);
                        UpdateMineMapBounds(p2, ref mapData.BoundsMin, ref mapData.BoundsMax, ref boundsInitialized);
                    }
                }
            }

            mapData.LineSpriteGenerator.BuildPaths();

            sw.Stop();
            Debug.Log($"VectorMineMap: Map Segments: {segCount} Grid sweep took {sw.ElapsedMilliseconds} ms");

            //_lineSprite.UpdateSprite(gameObject);

            //UpdateTransformMatrices();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"VectorMineMap: Error building mine map: {ex.Message} {ex.StackTrace}");
        }
        finally
        {
        }

        return mapData;
    }

    private async Task BuildBaseMine()
    {
        if (_mapBuildInProgress)
            return;

        try
        {
            _mapBuildInProgress = true;
            _lineThickness = LineThickness;

            if (ScenarioSaveLoad.Settings != null)
                _lineThickness *= ScenarioSaveLoad.Settings.MineMapLineWidthScale;

            string mapDataFilename = null;

            //if (ScenarioSaveLoad.Instance != null && ScenarioSaveLoad.Settings.MapDataFile != null)
            //{
            //    mapDataFilename = Path.Combine(ScenarioSaveLoad.Instance.GetScenarioFilePath(), ScenarioSaveLoad.Settings.MapDataFile);

            //    ////temp fix for scenario editor
            //    //if (mapDataFilename.Contains("PlayModeSave"))
            //    //    mapDataFilename = null;
            //}

            //if (mapDataFilename != null && File.Exists(mapDataFilename))
            //{
            //    Debug.Log($"Loading mine map from file {mapDataFilename}");

            //    LoadMapFromFile(mapDataFilename);

            //    return;
            //}

            if (ScenarioSaveLoad.ActiveScenario != null && LoadMapFromString(ScenarioSaveLoad.ActiveScenario.MapData))
            {
                Debug.Log("VectorMineMap: Loaded map data from scenario map data string");
                return;
            }

            var mapData = await BuildBaseMapData(MapWallColor, _lineThickness);
            _lineSprite = mapData.LineSpriteGenerator;
            MapGrid = mapData.MapGrid;
            _mineMin = mapData.BoundsMin;
            _mineMax = mapData.BoundsMax;
            _boundsInitialized = true;

            //if (mapDataFilename != null && !mapDataFilename.Contains("PlayModeSave") && _lineSprite.PathCount > 0)
            //    SaveMapToFile(mapData, mapDataFilename);

            InitializeMapFromLoadedData();
        }
        finally
        {
            _mapBuildInProgress = false;
        }
    }

    public static byte[] SaveMapToBytes(VectorMineMapData data)
    {
        byte[] bytes = null;

        using (var stream = new MemoryStream())
        {
            using (var compress = new GZipStream(stream, CompressionMode.Compress, true))
            using (var writer = new BinaryWriter(compress, System.Text.Encoding.UTF8, true))
            {
                SaveMapData(data, writer);

                writer.Flush();
                compress.Flush();
            }

            stream.Flush();
            bytes = stream.ToArray();
        }

        return bytes;
    }

    public static string SaveMapToString(VectorMineMapData data)
    {
        var mapBytes = SaveMapToBytes(data);
        return System.Convert.ToBase64String(mapBytes);
    }

    public static void SaveMapToFile(VectorMineMapData data, string filename)
    {
        if (filename == null || File.Exists(filename))
            return;

        Debug.Log($"Saving map to file {filename}");

        using (var file = new FileStream(filename, FileMode.Create))
        using (var compress = new GZipStream(file, CompressionMode.Compress))
        using (var writer = new BinaryWriter(compress, System.Text.Encoding.UTF8, true))
        {
            SaveMapData(data, writer);
        }
    }

    public static void SaveMapData(VectorMineMapData data, BinaryWriter writer)
    {
        writer.WriteVector(data.BoundsMin);
        writer.WriteVector(data.BoundsMax);

        data.LineSpriteGenerator.Write(writer);
        data.MapGrid.Write(writer);
    }

    private bool LoadMapFromString(string mapString)
    {
        if (mapString == null || mapString.Length <= 0)
            return false;

        try
        {
            var mapBytes = System.Convert.FromBase64String(mapString);

            using (MemoryStream ms = new MemoryStream(mapBytes))
            using (var decompress = new GZipStream(ms, CompressionMode.Decompress, true))
            using (BinaryReader reader = new BinaryReader(decompress, System.Text.Encoding.UTF8, true))
            {
                LoadMapData(reader);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error reading map data string: {ex.Message} {ex.StackTrace}");
            return false;
        }

        InitializeMapFromLoadedData();

        return true;
    }

    private void LoadMapData(BinaryReader reader)
    {
        _lineSprite = new LineSpriteGenerator();// gameObject.AddComponent<LineSpriteGenerator>();
        MapGrid = new MapGrid(false);

        _lineThickness = LineThickness;

        if (ScenarioSaveLoad.Settings != null)
            _lineThickness *= ScenarioSaveLoad.Settings.MineMapLineWidthScale;

        _mineMin = reader.ReadVector2();
        _mineMax = reader.ReadVector2();
        _boundsInitialized = true;

        _lineSprite.Read(reader, MapWallColor, _lineThickness);
        MapGrid.Read(reader);
    }

    private void InitializeMapFromLoadedData()
    {

        foreach (var sym in _symbolMap.Values)
        {
            UpdateMineMapBounds(new Vector2(sym.Symbol.WorldPosition.x, sym.Symbol.WorldPosition.z));
        }

        foreach (var sym in _localSymbols.Values)
        {
            UpdateMineMapBounds(new Vector2(sym.Symbol.WorldPosition.x, sym.Symbol.WorldPosition.z));
        }

        var activeScene = SceneManager.GetActiveScene();
        var rootObjects = activeScene.GetRootGameObjects();

        _mineSegmentQuery = new KDQuery();
        _mineSegmentQueryResults = new List<int>();
        _mineSegmentMap = new Dictionary<int, MineSegment>();
        var segmentPointList = new List<Vector3>();

        foreach (var obj in rootObjects)
        {
            if (obj == null)
                continue;

            var segments = obj.GetComponentsInChildren<MineSegment>();
            foreach (var seg in segments)
            {
                if (!seg.IncludeInMap)
                    continue;

                _mineSegmentMap.Add(segmentPointList.Count, seg);
                segmentPointList.Add(seg.transform.position);
            }
        }

        _mineSegmentTree = new KDTree(segmentPointList.ToArray(), 16);

        _lineSprite.UpdateSprite(gameObject);
        UpdateTransformMatrices();
    }

    private void LoadMapFromFile(string filename)
    {
        using (var file = new FileStream(filename, FileMode.Open))
        using (var decompress = new GZipStream(file, CompressionMode.Decompress))
        using (var reader = new BinaryReader(decompress))
        {
            LoadMapData(reader);
        }

        InitializeMapFromLoadedData();
    }

    private Bounds ComputeMineBounds(Object[] mineSegs)
    {
        Bounds b = new Bounds();

        bool bFirst = true;

        foreach (var obj in mineSegs)
        {
            var seg = (MineSegment)obj;

            var segb = seg.SegmentBounds;
            segb.center += seg.transform.position;

            if (bFirst)
            {
                bFirst = false;
                //b = segb;
                b = new Bounds(seg.transform.position, Vector3.zero);
            }
            else
            {
                //b.Encapsulate(segb);
                b.Encapsulate(seg.transform.position);
            }

        }

        return b;
    }

    private SymbolInfo FindSymbolInfo(MineMapSymbol symbol)
    {
        SymbolInfo info = null;

        if (_symbolMap.TryGetValue(symbol.SymbolID, out info))
            return info;

        if (_localSymbols.TryGetValue(symbol, out info))
            return info;

        return null;
    }

    void OnSymbolAdded(MineMapSymbol symbol)
    {
        if (_mapBuildInProgress)
            return;

        if (_lineSprite == null)
            return;

        if (_symbolMap.ContainsKey(symbol.SymbolID) && symbol.SymbolID != -1)
        {
            //Debug.Log($"MineSymbol: Duplicate symbol {symbol.SymbolPrefab} id: {symbol.SymbolID} not added");
            return;
        }

        SymbolInfo info = InstantiateSymbol(symbol);
        if (info == null)
            return;

        if (symbol.SymbolID != -1)
            _symbolMap.Add(symbol.SymbolID, info);
        else
            _localSymbols[symbol] = info;


        PositionSymbol(info, symbol.WorldPosition, symbol.WorldRotation);

        //UpdateMineMapBounds(new Vector2(symbol.WorldPosition.x, symbol.WorldPosition.z));
        //UpdateTransformMatrices();
        

    }

    private SymbolInfo InstantiateSymbol(MineMapSymbol symbol)
    {
        //Debug.Log($"MineSymbol: Added {symbol.SymbolPrefab} Count: {_symbolMap.Count}");
        SymbolInfo info = new SymbolInfo();
        info.Symbol = symbol;
        info.Object = symbol.Instantiate();
        if (info.Object == null)
            return null;

        info.RT = info.Object.GetComponent<RectTransform>();

        if (info.Object == null || info.RT == null)
        {
            Debug.LogError($"VectorMineMap: Instantiation failed for symbol {symbol.SymbolPrefab}");
            return null;
        }

        info.RT.SetParent(transform, false);
        info.RT.anchorMin = Vector3.zero;
        info.RT.anchorMax = Vector3.zero;

        ResetSymbolColor(info);

        return info;
    }

    public void ResetSymbolColor(MineMapSymbol symbol)
    {
        var info = FindSymbolInfo(symbol);
        if (info == null)
            return;

        ResetSymbolColor(info);
    }

    private void ResetSymbolColor(SymbolInfo info)
    {
        if (!OverrideMapSymbolColors)
            return;

        var symbol = info.Symbol;

        if (info.Symbol.SymbolText.Contains("Battery"))
        {
            symbol.SetColor(info.Object, Color.red);
        }
        //else if (info.Symbol.AddressableKey == "MineMapSymbols/AirDirection")
        //{
        //    Color col = Color.blue;
        //    ColorUtility.TryParseHtmlString("#005eaa", out col);
        //    symbol.SetColor(info.Object, col);
        //}
        //else if (info.Symbol.AddressableKey == "MineMapSymbols/Marker")
        //{
        //    Color col = new Color(1, 0.57f, 0);            
        //    symbol.SetColor(info.Object, info.Symbol.Color);
        //}
        else if (info.Symbol.AllowColorChange)
        {
            symbol.SetColor(info.Object, info.Symbol.Color);
        }
        else if (OverrideMapSymbolColors)
        {
            symbol.SetColor(info.Object, MapSymbolColor);
        }
        else
        {
            symbol.SetColor(info.Object, info.Symbol.Color);
        }
    }

    public void SetSymbolColor(MineMapSymbol symbol, Color color)
    {
        var info = FindSymbolInfo(symbol);
        if (info == null)
            return;

        SetSymbolColor(info, color);
    }

    private void SetSymbolColor(SymbolInfo info, Color color)
    {
        info.Symbol.SetColor(info.Object, color);
        MineMapSymbolManager.UpdateSymbolColor(info.Symbol);
        if (MineMapSymbolManager != null && MineMapSymbolManager.TryGetComponent<NetSyncSymbolManager>(out var netSync))
        {
            netSync.SetMapChanged();
        }
    }

    public void RotateSymbol(MineMapSymbol symbol, float rotation)
    {
        var info = FindSymbolInfo(symbol);
        if (info == null) return;
        Quaternion worldRotation = symbol.WorldRotation;
        worldRotation *= Quaternion.Euler(0, rotation, 0);
        symbol.WorldRotation = worldRotation;
        //RotateSymbol(info, rotation);        
        if (MineMapSymbolManager != null && MineMapSymbolManager.TryGetComponent<NetSyncSymbolManager>(out var netSync))
        {
            netSync.SetMapChanged();
        }
    }

    private void RotateSymbol(SymbolInfo info, float rotation)
    {
        Quaternion rot = info.RT.rotation;        
        rot *= Quaternion.Euler(0, rotation, 0);
        info.RT.rotation = rot;
    }

    public void FlipSymbolXScale(MineMapSymbol symbol)
    {
        var info = FindSymbolInfo(symbol);
        if(info == null) return;

        FlipSymbolXScale(info);
        if (MineMapSymbolManager != null && MineMapSymbolManager.TryGetComponent<NetSyncSymbolManager>(out var netSync))
        {
            netSync.SetMapChanged();
        }
    }

    public void PositionSymbol(MineMapSymbol symbol, Vector3 worldPosition, Quaternion symbolRotation, bool updateSymbolPos = false)
    {
        var info = FindSymbolInfo(symbol);
        if (info == null)
            return;

        //symbol.WorldRotation = worldRotation;
        PositionSymbol(info, worldPosition, symbolRotation, updateSymbolPos);

        if (MineMapSymbolManager != null && MineMapSymbolManager.TryGetComponent<NetSyncSymbolManager>(out var netSync))
        {
            netSync.SetMapChanged();
        }
    }

    public bool TryFindSymbolObjectWorldPosition(MineMapSymbol symbol, out Vector3 worldPos)
    {
        worldPos = Vector3.zero;

        var info = FindSymbolInfo(symbol);
        if (info == null)
            return false;

        if (info.Object != null)
        {
            worldPos = info.Object.transform.position;
            return true;
        }

        return false;

    }

    private bool SpanEntry(SymbolInfo info, Vector3 worldPosition)
    {
        var symbol = info.Symbol;
        if (symbol == null)
            return false;

        Vector2 start, end;
        Vector3 v3start, v3end;
        Vector2 canvStart, canvEnd;

        if (MapGrid.RaycastVertical(worldPosition.XZProjection(), 8, out start, out end))
        {
            v3start = new Vector3(start.x, 1.0f, start.y);
            v3end = new Vector3(end.x, 1.0f, end.y);

            var center = (v3start + v3end) * 0.5f;
            info.RT.anchoredPosition = WorldSpaceToCanvas(center);
            canvStart = WorldSpaceToCanvas(v3start);
            canvEnd = WorldSpaceToCanvas(v3end);

            var size = symbol.Size;
            size.y = (canvEnd - canvStart).magnitude;
            info.RT.sizeDelta = size;
            info.RT.localRotation = Quaternion.identity;

            //Debug.Log($"Placed symbol {symbol.name} spanning vertically height {size.y}");

            info.SymbolPosition = center;
            info.SymbolRotation = Quaternion.identity;

            //Debug.DrawLine(v3start, v3end, Color.magenta, 15);
        }
        else if (MapGrid.RaycastHorizontal(worldPosition.XZProjection(), 8, out start, out end))
        {
            v3start = new Vector3(start.x, 1.0f, start.y);
            v3end = new Vector3(end.x, 1.0f, end.y);

            var center = (v3start + v3end) * 0.5f;
            info.RT.anchoredPosition = WorldSpaceToCanvas(center);
            ////To allow for symbols we can manuual rotate when spanning across the intersection, sets rotation to expected value based on tests
            //if (!info.Symbol.AllowManualRotations)
            //{
            //    info.RT.localRotation = Quaternion.Euler(0, 0, -90);
            //}
            //else
            //{
            //    info.RT.localRotation = Quaternion.Euler(0, 0, 90);
            //}

            info.RT.localRotation = Quaternion.Euler(0, 0, -90);
            canvStart = WorldSpaceToCanvas(v3start);
            canvEnd = WorldSpaceToCanvas(v3end);

            var size = symbol.Size;
            size.y = (canvEnd - canvStart).magnitude;
            info.RT.sizeDelta = size;

            //Debug.Log($"Placed symbol {symbol.name} spanning horizontally width {size.y}");

            info.SymbolPosition = center;
            info.SymbolRotation = CanvasSpaceToWorld(info.RT.localRotation);

            //Debug.DrawLine(v3start, v3end, Color.magenta, 15);
        }
        else
            return false;

        return true;
    }

    private bool SpanDiagonal(SymbolInfo info, Vector3 worldPosition, Quaternion symbolRotation)
    {
        var symbol = info.Symbol;
        if (symbol == null)
            return false;

        var seg = FindClosestSegment(worldPosition);
        if (seg == null || !seg.IsIntersection)
            return false;

        
        worldPosition = seg.transform.position;

        Vector2 start, end;
        Vector3 v3start, v3end;
        Vector2 canvStart, canvEnd;

        if (!MapGrid.RaycastDiagonal(worldPosition.XZProjection(), 8, out start, out end))
            return false;
        
        v3start = new Vector3(start.x, 1.0f, start.y);
        v3end = new Vector3(end.x, 1.0f, end.y);

        var center = (v3start + v3end) * 0.5f;
        info.RT.anchoredPosition = WorldSpaceToCanvas(center);
        //info.RT.anchoredPosition = WorldSpaceToCanvas(worldPosition);
        canvStart = WorldSpaceToCanvas(v3start);
        canvEnd = WorldSpaceToCanvas(v3end);

        var size = symbol.Size;
        //size.y = ((canvEnd - canvStart).magnitude) / 2;
        //size.x = ((canvEnd - canvStart).magnitude) / 2;
        //size.y *= 1.5f;
        size.y = (canvEnd - canvStart).magnitude - MapGrid.GridSize;
        info.RT.sizeDelta = size;

        //Debug.Log($"Placed symbol {symbol.name} spanning vertically height {size.y}");

        //info.Symbol.WorldPosition = center;
        //info.Symbol.WorldRotation = Quaternion.identity;
        //Quaternion rot = symbolRotation;
        //if (symbolRotation == Quaternion.identity)
        //{
        //    rot *= Quaternion.Euler(0, 45, 0);
        //    //info.RT.localRotation *= Quaternion.Euler(0, 0, 45);
        //}

        //The symbol must be rotated to 45 degrees +/- 90 degrees
        //any other angle reset to 45

        Quaternion rot = WorldSpaceToCanvas(symbolRotation);
        Quaternion rotA = Quaternion.Euler(0, 0, 45);
        Quaternion rotB = Quaternion.Euler(0, 0, 45+90);
        Quaternion rotC = Quaternion.Euler(0, 0, 45-90);

        if (Quaternion.Angle(rot, rotA) > 0.5f &&
            Quaternion.Angle(rot, rotB) > 0.5f &&
            Quaternion.Angle(rot, rotC) > 0.5f)
        {
            rot = rotA;
        }

        info.RT.localRotation = rot;

        //Quaternion rot = Quaternion.Euler(0, 45, 0);
        //info.RT.localRotation = Quaternion.Euler(0, 0, rot.eulerAngles.y);
        //Debug.Log($"Placing spanning and scale across symbol: {info.Symbol.AddressableKey} with rotation {rot.eulerAngles}");

        //Debug.DrawLine(v3start, v3end, Color.magenta, 15);
        //Overriding the rotation symbol keys to null if the map board is doing the diagonal spanning for you. Otherwise you might end up with a mirror instead an cannot rotate the symbol as expected
        info.Symbol.ClockWiseSymbolKey = null;
        info.Symbol.CounterClockwiseSymbolKey = null;

        info.SymbolPosition = center;
        info.SymbolRotation = CanvasSpaceToWorld(info.RT.localRotation);

        //}
        //else
        //{
        //    info.RT.anchoredPosition = WorldSpaceToCanvas(worldPosition);
        //    info.RT.sizeDelta = symbol.Size;
        //    Debug.Log($"Placed symbol {symbol.name}, span failed");

        //    //info.Symbol.WorldRotation = Quaternion.identity;
        //    Quaternion rot = symbol.WorldRotation;
        //    if (symbol.WorldRotation == Quaternion.identity)
        //    {
        //        rot *= Quaternion.Euler(0, 90, 0);
        //        //info.RT.localRotation *= Quaternion.Euler(0, 0, 45);
        //    }
        //    info.SymbolRotation = rot;
        //    info.RT.localRotation = Quaternion.Euler(0, 0, rot.eulerAngles.y);

        return true;
    }

    private bool SnapToIntersection(SymbolInfo info, Vector3 worldPosition, Quaternion symbolRotation)
    {

        var seg = FindClosestSegment(worldPosition);
        if (seg == null || !seg.IsIntersection)
            return false;

        
        //worldPosition = seg.transform.position;
        var bounds = seg.IntersectionBounds;
        //bounds.Expand(MapGrid.GridSize / 2.0f);

        var size = WorldBoundsToCanvasSize(bounds, seg.transform.localToWorldMatrix, out var centerPos);
        //worldPosition = MapGrid.GetGridPosition(worldPosition);
        worldPosition = centerPos;
        
        //if(Vector3.Distance(info.RT.localPosition, worldPosition) > 6)
        //{
        //    return false;
        //}

        info.RT.anchoredPosition = worldPosition;//WorldSpaceToCanvas(seg.transform.position);
        info.RT.sizeDelta = size;

        info.SymbolPosition = worldPosition;
        info.SymbolRotation = CanvasSpaceToWorld(info.RT.localRotation);

        //Vector2 start, end;
        //Vector3 v3start, v3end;
        //Vector2 canvStart, canvEnd;

        //if (MapGrid.RaycastDiagonal(worldPosition.XZProjection(), 8, out start, out end))
        //{
        //    v3start = new Vector3(start.x, 1.0f, start.y);
        //    v3end = new Vector3(end.x, 1.0f, end.y);

        //    var center = (v3start + v3end) * 0.5f;
        //    info.RT.anchoredPosition = WorldSpaceToCanvas(center);
        //    canvStart = WorldSpaceToCanvas(v3start);
        //    canvEnd = WorldSpaceToCanvas(v3end);

        //    var size = symbol.Size;
        //    size.y = ((canvEnd - canvStart).magnitude) / 2;
        //    size.x = ((canvEnd - canvStart).magnitude) / 2;
        //    size *= 1.1f;
        //    info.RT.sizeDelta = size;

        //    //Debug.Log($"Placed symbol {symbol.name} spanning vertically height {size.y}");

        //    info.SymbolPosition = center;
        //    info.SymbolRotation = Quaternion.identity;

        //    //Debug.DrawLine(v3start, v3end, Color.magenta, 15);
        //}
        return true;
    }

    private void FlipSymbolXScale(SymbolInfo info)
    {
        var rect = info.RT;
        float x = rect.localScale.x;
        x *= -1;
        info.RT.localScale = new Vector3(x, rect.localScale.y, rect.localScale.z);
        info.Symbol.Size = new Vector2(x, rect.localScale.y);
    }

    private void PositionSymbol(SymbolInfo info, Vector3 worldPosition, Quaternion symbolRotation, bool updateSymbolPos = false)
    {
        var symbol = info.Symbol;
        if (symbol == null) 
            return;

        //symbol.WorldPosition = worldPosition;
        info.SymbolPosition = worldPosition;
        info.SymbolRotation = symbolRotation;

        if (symbol.SpanEntry && SpanEntry(info, worldPosition))
        {
            //spanned entry
        }
        else if (symbol.SpanDiagonal && SpanDiagonal(info, worldPosition, symbolRotation))
        {
            //spanned diagonally
        }
        else if (symbol.SnapToIntersection && SnapToIntersection(info, worldPosition, symbolRotation))
        {
            //snapped to intersection
        }
        else
        {
            //default placement
            info.RT.anchoredPosition = WorldSpaceToCanvas(worldPosition);
            info.RT.sizeDelta = symbol.Size;
            //info.RT.localRotation = Quaternion.identity;
            //info.SymbolRotation = Quaternion.identity;

            info.RT.localRotation = WorldSpaceToCanvas(symbolRotation);
            info.SymbolRotation = symbolRotation;

        }

        if (updateSymbolPos)
        {
            //update symbol's base position
            info.Symbol.WorldPosition = info.SymbolPosition;
            info.Symbol.WorldRotation = info.SymbolRotation;
        }

        info.LastPosition = info.Symbol.WorldPosition;
        info.LastRotation = info.Symbol.WorldRotation;

        //info.LastPosition = info.SymbolPosition;
        UpdateSymbolPosition(info);
    }

    void OnSymbolRemoved(MineMapSymbol symbol)
    {
        if (_mapBuildInProgress)
            return;

        //Debug.Log($"MineSymbol: Removed {symbol.SymbolPrefab} Count: {_symbolMap.Count}");

        SymbolInfo info;
        if (_symbolMap.TryGetValue(symbol.SymbolID, out info))
        {
            _symbolMap.Remove(symbol.SymbolID);
            Destroy(info.Object);
        }
        else if (_localSymbols.TryGetValue(symbol, out info))
        {
            _localSymbols.Remove(symbol);
            Destroy(info.Object);
        }
    }

    private void DestroySymbol(SymbolInfo info)
    {
        if (info == null)
            return;

        if (info.Object != null)
            Destroy(info.Object);

        //if (info.Symbol != null)
        //    Destroy(info.Symbol);
    }

    void DestroySymbols(IEnumerable<SymbolInfo> symbolList)
    {
        foreach (var info in symbolList)
        {
            //if (info.Object != null)
            //    Destroy(info.Object);
            DestroySymbol(info);
        }
    }

    void ClearSymbols()
    {
        if (_symbolMap == null)
            return;

        DestroySymbols(_symbolMap.Values);
        DestroySymbols(_localSymbols.Values);

        _symbolMap.Clear();
        _localSymbols.Clear();
    }

    void Update()
    {
        //if (_symbolInfoList.Count < ActiveSymbols.Count)
        //{
        //    for (int i = _symbolInfoList.Count; i < ActiveSymbols.Count; i++)
        //    {
        //        //var obj = new GameObject("MineSymbol");
        //        //obj.AddComponent<RectTransform>();
        //        //obj.transform.SetParent(transform);

        //        var symbol = ActiveSymbols[i];

        //        if (symbol.MineSymbolType == MineMapSymbol.SymbolType.Prefab && symbol.SymbolPrefab != null)
        //        {
        //            SymbolInfo info = new SymbolInfo();
        //            info.Symbol = symbol;
        //            info.Object = GameObject.Instantiate<GameObject>(symbol.SymbolPrefab);
        //            info.RT = info.Object.GetComponent<RectTransform>();

        //            info.RT.SetParent(transform);
        //            info.RT.anchorMin = Vector3.zero;
        //            info.RT.anchorMax = Vector3.zero;

        //            info.RT.sizeDelta = symbol.Size;

        //            var svg = info.Object.GetComponent<SVGImage>();
        //            if (svg != null)
        //            {
        //                svg.color = symbol.Color;
        //            }

        //            _symbolInfoList.Add(info);
        //        }
        //        else
        //        {
        //            _symbolInfoList.Add(null);
        //        }

        //    }
        //}

        //_lineSprite.UpdateSprite(Random.value * 0.25f + 0.01f);


        UpdateTransformMatrices();

        UpdateSymbolPositions(_symbolMap.Values);
        UpdateSymbolPositions(_localSymbols.Values);
    }

    void UpdateSymbolPositions(IEnumerable<SymbolInfo> symbolList)
    {
        foreach (var info in symbolList)
        {
            //if (info.Symbol.SpanEntry)
            //    continue;

            //UpdateMineMapBounds(info.Symbol.WorldPosition.XZProjection());

            UpdateSymbolPosition(info);
        }
    }

    void UpdateSymbolPosition(SymbolInfo info)
    {
        if (info.Symbol.WorldPosition.IsNaN())
            return;

        //detect if the symbol has moved & potentially needs to be re-scaled
        if (info.Symbol.WorldPosition != info.LastPosition ||
            Quaternion.Angle(info.Symbol.WorldRotation.normalized, info.LastRotation.normalized) > 1.0f)
        {
            info.LastPosition = info.Symbol.WorldPosition;
            info.LastRotation = info.Symbol.WorldRotation;

            PositionSymbol(info, info.Symbol.WorldPosition, info.Symbol.WorldRotation);

            info.LastPosition = info.Symbol.WorldPosition;
            info.LastRotation = info.Symbol.WorldRotation;
            return;
        }

        info.RT.anchoredPosition = WorldSpaceToCanvas(info.SymbolPosition);

        if (info.Symbol.SpanEntry)
            return;

        var localScale = info.RT.localScale;
        if (info.Symbol.Size.x >= 0)
        {
            localScale.x = Mathf.Abs(localScale.x);
        }
        else
        {
            //sybol flipped on Y axis
            localScale.x = Mathf.Abs(localScale.x) * -1.0f;
        }
        info.RT.localScale = localScale;

        if (!info.Symbol.IgnoreRotation)
            info.RT.localRotation = WorldSpaceToCanvas(info.SymbolRotation);
        else
            info.RT.localRotation = Quaternion.identity;
    }

    private Vector2 WorldBoundsToCanvasSize(Bounds bounds, Matrix4x4 transformMatrix, out Vector3 centerPos)
    {
        var min = bounds.min;
        var max = bounds.max;

        min = transformMatrix.MultiplyPoint(min);
        max = transformMatrix.MultiplyPoint(max);

        //align with grid wall position
        //min = MapGrid.GetGridPosition(min);
        //max = MapGrid.GetGridPosition(max);
        //max.x += MapGrid.GridSize;
        //max.z += MapGrid.GridSize;

        var offset = new Vector3(MapGrid.GridSize / 2.0f, 0, MapGrid.GridSize / 2.0f);
        min -= offset;
        max += offset;

        centerPos = (min + max) * 0.5f;

        min = WorldSpaceToCanvas(min);
        max = WorldSpaceToCanvas(max);

        Debug.DrawLine(CanvasSpaceToWorldCanvasSpace(min), CanvasSpaceToWorldCanvasSpace(max), Color.magenta);

        var size = new Vector2(max.x - min.x, max.y - min.y);

        return size;
    }

    public Quaternion WorldSpaceToCanvas(Quaternion rot)
    {
        //extract the Y axis rotation as a Z axis rotation
        var euler = rot.eulerAngles;

        return Quaternion.AngleAxis(-euler.y, new Vector3(0, 0, 1));
    }

    public Quaternion CanvasSpaceToWorld(Quaternion rot)
    {
        var euler = rot.eulerAngles;

        return Quaternion.AngleAxis(-euler.z, new Vector3(0, 1, 0));
    }

    private Matrix4x4 BuildWorldToCanvasMat()
    {
        Matrix4x4 mat = Matrix4x4.identity;

        if (BorderSize <= 0)
            BorderSize = 0.1f;

        var border = new Vector2(BorderSize, BorderSize);


        //var mineMax = _mineMax + border;
        //var mineMin = _mineMin - border;
        var mineMax = _mineMax;
        var mineMin = _mineMin;

        //_mineMax += new Vector2(0.1f, 0.1f);
        //_mineMin -= new Vector2(0.1f, 0.1f);

        var mineDim = mineMax - mineMin;
        if (mineDim.magnitude < 0.1f)
        {
            mineMax = _mineMax + border;
            mineMin = _mineMin - border;
            mineDim = mineMax - mineMin;
        }

        var canvSize = _rect.rect.size;
        if (canvSize.x <= 0 || canvSize.y <= 0)
            return Matrix4x4.identity;

        var mineAspect = mineDim.x / mineDim.y;
        var canvAspect = canvSize.x / canvSize.y;

        if (float.IsNaN(canvAspect) || float.IsNaN(mineAspect))
            return Matrix4x4.identity;

        //translate the origin to the corner of the mine
        mat = Matrix4x4.Translate(new Vector3(mineMin.x * -1.0f, 0, mineMin.y * -1.0f)) * mat;
        //scale to be the percent-distance into the mine from the min corner
        mat = Matrix4x4.Scale(new Vector3(1.0f / mineDim.x, 1, 1.0f / mineDim.y)) * mat;

        if (mineAspect == canvAspect || !_svgImage.preserveAspect)
        {
            //scale by full size canvas
            mat = Matrix4x4.Scale(new Vector3(canvSize.x, 1, canvSize.y)) * mat;
        }
        else if (mineAspect > canvAspect)
        {
            //mine is wider than canvas, effective canvas height is smaller
            //(bars on top/bottom)
            var effectiveHeight = canvSize.x * (1.0f / mineAspect);

            //scale by effective height
            mat = Matrix4x4.Scale(new Vector3(canvSize.x, 1, effectiveHeight)) * mat;

            //apply offset since map is centered
            mat = Matrix4x4.Translate(new Vector3(0, 0, (canvSize.y - effectiveHeight) / 2.0f)) * mat;

        }
        else
        {
            //mine is taller than canvas, effective canvas width is smaller
            //(bars on left/right)
            var effectiveWidth = canvSize.y * mineAspect;

            //scale by effective width
            mat = Matrix4x4.Scale(new Vector3(effectiveWidth, 1, canvSize.y)) * mat;

            //apply offset since map is centered
            mat = Matrix4x4.Translate(new Vector3((canvSize.x - effectiveWidth) / 2.0f, 0, 0)) * mat;
        }

        return mat;
    }

    private void UpdateTransformMatrices()
    {
        var mineMapSymbolScale = 1.0f;

        if (ScenarioSaveLoad.Settings != null)
            mineMapSymbolScale = ScenarioSaveLoad.Settings.MineMapSymbolScale;

        if (mineMapSymbolScale <= 0.0f)
            mineMapSymbolScale = 1.0f;

        //rescale so that larger numbers make larger mine map symbols,
        //and a value of 1 results in the default multiplier of 10
        mineMapSymbolScale = 6.6f / mineMapSymbolScale;

        _rect.sizeDelta = (_mineMax - _mineMin) * mineMapSymbolScale;
        _worldToCanvasMat = BuildWorldToCanvasMat();
        _canvasToWorldMat = _worldToCanvasMat.inverse;
    }

    public Vector2 WorldSpaceToCanvas(Vector3 worldSpace)
    {
        Matrix4x4 mat = Matrix4x4.identity;

        //mat = BuildWorldToCanvasMat();
        var canvSpace = _worldToCanvasMat.MultiplyPoint(worldSpace);
        return canvSpace.XZProjection();
    }

    public Vector3 CanvasSpaceToWorld(Vector2 canv)
    {
        Matrix4x4 mat = Matrix4x4.identity;

        Vector3 canvSpace = new Vector3(canv.x, 0, canv.y);
        //mat = BuildWorldToCanvasMat();
        //mat = mat.inverse;
        var worldSpace = _canvasToWorldMat.MultiplyPoint(canvSpace);
        return worldSpace;
    }

    /// <summary>
    /// transform a world position on the mine map canvas to a world position in the mine
    /// </summary>
    /// <param name="worldCanvas"></param>
    /// <returns></returns>
    public Vector3 WorldCanvasSpaceToWorld(Vector3 worldCanvas)
    {
        Vector2 canvPos = _rect.InverseTransformPoint(worldCanvas);
        canvPos.x -= _rect.rect.x;
        canvPos.y -= _rect.rect.y;

        var worldPos = CanvasSpaceToWorld(canvPos);

        return worldPos;
        
    }

    public Vector3 CanvasSpaceToWorldCanvasSpace(Vector3 worldCanvas)
    {
        worldCanvas.x += _rect.rect.x;
        worldCanvas.y += _rect.rect.y;
        Vector3 canvSpace = _rect.TransformPoint(worldCanvas);

        return canvSpace;
    }

    private int _testSymbolIndex = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        MineMapClickedEventData clickEvent;




        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, null, out pos);
        pos += (_rect.rect.size * 0.5f);

        //Debug.Log(pos + ", " + eventData.button.ToString());
        //Debug.Log($"MapClick: {pos}, rect: {_rect.rect}, scale: {transform.lossyScale}");

        Vector3 world = CanvasSpaceToWorld(pos);

        clickEvent = new MineMapClickedEventData
        {
            PointerEvent = eventData,
            WorldSpacePosition = world,

        };

        MapClicked?.Invoke(clickEvent);


        if (eventData.button == PointerEventData.InputButton.Left)
        {

            //if (ResearcherCam != null)
            //{
            //    //var dmCam = ResearcherCam.GetComponent<DMCameraController>();
            //    //ResearcherCam.transform.position = new Vector3(worldPos.x, ResearcherCam.transform.position.y, worldPos.z);
            //    ResearcherCam.transform.position = new Vector3(world.x, ResearcherCam.transform.position.y, world.z);

            //    var rcam = ResearcherCam.GetComponent<ResearcherCamController>();
            //    if (rcam != null)
            //        rcam.FollowTransform(null);
            //}
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            //var symbolManager = MineMapSymbolManager;
            //if (symbolManager == null)
            //    symbolManager = MineMapSymbolManager.GetDefault();

            //MineMapSymbol nearestSymbol = null;
            //float distToSymbol = float.MaxValue;

            //if (symbolManager.GetNearestSymbol(world, out nearestSymbol, out distToSymbol) && distToSymbol < 3)
            //{
            //    symbolManager.RemoveSymbol(nearestSymbol);
            //}
            //else
            //{
            //    var randSymbols = new string[] {
            //    "MineMapSymbols/GasReadingText",
            //    "MineMapSymbols/Seal",
            //    "MineMapSymbols/Barricade",
            //    "MineMapSymbols/PermStop",
            //    "MineMapSymbols/Caved",
            //    "MineMapSymbols/CheckCurtain",                
            //    "MineMapSymbols/GasTest",
            //    "MineMapSymbols/MobileEquipment",
            //    "MineMapSymbols/Object",
            //    "MineMapSymbols/TestTextSymbol"
            //    };

            //    //int index = Random.Range(0, randSymbols.Length);

            //    symbolManager.InstantiateSymbol(randSymbols[_testSymbolIndex], world, Quaternion.identity);

            //    _testSymbolIndex++;
            //    if (_testSymbolIndex >= randSymbols.Length)
            //        _testSymbolIndex = 0;
            //}
        }
    }
}
