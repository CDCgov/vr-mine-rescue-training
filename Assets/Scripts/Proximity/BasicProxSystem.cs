using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#pragma warning disable 0219


public class BasicProxSystem : ProxSystem 
{
    [System.Serializable]
    public struct FieldGenerator// : UnityEngine.Object
    {
        public Transform Position;
        public Vector3 YellowRange;
        public Vector3 RedRange;
    }
    public List<FieldGenerator> FieldGenerators;

    public GameObject RedVis;
    public GameObject YellowVis;

    private Dictionary<Collider, int> _closeObjects;

    private ProxZone _activeProxZone = ProxZone.GreenZone;
    private ProxZone _activeVisualization = ProxZone.GreenZone;

    private static Mesh _sphereMesh;
    private static Material _visMat;

    public override void DisableZoneVisualization()
    {
        _activeVisualization = ProxZone.GreenZone;
    }

    public override void EnableZoneVisualization(VisOptions opt)
    {
        _activeVisualization = ProxZone.YellowZone;
    }

    public override ProxZone GetActiveProxZone()
    {
        return _activeProxZone;
    }

    public override IEnumerator<GameObject> GetObjectsInZone(ProxZone zone)
    {
        yield return null;
    }

    void OnDrawGizmosSelected()
    {
        Matrix4x4 gizMat = Gizmos.matrix;

        foreach (FieldGenerator gen in FieldGenerators)
        {
            if (gen.Position != null)
            {
                Gizmos.matrix = Matrix4x4.TRS(gen.Position.position, Quaternion.identity, gen.YellowRange);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Vector3.zero, 1.0f);

                Gizmos.matrix = Matrix4x4.TRS(gen.Position.position, Quaternion.identity, gen.RedRange);
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Vector3.zero, 1.0f);
            }
        }

        Gizmos.matrix = gizMat;
    }
    
    protected override void Start()
    {
        if (_sphereMesh == null)
        {
            _sphereMesh = ProcSphere.GenSphere();
        }

        if (_visMat == null)
        {
            _visMat = Resources.Load<Material>("ProxZoneMat");
        }

        if (FieldGenerators == null)
            return;

        _closeObjects = new Dictionary<Collider, int>();

        foreach (FieldGenerator gen in FieldGenerators)
        {
            Vector3 localCenter = gen.Position.position;
            localCenter = transform.InverseTransformPoint(localCenter);

            SphereCollider col = gameObject.AddComponent<SphereCollider>();
            col.radius = Mathf.Max(gen.RedRange.x, gen.RedRange.y, gen.RedRange.z, gen.YellowRange.x, gen.YellowRange.y, gen.YellowRange.z);
            col.center = localCenter;
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        int count = 0;
        _closeObjects.TryGetValue(other, out count);
        _closeObjects[other] = count + 1;
    }

    void OnTriggerExit(Collider other)
    {
        int count = 1;
        _closeObjects.TryGetValue(other, out count);
        count--;
        if (count <= 0)
            _closeObjects.Remove(other);
    }

    protected override void Update()
    {
        /*
        int mask = LayerMask.GetMask("Player");
        int playerLayer = LayerMask.NameToLayer("Player");

        ProxZone curZone = ProxZone.GreenZone;
        foreach (Collider col in _closeObjects.Keys)
        {
            //if ((col.gameObject.layer & mask) == 0)
            //continue;
            if (col.gameObject.layer != playerLayer)
                continue;

            ProxZone colZone = TestPoint(col.transform.position);

            if (colZone > curZone)
            {
                curZone = colZone;
            }

            if (curZone == ProxZone.RedZone)
                break; // don't need to keep testing
        }*/

        if (_activeVisualization != ProxZone.GreenZone)
        {
            MaterialPropertyBlock mp = new MaterialPropertyBlock();

            switch (_activeVisualization)
            {
                case ProxZone.YellowZone:
                    //_visMat.color = Color.yellow;
                    mp.SetColor("_Color", Color.yellow);
                    mp.SetColor("_EmissionColor", new Color(0.2f, 0.2f, 0));
                    break;

                case ProxZone.RedZone:
                    //_visMat.color = Color.red;
                    mp.SetColor("_Color", Color.red);
                    mp.SetColor("_EmissionColor", new Color(0.2f, 0, 0));
                    break;
            }


            foreach (FieldGenerator gen in FieldGenerators)
            {
                Vector3 scale = Vector3.one;
                switch (_activeVisualization)
                {
                    case ProxZone.YellowZone:
                        scale = gen.YellowRange;
                        break;
                    case ProxZone.RedZone:
                        scale = gen.RedRange;
                        break;
                }
                Matrix4x4 trs = Matrix4x4.TRS(gen.Position.position, Quaternion.identity, scale);

                Graphics.DrawMesh(_sphereMesh, trs, _visMat, 0, Camera.main, 0, mp);
            }
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

        //_activeProxZone = curZone;
    }

    public override ProxZone TestPoint(Vector3 position)
    {
        bool bInYellowZone = false;

        foreach (FieldGenerator gen in FieldGenerators)
        {
            //transform the point to be relative to the position of the generator ellipsoid
            Vector3 tpos = position - gen.Position.position;

            //check red zone
            float tresult = EvalPointInEllipsoid(tpos, gen.RedRange);
            if (tresult <= 1)
                return ProxZone.RedZone; //exit early

            if (!bInYellowZone)
            {
                //check yellow zone
                tresult = EvalPointInEllipsoid(tpos, gen.YellowRange);
                if (tresult <= 1)
                    bInYellowZone = true;
            }

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

    public override Bounds ComputeProxSystemBounds()
    {
        throw new NotImplementedException();
    }
}