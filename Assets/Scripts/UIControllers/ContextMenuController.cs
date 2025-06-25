
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NIOSH_EditorLayers;
using System;

public class ContextMenuController : MonoBehaviour
{

    public GameObject CableNodeUI;
    public GameObject LifelineMarkerUI;

    [SerializeField] GameObject innerRing;



    /// Node Toggles    
    [SerializeField] Toggle addNodeToggle;
    [SerializeField] Toggle swapNodeToggle;
    [SerializeField] Toggle editSlackToggle;
    [SerializeField] Toggle hideGizmosToggle;

    /// Marker Toggles
    [SerializeField] Toggle addMarkerToggle;
    [SerializeField] Toggle swapMarkerToggle;
    [SerializeField] Toggle editMarkerColorToggle;
    [SerializeField] Button flipMarkerButton;


    [SerializeField] Button removeButton;

    public Button AddRoofNodeButton;
    public Button AddFloorNodeButton;

    [SerializeField] float toggleMinSize = 10f;
    [SerializeField] float toggleDefaultSize = 30f;


    public RuntimeCableEditor CableEditor;
    public RuntimeMarkerEditor MarkerEditor;
    public GameObject contextMenuUI;
    public GameObject CableDeletionPrompt;
    public Button ConfirmDeleteCableButton;
    public Button CancelDeleteCableButton;


    bool nodeSelected;
    LayerManager layerManager;
    public LayerManager.EditorLayer editorLayer;

    InputTargetController inputTargetController;
    public InputTargetController.InputTarget inputTarget;

    public Camera sceneCamera;
    public Transform target;

    Sprite lastToggleImage;
    [SerializeField] Sprite backToggleImage;

    public Placer Placer;
    public bool lifelineActive;


    [SerializeField] Sprite _backSprite;
    Sprite _lastSprite;

    Dictionary<Toggle, Sprite> _lastSprites = new Dictionary<Toggle, Sprite>();

    //[SerializeField] GameObject cableNodePreview;
    //[SerializeField] GameObject markerGizmo;
    //Renderer _targetRenderer;

    bool subMenuOpen;

    // Start is called before the first frame update
    void Start()
    {

        //if (cableNodePreview) cableNodePreview.SetActive(false);
        //if (markerGizmo) markerGizmo.SetActive(false);
        CableEditor = GetComponent<RuntimeCableEditor>();
        MarkerEditor = GetComponent<RuntimeMarkerEditor>();

        sceneCamera = Camera.main;
        inputTargetController = FindObjectOfType<InputTargetController>();

        LayerManager.Instance.layerChanged += OnNewLayer;
        if (inputTargetController) inputTargetController.onNewInputTarget += OnNewInputTarget;
        layerManager = LayerManager.Instance;

        Placer = FindObjectOfType<Placer>();
        ///Toggles
        if (addNodeToggle) addNodeToggle.onValueChanged.AddListener(ToggleAddNode);
        if (swapNodeToggle) swapNodeToggle.onValueChanged.AddListener(ToggleSwapNode);
        if (editSlackToggle) editSlackToggle.onValueChanged.AddListener(ToggleEditSlack);
        if (addMarkerToggle) addMarkerToggle.onValueChanged.AddListener(ToggleAddMarker);
        if (swapMarkerToggle) swapMarkerToggle.onValueChanged.AddListener(ToggleSwapMarker);
        if (editMarkerColorToggle) editMarkerColorToggle.onValueChanged.AddListener(ToggleEditMarkerColor);
        if (removeButton) removeButton.onClick.AddListener(RemoveTarget);
        if (AddRoofNodeButton) AddRoofNodeButton.onClick.AddListener(DisableAddRoofNodeButton);
        if (AddFloorNodeButton) AddFloorNodeButton.onClick.AddListener(DisableAddFloorNodeButton);

        contextMenuUI.SetActive(false);

        if (Placer != null)
            Placer.SelectedObjectChanged += OnSelectedObjectChanged;

    }

    //use interaction disabling to force color
    private void DisableAddRoofNodeButton()
    {
        AddRoofNodeButton.interactable = false;
        AddFloorNodeButton.interactable = true;
    }
    private void DisableAddFloorNodeButton()
    {
        AddRoofNodeButton.interactable = true;
        AddFloorNodeButton.interactable = false;
    }

    private void OnSelectedObjectChanged(GameObject obj)
    {
        if (obj == null)
            DeselectTarget();
    }

