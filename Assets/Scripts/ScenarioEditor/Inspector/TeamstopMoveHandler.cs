using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamstopMoveHandler : MonoBehaviour, IScenarioEditorMouseClick
{
    public ComponentInfo_MineSegment mineSegment;
    public bool AllowTurnOffTeamStop = true;

    private Vector3 _startScale;

    public bool IsSelectionLocked { get { return false; } }

    private void Start()
    {
        //_startScale = transform.lossyScale;
        //Collider[] cols = Physics.OverlapSphere(transform.position, 0.25f);
        //foreach (var col in cols)
        //{
        //    TeamstopMoveHandler tsMH = col.GetComponent<TeamstopMoveHandler>();
        //    if(tsMH != null)
        //    {
        //        ComponentInfo_MineSegment ms = tsMH.mineSegment;
        //        if(ms != null)
        //        {
        //            foreach (Transform child in ms.TeamstopObject.transform)
        //            {
        //                if (child != transform)
        //                {
        //                    tsMH.AllowTurnOffTeamStop = false;
        //                    Destroy(child.gameObject);
        //                }
        //            }
        //            transform.parent = ms.TeamstopObject.transform;
        //            mineSegment = ms;                    
        //        }
        //    }
        //}
    }

    public void OnScenarioEditorMouseDown(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData)
    {
        CheckMoved();
        //if(mineSegment == null)
        //{
        //    return;
        //}
        //if (transform.localPosition != Vector3.zero)
        //{
        //    transform.parent = null;
        //    mineSegment.IsTeamstop = false;
        //    mineSegment.ConfigureTeamstop(false);
        //}


        //if(transform.lossyScale != _startScale)
        //{
        //    Transform parent = transform.parent;
        //    transform.parent = null;
        //    transform.localScale = Vector3.one;
        //    transform.parent = parent;
        //    _startScale = transform.lossyScale;
        //}
    }

    public void OnScenarioEditorMouseFocusLost(Placer placer)
    {
        CheckMoved();
        //if (mineSegment == null)
        //{
        //    return;
        //}
        //if (transform.localPosition != Vector3.zero)
        //{
        //    transform.parent = null;
        //    mineSegment.IsTeamstop = false;
        //    mineSegment.ConfigureTeamstop(false);
        //}
    }

    public void OnScenarioEditorMouseUp(Placer placer, int button, RaycastHit cursorHit, ScenarioCursorData cursorData)
    {
        CheckMoved();
        //if (mineSegment == null)
        //{
        //    return;
        //}
        //if (transform.localPosition != Vector3.zero)
        //{
        //    transform.parent = null;
        //    mineSegment.IsTeamstop = false;
        //    mineSegment.ConfigureTeamstop(false);
        //}
    }    

    private void CheckMoved()
    {
        if (mineSegment == null)
            return;

        if (transform.localPosition == Vector3.zero)
            return;

        //we have been moved, dissassociate from the mine segment, turn off the IsTeamstop property on the segment
        //and reset our transform parent to the normal asset parent
        transform.parent = mineSegment.transform.parent;
        transform.localScale = Vector3.one;

        mineSegment.IsTeamstop = false;
        mineSegment.ResetTeamstopGUID();
        mineSegment = null;

        if (transform.TryGetComponent<PlacablePrefab>(out var placeable))
        {
            placeable.SetIgnoreSave(false);
        }
    }

    void OnDestroy() 
    {
        if (mineSegment == null)
        {
            return;
        }
        if (AllowTurnOffTeamStop)
        {
            mineSegment.IsTeamstop = false;
        }
    }
}
