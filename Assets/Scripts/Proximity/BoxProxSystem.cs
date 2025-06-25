using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxProxSystem : ProxSystem
{
    public const float FEET_TO_METERS = 0.3048f;
    public const float METERS_TO_FEET = 1.0f / FEET_TO_METERS;

    public Bounds YellowZoneBounds;
    public Bounds RedZoneBounds;

    public Vector3 CenterOffset = Vector3.zero;

    public bool DrawYellowShell;
    public bool DrawRedShell;

    public int GridLineCount = 50;
    public float GridLineSpacing = 5;
    public Vector3 GridOffset = Vector3.zero;
    public Color GridColor = new Color(0, 1, 0, 0.5f);

    private LineRenderer _lrRed;
    private LineRenderer _lrYellow;

    private ProxSystem.VisOptions _visOpts;

    public override Bounds ComputeProxSystemBounds()
    {
        return YellowZoneBounds;
    }

    public override void DisableZoneVisualization()
    {
        DrawRedShell = false;
        DrawYellowShell = false;

        if (_lrRed != null & _lrYellow != null)
        {
            _lrYellow.enabled = false;
            _lrRed.enabled = false;

            _lrYellow.gameObject.SetActive(false);
            _lrRed.gameObject.SetActive(false);
        }

    }

    public override void EnableZoneVisualization(VisOptions opt)
    {
        DrawRedShell = opt.ShowRedShell;
        DrawYellowShell = opt.ShowYellowShell;

        if (_lrRed != null & _lrYellow != null)
        {
            _lrYellow.enabled = DrawYellowShell;
            _lrRed.enabled = DrawRedShell;

            DrawBoundsVis(_lrYellow, YellowZoneBounds);
            DrawBoundsVis(_lrRed, RedZoneBounds);

            _lrYellow.gameObject.SetActive(DrawYellowShell);
            _lrRed.gameObject.SetActive(DrawRedShell);
        }

        _visOpts = opt;
    }

    private void DrawBoundsVis(LineRenderer lr, Bounds b)
    {
        Vector3 pos = b.center + CenterOffset;
        pos.y = 0.2f;

        Vector3[] points = new Vector3[5];

        float xExtent = b.extents.x;
        float zExtent = b.extents.z;

        points[0] = pos + new Vector3(xExtent, 0, zExtent);
        points[1] = pos + new Vector3(xExtent, 0, -zExtent);
        points[2] = pos + new Vector3(-xExtent, 0, -zExtent);
        points[3] = pos + new Vector3(-xExtent, 0, zExtent);
        points[4] = pos + new Vector3(xExtent, 0, zExtent);


        lr.numCornerVertices = 15;
        lr.startWidth = lr.endWidth = 0.1f;
        lr.loop = false;
        lr.useWorldSpace = false;
        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }

    public override IEnumerator<GameObject> GetObjectsInZone(ProxZone zone)
    {
        yield return null;
    }

    /// <summary>
    /// test world space position against prox system
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public override ProxZone TestPoint(Vector3 position)
    {
        Vector3 lpos = transform.InverseTransformPoint(position);
        lpos -= CenterOffset;

        if (RedZoneBounds.Contains(lpos))
            return ProxZone.RedZone;
        else if (YellowZoneBounds.Contains(lpos))
            return ProxZone.YellowZone;
        else
            return ProxZone.GreenZone;
    }

    private void OnDrawGizmos()
    {
        if (DrawRedShell)
        {
            DrawBounds(RedZoneBounds, Color.red);
        }

        if (DrawYellowShell)
        {
            DrawBounds(YellowZoneBounds, Color.yellow);
        }


        /*Bounds b = Util.ComputeBounds(transform.parent.gameObject);
        Debug.Log(b.extents);
        b.center -= transform.parent.position;
        DrawBounds(b, Color.green);
        */
    }

    private void DrawBounds(Bounds b, Color color)
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = color;
        //Gizmos.DrawCube(b.center + CenterOffset, b.size);
        Gizmos.DrawWireCube(b.center + CenterOffset, b.size);
        Gizmos.matrix = Matrix4x4.identity;
    }

    private LineRenderer CreateVisRenderer(string name, Color color)
    {
        GameObject visObj = new GameObject(name);
        visObj.transform.SetParent(transform, false);
        visObj.transform.localPosition = Vector3.zero;
        visObj.transform.localRotation = Quaternion.identity;
        visObj.layer = LayerMask.NameToLayer("TransparentFX");

        LineRenderer lr = visObj.AddComponent<LineRenderer>();
        lr.startColor = lr.endColor = color;
        lr.material = Resources.Load<Material>("BoxProxVisMat");

        return lr;
    }

    protected override void Start()
    {
        base.Start();



        _lrRed = CreateVisRenderer("Red Vis", Color.red);
        _lrYellow = CreateVisRenderer("Yellow Vis", Color.yellow);


        _lrRed.enabled = false;
        _lrYellow.enabled = false;

        EnableZoneVisualization(new VisOptions(DrawRedShell, DrawYellowShell));
    }

    protected override void Update()
    {
        base.Update();


    }

    public Bounds ComputeMachineBounds()
    {
        Bounds machineBounds = new Bounds();

        Transform tparent = transform.parent;
        if (tparent == null)
            return machineBounds;

        GameObject parent = transform.parent.gameObject;

        /*

        Bounds b = Util.ComputeBounds(parent);
        Debug.Log(b.extents);
        b.center -= tparent.position;
        
        _machineBounds = b;
        */

        //compute bounds from attached box collider
        BoxCollider col = parent.GetComponentInChildren<BoxCollider>();
        if (col == null)
            machineBounds = new Bounds(Vector3.zero, Vector3.one);

        Vector3 center = col.transform.TransformPoint(col.center);
        center = transform.InverseTransformPoint(center);

        Vector3 size = col.transform.TransformDirection(col.size);
        size = transform.InverseTransformDirection(size);

        size.x = Mathf.Abs(size.x);
        size.y = Mathf.Abs(size.y);
        size.z = Mathf.Abs(size.z);

        machineBounds = new Bounds(center, size);
        return machineBounds;
        //DrawBounds(b, Color.green);
    }

    /// <summary>
    /// copute prox zone offsets off machine bounds in feet
    /// </summary>
    /// <param name="proxb"></param>
    /// <param name="front"></param>
    /// <param name="back"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public void ComputeMachineOffsets(Bounds machineBounds, Bounds proxb, out float front, out float back, out float left, out float right)
    {

        Vector3 center = CenterOffset + proxb.center;
        Vector3 mcenter = machineBounds.center;

        front = (center.z + proxb.extents.z) - (mcenter.z + machineBounds.extents.z);
        back = (mcenter.z - machineBounds.extents.z) - (center.z - proxb.extents.z);
        left = (mcenter.x - machineBounds.extents.x) - (center.x - proxb.extents.x);
        right = (center.x + proxb.extents.x) - (mcenter.x + machineBounds.extents.x);

        front *= METERS_TO_FEET;
        back *= METERS_TO_FEET;
        left *= METERS_TO_FEET;
        right *= METERS_TO_FEET;
    }

    public void SetMachineOffsetsYellow(float front, float back, float left, float right)
    {
        var machineBounds = ComputeMachineBounds();
        SetMachineOffsets(machineBounds, front, back, left, right, ref YellowZoneBounds);
    }

    public void SetMachineOffsetsRed(float front, float back, float left, float right)
    {
        var machineBounds = ComputeMachineBounds();
        SetMachineOffsets(machineBounds, front, back, left, right, ref RedZoneBounds);
    }

    /// <summary>
    /// set prox zone offsets in feet from machine bounds
    /// </summary>
    /// <param name="proxb"></param>
    /// <param name="front"></param>
    /// <param name="back"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    public void SetMachineOffsets(Bounds machineBounds, float front, float back, float left, float right, ref Bounds proxb)
    {

        //Vector3 center = proxSystem.CenterOffset + proxb.center;
        Vector3 mcenter = machineBounds.center;

        Vector3 pext = proxb.extents;
        Vector3 pct = proxb.center;

        front *= FEET_TO_METERS;
        back *= FEET_TO_METERS;
        left *= FEET_TO_METERS;
        right *= FEET_TO_METERS;

        //compute extent by summing machine + front + back
        pext.z = (front + back + machineBounds.extents.z * 2) * 0.5f;
        //compute center to match front/back alignment			
        pct.z = mcenter.z + ((machineBounds.extents.z + front) - pext.z);

        //same for sides
        pext.x = (left + right + machineBounds.extents.x * 2) * 0.5f;
        pct.x = mcenter.x + ((machineBounds.extents.x + right) - pext.x);

        //set height to 50 feet (extent is half size)
        pext.y = 25 * FEET_TO_METERS;

        proxb.extents = pext;
        proxb.center = pct;
    }
}
