using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum RoofboltZoneType
{
    Centered,
    East,
    West,
    North,
    South,
    EastWestStraight,
    NorthSouthStraight
}

[System.Serializable]
public struct PairedBolts
{
    [SerializeField]
    public Transform[] Bolts;
}

public struct PairedBoltPositions
{
    public Vector3 Pos1;
    public Vector3 Pos2;
    public bool HasPairedBolt;

    public PairedBoltPositions(Vector3 pos1, Vector3 pos2, bool hasPairedBolt)
    {
        Pos1 = pos1;
        Pos2 = pos2;
        HasPairedBolt = hasPairedBolt;
    }
}

public class RoofBoltGeneratorZone : MonoBehaviour
{
    //public Transform FrontLeft;
    //public Transform FrontRight;
    //public Transform RearLeft;
    //public Transform RearRight;
    public Transform MineSegmentParent;

    public Bounds BoltContainment;
    public Vector3 EdgePointLocal;
    [Range(0.61f, 1.82f)]
    public float BoltSpacingDistance = 1.20f;
    public float BoltRibOffset = 1.067f;/*3.5 ft*/
    public bool HasCornerCurtainOption = false;
    public GameObject RoofBoltPrefab;
    public RoofboltZoneType ZoneType;
    public PairedBolts[] ExtraBolts;    
    
    [HideInInspector]
    public List<PairedBoltPositions> CornerCurtains;

    public List<GameObject> BoltList { get; private set; }

    public Transform BoltParent
    {
        get { return _boltParent.transform; }
    }

    private List<PairedBolts> _pairedBolts;    

