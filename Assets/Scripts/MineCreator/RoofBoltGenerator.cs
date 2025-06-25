using NIOSH_MineCreation;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Script to place on a mine tile prefab to generate roof bolts based on spacing
/// </summary>
public class RoofBoltGenerator : MonoBehaviour
{
    public GameObject RoofBoltPrefab;
    public float RoofBoltSpacing;
    public float BoltRibOffset = 1.067f;
    public bool BoltsGenerated = false;
    public bool EnableCornerCurtains = false;

    public List<RoofBoltGeneratorZone> Zones;
    public MineBuilder BuilderScript;

    private List<GameObject> _bolts;

    private List<Vector3> _hitPositions;
    private List<Vector3> _hitNormals;

    //private List<Vector3> _raycastOrigins;
    //private List<Vector3> _raycastDirections;
    //private List<Vector3> _colliderOrigins;
    private List<Collider> _colliders;

    private PlacablePrefab _placablePrefab;

    private void Awake()
    {
        _bolts = new List<GameObject>();

        _hitPositions = new List<Vector3>();
        _hitNormals = new List<Vector3>();
        //_raycastOrigins = new List<Vector3>();
        //_raycastDirections = new List<Vector3>();
        //_colliderOrigins = new List<Vector3>();
        _colliders = new List<Collider>();
    }

    private void Start()
    {

        var settings = ScenarioSaveLoad.Settings;
        if (settings != null && settings.MineSettings != null)
        {
            RoofBoltSpacing = settings.MineSettings.BoltSpacing;
            BoltRibOffset = settings.MineSettings.BoltRibOffset;
            //Debug.Log($"Setting bolt spacing for: {gameObject.name} spacing is {settings.MineSettings.BoltSpacing}");
            EnableCornerCurtains = settings.MineSettings.EnableCornerCurtains;
        }

        MineCreator mc = MineCreator.GetDefault();
        if (mc != null)
        {
            BuilderScript = mc.GetBuilderRef();
        }
        if(BuilderScript != null)
        {
            //RoofBoltSpacing = mc.BoltSpacing;
            BuilderScript.onBuildComplete += GenerateBolts;
            //Debug.Log("Builder scirpt event watcher made");
        }
        //else
        //{
        //    Debug.Log("Mine creator was null");
        //}

        _placablePrefab = GetComponent<PlacablePrefab>();
        _placablePrefab.OnPlaced += GenerateBolts;
        ScenarioSaveLoad.Instance.MineSettingsChanged += Instance_MineSettingsChanged;

        //GenerateBolts();
    }

    private void Instance_MineSettingsChanged()
    {
        //Debug.Log($"Generator zone detected setting change!");
        bool generateBolts = false;
        Debug.Log($"Checking for bolt space changes: {RoofBoltSpacing} and {ScenarioSaveLoad.Settings.MineSettings.BoltSpacing}");
        if(RoofBoltSpacing != ScenarioSaveLoad.Settings.MineSettings.BoltSpacing)
        {
            Debug.Log($"Bolt spacing change detected!{RoofBoltSpacing} vs {ScenarioSaveLoad.Settings.MineSettings.BoltSpacing}");
            RoofBoltSpacing = (float)ScenarioSaveLoad.Settings.MineSettings.BoltSpacing;
            generateBolts = true;
            BoltsGenerated = false;
            
        }

        if (BoltRibOffset != ScenarioSaveLoad.Settings.MineSettings.BoltRibOffset)
        {
            Debug.Log($"Bolt offset change detected!{BoltRibOffset} vs {ScenarioSaveLoad.Settings.MineSettings.BoltRibOffset}");
            BoltRibOffset = (float)ScenarioSaveLoad.Settings.MineSettings.BoltRibOffset;
            generateBolts = true;
            BoltsGenerated = false;
            
        }

        if(EnableCornerCurtains != ScenarioSaveLoad.Settings.MineSettings.EnableCornerCurtains)
        {
            EnableCornerCurtains = ScenarioSaveLoad.Settings.MineSettings.EnableCornerCurtains;
            generateBolts = true;
            BoltsGenerated = false;
        }

        if (generateBolts)
        {
            GenerateBolts();
        }
    }

