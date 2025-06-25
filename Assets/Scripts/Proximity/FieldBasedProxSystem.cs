using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#pragma warning disable 0219


[System.Serializable]
public struct FieldGenerator// : UnityEngine.Object
{
    public Transform Position;

    [NonSerialized]
    public Vector3 YellowRange;
    [NonSerialized]
    public Vector3 RedRange;


    [Tooltip("Magnetic Flux Density - Yellow Zone")]
    //[Range(0.00005f, 100)]
    public float B_Yellow;

    [Tooltip("Magnetic Flux Density - Red Zone")]
    //[Range(0.00005f, 100)]
    public float B_Red;

    [Tooltip("Shell Base Shape Constant")]
    public float Ca_ShellBaseShapeConst;
    [Tooltip("Shell Shape Changing Constant")]
    public float Da_ShellShapeChangeConst;
    [Tooltip("Shell Base Size Constant")]
    public float Cb_ShellBaseSizeConst;
    [Tooltip("Shell Size Changing Constant")]
    public float Db_ShellSizeChangeConst;

    [NonSerialized]
    [HideInInspector]
    public Mesh VisMeshYellow;

    [NonSerialized]
    [HideInInspector]
    public Mesh VisMeshRed;

    [NonSerialized]
    public float MaxDist;
}

public class FieldBasedProxSystem : ProxSystem 
{
    public float UnitScale = 0.001f;
    public List<FieldGenerator> FieldGenerators;

    public GameObject RedVis;
    public GameObject YellowVis;

    public Color YellowColor = Color.yellow;
    public Color RedColor = Color.red;

    public bool OverrideVisCameraDistance = false;

    public float GridSpacing = 1;
    public int GridLineCount = 25;
    public int GridSecondDivisor = 5;
    public Vector2 GridShift = Vector2.zero;
    public Color GridColor = new Color(0, 1, 0, 0.5f);
    public Color GridSecondColor = new Color(1, 0, 0, 0.5f);

    //private ProxZone _activeVisualization = ProxZone.GreenZone;
    private bool _showYellowShell = false;
    private bool _showRedShell = false;

    private Mesh _visMesh;
    private static Material _visMat;

    private InstancedMeshShell _redShell;
    private InstancedMeshShell _yellowShell;

    private Vector3[] _visDirSamples;
    private Material _visShellMat;
    private MaterialPropertyBlock _mpbYellow;
    private MaterialPropertyBlock _mpbRed;


    private void InitializeShells()
    {
        _redShell = new InstancedMeshShell();
        _yellowShell = new InstancedMeshShell();

        InitializeVisSamples();

        _visShellMat = Resources.Load<Material>("UnlitColored");
    }

    private void InitializeVisSamples()
    {
        //_visDirSamples = ComputeVisSamples(48, 32);
        _visDirSamples = ComputeVisSamples(32, 22);
    }

    private Vector3[] ComputeVisSamples(int xcount, int ycount)
    {
        return ProcSphere.GenSphereVertices(1, xcount, ycount);
    }

    public override void DisableZoneVisualization()
    {
        //_activeVisualization = ProxZone.GreenZone;
        _showRedShell = false;
        _showYellowShell = false;
    }

    public override void EnableZoneVisualization(VisOptions opt)
    {
        /*if (opt.ShowYellowShell)
            _activeVisualization = ProxZone.YellowZone;
        else
            _activeVisualization = ProxZone.RedZone; */

        _showYellowShell = opt.ShowYellowShell;
        _showRedShell = opt.ShowRedShell;

        //UpdateAllVisMeshes();
    }
    
    public override IEnumerator<GameObject> GetObjectsInZone(ProxZone zone)
    {
        yield return null;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
        
        if (_showYellowShell)
        {
            
        }
    }

