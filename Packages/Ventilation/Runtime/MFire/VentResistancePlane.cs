using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class VentResistancePlane : MonoBehaviour, ISelectableObject
{
    public VentilationManager VentilationManager;

    public float PlaneWidth;
    public float PlaneHeight;
    public Vector3 PlaneOffset = Vector3.zero;
    public float AddedResistance = 0;
    public bool UseDefaultResistance = false;

    private Vector3 _lastPosition = Vector3.zero;
    private List<VentAirway> _airways;
    private HashSet<VentAirway> _airwayHashSet;

    //private List<int> _airwayResults = new List<int>();
    //private List<float> _airwayDistances = new List<float>();
    //private HashSet<int> _airwaySet = new HashSet<int>();


    // Start is called before the first frame update
    void Start()
    {
        if (UseDefaultResistance)
        {
            SystemManager systemManager = SystemManager.GetDefault();
            if (systemManager != null && systemManager.SystemConfig != null)
                AddedResistance = systemManager.SystemConfig.DefaultCurtainResistance;
        }
    }

    void OnEnable()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);


        VentilationManager.VentilationWillUpdate += OnVentilationWillUpdate;
        VentilationManager.VentGraphReset += OnVentGraphReset;
    }

    private void OnVentGraphReset(VentGraph obj)
    {
        _airwayHashSet = null;
        _airways = null;
    }

    void OnDisable()
    {
        if (VentilationManager != null)
            VentilationManager.VentilationWillUpdate -= OnVentilationWillUpdate;
    }

    public virtual void UpdateControlResistance()
    {
        //do nothing by default;
    }

    private void OnVentilationWillUpdate(VentGraph obj)
    {
        if (_airwayHashSet == null || _airways == null || _lastPosition != transform.position)
        {
            UpdateAffectedAirways();
        }

        UpdateControlResistance();

        foreach (var airway in _airways)
        {
            airway.ControlResistance += AddedResistance;
        }
    }

    void UpdateAffectedAirways()
    {
        _lastPosition = transform.position;

        if (_airways == null)
            _airways = new List<VentAirway>();
        if (_airwayHashSet == null)
            _airwayHashSet = new HashSet<VentAirway>();

        _airways.Clear();
        _airwayHashSet.Clear();

        var ventGraph = VentilationManager.GetVentilationGraph();
        if (ventGraph == null)
            return;

        //_airwayResults.Clear();
        //_airwayDistances.Clear();
        //_airwaySet.Clear();

        //if (!ventGraph.FindNearbyAirways(transform.position, 12, _airwayResults, _airwayDistances))
        //    return;

        //foreach (int airwayIndex in _airwayResults)
        //{
        //    _airwaySet.Add(airwayIndex);
        //}

        //foreach (int airwayIndex in _airwaySet)
        //{
        //    ventGraph.FindAirway(airwayIndex);
        //}

        if (!ventGraph.FindNearbyAirways(transform.position, 12, _airwayHashSet))
            return;

        Plane plane = new Plane(transform.forward, transform.position + PlaneOffset);
        var planeW = transform.right;
        var planeH = transform.up;

        foreach (var airway in _airwayHashSet)
        {
            var v1 = airway.Start.WorldPosition;
            var v2 = airway.End.WorldPosition;

            var dir = v2 - v1;
            var length = dir.magnitude;
            dir.Normalize();

            Ray r = new Ray(v1, dir);
            float dist;

            if (!plane.Raycast(r, out dist))
                continue;

            if (dist > length)
                continue;

            //get vector from plane origin to raycast hit
            var rayHitPos = v1 + dir * dist;
            var v = (transform.position + PlaneOffset) - rayHitPos;

            //compute horizontal and vertical distance component
            var w = Mathf.Abs(Vector3.Dot(v, planeW));
            var h = Mathf.Abs(Vector3.Dot(v, planeH));

            var scale = transform.localScale;

            if (w < (PlaneWidth * scale.x) && h < (PlaneHeight * scale.y))
            {
                Debug.Log($"Airway {airway.AirwayID} intersects vent plane {gameObject.name} w:{w:F1} h:{h:F1}");
                _airways.Add(airway);
            }
            
        }
    }

    void OnDrawGizmosSelected()
    {
        var oldMat = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(new Vector3(0, 0, 0) + PlaneOffset, new Vector3(PlaneWidth*2.0f, PlaneHeight*2.0f, 0.1f));
    }

    public string GetObjectDisplayName()
    {
        return "Vent Resist Plane";
    }

    public void GetObjectInfo(StringBuilder sb)
    {

    }

}
