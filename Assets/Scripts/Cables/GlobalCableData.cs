using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CableType
{
    Lifeline,
    Power,
    Water,
    Curtain,
    Line,
}
public class GlobalCableData : MonoBehaviour
{

    public GameObject directionPrefab;
    public GameObject tagPrefab;
    public GameObject manDoorPrefab;
    public GameObject branchLinePrefab;
    public GameObject refugeChamberPrefab;
    public GameObject SCSRCachePrefab;

    public GameObject cableHangerPrefab;
    public GameObject bratticeHookPrefab;
    public GameObject SingleHookPrefab;
    public GameObject DoubleHookPrefab;
    public GameObject QuickReleasePrefab;

    public Material powerCableMaterial;
    public Material waterCableMaterial;
    public Material lifelinCableMaterial;
    public Material curtainMaterial;
    public Material lineCurtainMaterial;

    public float powerCableThickness;
    public float waterCableThickness;
    public float lifelineCableThickness;
    public float curtainThickness;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        Util.DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void SetMarkerPosition(Transform t, int i, ComponentInfo_Lifeline data)
    //{
    //    // t.position = data.transform.TransformPoint(data.cable.GetSmoothedPoints()[i]);
    //    t.position = data.IndividualMarkerContainer.TransformPoint(data.Cable.GetSmoothedPoints()[i]);

    //}

    //called from Gizmo Button


    public void SetMarkerRotation(Transform t, int i, ComponentInfo_Lifeline data)
    {
        Vector3 direction;
        if (i < data.Cable.GetSmoothedPoints().Count - 1)
        {
            direction = data.Cable.GetSmoothedPoints()[i + 1] - data.Cable.GetSmoothedPoints()[i];
        }
        else
        {
            direction = data.Cable.GetSmoothedPoints()[i - 1] - data.Cable.GetSmoothedPoints()[i];
        }
        t.rotation = Quaternion.LookRotation(direction);

    }
    public Material GetMaterial(CableType type)
    {
        Material _mat;

        switch (type)
        {

            case CableType.Lifeline:
                _mat = lifelinCableMaterial;
                break;
            case CableType.Power:
                _mat = powerCableMaterial;
                break;
            case CableType.Water:
                _mat = waterCableMaterial;
                break;
            case CableType.Curtain:
                _mat = curtainMaterial;
                break;
            case CableType.Line:
                _mat = lineCurtainMaterial;
                break;
            default:
                _mat = lifelinCableMaterial;
                break;
        }
        return _mat;
    }

    public float GetDiameter(CableType type)
    {
        float d;

        switch (type)
        {
            case CableType.Lifeline:
                d = lifelineCableThickness;
                break;
            case CableType.Power:
                d = powerCableThickness;
                break;
            case CableType.Water:
                d = waterCableThickness;
                break;
            default:
                d = lifelineCableThickness;
                break;
        }
        return d;
    }
}