    private Bounds SmallerBound;
    private GameObject _boltParent;
    /*
    public void GenerateBolts()
    {
        if(BoltSpacingDistance == 0)
        {
            Debug.Log("Bolt space was zero! Returning. . .");
            return;
        }
        //ClearExistingBolts();
        
        Bounds container = BoltContainment;
        

        container.center = transform.position;
        container.extents = new Vector3(BoltContainment.extents.x * MineSegmentParent.localScale.x, BoltContainment.extents.y, BoltContainment.extents.z * MineSegmentParent.localScale.z);
        SmallerBound.center = transform.position;

        switch (ZoneType)
        {
            case RoofboltZoneType.Centered:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                //SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z);
                break;
            case RoofboltZoneType.East:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                break;
            case RoofboltZoneType.West:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                break;
            case RoofboltZoneType.North:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                break;
            case RoofboltZoneType.South:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                break;
            case RoofboltZoneType.EastWestStraight:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                break;
            case RoofboltZoneType.NorthSouthStraight:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                break;
            default:
                break;
        }
        
        List<Vector3> points = new List<Vector3>();
        List<GameObject> spawnedBolts = new List<GameObject>();
        //Testing bolt pattern square first
        if (EdgePointLocal == Vector3.zero)
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;


            while (jInBounds)
            {
                iInBounds = true;
                
                while (iInBounds)
                {
                    Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, BoltSpacingDistance / 2 + j * BoltSpacingDistance);
                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;
                            continue;
                        }
                    }
                    else
                    {
                        points.Add(one);
                        GameObject bolt1 = Instantiate(RoofBoltPrefab);
                        //bolt1.transform.parent = transform;
                        bolt1.transform.position = one;
                        spawnedBolts.Add(bolt1);
                    }
                    Vector3 two = transform.position + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    points.Add(two);
                    GameObject bolt2 = Instantiate(RoofBoltPrefab);
                    //bolt2.transform.parent = transform;
                    bolt2.transform.position = two;
                    spawnedBolts.Add(bolt2);
                    Vector3 three = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    points.Add(three);
                    GameObject bolt3 = Instantiate(RoofBoltPrefab);
                    //bolt3.transform.parent = transform;
                    bolt3.transform.position = three;
                    spawnedBolts.Add(bolt3);
                    Vector3 four = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, BoltSpacingDistance / 2 + j * BoltSpacingDistance);
                    points.Add(four);
                    GameObject bolt4 = Instantiate(RoofBoltPrefab);
                    //bolt4.transform.parent = transform;
                    bolt4.transform.position = four;
                    spawnedBolts.Add(bolt4);
                    i++;
                    //test
                    if (i > 50)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 50)
                {
                    jInBounds = false;
                }
            }
        }
        else
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;
            bool flatLastRow = false;
            while (jInBounds)
            {
                iInBounds = true;
                PairedBolts boltPairCache = new PairedBolts();
                boltPairCache.Bolts = new Transform[2];
                while (iInBounds)
                {
                    //Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    Vector3 one = transform.TransformPoint(EdgePointLocal) + transform.right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - transform.forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);

                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;

                            //Do stuff with paired bolt cache!
                            CurtainReceiver r1 = boltPairCache.Bolts[0].GetComponent<CurtainReceiver>();
                            CurtainReceiver r2 = boltPairCache.Bolts[1].GetComponent<CurtainReceiver>();
                            r1.enabled = true;
                            r2.enabled = true;
                            if (r1 != null && r2 != null)
                            {
                                r1.PairedReceiver = r2;
                                r2.PairedReceiver = r1;                                
                            }
                            continue;
                        }
                    }
                    else
                    {
                        points.Add(one);
                        //Spawn bolt, add to cache!
                        GameObject bolt1 = Instantiate(RoofBoltPrefab);
                        //bolt1.transform.parent = transform;
                        bolt1.transform.position = one;
                        boltPairCache.Bolts[0] = bolt1.transform;
                        spawnedBolts.Add(bolt1);
                    }
                    Vector3 two = transform.TransformPoint(EdgePointLocal) - transform.right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - transform.forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);

                    points.Add(two);

                    GameObject bolt2 = Instantiate(RoofBoltPrefab);
                    //bolt2.transform.parent = transform;
                    bolt2.transform.position = two;
                    boltPairCache.Bolts[1] = bolt2.transform;
                    spawnedBolts.Add(bolt2);
                    i++;
                    //test
                    if (i > 50)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 50)
                {
                    jInBounds = false;
                }
            }
        }

        foreach (GameObject bolt in spawnedBolts)
        {
            CurtainReceiver cr = bolt.GetComponent<CurtainReceiver>();
            if(cr == null)
            {
                continue;
            }
            if (!cr.enabled)
            {
                Destroy(cr);
            }
        }
        int incrementCounter = 10;
        for (int i = 0; i < spawnedBolts.Count; i++)
        {
            spawnedBolts[i].transform.Rotate(0, incrementCounter, 0, Space.Self);

            incrementCounter = -incrementCounter;
            if(incrementCounter > 0)
            {
                incrementCounter--;
            }
            else
            {
                incrementCounter++;
            }

            if(incrementCounter == 0)
            {
                incrementCounter = 10;
            }
        }

        if (ExtraBolts.Length > 0)
        {
            foreach (PairedBolts pair in ExtraBolts)
            {
                if(pair.Bolts[0] == null)
                {
                    continue;
                }
                GameObject bolt1 = Instantiate(RoofBoltPrefab);
                //bolt1.transform.parent = pair.Bolts[0];

                if (pair.Bolts[1] == null)
                {
                    continue;
                }
                
            }
        }
        if(BoltList == null)
        {
            BoltList = new List<GameObject>();
        }
        BoltList.Clear();
       
        foreach (var bolt in spawnedBolts)
        {
            BoltList.Add(bolt);
            
            bolt.transform.parent = _boltParent.transform;
        }
    }
    */
    /*
    public void GenerateBolts(out List<GameObject> bolts)
    {
        List<GameObject> spawnedBolts = new List<GameObject>();
        if (BoltSpacingDistance == 0)
        {
            Debug.Log("Bolt space was zero! Returning. . .");
            bolts = new List<GameObject>();
            return;
        }
        //ClearExistingBolts();

        Bounds container = BoltContainment;

        Vector3 right, forward;

        container.center = transform.position;
        container.extents = new Vector3(BoltContainment.extents.x * MineSegmentParent.localScale.x, 1000, BoltContainment.extents.z * MineSegmentParent.localScale.z);
        SmallerBound.center = transform.position;

        switch (ZoneType)
        {
            case RoofboltZoneType.Centered:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                //SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z);
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
            case RoofboltZoneType.East:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                right = new Vector3(0, 0, -1);
                forward = new Vector3(1, 0, 0);
                break;
            case RoofboltZoneType.West:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                right = new Vector3(0, 0, 1);
                forward = new Vector3(-1, 0, 0);
                break;
            case RoofboltZoneType.North:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
            case RoofboltZoneType.South:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                right = new Vector3(-1, 0, 0);
                forward = new Vector3(0, 0, -1);
                break;
            case RoofboltZoneType.EastWestStraight:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                right = new Vector3(0, 0, -1);
                forward = new Vector3(1, 0, 0);
                break;
            case RoofboltZoneType.NorthSouthStraight:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
            default:
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
        }

        List<Vector3> points = new List<Vector3>();

        //Testing bolt pattern square first
        //if (EdgePointLocal == Vector3.zero)
        if (ZoneType == RoofboltZoneType.Centered)
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;


            while (jInBounds)
            {
                iInBounds = true;

                while (iInBounds)
                {
                    Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, BoltSpacingDistance / 2 + j * BoltSpacingDistance);
                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;
                            continue;
                        }
                    }
                    else
                    {
                        points.Add(one);
                        GameObject bolt1 = Instantiate(RoofBoltPrefab);
                        //bolt1.transform.parent = transform;
                        bolt1.transform.position = one;
                        spawnedBolts.Add(bolt1);
                    }
                    Vector3 two = transform.position + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    points.Add(two);
                    GameObject bolt2 = Instantiate(RoofBoltPrefab);
                    //bolt2.transform.parent = transform;
                    bolt2.transform.position = two;
                    spawnedBolts.Add(bolt2);
                    Vector3 three = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    points.Add(three);
                    GameObject bolt3 = Instantiate(RoofBoltPrefab);
                    //bolt3.transform.parent = transform;
                    bolt3.transform.position = three;
                    spawnedBolts.Add(bolt3);
                    Vector3 four = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, BoltSpacingDistance / 2 + j * BoltSpacingDistance);
                    points.Add(four);
                    GameObject bolt4 = Instantiate(RoofBoltPrefab);
                    //bolt4.transform.parent = transform;
                    bolt4.transform.position = four;
                    spawnedBolts.Add(bolt4);
                    i++;
                    //test
                    if (i > 50)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 50)
                {
                    jInBounds = false;
                }
            }
        }
        else
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;
            bool flatLastRow = false;

            var startPoint = EdgePointLocal;

            if (ZoneType == RoofboltZoneType.EastWestStraight)
                startPoint.x = BoltContainment.max.x;
            else if (ZoneType == RoofboltZoneType.NorthSouthStraight)
                startPoint.z = BoltContainment.max.z;

            startPoint = transform.TransformPoint(startPoint);

            while (jInBounds)
            {
                iInBounds = true;
                PairedBolts boltPairCache = new PairedBolts();
                boltPairCache.Bolts = new Transform[2];
                while (iInBounds)
                {
                    //Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    Vector3 one = startPoint + right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);

                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;

                            if (ZoneType != RoofboltZoneType.Centered)
                            {
                                //Do stuff with paired bolt cache!
                                CurtainReceiver r1 = boltPairCache.Bolts[0].GetComponent<CurtainReceiver>();
                                CurtainReceiver r2 = boltPairCache.Bolts[1].GetComponent<CurtainReceiver>();
                                r1.enabled = true;
                                r2.enabled = true;
                                if (r1 != null && r2 != null)
                                {
                                    r1.PairedReceiver = r2;
                                    r2.PairedReceiver = r1;
                                }
                            }                            
                            continue;
                        }
                    }
                    else
                    {
                        points.Add(one);
                        //Spawn bolt, add to cache!
                        GameObject bolt1 = Instantiate(RoofBoltPrefab);
                        //bolt1.transform.parent = transform;
                        bolt1.transform.position = one;
                        boltPairCache.Bolts[0] = bolt1.transform;
                        spawnedBolts.Add(bolt1);
                    }
                    Vector3 two = startPoint - right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);

                    points.Add(two);

                    GameObject bolt2 = Instantiate(RoofBoltPrefab);
                    //bolt2.transform.parent = transform;
                    bolt2.transform.position = two;
                    boltPairCache.Bolts[1] = bolt2.transform;
                    spawnedBolts.Add(bolt2);
                    i++;
                    //test
                    if (i > 50)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 50)
                {
                    jInBounds = false;
                }
            }
        }

        foreach (GameObject bolt in spawnedBolts)
        {
            CurtainReceiver cr = bolt.GetComponent<CurtainReceiver>();
            if (cr == null)
            {
                continue;
            }
            if (!cr.enabled)
            {
                Destroy(cr);
            }
        }
        int incrementCounter = 10;
        for (int i = 0; i < spawnedBolts.Count; i++)
        {
            spawnedBolts[i].transform.Rotate(0, incrementCounter, 0, Space.Self);

            incrementCounter = -incrementCounter;
            if (incrementCounter > 0)
            {
                incrementCounter--;
            }
            else
            {
                incrementCounter++;
            }

            if (incrementCounter == 0)
            {
                incrementCounter = 10;
            }
        }

        if (ExtraBolts.Length > 0)
        {
            foreach (PairedBolts pair in ExtraBolts)
            {
                if (pair.Bolts[0] == null)
                {
                    continue;
                }
                GameObject bolt1 = Instantiate(RoofBoltPrefab);
                //bolt1.transform.parent = pair.Bolts[0];

                if (pair.Bolts[1] == null)
                {
                    continue;
                }

            }
        }
        if (BoltList == null)
        {
            BoltList = new List<GameObject>();
        }
        BoltList.Clear();
        
        foreach (var bolt in spawnedBolts)
        {
            BoltList.Add(bolt);
            bolt.transform.parent = _boltParent.transform;
            
        }
        bolts = BoltList;
    }
    */

