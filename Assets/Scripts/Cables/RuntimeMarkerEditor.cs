using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using NIOSH_EditorLayers;


public class RuntimeMarkerEditor : MonoBehaviour
{
    [SerializeField] GameObject markerGizmo;

    public RuntimeCableEditor RuntimeCableEditor;
    
    public int TagMarkerOffset = 2;// index offset used when populating tags to prevent overlap with directional markers
    public bool HoveringOverMarker;

    public Button addDirectionalButton;
    public Button addTagButton;
    public Button addManDoorButton;
    public Button addBranchLineButton;
    public Button addRefugeChamberButton;
    public Button addSCSRButton;
    public Button flipMarkerButton;
    public Button swapDirectionalButton;
    public Button swapTagButton;
    public Button swapManDoorButton;
    public Button swapBranchLineButton;
    public Button swapRefugeChamberButton;
    public Button swapSCSRButton;
    public Button ColorButton0;
    public Button ColorButton1;
    public Button ColorButton2;
    public Button ColorButton3;
    public Button ColorButton4;

    //public List<Button> ColorButtons = new List<Button>();

    public LifelineItem target;
    //public ComponentInfo_Lifeline _info;

    Button populateTagButton;
    Button populateDirectionButton;
    GlobalCableData globalCableData;
    Camera sceneCamera;  
    LayerManager.EditorLayer editorLayer;

    //store markers so we can move them when gemetry gets updated
    InputTargetController inputTargetController;
    InputTargetController.InputTarget inputTarget;
    ContextMenuController contextMenu;

    RuntimeCableEditor cableEditor;
    GameObject contextMenuUI;

    private Placer _placer;
    
    #region UnityMethods

    void Start()
    {
        if (RuntimeCableEditor == null)
            RuntimeCableEditor = RuntimeCableEditor.GetDefault(gameObject);

        _placer = FindObjectOfType<Placer>();
        globalCableData = FindObjectOfType<GlobalCableData>();
        contextMenu = FindObjectOfType<ContextMenuController>();
        contextMenuUI = contextMenu.contextMenuUI;
        sceneCamera = Camera.main;
        inputTargetController = FindObjectOfType<InputTargetController>();
        cableEditor = GetComponent<RuntimeCableEditor>();
        if (inputTargetController) inputTargetController.onNewInputTarget += OnNewInputTarget;
        LayerManager.Instance.layerChanged += OnNewLayer;
        if (addTagButton) addTagButton.onClick.AddListener(AddTag);
        if (addSCSRButton) addSCSRButton.onClick.AddListener(AddSCSRCache);
        if (addRefugeChamberButton) addRefugeChamberButton.onClick.AddListener(AddRefugeChamber);
        if (addBranchLineButton) addBranchLineButton.onClick.AddListener(AddBranchLine);
        if (addManDoorButton) addManDoorButton.onClick.AddListener(AddManDoor);
        if (addDirectionalButton) addDirectionalButton.onClick.AddListener(AddDirectional);
        if (populateTagButton) populateTagButton.onClick.AddListener(PopulateTag);
        if (populateDirectionButton) populateDirectionButton.onClick.AddListener(PopulateDirection);
        if (flipMarkerButton) flipMarkerButton.onClick.AddListener(FlipTargetMarker);


        if (swapTagButton) swapTagButton.onClick.AddListener(SwapMarkerTag);
        if (swapSCSRButton) swapSCSRButton.onClick.AddListener(SwapMarkerCache);
        if (swapRefugeChamberButton) swapRefugeChamberButton.onClick.AddListener(SwapMarkerRefugeChamber);
        if (swapBranchLineButton) swapBranchLineButton.onClick.AddListener(SwapMarkerBranchLine);
        if (swapManDoorButton) swapManDoorButton.onClick.AddListener(SwapMarkerManDoor);
        if (swapDirectionalButton) swapDirectionalButton.onClick.AddListener(SwapMarkerDirectional);

        ColorButton0.onClick.AddListener(delegate { ChangeTargetTagColor(0); });
        ColorButton1.onClick.AddListener(delegate { ChangeTargetTagColor(1); });
        ColorButton2.onClick.AddListener(delegate { ChangeTargetTagColor(2); });
        ColorButton3.onClick.AddListener(delegate { ChangeTargetTagColor(3); });
        ColorButton4.onClick.AddListener(delegate { ChangeTargetTagColor(4); });

        /*
        for (int i = 0; i < ColorButtons.Count - 1; i++)
        {
            ColorButtons[i].onClick.AddListener(delegate { ChangeTargetTagColor(i); });
        }*/

    }

