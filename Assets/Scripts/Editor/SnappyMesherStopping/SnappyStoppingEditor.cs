using UnityEngine;
using UnityEditor;

public class SnappyStoppingEditor : EditorWindow
{
    // Toggle value for Add Stopping Mode radio buttons.
    private int _checkOut = 0;

    // User options for stopping.
    private bool _frontFoamOpt = false;
    private bool _backFoamOpt = false;
    private int _manDoorOpt = 0;

    // Public gameobject for preview stopping.
    public GameObject HoldStop;

    // Transform for target stopping position. 
    private Transform _stopPos;
    private RaycastHit _stopHit;

    private GameObject _previewInstance;
    private int _raycastMask;

    // Add Unity Editor menu item.
    [MenuItem("Create Mine/Add Snappy Stoppings", priority = 600)]

    // Create stopping editor window at initialization.
    static void Init()
    {
        SnappyStoppingEditor stoppingWindow = (SnappyStoppingEditor)EditorWindow.GetWindow(typeof(SnappyStoppingEditor));
        stoppingWindow.Show();
    }

    private void Awake()
    {
        _raycastMask = LayerMask.GetMask("Floor");
        SceneView.duringSceneGui += SceneGUI;
    }

    private void OnDestroy()
    {
        DestroyPreview();
        _checkOut = 0;
        //SceneView.onSceneGUIDelegate -= SceneGUI;
        SceneView.duringSceneGui -= SceneGUI;
    }

    // GUI controls.
    private void OnGUI() 
    {
        GUILayout.Label("Stopping Foam Options", EditorStyles.boldLabel);
        _frontFoamOpt = GUILayout.Toggle(_frontFoamOpt, "Add Foam to Front");
        _backFoamOpt = GUILayout.Toggle(_backFoamOpt, "Add Foam to Back");
        GUILayout.Label("Man Door Options", EditorStyles.boldLabel);
        var manText = new string[] { "No Man Door", "Front-Facing Man Door", "Back-Facing Man Door" };
        _manDoorOpt = GUILayout.SelectionGrid(_manDoorOpt, manText, 1, EditorStyles.radioButton);
        GUILayout.Label("Toggle Add Stopping Mode", EditorStyles.boldLabel);
        var radioText = new string[] { "Add Stopping Mode Off", "Add Stopping Mode On" };
        // Enable add stopping mode.
        if ((_checkOut = GUILayout.SelectionGrid(_checkOut, radioText, 1, EditorStyles.radioButton)) == 1)
        {
            //SceneView.onSceneGUIDelegate += SceneGUI;
        }
        else
            DestroyPreview();

        if (GUILayout.Button("Build Snappy Stopping"))
        {
            ProcBuild();
        }
    }

    // Allows clicking on scene to add preview stopping.
    public void SceneGUI(SceneView sceneView)
    {
        // Check to disable Add Stopping Mode.   
        if (_checkOut == 0)
        {
            //SceneView.onSceneGUIDelegate -= SceneGUI;
            return;
        }

        // Capture mouse event->left-click release.
        Event checkMouse = Event.current;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        if (checkMouse.button == 0 && checkMouse.type == EventType.MouseUp)
        {
            // Raycast from camera position and check for straight tile collision.
            var stopRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(stopRay, out RaycastHit hit, 500, _raycastMask) /* && hit.collider.gameObject.name.Contains("traight") */)
            {
                _stopHit = hit;
                // Check for existing preview stopping and removal if found.
                //var checkStop = GameObject.Find("PreviewStopping(Clone)");
                //if (checkStop != null)
                //{
                //    DestroyImmediate(checkStop);
                //}
                // Assign raycast collision point to stopping options(stopOpt).
                DestroyPreview();

                float nsDist = float.MaxValue;
                float ewDist = float.MaxValue;

                var pos = hit.point + Vector3.up;

                if (Physics.Raycast(pos, hit.transform.forward, out RaycastHit nsHit, 500, _raycastMask))
                {
                    nsDist = nsHit.distance;
                    Debug.Log($"SnappyStoppingEditor: NS Distance {nsDist:F2}");
                }

                if (Physics.Raycast(pos, hit.transform.right, out RaycastHit ewHit, 500, _raycastMask))
                {
                    ewDist = ewHit.distance;
                    Debug.Log($"SnappyStoppingEditor: EW Distance {ewDist:F2}");
                }

                if (Physics.Raycast(pos, hit.transform.forward * -1, out RaycastHit nsHit2, 500, _raycastMask))
                {
                    nsDist += nsHit2.distance;
                }
                else
                    nsDist += 500;

                if (Physics.Raycast(pos, hit.transform.right * -1, out RaycastHit ewHit2, 500, _raycastMask))
                {
                    ewDist += ewHit2.distance;
                }
                else
                    nsDist += 500;

                //if (hit.collider.gameObject.name.Contains("NS"))
                if (nsDist > ewDist)
                {
                    _previewInstance = Instantiate(HoldStop, new Vector3(hit.transform.position.x, hit.transform.position.y, hit.point.z), hit.transform.rotation);
                }
                else //if (hit.collider.gameObject.name.Contains("EW"))
                {
                    _previewInstance = Instantiate(HoldStop, new Vector3(hit.point.x, hit.transform.position.y, hit.transform.position.z), hit.transform.rotation * Quaternion.Euler(0, 90, 0));
                }
            }
        }
    }


    private void DestroyPreview()
    {
        //_currentTile = null;
        if (_previewInstance != null)
        {
            DestroyImmediate(_previewInstance);
            _previewInstance = null;
        }
    }

    private void ProcBuild()
    {
        // Confirm preview stopping exists.
        //var checkStop = GameObject.Find("PreviewStopping(Clone)");
        var checkStop = _previewInstance;
        if (checkStop != null)
        {
            // Add preview stopping transform to stopping options(stopOpt).
            _stopPos = checkStop.transform;
            // Add procedural script to preview stopping.
            SnappyMesherStopping updateStop = checkStop.AddComponent<SnappyMesherStopping>();
            // Create Foam Options class foamOpt.
            FoamOptions foamOpt = updateStop.SnapStop(_frontFoamOpt, _backFoamOpt, _manDoorOpt, _stopHit, _stopPos);
            // Add procedural foam script to stopping.
            SnappyMesherFoam updateFoam = checkStop.AddComponent<SnappyMesherFoam>();
            // Check options and run proc foam script for stopping front if requested.
            if (_frontFoamOpt == true)
            {
                updateFoam.SnapFoam(-0.098425f, 0, foamOpt.stopTrans, foamOpt.stopTrans, foamOpt.frontFoamLeft, foamOpt.frontFoamTop, foamOpt.frontFoamRight);
            }
            // Check options and run proc foam script for stopping back if requested.
            if (_backFoamOpt == true)
            {
                foamOpt.backFoamTop.Reverse();
                updateFoam.SnapFoam(0.098425f, 180, foamOpt.stopTrans, foamOpt.stopTrans, foamOpt.backFoamRight, foamOpt.backFoamTop, foamOpt.backFoamLeft);
            }
        }
      
        // Temp fix for new game object in hierarchy.
        var garbageObj = GameObject.Find("New Game Object");
        if (garbageObj != null)
        {
            DestroyImmediate(garbageObj);
        }
        DestroyImmediate(checkStop);

    }
   
}