    private void ClearExistingBolts()
    {
        Debug.Log("Destroying existing bolts.");
        int i = 0;
        RoofBolt[] allBolts = MineSegmentParent.GetComponentsInChildren<RoofBolt>();
        foreach(RoofBolt bolt in allBolts)
        {
            Destroy(bolt.gameObject);
            i++;
        }
        Debug.Log($"{i} Bolts should be destroyed.");
    }

    private void Awake()
    {
        _boltParent = new GameObject();
        _boltParent.name = "DynamicBolts";
        _boltParent.layer = LayerMask.NameToLayer("RoofBolts");
    }

    private void OnDestroy()
    {
        Destroy(_boltParent);
    }

    public IEnumerable<PairedBoltPositions> GenerateBoltPositions()
    {
        if (HasCornerCurtainOption)
        {
            CornerCurtains = new List<PairedBoltPositions>();
            foreach (var item in ExtraBolts)
            {
                PairedBoltPositions pbp = new PairedBoltPositions(item.Bolts[0].position, item.Bolts[1].position, true);
                if (CornerCurtains == null)
                {
                    CornerCurtains = new List<PairedBoltPositions>();

                }
                CornerCurtains.Add(pbp);
            }
        }
        if (BoltSpacingDistance == 0)
        {
            Debug.Log("Bolt space was zero! Returning. . .");
            yield break;
        }

        Vector3 right, forward, origin;
        float width, depth;
        Bounds bounds = BoltContainment;
        bounds.center = transform.position;
        bounds.extents = new Vector3(BoltContainment.extents.x * MineSegmentParent.localScale.x, 1000, BoltContainment.extents.z * MineSegmentParent.localScale.z);

        bool generatePairs = true;

        if (ZoneType == RoofboltZoneType.Centered)
            generatePairs = false;


        switch (ZoneType)
        {
            default:
            case RoofboltZoneType.North:
            case RoofboltZoneType.South:
            case RoofboltZoneType.NorthSouthStraight:
            case RoofboltZoneType.Centered:
                origin = new Vector3(bounds.min.x, transform.position.y, bounds.min.z);

                //offset origin to align with the global inby grid
                int globalRow = (int)Mathf.Ceil(origin.z / BoltSpacingDistance);
                origin.z = BoltSpacingDistance * (float)globalRow;

                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                width = bounds.max.x - bounds.min.x;
                depth = bounds.max.z - origin.z;

                foreach (var boltPair in GenerateBoltPositions(origin, forward, right, width, depth, BoltRibOffset, BoltSpacingDistance, generatePairs))
                    yield return boltPair;

                break;

            case RoofboltZoneType.East:
            case RoofboltZoneType.West:
            case RoofboltZoneType.EastWestStraight:
                origin = new Vector3(bounds.min.x, transform.position.y, bounds.max.z);
                right = new Vector3(0, 0, -1);
                forward = new Vector3(1, 0, 0);
                width = bounds.max.z - bounds.min.z;
                depth = bounds.max.x - bounds.min.x;
                bool generateFirstRow = true;
                bool generateLastRow = true;

                float crosscutOffset = BoltSpacingDistance - BoltRibOffset - 0.1524f;
                if (crosscutOffset < 0)
                    crosscutOffset = 0;

                if (ZoneType == RoofboltZoneType.East)
                {
                    //East zones start in an intersection and end in a straight / face
                    //generateLastRow = false; 
                    origin += forward * crosscutOffset;
                    depth -= (crosscutOffset + 0.5f * BoltSpacingDistance);
                }
                else if (ZoneType == RoofboltZoneType.West)
                {
                    //West zones start in a straight / face and end in an intersection                    
                    //generateFirstRow = false; 
                    origin += forward * (BoltSpacingDistance * 0.5f);
                    depth -= (crosscutOffset + 0.5f * BoltSpacingDistance);
                }
                else
                {
                    //start and end the center section 1/2 bolt spacing distance from the ends
                    origin += forward * (BoltSpacingDistance * 0.5f);
                    depth -= BoltSpacingDistance;
                }

                foreach (var boltPair in GenerateBoltPositionsEvenDistribution(origin, forward, right, width, depth, BoltRibOffset, BoltSpacingDistance, 
                    generatePairs, generateFirstRow, generateLastRow))
                    yield return boltPair;
                break;
            
        }

        
        
    }

