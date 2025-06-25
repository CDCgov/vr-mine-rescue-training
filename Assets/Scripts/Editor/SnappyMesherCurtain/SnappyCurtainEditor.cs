using UnityEngine;
using UnityEditor;

public class SnappyCurtainEditor : EditorWindow
{
    //private bool _addCurt = false;
    private bool _addCurtainMode = false;
    private int _curtOpt = 0;
    private int _curtOptSide = 0;
    private int _curtOptTop = 0;
    public GameObject PreviewCurtain;
    private RaycastHit _curtHit;

    private string[] _noYesStrings;
    private string[] _curtainStyles;

    private GameObject _previewInstance;
    private int _raycastMask;
    private GameObject _currentTile;


    [MenuItem("Create Mine/Add Snappy Curtains", priority = 600)]
    static void Init()
    {
        SnappyCurtainEditor curtainWindow = (SnappyCurtainEditor)EditorWindow.GetWindow(typeof(SnappyCurtainEditor));
        curtainWindow.Show();        
    }

    private void Awake()
    {
        Debug.Log($"SnappyCurtainEditor Awake");

        _curtainStyles = new string[] { "Clear_Flypad", "ClearCurtain_Single", "YellowCurtain_Single", "Curtain_Multi-Brattice", "Curtain_Single-Brattice" };
        _noYesStrings = new string[] { "No", "Yes" };

        _raycastMask = LayerMask.GetMask("Floor");

        SceneView.duringSceneGui += SceneGUI;
    }

    private void OnDestroy()
    {
        Debug.Log($"SnappyCurtainEditor OnDestroy");
        //_addCurtainMode = 0;
        //SceneView.onSceneGUIDelegate -= SceneGUI;
        SceneView.duringSceneGui -= SceneGUI;

        DestroyPreviewCurtain();
    }


    private void OnGUI()
    {
        //var curtText = new string[] { "Clear_Flypad", "ClearCurtain_Single", "YellowCurtain_Single", "Curtain_Multi-Brattice", "Curtain_Single-Brattice" };
        //var curtSideText = new string[] { "No", "Yes" };
        //var curtTopText = new string[] { "No", "Yes" };
        //var radioText = new string[] { "Add Curtain Mode Off", "Add CurtainMode On" };


        GUILayout.Label("Curtain Style", EditorStyles.boldLabel);        
        _curtOpt = GUILayout.SelectionGrid(_curtOpt, _curtainStyles, 1, EditorStyles.radioButton);

        GUILayout.Label("Curtain Options", EditorStyles.boldLabel);
        GUILayout.Label("Add gaps on left and right sides of curtain?");
        _curtOptSide = GUILayout.SelectionGrid(_curtOptSide, _noYesStrings, 1, EditorStyles.radioButton);
        GUILayout.Label("Add gaps on top and bottom of curtain?");
        _curtOptTop = GUILayout.SelectionGrid(_curtOptTop, _noYesStrings, 1, EditorStyles.radioButton);

        GUILayout.Label("Add Curtain Mode", EditorStyles.boldLabel);
        //_addCurtainMode = GUILayout.SelectionGrid(_addCurtainMode, radioText, 1, EditorStyles.radioButton)) == 1);
        _addCurtainMode = GUILayout.Toggle(_addCurtainMode, "Add Curtain Mode");

        if (!_addCurtainMode)
            DestroyPreviewCurtain();

        if (GUILayout.Button("Build Snappy Curtain"))
        {
            CurtBuild();
        }
    }

    private void DestroyPreviewCurtain()
    {
        _currentTile = null;
        if (_previewInstance != null)
        {
            DestroyImmediate(_previewInstance);
            _previewInstance = null;
        }
    }

    void SceneGUI(SceneView sceneView)
    {
        if (!_addCurtainMode)
        {
            //SceneView.onSceneGUIDelegate -= SceneGUI;
            return;
        }

        Event checkMouse = Event.current;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (checkMouse.button == 0 && checkMouse.type == EventType.MouseUp)
        {
            Debug.Log($"SnappyCurtainEditor: MouseUp");

            var stopRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(stopRay, out RaycastHit hit, 500, _raycastMask))
            {
                Debug.Log($"SnappyCurtainEditor: Raycast hit {hit.collider.gameObject.name}");

                if (hit.collider.gameObject.name.Contains("traight"))
                {
                    _curtHit = hit;
                    //var checkCurt = GameObject.Find("PreviewStopping(Clone)");
                    //if (checkCurt != null)
                    //{
                    //    DestroyImmediate(checkCurt);
                    //}
                    DestroyPreviewCurtain();

                    if (hit.collider.gameObject.name.Contains("NS"))
                    {
                        _previewInstance = Instantiate<GameObject>(PreviewCurtain,
                            new Vector3(hit.transform.position.x, hit.transform.position.y, hit.point.z), hit.transform.rotation);
                    }
                    else if (hit.collider.gameObject.name.Contains("EW"))
                    {
                        _previewInstance = Instantiate<GameObject>(PreviewCurtain,
                            new Vector3(hit.point.x, hit.transform.position.y, hit.transform.position.z), hit.transform.rotation * Quaternion.Euler(0, 90, 0));
                    }
                }
            }
        }
    }
    public void CurtBuild()
    {
        //GameObject checkCurt = GameObject.Find("PreviewStopping(Clone)");
        //if (checkCurt != null)
        //{
        //    var test = checkCurt.AddComponent<SnappyMesherCurtain>();
        //    test.SnapCurt(test.transform, _curtHit, _curtOpt, _curtOptSide, _curtOptTop);
        //}

        if (_previewInstance != null)
        {
            var snappyCurtain = _previewInstance.AddComponent<SnappyMesherCurtain>();
            snappyCurtain.SnapCurt(snappyCurtain.transform, _curtHit, _curtOpt, _curtOptSide, _curtOptTop);
        }

        DestroyImmediate(_previewInstance);
        //temp fix for new game object in hierarchy
        var garbageObj = GameObject.Find("New Game Object");
        if (garbageObj != null)
        {
            DestroyImmediate(garbageObj);
        }
    }
    
}