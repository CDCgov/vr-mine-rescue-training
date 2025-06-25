using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.EventSystems;
using NIOSH_EditorLayers;
using static NIOSH_EditorLayers.LayerManager;
using NIOSH_MineCreation;

[System.Serializable]
public class CablePlacerLogic : PlacerLogic
{
    public CablePlacerLogic(List<GizmoOrder> kinds, Placer placer, CameraManager camManager) : base(kinds, placer, camManager)
    {
    }
    /// <summary>
    /// Forces a selected object, skipping the normal raycast select.
    /// For example if the UI wants to select an object.
    /// </summary>
    /// <param name="obj"></param>
    //public override void ForceSelectObject(GameObject obj)
    //{
    //    if (!obj) return;
    //    PlacablePrefab prefab = obj.GetComponent<PlacablePrefab>();

    //    if (prefab != null && prefab.GetPlacementLayer() == LayerManager.GetCurrentLayer())
    //    {
    //        if (!prefab.gameObject.activeSelf) { Debug.Log("PREFAB NOT ACTIVE"); prefab.gameObject.SetActive(true); }
    //        if (scenePlacer) scenePlacer.onObjectSelected?.Invoke(obj.gameObject);
    //    }
    //}
    //public override void DoLogic()
    //{
    //    CheckForDeselect();
    //    CheckForDelete();
    //}



    //public override void CheckForDelete()
    //{
    //    if (Input.GetKey(KeyCode.Delete))
    //    {
    //        if (selectedObject != null)
    //        {
    //            GameObject temp = selectedObject.gameObject;
    //            DeselectObject();
    //            GameObject.Destroy(temp);
    //        }
    //    }
    //}

    //public override void CheckForDeselect()
    //{
    //    if (Input.GetKey(KeyCode.Escape))
    //    {
    //        DeselectObject();
   
    //    }
    //}

    //void DeselectObject()
    //{
    //    if (selectedObject != null)
    //    {
    //        selectedObject.gameObject.layer = LayerMask.NameToLayer("Default");
    //        int count = selectedObject.transform.childCount;
    //        if (count > 0)
    //        {
    //            for (int i = 0; i < count; i++)
    //            {
    //                Transform objC = selectedObject.transform.GetChild(i);
    //                objC.gameObject.layer = LayerMask.NameToLayer("Default");
    //                int subCount = objC.transform.childCount;

    //                if (subCount > 0)
    //                {
    //                    foreach (Transform child in objC.transform)
    //                    {
    //                        child.gameObject.layer = LayerMask.NameToLayer("Default");
    //                    }
    //                }
    //            }
    //        }
    //        selectedObjCollider = null;
    //        selectedObject = null;
    //        if (scenePlacer) scenePlacer.onObjectDeselected?.Invoke();
    //    }

    //}

    //public void ForceDelete()
    //{
    //    if (selectedObject != null)
    //    {
    //        GameObject temp = selectedObject.gameObject;
    //        //DeselectObject();
    //        GameObject.Destroy(temp);
    //    }
    //}

}
