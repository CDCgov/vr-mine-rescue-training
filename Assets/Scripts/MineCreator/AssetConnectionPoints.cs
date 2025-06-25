using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class AssetConnectionPoints : MonoBehaviour
{
    public Transform[] ConnectionPointList;
    //public string[] ConnectionPointIDs;
    public Dictionary<string, Transform> ConnectionPoints;

    public bool UniversalConnections = false;
    // Start is called before the first frame update
    void Start()
    {
        ConnectionPoints = new Dictionary<string, Transform>();
        for(int i = 0; i < ConnectionPointList.Length; i++)
        {
            ConnectionPoints.Add(ConnectionPointList[i].name, ConnectionPointList[i]);
        }
    }    

    private void OnDrawGizmosSelected()
    {
        if(ConnectionPointList == null)
        {
            return;
        }
        for (int i = 0; i < ConnectionPointList.Length; i++)
        {
            //Vector3 pos = transform.TransformPoint(ConnectionPointList[i]);
            Vector3 pos = ConnectionPointList[i].position;
            Gizmos.DrawSphere(pos, 0.1f);
            //Vector3 pos = ConnectionPointList[i];
            pos.y -= 0.1f;
#if UNITY_EDITOR
            Handles.Label(pos, ConnectionPointList[i].name);
#endif
        }        
    }
}