    public override Bounds ComputeProxSystemBounds()
    {
        Bounds b = new Bounds();

        int count = 0;
        Vector3 avgCenter = Vector3.zero;
        foreach (FieldGenerator gen in FieldGenerators)
        {
            Vector3 localCenter = gen.Position.position;
            localCenter = transform.InverseTransformPoint(localCenter);

            avgCenter += localCenter;
            count++;
        }

        avgCenter = avgCenter / (float)count;
        b.center = avgCenter;
        b.size = Vector3.zero;

        foreach (FieldGenerator gen in FieldGenerators)
        {
            Vector3 genCenter = gen.Position.position;
            genCenter = transform.InverseTransformPoint(genCenter);
            Vector3 genSize = new Vector3(gen.MaxDist, gen.MaxDist, gen.MaxDist) * 2.0f;
            Bounds genBounds = new Bounds(genCenter, genSize);

            b.Encapsulate(genBounds);
        }

        return b;
    }

    protected override void Start()
    {

        InitializeShells();

        UpdateAllVisMeshes();

        if (_visMat == null)
        {
            _visMat = Resources.Load<Material>("ProxZoneMat");
        }

        if (FieldGenerators == null)
            return;

        base.Start();

    }

    

    public void UpdateAllVisMeshes()
    {
        if (FieldGenerators.Count <= 0)
            return;

        for (int i = 0; i < FieldGenerators.Count; i++)
        {
            FieldGenerator gen = FieldGenerators[i];
            UpdateVisMesh(ref gen);
            FieldGenerators[i] = gen;
        }
    }

    private void UpdateVisMesh(ref FieldGenerator gen)
    {
        if (gen.VisMeshYellow == null)
            gen.VisMeshYellow = ProcSphere.GenSphere();

        if (gen.VisMeshRed == null)
            gen.VisMeshRed = ProcSphere.GenSphere();

        gen.YellowRange = UpdateVisMesh(gen.VisMeshYellow, ProxZone.YellowZone, gen, gen.B_Yellow, gen.Ca_ShellBaseShapeConst, gen.Da_ShellShapeChangeConst, gen.Cb_ShellBaseSizeConst, gen.Db_ShellSizeChangeConst);
        gen.RedRange = UpdateVisMesh(gen.VisMeshRed, ProxZone.RedZone, gen, gen.B_Red, gen.Ca_ShellBaseShapeConst, gen.Da_ShellShapeChangeConst, gen.Cb_ShellBaseSizeConst, gen.Db_ShellSizeChangeConst);

        float tmp;
        tmp = gen.YellowRange.x;
        gen.YellowRange.x = gen.YellowRange.z;
        gen.YellowRange.z = tmp;

        tmp = gen.RedRange.x;
        gen.RedRange.x = gen.RedRange.z;
        gen.RedRange.z = tmp;
        //gen.YellowRange = gen.Position.TransformDirection(gen.YellowRange);
        //gen.RedRange = gen.Position.TransformDirection(gen.RedRange);

        gen.MaxDist = Mathf.Max(gen.YellowRange.x, gen.YellowRange.y, gen.YellowRange.z, gen.RedRange.x, gen.RedRange.y, gen.RedRange.z);

        //Debug.Log(gen.VisMeshYellow);
        //Debug.Log(gen.VisMeshRed);
    }

    private void UpdateVisShell(ProxShell shell, InstancedMeshShell meshShell)
    {
        meshShell.Clear();

        ComputeVisPoints(shell, (Vector3 pt) =>
        {
            meshShell.AddMarker(pt);
        });
    }