    private IEnumerable<PairedBoltPositions> GenerateBoltPositions(Vector3 origin, Vector3 forward, Vector3 right, float width, float depth, float ribOffset, float boltSpacingMax, bool generatePairs)
    {
        var pos = origin;

        while (Vector3.Distance(pos, origin) <= depth)
        {
            foreach (var boltPair in GenerateBoltRow(pos, forward, right, width, depth, ribOffset, BoltSpacingDistance, generatePairs))
                yield return boltPair;

            pos += forward * boltSpacingMax;
        }
    }

    private IEnumerable<PairedBoltPositions> GenerateBoltPositionsEvenDistribution(Vector3 origin, Vector3 forward, Vector3 right, float width, float depth, float ribOffset, float boltSpacingMax, 
        bool generatePairs, bool generateFirstRow = true, bool generateLastRow = true)
    {
        var pos = origin;

        int numRows = (int)(Mathf.Ceil(depth / boltSpacingMax) + 1);
        var boltSpacing = depth / (float)(numRows-1);

        int startRow = 0;
        if (!generateFirstRow)
            startRow = 1;
        if (!generateLastRow)
            numRows--;

        for (int i = startRow; i < numRows; i++)
        {
            pos = origin + boltSpacing * (float)i * forward;

            foreach (var boltPair in GenerateBoltRow(pos, forward, right, width, depth, ribOffset, BoltSpacingDistance, generatePairs))
                yield return boltPair;            
        }
    }

