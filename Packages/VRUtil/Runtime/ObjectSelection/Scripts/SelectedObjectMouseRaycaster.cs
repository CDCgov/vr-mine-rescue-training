using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedObjectMouseRaycaster : MonoBehaviour
{
    public SelectedObjectManager SelectedObjectManager;

    public Camera RaycastSource;
    public int MouseButtonNumber = 0;
    public LayerMask RaycastLayerMask;
    public float MaxDistance = 1000;
    public QueryTriggerInteraction TriggerInteraction;

    void Start()
    {
        if (SelectedObjectManager == null)
            SelectedObjectManager = SelectedObjectManager.GetDefault(gameObject);

        SelectedObjectManager.gameObject.tag = "Manager";
        if (RaycastSource == null)
            RaycastSource = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformRaycast();
        }
    }

    private bool IsObjectSelectable(GameObject obj)
    {
        if (obj.TryGetComponent<SelectableObject>(out var selObj) ||
            obj.TryGetComponent<ISelectableObjectAction>(out var selObjAction) ||
            obj.TryGetComponent<ISelectableObjectInfo>(out var selObjInfo))
            return true;

        return false;
    }

    void PerformRaycast()
    {
        if (RaycastSource == null)
            return;

        bool overUIObject = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (overUIObject)
            return;

        //RaycastHit hitInfo;
        Ray mouseRay = RaycastSource.ScreenPointToRay(Input.mousePosition);

        //if (Physics.Raycast(RaycastSource.position, RaycastSource.forward, out hitInfo, MaxDistance, RaycastLayerMask, TriggerInteraction))

        //if (Physics.Raycast(mouseRay, out hitInfo, MaxDistance, RaycastLayerMask, TriggerInteraction))
        //{
        //    SelectedObjectManager.SetSelectedObject(hitInfo.collider.gameObject);
        //}

        var hits = Physics.RaycastAll(mouseRay, MaxDistance, RaycastLayerMask, TriggerInteraction);
        if (hits == null || hits.Length <= 0)
        {
            SelectedObjectManager.SetSelectedObject(null);
            return;
        }

        float minDist = float.MaxValue;
        //RaycastHit closestHit = new RaycastHit();
        GameObject closestObj = null;

        foreach (var hit in hits)
        {
            var dist = Vector3.Distance(hit.point, mouseRay.origin);
            //Debug.Log($"Raycast hit {hit.collider.name} dist {dist:F2}");

            //if (hit.collider.gameObject.GetComponent<SelectableObject>() == null &&
            //    hit.collider.gameObject.GetComponent<ISelectableObjectAction>() == null &&
            //    hit.collider.gameObject.GetComponent<ISelectableObjectInfo>() == null)
            //    continue;

            //if (dist < minDist)
            //{
            //    minDist = dist;
            //    closestHit = hit;
            //}

            if (dist >= minDist)
                continue;

            if (IsObjectSelectable(hit.collider.gameObject))
            {
                minDist = dist;
                closestObj = hit.collider.gameObject;
            }
            else if (hit.rigidbody != null && IsObjectSelectable(hit.rigidbody.gameObject))
            {
                minDist = dist;
                closestObj = hit.rigidbody.gameObject;
            }            
        }

        //if (closestHit.collider == null || closestHit.collider.gameObject == null)
        //    SelectedObjectManager.SetSelectedObject(null);
        //else
        //    SelectedObjectManager.SetSelectedObject(closestHit.collider.gameObject);

        SelectedObjectManager.SetSelectedObject(closestObj);
    }
}