    private void ComputeVisPoints(ProxShell shell, Action<Vector3> pointDel)
    {
        if (FieldGenerators == null)
            return;

        if (_visDirSamples == null)
            InitializeVisSamples();

        int minZone;
        if (shell == ProxShell.YellowShell)
            minZone = (int)ProxZone.YellowZone;
        else
            minZone = (int)ProxZone.RedZone;

        //for each generator
        for (int genIndex = 0; genIndex < FieldGenerators.Count; genIndex++)
        {
            FieldGenerator gen = FieldGenerators[genIndex];
            float B;

            if (shell == ProxShell.YellowShell)
                B = gen.B_Yellow;
            else
                B = gen.B_Red;
            
            //create transform matrices
            Matrix4x4 genToSystem = transform.worldToLocalMatrix * gen.Position.localToWorldMatrix;
            Matrix4x4 genToWorld = gen.Position.localToWorldMatrix;

            for (int ptIndex = 0; ptIndex < _visDirSamples.Length; ptIndex++)
            {
                Vector3 pt = _visDirSamples[ptIndex];

                float dist = ComputeShellSurfaceDistance(pt, B, gen.Ca_ShellBaseShapeConst, gen.Da_ShellShapeChangeConst, 
                    gen.Cb_ShellBaseSizeConst, gen.Db_ShellSizeChangeConst);

                pt = pt * dist;

                //transform the point from generator relative to the space of the prox system
                Vector3 sysPt = genToSystem.MultiplyPoint3x4(pt);

                //transform to world space
                Vector3 worldPt = genToWorld.MultiplyPoint3x4(pt);

                //bool insideShell = false;
                int otherGenCount = 0;

                for (int otherGen = 0; otherGen < FieldGenerators.Count; otherGen++)
                {
                    if (otherGen == genIndex)
                        continue;					

                    if (TestShellWorld(FieldGenerators[otherGen], shell, worldPt))
                    {
                        //shell is inside another generator
                        otherGenCount++;
                    }
                }

                //if (!insideShell)
                //pointDel(sysPt);

                if (otherGenCount <= 0)
                {
                    pointDel(sysPt);
                }

                //only draw points that are inside at least 2 generators
                // if (otherGenCount >= 1)
                // {
                // 	pointDel(sysPt);
                // }
                
            }

        }
    }

    private Vector3 UpdateVisMesh(Mesh m, ProxZone zone, FieldGenerator gen, float B, float Ca, float Da, float Cb, float Db)
    {
        Vector3[] vertices = m.vertices;

        float a = Ca * Mathf.Pow(B, -1 * Da);
        float b = Cb * Mathf.Pow(B, -1 * Db);

        float maxDist = 0;
        Vector3 bounds = Vector3.zero;

        for (int i = 0; i < vertices.Length; i++)
        {
            //get normalized vector in direction of vertex
            Vector3 v = vertices[i].normalized;
            Vector3 vsqrd;

            //compute distance from origin to the given magnetic shell strength
            //eq: sqrt(x^2 + y^2 + z^2) = a ((x^2 - y^2 - z^2) / (x^2 + y^2 + z^2)) + b

            vsqrd.x = v.x * v.x;
            vsqrd.y = v.y * v.y;
            vsqrd.z = v.z * v.z;

            float dist = a * ((vsqrd.x - vsqrd.y - vsqrd.z) / (vsqrd.x + vsqrd.y + vsqrd.z)) + b;
            dist *= UnitScale;
            if (dist <= 0.001 || dist > 1000)
                continue;

            maxDist = Mathf.Max(dist, maxDist);

            //move vertex to computed position
            v = v * dist;
            vertices[i] = v;

            /************************ HALF SHELL CUTOFF 
            if (vertices[i].z < -0.001f)
                vertices[i] = Vector3.zero;
                */

            /*********************** REMOVE INTERIOR 
            ProxZone ptZone = TestPoint(gen.Position.TransformPoint(vertices[i]));
            if (ptZone == zone || ptZone == ProxZone.RedZone)
            {
                vertices[i] = Vector3.zero;
            } */
            

            bounds.x = Mathf.Max(bounds.x, v.x);
            bounds.y = Mathf.Max(bounds.y, v.y);
            bounds.z = Mathf.Max(bounds.z, v.z);

        }

        m.vertices = vertices;
        //return maxDist;
        return bounds;
    }

    public static float ComputeShellSurfaceDistance(Vector3 dir, float B, float Ca, float Da, float Cb, float Db)
    {
        return ComputeShellSurfaceDistance(dir, B, Ca, Da, Cb, Db, 0.001f);
    }

    public static float ComputeShellSurfaceDistance(Vector3 dir, float B, float Ca, float Da, float Cb, float Db, float unitScale)
    {
        Vector3 vsqrd;

        float a = Ca * Mathf.Pow(B, -1 * Da);
        float b = Cb * Mathf.Pow(B, -1 * Db);

        //compute distance from origin to the given magnetic shell strength
        //eq: sqrt(x^2 + y^2 + z^2) = a ((x^2 - y^2 - z^2) / (x^2 + y^2 + z^2)) + b

        vsqrd.x = dir.x * dir.x;
        vsqrd.y = dir.y * dir.y;
        vsqrd.z = dir.z * dir.z;

        float dist = a * ((vsqrd.x - vsqrd.y - vsqrd.z) / (vsqrd.x + vsqrd.y + vsqrd.z)) + b;
        return dist * unitScale;
    }

