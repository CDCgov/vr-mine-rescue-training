using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LifelineMarkerGenerator))]
public class LifelineMarkerGeneratorEditor : Editor {

    public LLMarkerPlacementMode mode = LLMarkerPlacementMode.None;
    public List<Vector3> Points;
    public Transform LGeneratorTransform;
    public LifelineMarkerGenerator LGenenerator;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();        
        LifelineMarkerGenerator lGen = (LifelineMarkerGenerator)target;
        if(lGen.MandoorPrefab == null)
        {
            PopulatePrefabFields(lGen);
        }
        LGenenerator = lGen;
        LGeneratorTransform = lGen.transform;
        lGen.CableGeometryRef = lGen.gameObject.GetComponent<HangingGeometry>();
        HangingGeometry hGeo = lGen.CableGeometryRef;
        HangingGeometryEditor hGeEditor = Resources.FindObjectsOfTypeAll<HangingGeometryEditor>()[0];
        hGeEditor.SetHangPosition(2);
        
        float _markerDistance = 0;
        float _tagDistance = 0;
        if (GUILayout.Button("Generate Direction Markers"))
        {
            hGeo.RegenerateMesh();            
            List<Vector3> points = hGeo.GetSmoothedPoints();
            lGen.ReferencePoints = points;
            GameObject sPointsParent = new GameObject();
            sPointsParent.name = "Markers";
            sPointsParent.transform.parent = lGen.transform;
            sPointsParent.transform.localPosition = Vector3.zero;
            //Debug.Log(points.Count);

            if (lGen.IsBranchLine)
            {
                GameObject branchMarker = (GameObject)PrefabUtility.InstantiatePrefab(lGen.BranchLinePrefab);
                Vector3 branchDir = points[4] - points[3];
                branchMarker.transform.parent = GetIndvMarkerParent();
                branchMarker.transform.localPosition = points[3];
                branchMarker.transform.localRotation = Quaternion.LookRotation(branchDir);
                LifelineItem lItem = branchMarker.GetComponent<LifelineItem>();
                lItem.LifeLineGenRef = LGenenerator;
                lItem.SavedClosestPoint = points[3];
                lItem.ClosestPointIndex = 3;
            }

            for (int i = 0; i < (points.Count-1); i++)
            {
                _markerDistance += Vector3.Distance(points[i], points[i + 1]);
                if(_markerDistance >= lGen.MarkerDistance)
                {
                    Vector3 direction = points[i + 1] - points[i];
                    GameObject newMarker = (GameObject)PrefabUtility.InstantiatePrefab(lGen.MarkerPrefab);
                    newMarker.transform.parent = sPointsParent.transform;
                    newMarker.transform.localPosition = points[i + 1];
                    newMarker.transform.rotation = Quaternion.LookRotation(direction);
                    LifelineItem markerComponent = newMarker.GetComponent<LifelineItem>();
                    markerComponent.SavedClosestPoint = points[i + 1];
                    markerComponent.ClosestPointIndex = i + 1;
                    markerComponent.LifeLineGenRef = lGen;                    
                    _markerDistance = 0;
                }
            }
        }
        if (GUILayout.Button("Generate Tags"))
        {
            hGeo.RegenerateMesh();
            List<Vector3> points = hGeo.GetSmoothedPoints();
            lGen.ReferencePoints = points;
            GameObject sPointsParent = new GameObject();
            sPointsParent.name = "Tags";
            sPointsParent.transform.parent = lGen.transform;
            sPointsParent.transform.localPosition = Vector3.zero;
            //Debug.Log(points.Count);
            for (int i = 0; i < (points.Count - 1); i++)
            {
                _tagDistance += Vector3.Distance(points[i], points[i + 1]);
                if (_tagDistance >= lGen.TagDistance)
                {
                    Vector3 direction = points[i + 1] - points[i];
                    GameObject newMarker = (GameObject)PrefabUtility.InstantiatePrefab(lGen.TagPrefab);
                    newMarker.transform.parent = sPointsParent.transform;
                    newMarker.transform.localPosition = points[i + 1];
                    newMarker.transform.rotation = Quaternion.LookRotation(direction);
                    Quaternion rotFix = newMarker.transform.rotation;
                    Vector3 rotEul = rotFix.eulerAngles;
                    rotEul.x = 0;
                    rotEul.z = 0;
                    //rotEul.y = rotEul.y - 180;
                    rotFix = Quaternion.Euler(rotEul);
                    newMarker.transform.rotation = rotFix;
                    LifelineTag markerComponent = newMarker.GetComponent<LifelineTag>();
                    markerComponent.SavedClosestPoint = points[i + 1];
                    markerComponent.ClosestPointIndex = i + 1;
                    markerComponent.LifeLineGenRef = lGen;
                    markerComponent.UpdateTagColor(lGen.EscapewayColor);
                    _tagDistance = 0;
                }
            }
        }
        if(GUILayout.Button("Add Individual Tags"))
        {
            hGeo.RegenerateMesh();
            List<Vector3> points = hGeo.GetSmoothedPoints();
            Points = points;
            LifelineMarkerWindow window = new LifelineMarkerWindow();
            window.Points = points;
            window.Parent = lGen.transform;
            window.LLMarkerGenEditor = this;
            window.CreateWindow();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Add Branch Cable"))
        {
            hGeo.RegenerateMesh();
            List<Vector3> points = hGeo.GetSmoothedPoints();
            Points = points;
            
            mode = LLMarkerPlacementMode.AddBranch;
        }
    }

