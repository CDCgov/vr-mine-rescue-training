using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverviewRaycastHoverTooltip : MonoBehaviour
{
    public Camera OverviewCamera;
    public RectTransform OverviewRect;
    public float HoverTime = 0.5f;
    public EventTooltipHandler TooltipHandler;
    public VentilationManager VentilationManager;
    public TMPro.TextMeshProUGUI VentLabel;

    public SessionEventManager SessionEventManager;

    private Vector3 _lastHitPosition;
    private float _enableTooltipTime = Mathf.Infinity;
    private Vector3 _mouseDownPos = Vector3.zero;

    private void Start()
    {
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = OverviewCamera.ScreenPointToRay(Input.mousePosition);
        LayerMask mask = (1 << 9);
        LayerMask mas2 = (1 << 21);
        LayerMask mask3 = (1 << 22);
        LayerMask interimMask = mask | mas2;
        LayerMask layerMask = ~(interimMask | mask3);
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        float distance = Mathf.Infinity;
        bool success = false;
        RaycastHit priortyHit = new RaycastHit();
        bool priority = false;
        int priorityVal = 1000000;
        if ((Input.GetMouseButtonDown(0)))
        {
            _mouseDownPos = Input.mousePosition;
            //Debug.Log(_mouseDownPos);
        }
        foreach (RaycastHit raycastHit in hits)
        {
            //Debug.Log("Raycast all result: " + raycastHit.collider.name + " distance: " + raycastHit.distance);
            //MineEquipmentHost mElement1 = raycastHit.collider.GetComponentInParent<MineEquipmentHost>();
            //MineNPCHost mNPC1 = raycastHit.collider.GetComponentInParent<MineNPCHost>();
            //MineSegment mSegment1 = raycastHit.collider.GetComponentInParent<MineSegment>();
            //MineVehicleHost mVehicle1 = raycastHit.collider.GetComponentInParent<MineVehicleHost>();
            //MineVentControlHost mVent1 = raycastHit.collider.GetComponentInParent<MineVentControlHost>();
            IMineInformation mineInformation1 = raycastHit.collider.GetComponentInParent<IMineInformation>();

            if (mineInformation1 == null)
            {
                continue;
            }
            //if (mSegment1 != null)
            //{
            //    if (raycastHit.distance < distance)
            //    {
            //        hit = raycastHit;
            //        distance = raycastHit.distance;
            //    }
            //}
            //else
            //{
            //    if (raycastHit.distance < distance)
            //    {
            //        hit = raycastHit;
            //        distance = raycastHit.distance;
            //    }
            //    priortyHit = raycastHit;
            //    priority = true;
            //}



            if (mineInformation1.GetPriority() < priorityVal)
            {
                priorityVal = mineInformation1.GetPriority();
                hit = raycastHit;
            }
            else if (mineInformation1.GetPriority() == priorityVal)
            {
                if (raycastHit.distance < hit.distance)
                {
                    hit = raycastHit;
                }
            }
            success = true;
        }
        //if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        if (!success)
        {
            return;
        }

        if (priority)
        {
            hit = priortyHit;
        }
        //MineEquipmentHost mElement = hit.collider.GetComponentInParent<MineEquipmentHost>();
        //MineNPCHost mNPC = hit.collider.GetComponentInParent<MineNPCHost>();
        //MineSegment mSegment = hit.collider.GetComponentInParent<MineSegment>();
        //MineVehicleHost mVehicle = hit.collider.GetComponentInParent<MineVehicleHost>();
        //MineVentControlHost mVent = hit.collider.GetComponentInParent<MineVentControlHost>();
        IMineInformation mineInformation = hit.collider.GetComponentInParent<IMineInformation>();
        //if (mElement == null && mNPC == null && mSegment == null && mVehicle == null && mVent == null)
        //{
        //    return;
        //}

        //if(mineInformation != null)
        //{
        //    Debug.Log("Mine info successfull! " + mineInformation.GetMineInfo(hit.point));
        //}
        var airway = VentilationManager.FindClosestAirway(hit.point);
        double speed = 0;
        MineAtmosphere mineAtmo = new MineAtmosphere();
        if (airway != null)
        {
            speed = airway.MFAirway.FlowRate;
        }
        
        //use 0.2f meters above ground plane as sample position
        var pos = hit.point;
        pos.y = 0.2f;

        if (VentilationManager.GetMineAtmosphere(pos, out mineAtmo))
        {
            int co = (int)(mineAtmo.CarbonMonoxide * 1000000.0f);

            VentLabel.text = $"Methane: {(mineAtmo.Methane * 100).ToString("F2")}% \nCO: {co}ppm \nOxygen: { (mineAtmo.Oxygen * 100).ToString("F2")}% \nAirspeed: { (speed).ToString("F1")}cfm ";
        }

        if (hit.point != _lastHitPosition)
        {
            _lastHitPosition = hit.point;
            _enableTooltipTime = Time.time + HoverTime;
        }
        else
        {



            if (Input.GetMouseButtonUp(0) && (Input.mousePosition == _mouseDownPos) && 
                RectTransformUtility.RectangleContainsScreenPoint(OverviewRect, Input.mousePosition))
            {


                string info = "";
                //Debug.Log($"MDPos: {_mouseDownPos}, {Input.mousePosition}");
                info = mineInformation.GetMineInfo(hit.point);
                //if (mElement != null)
                //{
                //    info = $"\u2022 Name: {mElement.GetMineElement().DisplayName}\n\u2022 Item Health: {mElement.GetMineElement().Health}\n\u2022 Position: {hit.point.x.ToString("F2")},{hit.point.z.ToString("F2")}";
                //}
                //else if(mNPC != null)
                //{
                //    info = $"\u2022 Name: {mNPC.MineNPC.DisplayName}\n\u2022 Item Health: {mNPC.MineNPC.Health}\n\u2022 Status: {mNPC.MineNPC.Status}\n\u2022 Position: {hit.point.x.ToString("F2")},{hit.point.z.ToString("F2")}";
                //}
                //else if(mVehicle != null)
                //{
                //    info = $"\u2022 Name: {mVehicle.MineVehicle.DisplayName}\n\u2022 Item Health: {mVehicle.MineVehicle.Health}\n\u2022 Position: {hit.point.x},{hit.point.z}";
                //}
                //else if(mSegment != null)
                //{
                //    string section = mSegment.GetObjectDisplayName();
                //    section.Replace('_', ' ');
                //    MineAtmosphere mineAtmo;
                //    if (VentilationManager.GetMineAtmosphere(hit.point, out mineAtmo))
                //    {
                //        info = $"\u2022 Mine Segment: {section}\n\u2022 Methane: {(mineAtmo.Methane*100).ToString("F2")}%\n\u2022 CO: {(int)mineAtmo.CarbonMonoxide} ppm\n\u2022 Oxygen: {(mineAtmo.Oxygen*100).ToString("F2")}%\n\u2022 Position: {hit.point.x.ToString("F2")},{hit.point.z.ToString("F2")}";
                //    }
                //    else
                //    {
                //        info = $"\u2022 Mine Segment: {section}\n\u2022 Position: {hit.point.x.ToString("F2")},{hit.point.z.ToString("F2")}";
                //    }
                //}
                //else if(mVent != null)
                //{
                //    info = $"\u2022 Name: {mVent.MineVentControl.DisplayName}\n\u2022 Item Health: {mVent.MineVentControl.Health}\n\u2022 Position: {hit.point.x.ToString("F2")},{hit.point.z.ToString("F2")}";
                //}

                //info = $"\u2022";
                //info = hit.collider.name;
                TooltipHandler.ActivateTooltipHover(info, new Vector2(hit.point.x, hit.point.z));
                _enableTooltipTime = Mathf.Infinity;
            }



        }

    }


    private void OnEnable()
    {
        _lastHitPosition = new Vector3(Mathf.PI, 12345, -Mathf.PI);//there's a chance that Vector3.zero could be the first hover point, this should avoid that problem
        _enableTooltipTime = Mathf.Infinity;
    }

    public void ClearHighlight()
    {

    }
}