    private void OnDestroy()
    {
        if(BuilderScript != null)
        {            
            BuilderScript.onBuildComplete -= GenerateBolts;
        }
        if(_placablePrefab != null)
        {
            _placablePrefab.OnPlaced -= GenerateBolts;
        }
        if (ScenarioSaveLoad.Instance != null)
            ScenarioSaveLoad.Instance.MineSettingsChanged -= Instance_MineSettingsChanged;
    }

    private void GenerateBolts(bool bolts)
    {        
        Debug.Log($"{gameObject.name} placed script called, making bolts @ {Time.frameCount}");
        //GenerateBolts();
        StartCoroutine(DelayedBoltSpawn(true));
    }

    IEnumerator DelayedBoltSpawn(bool bolts)
    {
        yield return new WaitForSeconds(1);
        if (_placablePrefab != null)
        {
            if (_placablePrefab.placed)
            {
                GenerateBolts();
            }
        }
        else
        {
            GenerateBolts();
        }
    }

    private GameObject SpawnBolt(Vector3 pos, Transform parent)
    {
        GameObject bolt1 = Instantiate(RoofBoltPrefab);
        bolt1.transform.SetParent(parent);
        bolt1.transform.position = pos;

        return bolt1;
    }

    private void SpawnBoltPair(Vector3 pos1, Vector3 pos2, Transform parent, out GameObject bolt1, out GameObject bolt2)
    {
        bolt1 = SpawnBolt(pos1, parent);
        bolt2 = SpawnBolt(pos2, parent);

        var recv1 = bolt1.GetComponent<CurtainReceiver>();
        var recv2 = bolt2.GetComponent<CurtainReceiver>();

        recv1.enabled = true;
        recv2.enabled = true;

        recv1.PairedReceiver = recv2;
        recv2.PairedReceiver = recv1;
    }
    