    private void OnDestroy()
    {
        ///Toggles
        if (addNodeToggle) addNodeToggle.onValueChanged.RemoveListener(ToggleAddNode);
        if (swapNodeToggle) swapNodeToggle.onValueChanged.RemoveListener(ToggleSwapNode);
        if (editSlackToggle) editSlackToggle.onValueChanged.RemoveListener(ToggleEditSlack);
        if (addMarkerToggle) addMarkerToggle.onValueChanged.RemoveListener(ToggleAddMarker);
        if (swapMarkerToggle) swapMarkerToggle.onValueChanged.RemoveListener(ToggleSwapMarker);
        if (editMarkerColorToggle) editMarkerColorToggle.onValueChanged.RemoveListener(ToggleEditMarkerColor);
        if (layerManager) layerManager.layerChanged -= OnNewLayer;
        if (inputTargetController) inputTargetController.onNewInputTarget -= OnNewInputTarget;
        if (removeButton) removeButton.onClick.RemoveListener(RemoveTarget);
        //if (scenePlacer) scenePlacer.onObjectDeselected -= DeselectTarget;

        if (Placer != null)
            Placer.SelectedObjectChanged -= OnSelectedObjectChanged;
        //if (scenePlacer)
        //    scenePlacer.SelectedObjectChanged -= OnSelectedObjectChanged;
    }