    private void OnDestroy()
    {

        LayerManager.Instance.layerChanged -= OnNewLayer;
        if (inputTargetController) inputTargetController.onNewInputTarget -= OnNewInputTarget;

        if (addTagButton) addTagButton.onClick.RemoveListener(AddTag);
        if (addSCSRButton) addSCSRButton.onClick.RemoveListener(AddSCSRCache);
        if (addRefugeChamberButton) addRefugeChamberButton.onClick.RemoveListener(AddRefugeChamber);
        if (addBranchLineButton) addBranchLineButton.onClick.RemoveListener(AddBranchLine);
        if (addManDoorButton) addManDoorButton.onClick.RemoveListener(AddManDoor);
        if (addDirectionalButton) addDirectionalButton.onClick.RemoveListener(AddDirectional);

        if (populateTagButton) populateTagButton.onClick.RemoveListener(PopulateTag);
        if (populateDirectionButton) populateDirectionButton.onClick.RemoveListener(PopulateDirection);

        if (flipMarkerButton) flipMarkerButton.onClick.RemoveListener(FlipTargetMarker);

        if (swapTagButton) swapTagButton.onClick.RemoveListener(SwapMarkerTag);
        if (swapSCSRButton) swapSCSRButton.onClick.RemoveListener(SwapMarkerCache);
        if (swapRefugeChamberButton) swapRefugeChamberButton.onClick.RemoveListener(SwapMarkerRefugeChamber);
        if (swapBranchLineButton) swapBranchLineButton.onClick.RemoveListener(SwapMarkerBranchLine);
        if (swapManDoorButton) swapManDoorButton.onClick.RemoveListener(SwapMarkerManDoor);
        if (swapDirectionalButton) swapDirectionalButton.onClick.RemoveListener(SwapMarkerDirectional);
    }

    void Update()
    {
        if (editorLayer == LayerManager.EditorLayer.Cables)
        {
            CheckForMarkerSelection(LayerMask.GetMask("Gizmo", "Default"));
            RepositionMarker();
        }
    }

    private ComponentInfo_Lifeline GetLifelineComponent()
    {
        if (RuntimeCableEditor == null || RuntimeCableEditor.SelectedCable == null)
            return null;

        if (RuntimeCableEditor.SelectedCable.TryGetComponent<ComponentInfo_Lifeline>(out var lifeline))
            return lifeline;

        return null;
    }

    #endregion
    /// <summary>
    /// Check to see if a node is being over and if clicked, select it
    /// </summary>
    /// <param name="mask"></param>
    void CheckForMarkerSelection(LayerMask mask)
    {
        if (inputTarget == InputTargetController.InputTarget.Viewport)
        {
            Ray mouseRay = sceneCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            ///Check for selectable Node
           // if (Physics.Raycast(mouseRay, out hit, 100, mask, QueryTriggerInteraction.Ignore))
            if (Physics.Raycast(mouseRay, out hit, 100, mask))
            {
                LifelineItem marker = hit.collider.gameObject.GetComponentInParent<LifelineItem>();
                ///detect marker
                //if (hit.collider.gameObject.TryGetComponentInParent(out LifelineItem marker))
                if(marker != null)
                {
                   //Debug.Log("Marker detected");
                    ///Select node
                    if (Input.GetMouseButtonDown(0))
                    {
                        _placer.CancelSelectionClick();
                        // Debug.Log("Marker selected");
                        //assign targeting data
                        /*
                        if (data != marker.GetComponentInParent<ComponentInfo_Lifeline>() || data == null)
                        {
                            data = marker.GetComponentInParent<ComponentInfo_Lifeline>();
                        }*/
                        Debug.Log("Click Marker");
                        //_info = marker.GetComponentInParent<ObjectInfo>().componentInfo_Lifelines[0];
                        //_info = marker.GetComponentInParent<ObjectInfo>().GetFirstModularComponent<ComponentInfo_Lifeline>();
                        target = marker;
                        contextMenu.SetTarget( marker.transform);
                        CursorImageController.instance.cursorState = CursorImageController.CursorImage.HandClosed;
                    }
                    else
                    {
                        CursorImageController.instance.cursorState = CursorImageController.CursorImage.HandOpen;
                    }
                }
            }
        }
    }


