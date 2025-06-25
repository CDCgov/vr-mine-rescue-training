using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public enum BoltPatternType
{
    FourFoot,
    FiveFoot
}
/// <summary>
/// A light processing burden class for roof bolts. Because they are cloned so many times in the mine, we do not want to have all the methods and properties present in mine element. Might be expanded later for procedural bolting.
/// </summary>
public class RoofBolt : MonoBehaviour 
{
    public float Thickness = 0;

    //TODO: this should be moved to a scriptable object or other referenced class - currently duplicated for all roofbolt instances
    //should exist once per roofbolt prefab/type
    public List<Vector3> HookPositionOffsets; 

    public Vector3 GetHookPositionWorldSpace(int index)
    {
        Vector3 pos = Vector3.zero;

        if (HookPositionOffsets != null && HookPositionOffsets.Count >= 1)
        {
            if (index < 0)
                index = 0;
            if (index >= HookPositionOffsets.Count)
                index = HookPositionOffsets.Count - 1;

            pos = HookPositionOffsets[index];
        }

        return transform.TransformPoint(pos);
    }

    private void Start()
    {
        if (HookPositionOffsets == null || HookPositionOffsets.Count <= 0)
            Reset();
    }

    private void Reset()
    {
        HookPositionOffsets = new List<Vector3>();
        HookPositionOffsets.Add(new Vector3(0.073f, -0.0042f, -0.062f));
        HookPositionOffsets.Add(new Vector3(-0.073f, -0.0042f, 0.062f));
        //HookPositionOffsets.Add(new Vector3(0.07f, -0.0042f, 0));
    }

    private void OnDrawGizmosSelected()
    {
        if (HookPositionOffsets == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < HookPositionOffsets.Count; i++)
        {
            Gizmos.DrawSphere(GetHookPositionWorldSpace(i), 0.005f);
        }
    }

    //public MineSegment MineSegmentRef;
    //public float Health = 1;
    //public bool Indestructible = true;
    //public BoltPatternType Pattern = BoltPatternType.FourFoot;
    //public int HangingObjectLimit = 2;
    //public List<GameObject> HangingGameObjects; //References to objects that are physically hanging onto the roof bolt. Currently set to be a list in case we want to hang more than one item
    //public float Offset = 0;
    //public bool PlaceOnSpawn = false;

    //private MeshFilter _boltMesh;

    //public Vector3 GetPosition()
    //{
    //    return this.transform.position;
    //}

    //public void MoveDownByOffset()
    //{
    //    transform.Translate(new Vector3(0, -Offset, 0), Space.Self);
    //}

    //private void Start()
    //{
    //    _boltMesh = GetComponent<MeshFilter>();
    //    Mesh mesh = _boltMesh.mesh;
    //    if (PlaceOnSpawn)
    //    {
    //        int layerMask = LayerMask.NameToLayer("MineSegment");
    //        RaycastHit hit, hit1, hit2, hit3, hit4;
    //        //if(Physics.Raycast(transform.position, transform.up, out hit,2, layerMask))
    //        //{
    //        //    //Physics.Raycast((transform.position + new Vector3(0.075f, 0, 0.075f)), transform.up, out hit1, 2, layerMask);
    //        //    //Physics.Raycast((transform.position + new Vector3(-0.075f, 0, 0.075f)), transform.up, out hit2, 2, layerMask);
    //        //    //Physics.Raycast((transform.position + new Vector3(0.075f, 0, -0.075f)), transform.up, out hit3, 2, layerMask);
    //        //    //Physics.Raycast((transform.position + new Vector3(-0.075f, 0, -0.075f)), transform.up, out hit4, 2, layerMask);

    //        //    //find min point?
    //        //    float highestY = float.NegativeInfinity;
    //        //    foreach(Vector3 vert in verts)
    //        //    {
    //        //        if(vert.y > highestY)
    //        //        {
    //        //            highestY = vert.y;
    //        //        }
    //        //    }
    //        //    //Debug.Log($"Bolt highest Y vert: {highestY}");


    //        //    transform.position = hit.point;
    //        //    //Vector3 avgNormal = (hit.normal + hit1.normal + hit2.normal + hit3.normal + hit4.normal) / 5;
    //        //    float rotationVal = transform.localEulerAngles.y;
    //        //    if(rotationVal > 180)
    //        //    {
    //        //        rotationVal = rotationVal - 360;
    //        //    }
    //        //    transform.eulerAngles = hit.normal * -1;
    //        //    //transform.eulerAngles = avgNormal * -1;
    //        //    //transform.Rotate(0, Random.Range(-10, 10), 0, Space.Self);
    //        //    transform.Rotate(0, rotationVal, 0, Space.Self);
    //        //    transform.Translate(0, -0.05f, 0, Space.Self);
    //        //}
    //    }
    //}

    /*
    /// <summary>
    /// Returns the List of game objects hanging on this roof bolt.
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetHangingList()
    {
        return HangingGameObjects;
    }
    /// <summary>
    /// Returns a Game Object array of the objects hanging off of the roof bolt.
    /// </summary>
    /// <returns></returns>
    public GameObject[] GetHangingGameObjects()
    {
        GameObject[] hanging = new GameObject[HangingGameObjects.Count];
        for(int i = 0; i < hanging.Length; i++)
        {
            hanging[i] = HangingGameObjects[i];
        }
        return hanging;
    }

    public bool HangGameObject(GameObject goToHang)
    {
        //A check should be placed here to determine if an identical type of object is already on this bolt (i.e. no duplicate spads, reflectors) -BDM
        if (HangingGameObjects.Count < HangingObjectLimit)
        {			
            HangingGameObjects.Add(goToHang);
            return true;
        }
        else
        {
            Debug.Log("Hanging object limit reached on " + gameObject.name);
            return false;
        }
    }

    public void RemoveHangingGameObject(GameObject goToRemove)
    {
        HangingGameObjects.Remove(goToRemove);
        //Call the game object's MEHost Script to do its proper destroy, unless handled elsewhere (say, on interaction script) -BDM
    }

    public static void InsertIntoWorld(Vector3 position, Quaternion rotation)
    {
        GameObject newBolt = Instantiate(Resources.Load("RoofBolt", typeof(GameObject))) as GameObject;
    }

    public void RemoveFromWorld()
    {
        //Destroy any items held by the removed roof bolt
        foreach (GameObject item in HangingGameObjects)
        {
            HangingGameObjects.Remove(item);
            //Call its RemoveFromWorld script
        }
        Destroy(this.gameObject);
    }
    */
    //private void OnDrawGizmosSelected()
    //{
    //    //Gizmos.DrawRay(transform.position, transform.up);
    //    //Gizmos.DrawRay((transform.position + new Vector3(0.075f, 0, 0.075f)), transform.up);
    //    //Gizmos.DrawRay((transform.position + new Vector3(-0.075f, 0, 0.075f)), transform.up);
    //    //Gizmos.DrawRay((transform.position + new Vector3(0.075f, 0, -0.075f)), transform.up);
    //    //Gizmos.DrawRay((transform.position + new Vector3(-0.075f, 0, -0.075f)), transform.up);
    //}
}