    private IEnumerable<PairedBoltPositions> GenerateBoltRow(Vector3 origin, Vector3 forward, Vector3 right, float width, float depth, float ribOffset, float boltSpacingMax, bool generatePairs)
    {
        //generate the two rib-adjacent bolts
        var bolt1 = origin + right * ribOffset;
        var bolt2 = origin + right * (width - ribOffset);

        if (generatePairs)
            yield return new PairedBoltPositions(bolt1, bolt2, true);
        else
        {
            yield return new PairedBoltPositions(bolt1, Vector3.zero, false);
            yield return new PairedBoltPositions(bolt2, Vector3.zero, false);
        }

        var centerWidth = width - (ribOffset * 2);

        //calculate number of additional bolts to not exceed bolt spacing
        var numBolts = (int)Mathf.Floor(centerWidth / boltSpacingMax);

        if (numBolts <= 0)
            yield break;

        var boltSpacing = centerWidth / ((float)(numBolts + 1));

        for (int i = 0; i < numBolts; i++)
        {
            yield return new PairedBoltPositions(bolt1 + right * (boltSpacing * (i + 1)), Vector3.zero, false);
        }
    }

    private void CalculateSmallerBounds(out Vector3 right, out Vector3 forward)
    {
        Bounds container = BoltContainment;        

        container.center = transform.position;
        container.extents = new Vector3(BoltContainment.extents.x * MineSegmentParent.localScale.x, 1000, BoltContainment.extents.z * MineSegmentParent.localScale.z);
        SmallerBound.center = transform.position;

        switch (ZoneType)
        {
            case RoofboltZoneType.Centered:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                //SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z);
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
            case RoofboltZoneType.East:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                right = new Vector3(0, 0, -1);
                forward = new Vector3(1, 0, 0);
                break;
            case RoofboltZoneType.West:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                right = new Vector3(0, 0, 1);
                forward = new Vector3(-1, 0, 0);
                break;
            case RoofboltZoneType.North:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
            case RoofboltZoneType.South:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                right = new Vector3(-1, 0, 0);
                forward = new Vector3(0, 0, -1);
                break;
            case RoofboltZoneType.EastWestStraight:
                SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
                right = new Vector3(0, 0, -1);
                forward = new Vector3(1, 0, 0);
                break;
            case RoofboltZoneType.NorthSouthStraight:
                SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
            default:
                right = new Vector3(1, 0, 0);
                forward = new Vector3(0, 0, 1);
                break;
        }
    }