    public void GenerateBolts()
    {
        if (BoltsGenerated)
        {
            return;
        }

        ClearExistingBolts();
        if(RoofBoltSpacing == 0)
        {
            return;
        }

        Physics.SyncTransforms();

        foreach (RoofBoltGeneratorZone zone in Zones)
        {
            zone.RoofBoltPrefab = RoofBoltPrefab;
            zone.BoltSpacingDistance = RoofBoltSpacing;
            zone.BoltRibOffset = BoltRibOffset;
            //zone.GenerateBolts();
            //List<GameObject> newBolts = new List<GameObject>();
            //zone.GenerateBolts(out newBolts);
            //_bolts.AddRange(newBolts);

            foreach (var boltPair in zone.GenerateBoltPositions())
            {
                if (boltPair.HasPairedBolt)
                {
                    SpawnBoltPair(boltPair.Pos1, boltPair.Pos2, zone.BoltParent, out var bolt1, out var bolt2);
                    _bolts.Add(bolt1);
                    _bolts.Add(bolt2);
                }
                else
                {
                    var bolt = SpawnBolt(boltPair.Pos1, zone.BoltParent);
                    CurtainReceiver cr = bolt.GetComponent<CurtainReceiver>();
                    if(cr != null)
                    {
                        Destroy(cr);
                    }
                    _bolts.Add(bolt);
                }
            }

            if (EnableCornerCurtains)
            {
                if (zone.HasCornerCurtainOption)
                {
                    if(zone.CornerCurtains != null)
                    {
                        foreach (var item in zone.CornerCurtains)
                        {
                            if (item.HasPairedBolt)
                            {
                                SpawnBoltPair(item.Pos1, item.Pos2, zone.BoltParent, out var bolt1, out var bolt2);
                                _bolts.Add(bolt1);
                                _bolts.Add(bolt2);
                            }
                            else
                            {
                                var bolt = SpawnBolt(item.Pos1, zone.BoltParent);
                                CurtainReceiver cr = bolt.GetComponent<CurtainReceiver>();
                                if (cr != null)
                                {
                                    Destroy(cr);
                                }
                                _bolts.Add(bolt);
                            }
                        }
                    }
                }
            }
        }
        

        //Now do the raycast stuff. . .
        var results = new NativeArray<RaycastHit>(_bolts.Count, Allocator.TempJob);

        var commands = new NativeArray<RaycastCommand>(_bolts.Count, Allocator.TempJob);
        //int layerMask = 1 << LayerMask.NameToLayer("Floor");
        int layerMask = LayerMask.GetMask("Floor");
        var queryParameters = new QueryParameters(layerMask, false, QueryTriggerInteraction.Collide, false);
        float distance = 3;
        var roofBoltLayer = LayerMask.NameToLayer("RoofBolts");

        for (int i = 0; i < _bolts.Count; i++)
        {            
            commands[i] = new RaycastCommand(_bolts[i].transform.position, _bolts[i].transform.up, queryParameters, distance);
            //_raycastOrigins.Add(_bolts[i].transform.position);
            //_raycastDirections.Add(_bolts[i].transform.up);
        }
        var handle = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));
        handle.Complete();

        //Debug.Log($"Raycast results count: {results.Length}");
        List<GameObject> objectsToDestroy = new List<GameObject>();
        int nullCount = 0;
        List<string> colliderNames = new List<string>();
        List<GameObject> badCastObjects = new List<GameObject>();
        List<int> cachedIndex = new List<int>();
        List<GameObject> boltsToDelete = new List<GameObject>();
        int priorIndex = -1;
        
        var roofBoltBlockers = GameObject.FindObjectsOfType<RoofBoltBlocker>();
        //List<Bounds> roofBoltBlockerZones = new List<Bounds>(roofBoltBlockers.Length);
        //foreach (var blocker in roofBoltBlockers)
        //{
        //    if (!blocker.TryGetComponent<Collider>(out var collider))
        //        continue;

        //    roofBoltBlockerZones.Add(collider.bounds);
        //}

        for (int i = 0; i < _bolts.Count; i++)
        {
            
            if(i >= results.Length)
            {
                //objectsToDestroy.Add(_bolts[i]);
                continue;
            }

            if (results[i].collider == null)
            {
                nullCount++;
                
            }
            else if (results[i].collider.gameObject != null &&
                results[i].collider.gameObject.layer == roofBoltLayer)
            {
                //Debug.Log($"RoofBoltGenerator: Removing roof bolt due to collider on {results[i].collider.gameObject.name}");
                boltsToDelete.Add(_bolts[i]);
            }
            else
            {                
                _bolts[i].transform.position = results[i].point;
                _hitPositions.Add(results[i].point);
                //Vector3 avgNormal = (hit.normal + hit1.normal + hit2.normal + hit3.normal + hit4.normal) / 5;
                _colliders.Add(results[i].collider);
                //float rotationVal = transform.localEulerAngles.y;
                //if (rotationVal > 180)
                //{
                //    rotationVal = rotationVal - 360;
                //}
                //_bolts[i].transform.eulerAngles = results[i].normal * -1;

                //_bolts[i].transform.rotation = Quaternion.FromToRotation(_bolts[i].transform.eulerAngles,results[i].normal * -1);
                _bolts[i].transform.rotation = Quaternion.FromToRotation(Vector3.up, results[i].normal * -1);
                _hitNormals.Add(results[i].normal);
                //transform.eulerAngles = avgNormal * -1;
                //transform.Rotate(0, Random.Range(-10, 10), 0, Space.Self);
                int index = priorIndex * i;
                priorIndex = index;
                priorIndex *= -1;
                float rotationVal = index % 45;

                _bolts[i].transform.Rotate(0, rotationVal, 0, Space.Self);
                _bolts[i].transform.Translate(0, -0.015f, 0, Space.Self);
                if(results[i].collider.CompareTag("Hazard"))
                {
                    boltsToDelete.Add(_bolts[i]);
                }
            }

            //foreach (var blockerZone in roofBoltBlockerZones)
            //{
            //    if (blockerZone.Contains(_bolts[i].transform.position))
            //    {
            //        boltsToDelete.Add(_bolts[i]);
            //    }
            //}

            foreach (var blocker in roofBoltBlockers)
            {
                if (!blocker.TryGetComponent<BoxCollider>(out var collider))
                    continue;

                var bounds = new Bounds(collider.center, collider.size);
                var pt = collider.transform.InverseTransformPoint(_bolts[i].transform.position);

                if (bounds.Contains(pt))
                {
                    boltsToDelete.Add(_bolts[i]);
                }
            }

        }



        //foreach (var item in colliderNames)
        //{
        //    Debug.Log($"Collided with: {item}");
        //}

        //Debug.Log($"Null hit count was: {nullCount}");
        int corrections = 0;
        int total = 0;
        foreach (var bolt in badCastObjects)
        {
            RaycastHit newHit;
            
            if (Physics.Raycast(bolt.transform.position, bolt.transform.up, out newHit, 2, layerMask))
            {
                bolt.transform.position = newHit.point;
                float rotationVal = transform.localEulerAngles.y;
                if (rotationVal > 180)
                {
                    rotationVal = rotationVal - 360;
                }
                bolt.transform.eulerAngles = newHit.normal * -1;
                bolt.transform.Rotate(0, rotationVal, 0, Space.Self);
                corrections++;
            }
            total++;
        }
        //Debug.Log($"Performed {corrections} corrections out of {total}.");

        foreach (var bolt in boltsToDelete)
        {
            CurtainReceiver cr = bolt.GetComponent<CurtainReceiver>();
            if(cr != null)
            {
                //Destroys the paired curtain receiver component if it exists.
                if(cr.PairedReceiver != null)
                    Destroy(cr.PairedReceiver);
            }
            Destroy(bolt);
        }
        boltsToDelete.Clear();
        //if(objectsToDestroy.Count > 0)
        //{
        //    foreach (var item in objectsToDestroy)
        //    {
        //        Destroy(item);
        //    }
        //}

        BoltsGenerated = true;

        //Process verts
        //Debug.Log("Process verts");
        //for (int i = 0; i < 1; i++)
        //{
        //    MeshFilter mf = _bolts[i].GetComponentInChildren<MeshFilter>();
        //    Vector3[] verts = mf.mesh.vertices;
        //    foreach (var vert in verts)
        //    {
        //        Debug.Log($"Vertex: {vert.ToString()}");
        //    }
        //}

        results.Dispose();
        commands.Dispose();
    }

    private void ClearExistingBolts()
    {
        if (_bolts == null)
            return;

        foreach(GameObject obj in _bolts)
        {
            Destroy(obj);
        }
        _bolts.Clear();
        _hitPositions.Clear();
        _hitNormals.Clear();
        _colliders.Clear();
        RoofBolt[] allBolts = transform.GetComponentsInChildren<RoofBolt>();
        foreach (RoofBolt bolt in allBolts)
        {
            bolt.gameObject.SetActive(false);
            Destroy(bolt.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        if (_hitPositions != null)
        {
            foreach (var hit in _hitPositions)
            {
                Gizmos.DrawSphere(hit, 0.01f);
            }
        }

        Gizmos.color = Color.cyan;
        if (_hitPositions != null && _hitNormals != null)
        {
            for (int i = 0; i < _hitNormals.Count; i++)
            {
                Gizmos.DrawRay(_hitPositions[i], _hitNormals[i] * -1);
            }
        }
        //Gizmos.color = Color.red;
        //if(_raycastOrigins == null || _raycastDirections == null)
        //{
        //    return;
        //}
        //for (int i = 0; i < _raycastOrigins.Count; i++)
        //{
        //    Gizmos.DrawRay(_raycastOrigins[i], _raycastDirections[i]);
        //}
    }
}