    void RepositionMarker()
    {
        
        if (inputTarget == InputTargetController.InputTarget.Viewport)
        {
            Ray mouseRay = sceneCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(mouseRay, out hit, 100, LayerMask.GetMask("Gizmo")))
                {
                    /// Reposition Node
                    
                    if (hit.collider.gameObject.name == "MoveMarkerUp") { MoveMarkerUp(); }
                    else if (hit.collider.gameObject.name == "MoveMarkerDown") {MoveMarkerDown(); }
                }
            }


        }
    }
    
    ///swap the currently selected marker out for the prefabed reference
    void SwapMarker(GameObject prefabRef)
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        ///cache and remove old marker
        int oldIndex = 0;
        lifeline.MarkerPointDictionary.TryGetValue(target.transform, out oldIndex);
        lifeline.MarkerPointDictionary.Remove(target.transform);
        Destroy(target.gameObject);
        AddMarker(prefabRef, oldIndex);
    }
    void SwapMarkerDirectional()
    {
        SwapMarker(globalCableData.directionPrefab);
    }
    void SwapMarkerManDoor()
    {
        SwapMarker(globalCableData.manDoorPrefab);
    }
    void SwapMarkerCache()
    {
        SwapMarker(globalCableData.SCSRCachePrefab);
    }
    void SwapMarkerRefugeChamber()
    {
        SwapMarker(globalCableData.refugeChamberPrefab);
    }
    void SwapMarkerBranchLine()
    {
        SwapMarker(globalCableData.branchLinePrefab);
    }
    void SwapMarkerTag()
    {
        SwapMarker(globalCableData.tagPrefab);
    }

    void PopulateMarkers(GameObject prefabRef, bool isTag = false)
    {
        Debug.Log("Populate Markers(" + prefabRef + ")");

        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        float markerSpacing = lifeline.GetMarkerSpacing();
        int lastValidIndex = 0;
        ///apply offset for tags to prevent overlap with directional markers
        //if (isTag) lastValidIndex = TagMarkerOffset;
        
        for (int i = 0; i < lifeline.Cable.GetSmoothedPoints().Count; i++)
        {
            int adjustedIndex = i;
            if (isTag)
            {
                adjustedIndex = i + TagMarkerOffset;
            }
            //add at zero
            if (i == lastValidIndex) 
            {
                
                AddMarker(prefabRef, adjustedIndex);
            }
            else
            {
                //calculate distance between current and last valid index
                float dist = Vector3.Distance(lifeline.Cable.GetSmoothedPoints()[i], lifeline.Cable.GetSmoothedPoints()[lastValidIndex]);
                if (dist >= markerSpacing)
                {
                    lastValidIndex = i;
                    AddMarker(prefabRef, adjustedIndex);
                }
            }
        }
    }

    void AddMarker(GameObject prefabRef, int i)
    {
        //data.cable.RegenerateMesh();
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        Transform parent = GetIndvMarkerParent();
        GameObject marker = Instantiate(prefabRef, lifeline.transform);
        lifeline.MarkerPointDictionary.Add(marker.transform, i);
       
        if(marker.TryGetComponent(out LifelineItem markerScript))
        {
            if (markerScript.itemType == LifelineItem.ItemType.Directional && !lifeline.DirectionalMarkers.Contains(markerScript))
            {
                 lifeline.DirectionalMarkers.Add(markerScript);
            }
            else if (markerScript.itemType == LifelineItem.ItemType.Tag && !lifeline.Tags.Contains(markerScript))
            {
                 lifeline.Tags.Add(markerScript);
            }
        }

        lifeline.SetMarkerPosition(marker.transform, i);
        lifeline.SetMarkerRotation(marker.transform, i);
        marker.transform.parent = parent;

        /*
        if (mode == LLMarkerPlacementMode.LifelineTag)
        {
            LifelineTag llTag = marker.GetComponent<LifelineTag>();
            llTag.UpdateTagColor(LGenerator.EscapewayColor);
        }*/

        //mode = LLMarkerPlacementMode.None;
        LifelineItem llItem = marker.GetComponent<LifelineItem>();
        llItem.LifeLineGenRef = lifeline.LGenerator;
        llItem.ClosestPointIndex = i;
        llItem.SavedClosestPoint = marker.transform.localPosition;
        lifeline.lifelineItems.Add(llItem);

        contextMenu.SetTarget(marker.transform);

       RepositionMarkerGizmo();
    }


    //public void SetMarkerPosition(Transform t, int i)
    //{
    //    var lifeline = GetLifelineComponent();
    //    if (lifeline == null)
    //        return;

    //    if (i < 0){ i = 0; }
    //    else if (i > lifeline.Cable.GetSmoothedPoints().Count - 1) { i = lifeline.Cable.GetSmoothedPoints().Count - 1; }
    //    //t.position = data.transform.TransformPoint(data.cable.GetSmoothedPoints()[i]);
    //    t.position = lifeline.IndividualMarkerContainer.TransformPoint(lifeline.Cable.GetSmoothedPoints()[i]);
    //    markerGizmo.transform.position = t.position;
    //}
    ////called from Gizmo Button


    //public void SetMarkerRotation(Transform t, int i)
    //{
    //    Vector3 direction;
    //    var lifeline = GetLifelineComponent();
    //    if (lifeline == null)
    //        return;

    //    if (i < 0) { i = 0; }
    //    else if (i > lifeline.Cable.GetSmoothedPoints().Count - 1) { i = lifeline.Cable.GetSmoothedPoints().Count - 1; }

    //    if (i < lifeline.Cable.GetSmoothedPoints().Count - 1)
    //    {
    //        direction = lifeline.Cable.GetSmoothedPoints()[i + 1] - lifeline.Cable.GetSmoothedPoints()[i];
    //    }
    //    else
    //    {
    //        direction = lifeline.Cable.GetSmoothedPoints()[i -1] - lifeline.Cable.GetSmoothedPoints()[i];
    //    }

    //    t.rotation = Quaternion.LookRotation(direction, Vector3.up);
    //    markerGizmo.transform.rotation = t.rotation;
    //}

    //public void RepositionMarkersWithCable()
    //{
    //    var lifeline = GetLifelineComponent();
    //    if (lifeline == null)
    //        return;

    //    foreach (KeyValuePair<Transform, int> pair in lifeline.MarkerPointDictionary)
    //    {
    //        SetMarkerRotation(pair.Key, pair.Value);
    //        SetMarkerPosition(pair.Key, pair.Value);
    //    }
    //}

    public void RemoveMarker()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null || target == null || markerGizmo == null)
            return;

        markerGizmo.transform.SetParent(null);

        lifeline.MarkerPointDictionary.Remove(target.transform);
        lifeline.lifelineItems.Remove(target);
        Destroy(target.gameObject);
        markerGizmo.SetActive(false);
    }


    Transform GetIndvMarkerParent()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return null;

        if (lifeline.IndividualMarkerContainer == null)
        {
            GameObject par = new GameObject("IndividualMarkers");
            par.transform.SetParent(lifeline.CableInfo.transform, worldPositionStays: false);
            //par.transform.position = _info.CableInfo.NodeGizmos[1].transform.position; 
            //par.transform.parent = _info.CableInfo.NodeGizmos[1].transform;
            //par.name = "IndividualMarkers";
            lifeline.IndividualMarkerContainer = par.transform;
        }

        return lifeline.IndividualMarkerContainer;
    }

    #region Events/Actions

    void OnNewInputTarget(InputTargetController.InputTarget _inputTarget)
    {
        inputTarget = _inputTarget;
    }

    void OnNewLayer(LayerManager.EditorLayer _editorLayer)
    {
        editorLayer = _editorLayer;
    }

    #endregion

    #region Button/UI



    void MoveMarkerUp()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        target.ClosestPointIndex += 1;
        lifeline.MarkerPointDictionary[target.transform] = target.ClosestPointIndex;
        lifeline.SetMarkerPosition(target.transform, target.ClosestPointIndex);
        lifeline.SetMarkerRotation(target.transform, target.ClosestPointIndex);

        _placer.CancelSelectionClick();
    }

    //Called from Gizmo button
    void MoveMarkerDown()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        target.ClosestPointIndex -= 1;
        lifeline.MarkerPointDictionary[target.transform] = target.ClosestPointIndex;
        lifeline.SetMarkerPosition(target.transform, target.ClosestPointIndex);
        lifeline.SetMarkerRotation(target.transform, target.ClosestPointIndex);

        _placer.CancelSelectionClick();
    }

    public void AddTag()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) AddMarker(globalCableData.tagPrefab, SmoothPointClosestToMidpoint());
    }
    
    public void AddDirectional()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) AddMarker(globalCableData.directionPrefab, SmoothPointClosestToMidpoint());
        Debug.Log("AddDirectional");
    }
    
    public void AddSCSRCache()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) AddMarker(globalCableData.SCSRCachePrefab, SmoothPointClosestToMidpoint());
    }
    
    public void AddBranchLine()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) AddMarker(globalCableData.branchLinePrefab, SmoothPointClosestToMidpoint());
    }
    
    public void AddManDoor()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) AddMarker(globalCableData.manDoorPrefab, SmoothPointClosestToMidpoint());
    }
    
    public void AddRefugeChamber()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) AddMarker(globalCableData.refugeChamberPrefab, SmoothPointClosestToMidpoint());
    }
    
    public void PopulateTag()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) PopulateMarkers(globalCableData.tagPrefab, true);
    }
    
    public void PopulateDirection()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (lifeline.Cable.CableNodes.Count > 1) PopulateMarkers(globalCableData.directionPrefab, false);

    }

    public void FlipTargetMarker()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        if (target)lifeline.FlipMarker(target);
    }
    //change selected target color
    public void ChangeTargetTagColor(int index)
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return;

        LifelineTag tag = target as LifelineTag;
        lifeline.ChangeTagColor(tag, index);
    }

    #endregion

    /// <summary>
    /// Get the index of the smoothing point closest to the midpoint between the current and previous node
    /// </summary>
    /// <returns></returns>
    public int SmoothPointClosestToMidpoint()
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null)
            return 0;

        //int targetIndex = cableEditor.targetIndex;
        int targetIndex = cableEditor.SelectedCable.SelectedNodeIndex;
        if (targetIndex < 0)
            return 0;

        ///calculate the midpoint between current and adjacent node
        Vector3 midPoint = Vector3.zero;
        
        if (targetIndex < lifeline.Cable.CableNodes.Count - 1)
        {
            Vector3 curNodePosition = lifeline.Cable.CableNodes[targetIndex].Position;
            Vector3 lastNodePosition = lifeline.Cable.CableNodes[targetIndex + 1].Position;
            midPoint = (lastNodePosition + curNodePosition) / 2;
            //Debug.Log("midpoint is at " + midPoint + " between " + targetIndex + " & " + (targetIndex - 1));
        }
        else
        {
            Vector3 curNodePosition = lifeline.Cable.CableNodes[targetIndex].Position;
            Vector3 nextNodePosition = lifeline.Cable.CableNodes[targetIndex - 1].Position;
            midPoint = (nextNodePosition + curNodePosition) / 2;
            //Debug.Log("midpoint is at " + midPoint + " between "+ targetIndex + " & " + (targetIndex + 1));
        }

        ///Step through each smooth point position and return the one with the shortest distance to midpoint
        Vector3 targetPoint = Vector3.zero;
        float shortestDistance = 1000f;
        int smoothTargetIndex = 0;
        for (int i = 0; i < lifeline.Cable.GetSmoothedPoints().Count; i ++)
        {
            Vector3 point = lifeline.IndividualMarkerContainer.TransformPoint(lifeline.Cable.GetSmoothedPoints()[i]); 
            float distance = Vector3.Distance(point, midPoint);
            //Debug.Log("distance between point " + point + " and " + midPoint + " = " + distance);

            if (distance < shortestDistance)
            {
                targetPoint = point;
                shortestDistance = distance;
                smoothTargetIndex = i;
            }
        }
 
        Debug.Log("closest smooth point to midpoint is " + targetPoint + " at index of " + smoothTargetIndex);
        return smoothTargetIndex;
    }


    public void ClearSelection()
    {
        markerGizmo.transform.SetParent(null);

        //disable gizmo
        markerGizmo.SetActive(false);
        target = null;
    }
    
    public void SelectMarker(LifelineItem marker)
    {
        target = marker;
        markerGizmo.SetActive(true);
        RepositionMarkerGizmo();
        
    }

    public void RepositionMarkerGizmo()
    {
        if (target == null)
            return;

        markerGizmo.transform.SetParent(target.transform);

        //enable and position Gizmo
        markerGizmo.transform.position = target.transform.position;
        markerGizmo.transform.rotation = target.transform.rotation;
        
    }



    public void ProcessMouseDrag(ScenarioCursorData prev, ScenarioCursorData current)
    {
        var lifeline = GetLifelineComponent();
        if (lifeline == null || target == null)
            return;

        var closestIndex = lifeline.Cable.GetClosestSmoothedPoint(current.SceneRay);
        if (closestIndex >= 0 && closestIndex != target.ClosestPointIndex)
        {
            lifeline.MoveLifelineItem(target, closestIndex);
        }
    }




}