    public void PopulatePrefabFields(LifelineMarkerGenerator gen)
    {
        string[] searchFolder = { "Assets/Prefabs/StationaryEquipment/Lifeline" };
        string[] guid = AssetDatabase.FindAssets("Lifeline_ManDoor", searchFolder);
        string path = AssetDatabase.GUIDToAssetPath(guid[0]);        
        gen.MandoorPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));

        guid = AssetDatabase.FindAssets("Lifeline_BranchLine", searchFolder);
        path = AssetDatabase.GUIDToAssetPath(guid[0]);
        gen.BranchLinePrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));

        guid = AssetDatabase.FindAssets("Lifeline_DirectionalCone", searchFolder);
        path = AssetDatabase.GUIDToAssetPath(guid[0]);
        gen.MarkerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));

        guid = AssetDatabase.FindAssets("Lifeline_ManDoor", searchFolder);
        path = AssetDatabase.GUIDToAssetPath(guid[0]);
        gen.MandoorPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));

        guid = AssetDatabase.FindAssets("Lifeline_RefugeChamber", searchFolder);
        path = AssetDatabase.GUIDToAssetPath(guid[0]);
        gen.RefugeChamberPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));

        guid = AssetDatabase.FindAssets("Lifeline_SCSRCache", searchFolder);
        path = AssetDatabase.GUIDToAssetPath(guid[0]);
        gen.SCSRCachePrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));

        guid = AssetDatabase.FindAssets("Lifeline_Tag", searchFolder);
        path = AssetDatabase.GUIDToAssetPath(guid[0]);
        gen.TagPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(Object));
    }

    private void OnSceneGUI()
    {        

        GameObject prefabRef = null;
        switch (mode)
        {
            case LLMarkerPlacementMode.MandoorMarker:
                prefabRef = LGenenerator.MandoorPrefab;
                
                break;
            case LLMarkerPlacementMode.RefugeMarker:
                prefabRef = LGenenerator.RefugeChamberPrefab;
                break;
            case LLMarkerPlacementMode.SCSRMarker:
                prefabRef = LGenenerator.SCSRCachePrefab;
                break;
            case LLMarkerPlacementMode.BranchMarker:
                prefabRef = LGenenerator.BranchLinePrefab;
                break;
            case LLMarkerPlacementMode.LifelineTag:
                prefabRef = LGenenerator.TagPrefab;
                break;
            case LLMarkerPlacementMode.LifelineDirection:
                prefabRef = LGenenerator.MarkerPrefab;
                break;
            case LLMarkerPlacementMode.AddBranch:                
                for (int i = 0; i < Points.Count; i++)
                {
                    if (Handles.Button(LGeneratorTransform.TransformPoint(Points[i]), Quaternion.identity, 0.1f, 0.1f, Handles.CubeHandleCap))
                    {
                        EditorUtility.SetDirty(LGenenerator);

                        Transform parent = GetIndvMarkerParent();
                        prefabRef = LGenenerator.BranchLinePrefab;

                        //GameObject marker = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef);
                        GameObject marker2 = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef);
                        //int point1 = i - 3;
                        int point2 = i + 3;
                        //Vector3 direction1;
                        Vector3 direction2;
                        //direction1 = Points[point1 + 1] - Points[point1];
                        direction2 = Points[point2 + 1] - Points[point2];

                        //marker.transform.position = LGeneratorTransform.TransformPoint(Points[point1]);
                        //marker.transform.rotation = Quaternion.LookRotation(direction1);
                        //marker.GetComponent<LifelineItem>().LifeLineGenRef = LGenenerator;
                        //marker.GetComponent<LifelineItem>().ClosestPointIndex = point1;
                        //marker.GetComponent<LifelineItem>().SavedClosestPoint = Points[point1];
                        //marker.transform.parent = parent;

                        marker2.transform.position = LGeneratorTransform.TransformPoint(Points[point2]);
                        marker2.transform.rotation = Quaternion.LookRotation(direction2);
                        marker2.GetComponent<LifelineItem>().LifeLineGenRef = LGenenerator;
                        marker2.GetComponent<LifelineItem>().ClosestPointIndex = point2;
                        marker2.GetComponent<LifelineItem>().SavedClosestPoint = Points[point2];
                        marker2.transform.parent = parent;

                        Transform branches = LGenenerator.transform.Find("Branches");
                        if (branches == null)
                        {
                            GameObject branchesGO = new GameObject();
                            branchesGO.name = "Branches";
                            branchesGO.transform.parent = LGenenerator.transform;
                            branches = branchesGO.transform;
                        }

                        GameObject obj = new GameObject("BranchLine");
                        obj.AddComponent<MeshRenderer>();
                        obj.AddComponent<MeshFilter>();
                        obj.transform.parent = branches;


                        HangingCable cable = obj.AddComponent<HangingCable>();
                        HangingCable sourceCable = LGenenerator.GetComponent<HangingCable>();
                        LifelineMarkerGenerator gen = obj.AddComponent<LifelineMarkerGenerator>();
                        gen.IsBranchLine = true;
                        PopulatePrefabFields(gen);
                        gen.CableGeometryRef = cable;
                        gen.MarkerDistance = LGenenerator.MarkerDistance;
                        gen.TagDistance = LGenenerator.TagDistance;
                        gen.EscapewayColor = LGenenerator.EscapewayColor;
                        if (sourceCable != null)
                        {
                            cable.CableDiameter = sourceCable.CableDiameter;
                            cable.CableHangerWidth = sourceCable.CableHangerWidth;
                            cable.DefaultCableSlope = sourceCable.DefaultCableSlope;
                            cable.CableMaterial = sourceCable.CableMaterial;
                        }

                        cable.AppendNode(LGeneratorTransform.TransformPoint(Points[i]), false);

                        var selection = new Object[1];
                        selection[0] = cable.gameObject;
                        Selection.objects = selection;
                        mode = LLMarkerPlacementMode.None;
                    }
                }                
                break;
            case LLMarkerPlacementMode.None:
                break;
            default:
                break;
        }

        if(mode != LLMarkerPlacementMode.None && mode != LLMarkerPlacementMode.AddBranch)
        {
            for (int i = 0; i < Points.Count; i++)
            {               
                if (Handles.Button(LGeneratorTransform.TransformPoint(Points[i]), Quaternion.identity, 0.1f, 0.1f, Handles.CubeHandleCap))
                {
                    Transform parent = GetIndvMarkerParent();
                    GameObject marker = (GameObject)PrefabUtility.InstantiatePrefab(prefabRef);
                    Vector3 direction;
                    if (i < Points.Count - 1)
                    {
                        direction = Points[i + 1] - Points[i];
                    }
                    else
                    {
                        direction = Points[i] - Points[i - 1];
                    }

                    marker.transform.position = LGeneratorTransform.TransformPoint(Points[i]);
                    marker.transform.rotation = Quaternion.LookRotation(direction);
                    marker.transform.parent = parent;

                    if(mode == LLMarkerPlacementMode.LifelineTag)
                    {
                        LifelineTag llTag = marker.GetComponent<LifelineTag>();
                        llTag.UpdateTagColor(LGenenerator.EscapewayColor);
                    }

                    mode = LLMarkerPlacementMode.None;
                    LifelineItem llItem = marker.GetComponent<LifelineItem>();
                    llItem.LifeLineGenRef = LGenenerator;
                    llItem.ClosestPointIndex = i;
                    llItem.SavedClosestPoint = marker.transform.localPosition;
                }
            }
        }
    }

    Transform GetIndvMarkerParent()
    {
        Transform parent = LGeneratorTransform.Find("IndividualMarkers");
        if (parent == null)
        {
            GameObject par = new GameObject();
            par.transform.position = LGeneratorTransform.position;
            par.transform.parent = LGeneratorTransform;
            par.name = "IndividualMarkers";
            parent = par.transform;
        }

        return parent;
    }
}