    public IEnumerable<PairedBoltPositions> GenerateBoltPositionsOld()
    {
        if (BoltSpacingDistance == 0)
        {
            Debug.Log("Bolt space was zero! Returning. . .");
            yield break;
        }

        Vector3 right, forward;
        CalculateSmallerBounds(out right, out forward);

        //Testing bolt pattern square first
        //if (EdgePointLocal == Vector3.zero)
        if (ZoneType == RoofboltZoneType.Centered)
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;


            while (jInBounds)
            {
                iInBounds = true;

                while (iInBounds)
                {
                    Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, BoltSpacingDistance / 2 + j * BoltSpacingDistance);
                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;
                            continue;
                        }
                    }
                    else
                    {
                        yield return new PairedBoltPositions(one, Vector3.zero, false);
                    }
                    Vector3 two = transform.position + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    yield return new PairedBoltPositions(two, Vector3.zero, false);


                    Vector3 three = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    yield return new PairedBoltPositions(three, Vector3.zero, false);

                    Vector3 four = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, BoltSpacingDistance / 2 + j * BoltSpacingDistance);
                    yield return new PairedBoltPositions(four, Vector3.zero, false);

                    i++;
                    //test
                    if (i > 50)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 50)
                {
                    jInBounds = false;
                }
            }
        }
        else
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;

            var startPoint = EdgePointLocal;

            if (ZoneType == RoofboltZoneType.EastWestStraight)
                startPoint.x = BoltContainment.max.x;
            else if (ZoneType == RoofboltZoneType.NorthSouthStraight)
                startPoint.z = BoltContainment.max.z;

            startPoint = transform.TransformPoint(startPoint);

            while (jInBounds)
            {
                iInBounds = true;
                //PairedBolts boltPairCache = new PairedBolts();
                //boltPairCache.Bolts = new Transform[2];
                Vector3 one = Vector3.zero, two = Vector3.zero;
                Vector3 prevOne = Vector3.zero, prevTwo = Vector3.zero;

                while (iInBounds)
                {
                    prevOne = one;
                    prevTwo = two;

                    //Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    one = startPoint + right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);
                    two = startPoint - right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);

                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;

                            //output last two positions as a pair
                            yield return new PairedBoltPositions(prevOne, prevTwo, true);
                            continue;
                        }
                    }

                    if (i != 0)
                    {
                        //output last two positions independantly since the next points are still in bounds
                        yield return new PairedBoltPositions(prevOne, Vector3.zero, false);
                        yield return new PairedBoltPositions(prevTwo, Vector3.zero, false);
                    }

                    



                    i++;
                    //test
                    if (i > 50)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 50)
                {
                    jInBounds = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Bounds container = BoltContainment;
        //Gizmos.DrawLine(RearLeft.position, FrontLeft.position);
        //Gizmos.DrawLine(FrontLeft.position, FrontRight.position);
        //Gizmos.DrawLine(FrontRight.position, RearRight.position);
        //Gizmos.DrawLine(RearRight.position, RearLeft.position);

        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(RearLeft.position, FrontRight.position);
        //Gizmos.DrawLine(FrontLeft.position, RearRight.position);

        container.center = transform.position;
        container.extents = new Vector3(BoltContainment.extents.x * MineSegmentParent.localScale.x, BoltContainment.extents.y, BoltContainment.extents.z * MineSegmentParent.localScale.z);
        //SmallerBound.center = transform.position;

        Vector3 right, forward;
        CalculateSmallerBounds(out right, out forward);

        //switch (ZoneType)
        //{
        //    case RoofboltZoneType.Centered:
        //        SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //        //SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z);
        //        break;
        //    case RoofboltZoneType.East:
        //        SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //        break;
        //    case RoofboltZoneType.West:
        //        SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //        break;
        //    case RoofboltZoneType.North:
        //        SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
        //        break;
        //    case RoofboltZoneType.South:
        //        SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
        //        break;
        //    case RoofboltZoneType.EastWestStraight:
        //        SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //        break;
        //    case RoofboltZoneType.NorthSouthStraight:
        //        SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
        //        break;
        //    default:
        //        break;
        //}
        //if (EdgePointLocal == Vector3.zero)
        //{   
        //    SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //}
        //else
        //{
        //    if(transform.localPosition.z > 0)
        //    {
        //        SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
        //    }
        //    else if(transform.localPosition.z < 0)
        //    {
        //        SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z);
        //    }
        //    else if(transform.localPosition.x > 0)
        //    {
        //        SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //    }
        //    else if(transform.localPosition.x < 0)
        //    {
        //        SmallerBound.extents = new Vector3(container.extents.x, container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //    }
        //    else
        //    {
        //        SmallerBound.extents = new Vector3(container.extents.x - (BoltSpacingDistance / 4), container.extents.y, container.extents.z - (BoltSpacingDistance / 4));
        //    }
        //}
        // bottom
        var p1 = new Vector3(container.min.x, container.min.y, container.min.z);
        var p2 = new Vector3(container.max.x, container.min.y, container.min.z);
        var p3 = new Vector3(container.max.x, container.min.y, container.max.z);
        var p4 = new Vector3(container.min.x, container.min.y, container.max.z);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);


        // top
        var p5 = new Vector3(container.min.x, container.max.y, container.min.z);
        var p6 = new Vector3(container.max.x, container.max.y, container.min.z);
        var p7 = new Vector3(container.max.x, container.max.y, container.max.z);
        var p8 = new Vector3(container.min.x, container.max.y, container.max.z);

        Gizmos.DrawLine(p5, p6);
        Gizmos.DrawLine(p6, p7);
        Gizmos.DrawLine(p7, p8);
        Gizmos.DrawLine(p8, p5);

        // sides
        Gizmos.DrawLine(p1, p5);
        Gizmos.DrawLine(p2, p6);
        Gizmos.DrawLine(p3, p7);
        Gizmos.DrawLine(p4, p8);


        //Smaller bound draw?
        Gizmos.color = Color.cyan;

        float y = transform.position.y;

        // bottom
        p1 = new Vector3(SmallerBound.min.x, y, SmallerBound.min.z);
        p2 = new Vector3(SmallerBound.max.x, y, SmallerBound.min.z);
        p3 = new Vector3(SmallerBound.max.x, y, SmallerBound.max.z);
        p4 = new Vector3(SmallerBound.min.x, y, SmallerBound.max.z);

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);


        // top
        p5 = new Vector3(SmallerBound.min.x, y, SmallerBound.min.z);
        p6 = new Vector3(SmallerBound.max.x, y, SmallerBound.min.z);
        p7 = new Vector3(SmallerBound.max.x, y, SmallerBound.max.z);
        p8 = new Vector3(SmallerBound.min.x, y, SmallerBound.max.z);

        Gizmos.DrawLine(p5, p6);
        Gizmos.DrawLine(p6, p7);
        Gizmos.DrawLine(p7, p8);
        Gizmos.DrawLine(p8, p5);

        // sides
        Gizmos.DrawLine(p1, p5);
        Gizmos.DrawLine(p2, p6);
        Gizmos.DrawLine(p3, p7);
        Gizmos.DrawLine(p4, p8);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(EdgePointLocal), 0.1f);

        Gizmos.color = Color.green;

        foreach (var item in GenerateBoltPositions())
        {
            if (item.HasPairedBolt)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawCube(item.Pos1, new Vector3(0.12f, 0.01f, 0.12f));
                Gizmos.DrawCube(item.Pos2, new Vector3(0.12f, 0.01f, 0.12f));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(item.Pos1, new Vector3(0.1f, 0.03f, 0.1f));
            }
        }

        if (CornerCurtains != null && HasCornerCurtainOption)
        {
            foreach (var item in CornerCurtains)
            {
                if (item.HasPairedBolt)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(item.Pos1, new Vector3(0.12f, 0.01f, 0.12f));
                    Gizmos.DrawCube(item.Pos2, new Vector3(0.12f, 0.01f, 0.12f));
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(item.Pos1, new Vector3(0.1f, 0.03f, 0.1f));
                }
            }
        }
        /*
        List<Vector3> points = new List<Vector3>();
        //Testing bolt pattern square first
        if (EdgePointLocal == Vector3.zero)
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;
            

            while (jInBounds)
            {
                iInBounds=true;
                while (iInBounds)
                {
                    Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance/2 + i*BoltSpacingDistance, 0, BoltSpacingDistance/2 + j*BoltSpacingDistance);
                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;
                            continue;
                        }
                    }
                    else
                    {
                        points.Add(one);
                    }
                    Vector3 two = transform.position + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    points.Add(two);
                    Vector3 three = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    points.Add(three);
                    Vector3 four = transform.position + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, BoltSpacingDistance / 2 + j * BoltSpacingDistance);
                    points.Add(four);
                    i++;
                    //test
                    if (i > 10)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 10)
                {
                    jInBounds = false;
                }
            }
        }
        else
        {
            int i = 0;
            int j = 0;
            bool iInBounds = true;
            bool jInBounds = true;
            bool flatLastRow = false;

            while (jInBounds)
            {
                iInBounds = true;
                while (iInBounds)
                {
                    //Vector3 one = transform.TransformPoint(EdgePointLocal) + new Vector3(BoltSpacingDistance / 2 + i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    Vector3 one = transform.TransformPoint(EdgePointLocal) + transform.right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - transform.forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);
                    
                    if (!SmallerBound.Contains(one))
                    {
                        if (i == 0)
                        {
                            iInBounds = false;
                            jInBounds = false;
                            continue;
                        }
                        else
                        {
                            j++;
                            i = 0;
                            continue;
                        }
                    }
                    else
                    {
                        points.Add(one);
                    }
                    //Vector3 two = transform.TransformPoint(EdgePointLocal) + new Vector3(-BoltSpacingDistance / 2 - i * BoltSpacingDistance, 0, -BoltSpacingDistance / 2 - j * BoltSpacingDistance);
                    Vector3 two = transform.TransformPoint(EdgePointLocal) - transform.right * ((BoltSpacingDistance / 2) + i * BoltSpacingDistance) - transform.forward * ((BoltSpacingDistance / 2) + j * BoltSpacingDistance);

                    points.Add(two);
                    
                    
                    i++;
                    //test
                    if (i > 10)
                    {
                        iInBounds = false;
                    }
                }
                if (j > 10)
                {
                    jInBounds = false;
                }
            }
        }

        foreach (var item in points)
        {
            Gizmos.DrawCube(item, new Vector3(0.1f, 0.01f, 0.1f));
        }*/
    }
}