    private float ComputeMagneticFlux(Vector3 pos, FieldGenerator gen)
    {
        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //sw.Start();

        float max_b = 5.0f;
        float min_b = 0.0f;

        float Ca = gen.Ca_ShellBaseShapeConst;
        float Da = gen.Da_ShellShapeChangeConst;
        float Cb = gen.Cb_ShellBaseSizeConst;
        float Db = gen.Db_ShellSizeChangeConst;

        float targetDist = pos.magnitude;
        Vector3 dir = pos.normalized;

        int iterations = 0;
        float B = 1.0f;
        float currentDist = ComputeShellSurfaceDistance(dir, B, Ca, Da, Cb, Db, UnitScale);
        float error = currentDist - targetDist;

        while (Mathf.Abs(error) > 0.003f && iterations < 50)
        {
            if (error > 0)
            {
                //distance is too high, which means b value is too low
                min_b = B;
                B = B + (max_b - B) * 0.5f;
            }
            else
            {
                //distance is too low, which means b value is too high
                max_b = B;
                B = B - (B - min_b) * 0.5f;
            }

            currentDist = ComputeShellSurfaceDistance(dir, B, Ca, Da, Cb, Db, UnitScale);
            error = currentDist - targetDist;

            iterations++;
        }
        
        if (Mathf.Abs(error) > 0.004f)
        {
            //Debug.LogErrorFormat("Error calculating magnetic flux, off by {0:F2}", error);
        }

        //sw.Stop();

        //Debug.LogFormat("Computed B value as {0:F2} with error {1:F4} in {2} iterations in {3:F6} ms", B, error, iterations, sw.ElapsedTicks);
        return B;
    }

    public void DrawVisShell(float minScale = 1.0f)
    {

        if (_redShell == null || _yellowShell == null)
            InitializeShells();

        Color yellow = Color.yellow;
        Color red = Color.red;

        float distToCamera = 0;
        if (Camera.main != null && Application.isPlaying)
            distToCamera = Vector3.Distance(Camera.main.transform.position, transform.position);

        //compute marker scale 
        float scale = 1.0f;

        // if (distToCamera > 0)
        // {
        // 	scale = Mathf.Clamp((distToCamera - DeformableProxSystem.PROX_SCALE_START) * DeformableProxSystem.PROX_SCALE_RATE + 1.0f, 1.0f, 5.0f);

        // 	//compute marker fade
        // 	if (distToCamera > DeformableProxSystem.PROX_VIS_FADE_DIST)
        // 	{
        // 		distToCamera -= DeformableProxSystem.PROX_VIS_FADE_DIST;
        // 		float alpha;
        // 		alpha = distToCamera / (DeformableProxSystem.PROX_VIS_CUTOFF_DIST - DeformableProxSystem.PROX_VIS_FADE_DIST);
        // 		alpha = 1.0f - alpha;

        // 		alpha = Mathf.Clamp(alpha, 0.0f, 1.0f);

        // 		red.a = alpha;
        // 		yellow.a = alpha;
        // 	}
        // }

        scale = Mathf.Clamp((distToCamera - DeformableProxSystem.PROX_SCALE_START) * DeformableProxSystem.PROX_SCALE_RATE + 1.0f, 1.0f, 5.0f);

        if (OverrideVisCameraDistance)
            scale = 0.3f;

        //compute marker fade
        if (OverrideVisCameraDistance)
        {
            red.a = 1.0f;
            yellow.a = 1.0f;
        }
        else if (distToCamera > DeformableProxSystem.PROX_VIS_FADE_DIST)
        {
            distToCamera -= DeformableProxSystem.PROX_VIS_FADE_DIST;
            float alpha;
            alpha = distToCamera / (DeformableProxSystem.PROX_VIS_CUTOFF_DIST - DeformableProxSystem.PROX_VIS_FADE_DIST);
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

        // if (scale < minScale)
        // 	scale = minScale;

        _mpbRed.SetFloat("_Scale", scale);
        _mpbYellow.SetFloat("_Scale", scale);

        if (_showYellowShell)
        {
            UpdateVisShell(ProxShell.YellowShell, _yellowShell);
            _yellowShell.DrawShell(transform.position, transform.localToWorldMatrix, _visShellMat, _mpbYellow);
        }
        
        if (_showRedShell)
        {
            UpdateVisShell(ProxShell.RedShell, _redShell);
            _redShell.DrawShell(transform.position, transform.localToWorldMatrix, _visShellMat, _mpbRed);
        }
    }


    protected override void Update()
    {
        base.Update();		

        //int mask = LayerMask.GetMask("Player");
        //int playerLayer = LayerMask.NameToLayer("Player");		

        if (_showRedShell || _showYellowShell)
        {

            DrawVisShell(1.85f);
            //DrawVisMesh();
            
            
        }

        //Debug.Log(curZone);

        ////////TEMP DEBUG INPUT//////////////////
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            DisableZoneVisualization();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            EnableZoneVisualization(new VisOptions(false, true));
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            EnableZoneVisualization(new VisOptions(true, false));
        }
        //////////////////////////////////////////
    }

