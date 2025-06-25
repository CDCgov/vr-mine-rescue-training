using MathNet.Numerics.IntegralTransforms;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;


/// <summary>
/// Class that holds logic on
/// how the placer should act.
/// One logic object could have
/// the placer snap to the
/// environment and follow the
/// mouse where the other
/// creates gizmo handles and
/// waits for input on those
/// </summary>

[System.Serializable]
public class PlacerLogic
{
    public PlacablePrefab selectedObject;

    protected PlacerGizmo selectedGizmo;            // Currently enabled control gizmo (if logic uses gizmo)
    protected Placer scenePlacer;                   // The placer object in the scene    
    protected LayerMask layerMask;    
    protected Collider selectedObjCollider;
    protected CameraManager camManager;
    protected bool isActive;

    //private List<GizmoOrder> gizmoKinds;

    public PlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager)
    {
        //gizmoKinds = new List<GizmoOrder>(kinds);
        layerMask = 1 << 6;
        layerMask = ~layerMask;
        scenePlacer = placer;
        this.camManager = camManager;
    }

    //public void ProcessInput()
    //{
        
    //}

    //private void RotateObject()
    //{

    //}

    //public virtual void ForceSelectObject(GameObject obj)
    //{
    //}
    //public virtual void ForceDeselectObject(GameObject obj)
    //{
    //}

    //public virtual void ForceDeselect()
    //{
    //}

    public virtual bool CheckIfActive()
    {
        return isActive;
    }

    public void SetSelectedGizmo(PlacerGizmo gizmo)
    {
        selectedGizmo = gizmo;
    }


    //public List<GizmoOrder> GetGizmoTypes()
    //{
    //    return gizmoKinds;
    //}
    public virtual void StartLogic()
    {

    }
    public virtual void DoLogic()
    {
    }
    public virtual void DoSupportLogic()
    {
    }
    public virtual void CheckForDelete()
    {

    }
    public virtual void CheckForFocus()
    {


    }
    public virtual void CheckForDeselect()
    {

    }

    public virtual void CheckForPlacementOverride(PlacablePrefab placable)
    {
        //if (placable.TryGetComponent(out ObjectInfo info))
        //{
            
        //    scenePlacer.OverrideManipulationLogic(info.PlacementTypeOverride);
        //}
    }
}