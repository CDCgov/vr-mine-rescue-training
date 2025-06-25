///<summary>================================================================================
///
/// Stores logic for the placable objects that can be put into a room.
/// 
///</summary>===============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using NIOSH_MineCreation;
using static NIOSH_EditorLayers.LayerManager;

public enum PlacementMode { Anchor, Collider, Pivot }

public class PlacablePrefab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Variables           =============================================================

    public MineLayerTileManager MineLayerTileManager;
    public Placer Placer;

    [UnityEngine.Serialization.FormerlySerializedAs("canPlaceOnFloor")]
    public bool CanPlaceOnFloor;
    [UnityEngine.Serialization.FormerlySerializedAs("canPlaceOnWall")]
    public bool CanPlaceOnWall;
    [UnityEngine.Serialization.FormerlySerializedAs("canPlaceOnRoof")]
    public bool CanPlaceOnRoof;
    public bool ParentToMineTile = true;

    //public NIOSH_EditorLayers.LayerManager.EditorLayer placementLayer;
    public Transform placement_anchor;

    public EditorLayer PlacementLayer 
    {
        get
        {
            if (_info == null)
                return EditorLayer.Object;

            return _info.editorLayer;
        }
    }

    /// <summary>
    /// Indicator for if this object has been placed in the room
    /// </summary>
    public bool placed = false;
    public bool isIgnoreSave;

    public bool ShowSelectionBox = true;
    public bool RotateToSurface = true;

    public UnityAction<bool> OnPlaced;

    private BoxCollider _collider;
    private MeshCollider _meshCollider;    
    private MeshRenderer _meshRenderer;
    private ObjectInfo _info;

    private string _displayName;
    //private PlacementMode _placementMode;    
    

    public bool IsPlaced
    {
        get
        {
            return placed;
        }
    }

    #endregion


    #region Built-in Methods    =============================================================
    private void Awake()
    {
        _info = GetComponent<ObjectInfo>();
        //if (GetComponent<MineLayerTile>())
        //{
        //    placementLayer = EditorLayer.Mine;
        //}
        //else if (_info && _info.editorLayer == EditorLayer.Ventilation)
        //{
        //    placementLayer = EditorLayer.Ventilation;
        //}
        //else if (_info && _info.editorLayer == EditorLayer.Cables)
        //{
        //    placementLayer = EditorLayer.Cables;
        //}
        //else if (GetComponent<MineMapSymbolRenderer>() || GetComponent<VRPointOfInterest>())
        //{
        //    placementLayer = EditorLayer.SceneControls;
        //}
        //else
        //{
        //    placementLayer = EditorLayer.Object;
        //}
        // cache the box collider
        _collider = GetComponent<BoxCollider>();
        _meshCollider = GetComponent<MeshCollider>();

        _meshRenderer = GetComponent<MeshRenderer>();
        //foreach (Transform child in transform)
        //{
        //    if (child.name == "PlacementAnchor")
        //    {
        //        placement_anchor = child;
        //    }
        //}
        //_placementMode = PlacementMode.Pivot;

        //if (placement_anchor != null)
        //{
        //    _placementMode = PlacementMode.Anchor;
        //}
        //else if (_collider != null || _meshCollider != null)
        //{
        //    _placementMode = PlacementMode.Collider;
        //}
        if(_info)
        {
            _displayName = _info.DisplayName;
        }
    }

    public void Start()
    {
        if (ScenarioSaveLoad.IsScenarioEditor)
        {
            if (MineLayerTileManager == null)
                MineLayerTileManager = MineLayerTileManager.GetDefault();
            if (Placer == null)
                Placer = Placer.GetDefault();
        }
    }

    public void OnDisable()
    {
        if (Placer != null)
        {
            if (Placer.SelectedObject == gameObject)
            {
                Placer.DeselectObject();
            }
        }
    }

    public ObjectInfo GetObjectInfo()
    {
        return _info;
    }

    public string GetDisplayName()
    {
        if (string.IsNullOrEmpty(_displayName))
        {
            return transform.name;
        }
        else
        {
            return _displayName;
        }
    }

    public bool GetIsIgnoreSave()
    {
        return isIgnoreSave;
    }



    /// <summary>
    /// Selection logic
    /// </summary>
    void IPointerClickHandler.OnPointerClick(PointerEventData pointerEventData)
    {
    }



    // Highlight on mouse over
    void IPointerEnterHandler.OnPointerEnter(PointerEventData pointerEventData)
    {
        ObjectHovered(true);

    }

    private void OnHoverEnter()
    {
        ObjectHovered(true);
    }

    private void OnHoverExit()
    {
        ObjectHovered(false);
    }

    private void ObjectHovered(bool entered)
    {
        //Debug.Log("hovered");
    }

    // Hide outline when mouse leaves
    void IPointerExitHandler.OnPointerExit(PointerEventData pointerEventData)
    {
        ObjectHovered(false);
    }

    // Unsubscribe from actions
    protected void OnDestroy()
    {

    }
    #endregion


    #region Custom Methods      =============================================================

    /// <summary>
    /// Mark this object as placed in the room
    /// </summary>
    public void SetPlaced()
    {
        placed = true;
        //placedOnce = true;
        OnPlaced?.Invoke(placed);
    }

    public void UnPlace()
    {
        placed = false;
        OnPlaced?.Invoke(placed);
    }

    //public bool GetPlacedOnce()
    //{
    //    return placedOnce;
    //}

    public void SetAsPlaced()
    {
        placed = true;
    }

    //public bool CheckIfPlaced()
    //{
    //    return placed;
    //}


    public Vector3 GetPlacementOffset()
    {
        Vector3 offset = Vector3.zero;
        // if anchor object return that

        // else if collider use that
        if (_meshCollider != null)
        {
            offset = _meshCollider.bounds.extents;
        }
        else if (_collider != null)
        {
            offset = _collider.bounds.extents;
        }



        // else use pivot/position of gameobject

        return offset;
    }

    public void SetIgnoreSave(bool ignoreSave)
    {
        isIgnoreSave = ignoreSave;
    }


    public Collider GetCollider()
    {
        Collider collider = null;
        if (_meshCollider != null)
        {
            collider = _meshCollider;
        }
        else if (_collider != null)
        {
            collider = _collider;
        }
        return collider;
    }

    public Renderer GetRenderer()
    {
        return _meshRenderer;
    }

    //public Transform GetAnchor()
    //{
    //    return placement_anchor;
    //}

    //public PlacementMode GetPlacementMode()
    //{
    //    return _placementMode;
    //}
    
    public void SearchForNewParent()
    {
        if (MineLayerTileManager == null || !ParentToMineTile)
            return;

        var simParent = MineLayerTileManager.GetClosestSimParent(transform.position);
        if (simParent == null)
            return;

        simParent.TryParent(gameObject);
    }





    #endregion
}
