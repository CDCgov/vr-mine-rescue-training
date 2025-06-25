using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingGeometry : MonoBehaviour
{
    [System.Serializable]
    public class CableNode
    {
        public Vector3 Position;
        public float Slope = -1f;
        public bool ShowCableHanger = true;
        public bool FixedPosition = true;
        public int NodeIndex;
    }

    public event Action MeshGenerated;

    public bool CheckPointInterference = false;
    public float InterferenceDistance = 0.03f;

    public float CableDiameter = 0.05f;
    public float CableHangerWidth = 0.08f;
    public float DefaultCableSlope = 0.2f;
    public int SegmentsPerMeter = 8;
    public int CableHangerPosition = 0;

    public GameObject CableHangerPrefab;
    public Material CableMaterial;

    public List<CableNode> CableNodes = new List<CableNode>();

    public Mesh GeneratedMesh;

    protected List<Vector3> _points;
    protected Vector3[] _pointsArray;
    protected List<Vector3> _smoothedPoints;
    protected int _floorLayerMask;


    public void Clear()
    {
        CableNodes.Clear();
        RegenerateMesh();
    }

    public void ReindexNodes()
    {
        if (CableNodes == null)
            return;

        for (int i = 0; i < CableNodes.Count; i++)
        {
            CableNodes[i].NodeIndex = i;
        }
    }

    public void AppendNode(Vector3 posWorldSpace, bool hanging)
    {
        if (CableNodes == null)
            CableNodes = new List<CableNode>();

        CableNode newNode = new CableNode();
        newNode.Position = posWorldSpace;
        newNode.ShowCableHanger = hanging;

        CableNodes.Add(newNode);
        ReindexNodes();
    }


    /// <summary>
    /// Insert a node at a given index
    /// </summary>
    /// <param name="posWorldSpace"></param>
    /// <param name="hanging"></param>
    /// <param name="index"></param>
    public CableNode InsertNode(Vector3 posWorldSpace, bool hanging, int index)
    {
        if (CableNodes == null)
            CableNodes = new List<CableNode>();

        CableNode newNode = new CableNode();
        newNode.Position = posWorldSpace;
        newNode.ShowCableHanger = hanging;
        newNode.Slope = DefaultCableSlope;

        CableNodes.Insert(index, newNode);
        ReindexNodes();

        return newNode;
    }

    public void RemoveNode(int index)
    {
        if (CableNodes == null || index >= CableNodes.Count)
            return;

        CableNodes.RemoveAt(index);
        ReindexNodes();
    }

    public void UpdateMaterial()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null)
            mr.material = CableMaterial;
    }

    protected float DistToGround(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos, Vector3.down, out hit,
            20.0f, _floorLayerMask, QueryTriggerInteraction.Ignore))
        {
            return (hit.point - pos).magnitude;
        }

        return 5.0f;
    }

    private void BuildCableSegment(CableNode node, Vector3 pos, Vector3 nextPos)
    {
        //transform to world space
        pos = transform.TransformPoint(pos);
        nextPos = transform.TransformPoint(nextPos);

        //compute segment length
        float segLength = (nextPos - pos).magnitude;

        //compute gravity drop point
        //float avgY = (pos.y + next.Position.y) * 0.5f;
        Vector3 midPoint = (pos + nextPos) * 0.5f;

        float slope = DefaultCableSlope;
        if (node.Slope >= 0)
            slope = node.Slope;

        float dropAmount = segLength * slope;

        Vector3 dropMidPoint = midPoint;
        dropMidPoint.y -= dropAmount;

        //check that the drop doesn't go through a collider

        /*
        Vector3 collisionCheckStart = midPoint;
        collisionCheckStart.y -= CableDiameter * 0.3f;		
        Vector3 collisionCheckDir = dropMidPoint - collisionCheckStart;
        
        //Debug.DrawLine(collisionCheckStart, collisionCheckStart + collisionCheckDir, Color.green, 1.0f);
        RaycastHit hit;
        if (Physics.Raycast(collisionCheckStart, collisionCheckDir.normalized, out hit,
            collisionCheckDir.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            //update point to hitpoint
            dropMidPoint = hit.point + hit.normal * (CableDiameter * 0.7f);
            dropMidPoint = transform.InverseTransformPoint(dropMidPoint);
        } */


        //check if either the start or end of this segment is grounded
        float startDistToGround = DistToGround(pos);
        float endDistToGround = DistToGround(nextPos);

        bool startGrounded = startDistToGround < (CableDiameter * 2);
        bool endGrounded = endDistToGround < (CableDiameter * 2);

        if (startGrounded && endGrounded)
        {
            //following the ground - don't need to continue
            return;

        }

        Vector3 dir = dropMidPoint - pos;
        //Debug.DrawLine(pos, dropMidPoint, Color.green, 0.1f);
        RaycastHit hit;
        if (Physics.Raycast(pos, dir.normalized, out hit,
            dir.magnitude, _floorLayerMask, QueryTriggerInteraction.Ignore))
        {
            //add the first intersection point
            Vector3 hitPointEntry = hit.point + hit.normal * (CableDiameter * 0.6f);

            //find the "exit" point from where it would've come up from the floor
            dir = dropMidPoint - nextPos;
            //Debug.DrawLine(nextPos, dropMidPoint, Color.red, 0.1f);
            if (Physics.Raycast(nextPos, dir.normalized, out hit,
                dir.magnitude, _floorLayerMask, QueryTriggerInteraction.Ignore))
            {

                Vector3 hitPointExit = hit.point + hit.normal * (CableDiameter * 0.6f);
                Vector3 hitMid = (hitPointEntry + hitPointExit) * 0.5f;

                if ((hitPointEntry - hitPointExit).magnitude > 0.08f)
                {

                    //add an additional points to straighten the cable

                    //_points.Add(transform.InverseTransformPoint(hitPointEntry));
                    Vector3 entryMid = (pos + hitPointEntry) * 0.5f;
                    Vector3 exitMid = (hitPointExit + nextPos) * 0.5f;


                    Vector3 entryMid2 = (entryMid + hitMid) * 0.5f;
                    Vector3 exitMid2 = (exitMid + hitMid) * 0.5f;

                    entryMid2 = (entryMid2 + hitPointEntry) * 0.5f;
                    exitMid2 = (exitMid2 + hitPointExit) * 0.5f;

                    //Debug.DrawLine(entryMid, hitMid, Color.blue, 0.2f);

                    if (!startGrounded)
                    {
                        _points.Add(transform.InverseTransformPoint(entryMid));
                        _points.Add(transform.InverseTransformPoint(entryMid2));
                    }
                    _points.Add(transform.InverseTransformPoint(hitMid));

                    if (!endGrounded)
                    {
                        _points.Add(transform.InverseTransformPoint(exitMid2));
                        _points.Add(transform.InverseTransformPoint(exitMid));
                    }

                    //dir = hitPointExit - hitPointEntry;
                    //float straightenDist = Mathf.Min(0.05f, dir.magnitude / 2.0f);
                    //dir = dir.normalized;

                    //_points.Add(transform.InverseTransformPoint(hitPointEntry + dir * straightenDist));
                    //_points.Add(transform.InverseTransformPoint(hitPointExit + dir * -1 * straightenDist));


                    //_points.Add(transform.InverseTransformPoint(hitPointExit));
                }
                else
                {
                    _points.Add(transform.InverseTransformPoint(hitMid));
                }
            }


            //BuildCableSegment(dropMidPoint, transform.InverseTransformPoint(nextPos));
        }
        else
        {
            _points.Add(transform.InverseTransformPoint(dropMidPoint));
        }
    }


    public virtual void RegenerateMesh(bool generateHangers = true)
    {

        if (CableNodes == null || CableNodes.Count < 2)
        {
            //if (GeneratedMesh != null)
            //	GeneratedMesh.Clear();			
            GeneratedMesh = new Mesh();

            return;
        }

        //find or create the cable hanger parent object
        Transform hangerParent = transform.Find("CableHangers");
        if (hangerParent == null)
        {
            GameObject parentObj = new GameObject();
            parentObj.name = "CableHangers";

            hangerParent = parentObj.transform;
            hangerParent.SetParent(transform, false);
            hangerParent.localPosition = Vector3.zero;
            hangerParent.localRotation = Quaternion.identity;
        }

        //remove existing hangers
        Util.DestoryAllChildren(hangerParent);

        //move gameobject's world position to cable start to help with object focus behaviour
        transform.position = CableNodes[0].Position;

        if (_points == null)
            _points = new List<Vector3>(CableNodes.Count * 5);

        if (_smoothedPoints == null)
            _smoothedPoints = new List<Vector3>(CableNodes.Count * 10);

        _points.Clear();
        _smoothedPoints.Clear();

        Vector3 lastPos = CableNodes[0].Position;
        lastPos = transform.InverseTransformPoint(lastPos);

        Vector3 pos = Vector3.zero, nextPos = Vector3.zero, dir = Vector3.forward;
        CableNode node = null, next = null;

        for (int i = 0; i < CableNodes.Count - 1; i++)
        {
            node = CableNodes[i];
            next = CableNodes[i + 1];

            pos = transform.InverseTransformPoint(node.Position);
            nextPos = transform.InverseTransformPoint(next.Position);

            //compute orientation of this node
            dir = (nextPos - lastPos).normalized;
            //Vector3 HangerRotation;
            //if (Mathf.Abs(Vector3.Angle(Vector3.forward, dir)) <= 45)
            //{
            //    HangerRotation = new Vector3(0, 0, 1);
            //}
            //else if (Mathf.Abs(Vector3.Angle(Vector3.right, dir)) <= 45)
            //{
            //    HangerRotation = new Vector3(1, 0, 0);
            //}
            //else if (Mathf.Abs(Vector3.Angle(Vector3.back, dir)) <= 45)
            //{
            //    HangerRotation = new Vector3(0, 0, -1);
            //}
            //else
            //{
            //    HangerRotation = new Vector3(-1, 0, 0);
            //}

            //create cable hanger
            if (generateHangers)
            {
                GenerateCableHanger(node, hangerParent, dir, ref pos);
                
                if (i == CableNodes.Count - 2)
                {
                    //generate cable hanger for last node
                    GenerateCableHanger(next, hangerParent, dir, ref nextPos);
                }
            }

            //add starting point(s)
            if (i > 0)
            {

                if (node.ShowCableHanger)
                {
                    //add the entry-point and exit-point to this hanger
                    Vector3 entry = pos - dir * CableHangerWidth;
                    Vector3 exit = pos + dir * CableHangerWidth;
                    _points.Add(entry);
                    _points.Add(pos);
                    _points.Add(exit);

                    //update starting point for the cable
                    pos = exit;
                }
                else
                    _points.Add(pos);

                //Debug.DrawLine(transform.TransformPoint(entry), transform.TransformPoint(pos), Color.green, 5.0f);
                //Debug.DrawLine(transform.TransformPoint(pos), transform.TransformPoint(exit), Color.red, 5.0f);
            }
            else
            {
                _points.Add(pos);
            }

            BuildCableSegment(node, pos, nextPos);

            lastPos = pos;
        }


        //Vector3 endPoint = transform.InverseTransformPoint(CableNodes[CableNodes.Count - 1].Position);

        _points.Add(nextPos);

        ComputeSmoothedPoints();
        GenerateMeshFromSmoothedPoints();


    }

    void GenerateCableHanger(CableNode node, Transform hangerParent, Vector3 dir, ref Vector3 pos)
    {
        if (!node.ShowCableHanger || CableHangerPrefab == null)
            return;

        GameObject hangerObj = GameObject.Instantiate<GameObject>(CableHangerPrefab);
        CableHanger hanger = hangerObj.GetComponent<CableHanger>();
        Debug.Assert(hanger != null, "Cable hanger prefab is missing CableHanger component");

        //orient cable hanger along path
        hangerObj.transform.SetParent(hangerParent);
        hangerObj.transform.localPosition = pos;
        hangerObj.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        //move cable to hanger path position
        pos = transform.InverseTransformPoint(hanger.CablePath.position);
    }

    private void ComputeSmoothedPoints()
    {
        //_smoothedPoints = _points;
        //return;

        CatmullRomSpline spline = new CatmullRomSpline();
        if (_pointsArray != null && _pointsArray.Length == _points.Count)
        {
            _points.CopyTo(_pointsArray);
        }
        else
            _pointsArray = _points.ToArray();

        spline.Init(_pointsArray);
        spline.UpdateSpline();

        //always start with the first point
        _smoothedPoints.Add(spline.Evaluate(0, 0));

        int numSegments = _pointsArray.Length - 1;
        for (int seg = 0; seg < numSegments; seg++)
        {
            Vector3 segStart = _pointsArray[seg];
            Vector3 segEnd = _pointsArray[seg + 1];
            float segLength = (segEnd - segStart).magnitude;

            int numSegPoints = (int)(segLength * SegmentsPerMeter);
            if (numSegPoints < 1)
                numSegPoints = 1;

            for (int i = 0; i < numSegPoints; i++)
            {
                float t = (float)(i + 1) / (float)numSegPoints;
                var pos = spline.Evaluate(seg, t);
                if (CheckPointInterference)
                    CheckInterference(ref pos);

                _smoothedPoints.Add(pos);
            }
        }

        /*
        int numPoints = (CableNodes.Count - 1) * 10;

        for (int i = 0; i < numPoints; i++)
        {
            float t = (float)i / (float)(numPoints - 1);

            Vector3 smoothedPoint = spline.Evaluate(t);
            _smoothedPoints.Add(smoothedPoint);
        } */
    }

    private void CheckInterference(ref Vector3 pos)
    {
        pos += CheckInterference(pos, new Vector3(1, 0, 0), InterferenceDistance);
        //pos += CheckInterference(pos, new Vector3(0, 1, 0), 0.05f);
        pos += CheckInterference(pos, new Vector3(0, 0, 1), InterferenceDistance);

        pos += CheckInterference(pos, new Vector3(-1, 0, 0), InterferenceDistance);
        pos += CheckInterference(pos, new Vector3(0, -1, 0), InterferenceDistance);
        pos += CheckInterference(pos, new Vector3(0, 0, -1), InterferenceDistance);
    }

    private Vector3 CheckInterference(Vector3 pos, Vector3 dir, float dist)
    {
        pos = transform.TransformPoint(pos);

        RaycastHit hit;
        var normal = dir * -1.0f;
        var rayPos = pos + normal * 0.5f;

        if (!Physics.Raycast(rayPos, dir, out hit,
            dist + 0.5f, _floorLayerMask, QueryTriggerInteraction.Ignore))
            return Vector3.zero;

        //Debug.DrawLine(rayPos, hit.point, Color.blue, 5.0f);

        //ignore hits to surfaces that aren't somewhat perpendicular 
        var dot = Vector3.Dot(normal, hit.normal);
        if (dot < 0.6f)
            return Vector3.zero;

        var moveDist = dist - (hit.distance - 0.5f);
        return normal * moveDist;

    }

    private void ComputeSmoothedPointsSubdiv()
    {
        LinkedList<Vector3> smoothedPoints = new LinkedList<Vector3>();

        for (int i = 1; i < _points.Count - 1; i++)
        {
            //points
            Vector3 p0 = _points[i - 1];
            Vector3 p1 = _points[i];
            Vector3 p2 = _points[i + 1];

            //segments
            Vector3 s1 = p1 - p0;
            Vector3 s2 = p2 - p1;

            //subdivide each segment if too long

        }
    }

    protected virtual void GenerateMeshFromSmoothedPoints()
    {
        if (GeneratedMesh == null)
            GeneratedMesh = new Mesh();

        if (GeneratedMesh.name == null || GeneratedMesh.name.Length < 5)
        {
            System.Guid id = System.Guid.NewGuid();
            GeneratedMesh.name = "CableMesh_" + id.ToString();
        }

        Vector3[] vertices = GeneratedMesh.vertices;
        int[] triangles = GeneratedMesh.triangles;
        Vector2[] uv = GeneratedMesh.uv;

        ProcGeometry.GenerateTube(_smoothedPoints, CableDiameter / 2.0f, ref vertices, ref triangles, ref uv);
        //Debug.Log(_smoothedPoints.Count);
        GeneratedMesh.Clear();
        GeneratedMesh.vertices = vertices;
        GeneratedMesh.triangles = triangles;
        GeneratedMesh.uv = uv;

        GeneratedMesh.RecalculateNormals();
        GeneratedMesh.RecalculateBounds();

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null)
            mf.sharedMesh = GeneratedMesh;

        MeshGenerated?.Invoke();
    }

    public List<Vector3> GetSmoothedPoints()
    {
        return _smoothedPoints;
    }

    public Vector3 GetSmoothedPoint(int i)
    {
        if (i < 0)
            i = 0;
        else if (i > _smoothedPoints.Count - 1)
            i = _smoothedPoints.Count - 1;

        return _smoothedPoints[i];
    }

    public int GetClosestSmoothedPoint(Ray ray)
    {
        if (_smoothedPoints == null || _smoothedPoints.Count <= 0)
            return -1;

        float minDist = float.MaxValue;
        int index = -1;

        for (int i = 0; i < _smoothedPoints.Count; i++)
        {
            var dist = Util.DistanceToLine(transform.TransformPoint(_smoothedPoints[i]), ray);

            if (dist < minDist)
            {
                index = i;
                minDist = dist;
            }
        }

        return index;
    }

    private void Awake()
    {
        if (CableNodes == null)
            CableNodes = new List<CableNode>();

        _floorLayerMask = LayerMask.GetMask("Floor");
    }

    // Use this for initialization
    void Start()
    {

    }


#if UNITY_EDITOR
    public void Reset()
    {
        if (CableHangerPrefab == null)
        {
            CableHangerPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/StationaryEquipment/Cables/CableHanger.prefab");
        }


    }
#endif 
}