public enum LLMarkerPlacementMode
{
    MandoorMarker,
    RefugeMarker,
    SCSRMarker,
    BranchMarker,
    LifelineTag,
    LifelineDirection,
    AddBranch,
    None
}
public class LifelineMarkerWindow : EditorWindow
{
    public List<Vector3> Points;
    public Transform Parent;
    public LifelineMarkerGeneratorEditor LLMarkerGenEditor;
    //private LLMarkerPlacementMode mode = LLMarkerPlacementMode.None;
    public void CreateWindow()
    {
        LifelineMarkerWindow window = (LifelineMarkerWindow)GetWindow(typeof(LifelineMarkerWindow), true, "Add Tags to Lifeline");
        window.minSize = new Vector2(400, 400);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Add Individual Tag Mode");
        if (GUILayout.Button("Add Mandoor Marker"))
        {
            LLMarkerGenEditor.mode = LLMarkerPlacementMode.MandoorMarker;
            this.Close();
        }
        if (GUILayout.Button("Add Refuge Chamber Marker"))
        {
            LLMarkerGenEditor.mode = LLMarkerPlacementMode.RefugeMarker;
            this.Close();
        }
        if (GUILayout.Button("Add SCSR Cache Marker"))
        {
            LLMarkerGenEditor.mode = LLMarkerPlacementMode.SCSRMarker;
            this.Close();
        }
        if (GUILayout.Button("Add Branch Line Marker"))
        {
            LLMarkerGenEditor.mode = LLMarkerPlacementMode.BranchMarker;
            this.Close();
        }
        if (GUILayout.Button("Add Directional Marker"))
        {
            LLMarkerGenEditor.mode = LLMarkerPlacementMode.LifelineDirection;
            this.Close();
        }
        if (GUILayout.Button("Add Tag Marker"))
        {
            LLMarkerGenEditor.mode = LLMarkerPlacementMode.LifelineTag;
            this.Close();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Close"))
        {            
            this.Close();
        }
    }    
}