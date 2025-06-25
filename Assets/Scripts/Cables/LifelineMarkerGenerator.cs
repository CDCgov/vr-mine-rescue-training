using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HangingGeometry))]
public class LifelineMarkerGenerator : MonoBehaviour {

    //Units are meters, per unity standard. Max for a cone is 100ft
    [Range(0.5f, 30.48f)]
    public float MarkerDistance = 2;
    [Range(0.5f, 7.62f)]
    public float TagDistance = 3;
    public Color EscapewayColor = Color.green;
    public TagColor EscapeColor = TagColor.Green;

    public HangingGeometry CableGeometryRef;

    public GameObject MarkerPrefab;
    public GameObject TagPrefab;
    public GameObject MandoorPrefab;
    public GameObject BranchLinePrefab;
    public GameObject RefugeChamberPrefab;
    public GameObject SCSRCachePrefab;

    public bool IsBranchLine = false;

    [HideInInspector]
    public List<Vector3> ReferencePoints;
    

    private float _MarkerDistance = 0;
    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    public void GenerateDirectionalCones()
    {
        //float slope = CableGeometryRef.DefaultCableSlope;
        //List<HangingGeometry.CableNode> nodes = CableGeometryRef.CableNodes;

        for (int i = 0; i < (CableGeometryRef.CableNodes.Count-1); i++)
        {
            //_MarkerDistance = 
        }
    }

    private void OnValidate()
    {
        foreach(Transform t in transform)
        {
            if(t.name == "Tags" && t.parent == transform)
            {
                foreach(Transform child in t)
                {
                    LifelineTag tag = child.GetComponent<LifelineTag>();
                    if(tag != null)
                    {
                        tag.UpdateTagColor(EscapewayColor);
                    }
                }
            }
        }
    }
}