    public bool TestShellWorld(FieldGenerator gen, ProxShell shell, Vector3 worldPos)
    {
        Vector3 localPos = gen.Position.InverseTransformPoint(worldPos);

        return TestShellLocal(gen, shell, localPos);
    }

    public bool TestShellLocal(FieldGenerator gen, ProxShell shell, Vector3 localPos)
    {
        float testDist = localPos.magnitude;
        localPos.Normalize();

        float shellB;
        if (shell == ProxShell.RedShell)
            shellB = gen.B_Red;
        else
            shellB = gen.B_Yellow;

        float shellDist = ComputeShellSurfaceDistance(localPos, shellB, gen.Ca_ShellBaseShapeConst, gen.Da_ShellShapeChangeConst, gen.Cb_ShellBaseSizeConst, gen.Db_ShellSizeChangeConst);

        if (testDist > shellDist) //point is outside shell
            return false;
        else
            return true;
    }

    public ProxZone TestWorldPoint(FieldGenerator gen, Vector3 worldPos)
    {
        Vector3 localPos = gen.Position.InverseTransformPoint(worldPos);

        //first check inside red shell
        if (TestShellLocal(gen, ProxShell.RedShell, localPos))
            return ProxZone.RedZone;

        //check if inside yellow shell
        if (TestShellLocal(gen, ProxShell.YellowShell, localPos))
            return ProxZone.YellowZone;

        return ProxZone.GreenZone;

        /*
        Vector3 tpos = gen.Position.InverseTransformPoint(position);
        float B = ComputeMagneticFlux(tpos, gen);

        if (B > gen.B_Red)
            return ProxZone.RedZone;
        else if (B > gen.B_Yellow)
            return ProxZone.YellowZone;
        else
            return ProxZone.GreenZone; */
    }

    public override ProxZone TestPoint(Vector3 position)
    {
        bool bInYellowZone = false;

        foreach (FieldGenerator gen in FieldGenerators)
        {

            ProxZone zone = TestWorldPoint(gen, position);

            if (zone == ProxZone.RedZone)
                return ProxZone.RedZone;
            else if (zone == ProxZone.YellowZone)
                bInYellowZone = true;

            /**** OLD METHOD USING POSITION FLUX
             * 
            Vector3 tpos = gen.Position.InverseTransformPoint(position);
            float B = ComputeMagneticFlux(tpos, gen);

            if (B > gen.B_Red)
                return ProxZone.RedZone;
            else if (B > gen.B_Yellow)
                bInYellowZone = true;
                */

        }

        if (bInYellowZone)
            return ProxZone.YellowZone;
        else
            return ProxZone.GreenZone;
    }

    float EvalPointInEllipsoid(Vector3 position, Vector3 ellipsoid)
    {
        //eval equation for ellipsoid x^2 / a^2 + y^2 / b^2 + z^2 / c^2 https://en.wikipedia.org/wiki/Ellipsoid

        float testX = position.x / ellipsoid.x;
        float testY = position.y / ellipsoid.y;
        float testZ = position.z / ellipsoid.z;

        return testX * testX + testY * testY + testZ * testZ;
    }
}