    // Update is called once per frame
    void Update()
    {
        ControlUIVisibility();

        if (target == null)
            return;

        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveTarget();
        }
    }

    //If target is active and visible, position UI, otherwise disable
    void ControlUIVisibility()
    {
        if (editorLayer == LayerManager.EditorLayer.Cables)
        {
            //if (!CableEditor.dragging)
            //{

            if (target != null)
            {
                PositionUI();
            }

            else if (contextMenuUI.activeInHierarchy) contextMenuUI.SetActive(false);

            //}
        }
    }
    void PositionUI()
    {
        ///Set context menu position to last node on cable

        if (!contextMenuUI.activeInHierarchy) { contextMenuUI.SetActive(true); }
        contextMenuUI.transform.position = sceneCamera.WorldToScreenPoint(target.position);
    }

    #region Events/Actions

    void OnNewInputTarget(InputTargetController.InputTarget _inputTarget)
    {
        inputTarget = _inputTarget;
    }

    void OnNewLayer(LayerManager.EditorLayer _editorLayer)
    {
        editorLayer = _editorLayer;

        if (editorLayer == LayerManager.EditorLayer.Cables)
        {
            contextMenuUI.gameObject.SetActive(true);

            //if (cableNodePreview) cableNodePreview.SetActive(true);
            //if (markerGizmo) markerGizmo.SetActive(true);
        }
        else
        {
            contextMenuUI.gameObject.SetActive(false);
            //if (cableNodePreview) cableNodePreview.SetActive(false);
            //if (markerGizmo) markerGizmo.SetActive(false);
        }
        //cableEditor.nodeAddMode = RuntimeCableEditor.NodeAddMode.AddModeOff;
    }

    #endregion

    #region UI control and events

    public void ToggleAddNode(bool state)
    {
        if (state == false)
        {
            CableEditor.NodeAddMode = NodeAddMode.AddModeOff;
            //CableEditor._nodePreview.gameObject.SetActive(false);
            ControlToggles(addNodeToggle, false);

        }
        ///Disable all other toggles
        else
        {
            ControlToggles(addNodeToggle, true);
            //CableEditor._nodePreview.gameObject.SetActive(true);
        }

        AddRoofNodeButton.interactable = true;
        AddFloorNodeButton.interactable = true;
    }
    public void ToggleSwapNode(bool state)
    {
        ///disable all sub menus
        if (state == false)
        {
            ControlToggles(swapNodeToggle, false);
        }
        ///open sub menu and Disable all others
        else
        {
            ControlToggles(swapNodeToggle, true);
        }
    }
    public void ToggleEditSlack(bool state)
    {
        ///disable all sub menus
        if (state == false)
        {
            ControlToggles(editSlackToggle, false);
        }
        ///open sub menu and Disable all others
        else
        {
            ControlToggles(editSlackToggle, true);
        }
    }
    public void ToggleAddMarker(bool state)
    {
        ///disable all sub menus
        if (state == false)
        {
            ControlToggles(addMarkerToggle, false);
        }
        ///open sub menu and Disable all others
        else
        {
            ControlToggles(addMarkerToggle, true);
        }
    }
    public void ToggleSwapMarker(bool state)
    {
        ///disable all sub menus
        if (state == false)
        {
            ControlToggles(swapMarkerToggle, false);
        }
        ///open sub menu and Disable all others
        else
        {
            ControlToggles(swapMarkerToggle, true);
        }
    }
    public void ToggleEditMarkerColor(bool state)
    {
        ///disable all sub menus
        if (state == false)
        {
            ControlToggles(editMarkerColorToggle, false);
        }
        ///open sub menu and Disable all others
        else
        {
            ControlToggles(editMarkerColorToggle, true);
        }
    }

    void ControlCableNodeToggle(Toggle target, bool openSubMenu)
    {
        ///deactivate all other toggle menus and minimize thier buttons
        if (openSubMenu)
        {
            if (target != addNodeToggle) { MinimizeUI(addNodeToggle.gameObject); }
            if (target != swapNodeToggle) { MinimizeUI(swapNodeToggle.gameObject); }
            if (target != editSlackToggle) { MinimizeUI(editSlackToggle.gameObject); }
            if (target != addMarkerToggle) { MinimizeUI(addMarkerToggle.gameObject); }

            //MinimizeUI(hideGizmosToggle.gameObject);
            MinimizeUI(removeButton.gameObject);

            innerRing.SetActive(false);
            ActivateToggle(target);

            _lastSprite = target.image.sprite;
            target.image.sprite = _backSprite;

        }
        ///Reset menu to main hub
        else
        {
            RestoreUI(addNodeToggle.gameObject);
            RestoreUI(swapNodeToggle.gameObject);
            RestoreUI(editSlackToggle.gameObject);
            if (CableEditor.SelectedLifeline != null)
            {
                RestoreUI(addMarkerToggle.gameObject);
            }
            RestoreUI(swapMarkerToggle.gameObject);
            RestoreUI(editMarkerColorToggle.gameObject);
            innerRing.SetActive(true);
            RestoreUI(flipMarkerButton.gameObject);
            //RestoreUI(hideGizmosToggle.gameObject);
            RestoreUI(removeButton.gameObject);
            target.image.sprite = _lastSprite;
        }
    }
    void ControlMarkerNodeToggle(Toggle target, bool openSubMenu)
    {
        ///deactivate all other toggle menus and minimize thier buttons
        subMenuOpen = openSubMenu;
        if (subMenuOpen)
        {
            if (target != swapMarkerToggle) { MinimizeUI(swapMarkerToggle.gameObject); }
            if (target != editMarkerColorToggle) { MinimizeUI(editMarkerColorToggle.gameObject); }

            MinimizeUI(flipMarkerButton.gameObject);
            MinimizeUI(removeButton.gameObject);


            innerRing.SetActive(false);
            ActivateToggle(target);

            _lastSprite = target.image.sprite;
            target.image.sprite = _backSprite;

        }
        ///Reset menu to main hub
        else
        {
            innerRing.SetActive(true);
            RestoreUI(swapMarkerToggle.gameObject);
            RestoreUI(editMarkerColorToggle.gameObject);
            RestoreUI(flipMarkerButton.gameObject);
            RestoreUI(removeButton.gameObject);
            target.image.sprite = _lastSprite;
        }
    }

    void ControlToggles(Toggle target, bool openSubMenu)
    {
        ///deactivate all other toggle menus and minimize thier buttons
        subMenuOpen = openSubMenu;
        if (subMenuOpen)
        {
            if (target != addNodeToggle) { MinimizeUI(addNodeToggle.gameObject); }
            if (target != swapNodeToggle) { MinimizeUI(swapNodeToggle.gameObject); }
            if (target != editSlackToggle) { MinimizeUI(editSlackToggle.gameObject); }
            if (target != addMarkerToggle) { MinimizeUI(addMarkerToggle.gameObject); }

            /// marker UI
            if (target != swapMarkerToggle) { MinimizeUI(swapMarkerToggle.gameObject); }
            if (target != editMarkerColorToggle) { MinimizeUI(editMarkerColorToggle.gameObject); }

            MinimizeUI(flipMarkerButton.gameObject);
            //MinimizeUI(hideGizmosToggle.gameObject);
            MinimizeUI(removeButton.gameObject);


            innerRing.SetActive(false);
            ActivateToggle(target);

            //_lastSprite = target.image.sprite;
            if (!_lastSprites.ContainsKey(target))
            {
                _lastSprites.Add(target, target.image.sprite);
            }
            else
            {
                _lastSprite = target.image.sprite;
            }
            target.image.sprite = _backSprite;

        }
        ///Reset menu to main hub
        else
        {
            RestoreUI(addNodeToggle.gameObject);
            RestoreUI(swapNodeToggle.gameObject);
            RestoreUI(editSlackToggle.gameObject);
            if (CableEditor.SelectedLifeline != null)
            {
                RestoreUI(addMarkerToggle.gameObject);
            }
            RestoreUI(swapMarkerToggle.gameObject);
            if(target.TryGetComponent<LifelineMarker>(out var marker))
            {
                if (marker.itemType == LifelineItem.ItemType.Tag)
                {
                    RestoreUI(editMarkerColorToggle.gameObject);
                }
            }
            //RestoreUI(editMarkerColorToggle.gameObject);
            innerRing.SetActive(true);
            RestoreUI(flipMarkerButton.gameObject);
            //RestoreUI(hideGizmosToggle.gameObject);
            RestoreUI(removeButton.gameObject);
            //target.image.sprite = _lastSprite;
            if(_lastSprites.TryGetValue(target, out var sprite))
            {
                target.image.sprite = sprite;
            }
            else
            {
                target.image.sprite = _lastSprite;
            }
        }
    }
    void MinimizeUI(GameObject target)
    {
        target.gameObject.SetActive(false);
    }
    void ActivateToggle(Toggle target)
    {
        target.isOn = true;
        var t = target.transform as RectTransform;
        t.sizeDelta = new Vector2(toggleDefaultSize, toggleDefaultSize);
    }
    void RestoreUI(GameObject target)
    {
        target.gameObject.SetActive(true);
        //target.isOn = false;
        //var t = target.transform as RectTransform;
        //t.sizeDelta = new Vector2(toggleDefaultSize, toggleDefaultSize);
    }
    void RemoveTarget()
    {
        //Destroy cable node
        if (target.TryGetComponent(out NodeGizmo gizmo))
        {

            CableEditor.RemoveCurrentNode();
            //reposition context menu
            target = CableEditor.CurrentGizmo().transform;
        }
        else if (target.TryGetComponent(out LifelineItem lifeline))
        {
            MarkerEditor.RemoveMarker();

            target = CableEditor.CurrentGizmo().transform;
            SetTarget(target);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        Debug.Log("Set Context Menu Target");
        //reset material on node
        DeselectTarget();

        target = newTarget;
        //_targetRenderer = newTarget.GetComponentInChildren<Renderer>();
        contextMenuUI.SetActive(true);


        if (newTarget.TryGetComponent(out LifelineItem marker))
        {
            MarkerEditor.SelectMarker(marker);
            MarkerOnlyUI(marker.itemType == LifelineItem.ItemType.Tag);
        }
        else if (newTarget.TryGetComponent(out NodeGizmo nextNode))
        {
            //CableEditor.OnSelectNode(nextNode.gameObject);
            NodeOnlyUI();
        }


        Placer.SelectObject(target.GetComponentInParent<ObjectInfo>().gameObject);
        AddRoofNodeButton.gameObject.SetActive(true);
        AddFloorNodeButton.gameObject.SetActive(true);
        //
        //        //scenePlacer.SelectSpecificGameObject(target.GetComponentInParent<ObjectInfo>().gameObject);
        //        scenePlacer.SelectObject(target.GetComponentInParent<ObjectInfo>().gameObject);
        //
    }

    public void DeselectTarget()
    {
        Debug.Log("Try Deselect");
        if (target != null)
        {
            //if (target.TryGetComponent(out NodeGizmo lastNode)) { CableEditor.OnDeselectNode(lastNode.gameObject); }
            //else { MarkerEditor.OnDeselectMarker(); }

            MarkerEditor.ClearSelection();

            Debug.Log("Deselect Success");
            target = null;
        }
        contextMenuUI.SetActive(false);
    }

    ///When a node is selected, disable interaction on marker only UI
    void NodeOnlyUI()
    {
        ///disable marker sub menus
        if (swapMarkerToggle.isOn) { swapMarkerToggle.isOn = false; }
        if (editMarkerColorToggle.isOn) { editMarkerColorToggle.isOn = false; }


        ///set add new marker toggle state based on if is lifeline
        if (CableEditor.SelectedLifeline == null)
        {
            MinimizeUI(addMarkerToggle.gameObject);
        }
        else if (subMenuOpen == false) { RestoreUI(addMarkerToggle.gameObject); }
        //if (subMenuOpen == false) 
        //{ 
        //    RestoreUI(hideGizmosToggle.gameObject); 
        //}

        LifelineMarkerUI.SetActive(false);
        CableNodeUI.SetActive(true);

    }

    ///When a marker is selected, disable interaction on node only UI
    void MarkerOnlyUI(bool isTag)
    {
        ///disable cable node sub menus
        if (addMarkerToggle.isOn) { addMarkerToggle.isOn = false; }
        if (editSlackToggle.isOn) { editSlackToggle.isOn = false; }
        if (swapNodeToggle.isOn) { swapNodeToggle.isOn = false; }
        if (addNodeToggle.isOn) { addNodeToggle.isOn = false; }

        //MinimizeUI(hideGizmosToggle.gameObject);

        LifelineMarkerUI.SetActive(true);
        CableNodeUI.SetActive(false);
        editMarkerColorToggle.gameObject.SetActive(isTag);


    }


    #endregion
}
