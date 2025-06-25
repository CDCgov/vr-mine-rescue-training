using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MineMapControllerVRDebug : MineMapController
{
    public MineMapSymbolManager MineMapSymbolManagerOverride;
    public VRDebugUIController VRDebugUIControllerRef;

    protected override void Start()
    {        
        //_symbolManager = MineMapSymbolManagerOverride;
        //MineMapSymbolRenderer[] startSymbols = GameObject.FindObjectsOfType<MineMapSymbolRenderer>();
        //foreach (MineMapSymbolRenderer sym in startSymbols)
        //{
        //    if (sym.ShowOnMapMan)
        //    {
        //        var symbol = Instantiate<MineMapSymbol>(sym.Symbol);
        //        symbol.WorldPosition = sym.transform.position;
        //        symbol.WorldRotation = sym.transform.rotation;
        //        symbol.Color = sym.Color;
        //        symbol.DoNotDelete = true;

        //        //VectorMineMap.ActiveSymbols.Add(symbol);
        //        _symbolManager.AddSymbol(symbol);
        //    }
        //}
        if(VRDebugUIControllerRef == null)
        {
            VRDebugUIControllerRef = GetComponentInParent<VRDebugUIController>();
        }
        TeleportButton.onClick.AddListener(AdditionalTeleportButtonClickBhvr);
        base.Start();
    }

    protected void AdditionalTeleportButtonClickBhvr()
    {
        if(VRDebugUIControllerRef != null)
            VRDebugUIControllerRef.CloseMenu();
    }
    protected override void OnMapClicked(MineMapClickedEventData eventData)
    {
        Vector2 pos = eventData.PointerEvent.position;
        Debug.Log("Pos preprocessed: " + pos);
        //Debug.Log($"Map Clicked {pos.ToString()}");
        Debug.Log("World pos? " + eventData.PointerEvent.pointerCurrentRaycast.worldPosition);
        
        var rt = MineMap.GetComponent<RectTransform>();

        Vector2 size = Vector2.Scale(rt.rect.size, rt.transform.lossyScale);
        //Vector2 size = Vector2.Scale(rt.rect.size, new Vector2(0.005f, 0.005f));
        size = Vector2.Scale(size, new Vector2(5, 5));
        var r = new Rect((Vector2)rt.transform.position - (size * 0.5f), size);


        Vector2 freshPos = RectTransformUtility.WorldToScreenPoint(eventData.PointerEvent.pressEventCamera, 
            eventData.PointerEvent.pointerCurrentRaycast.worldPosition);
        RectTransform rect = MineMap.GetComponent<RectTransform>();
        

        Vector3 output = rect.InverseTransformPoint(eventData.PointerEvent.pointerCurrentRaycast.worldPosition);
        //Debug.Log("Offset: " + rect.offsetMin);
        Vector2 fixAttempt = new Vector2(output.x, output.y);
        Debug.Log(rt.rect.size);
        //Debug.Log("Attempt to Fix: " + (fixAttempt.x + (rt.rect.size.x/2)) + "," + (fixAttempt.y + (rt.rect.size.y/2)));
        
        //Debug.Log("Local Position? " + output);
        //pos -= r.center;
        pos -= r.min;
        //pos /= Mathf.Max(r.width, r.height);
        //pos.x /= r.width;
        //pos.y /= r.height;
        Vector2 update = new Vector2(pos.x * 0.005f, pos.y * 0.005f);
        pos = update;

        Vector2 fixedPosition = new Vector2(fixAttempt.x + (rt.rect.size.x / 2), fixAttempt.y + (rt.rect.size.y / 2));
        
        //Vector3 world = MineMap.CanvasSpaceToWorld(pos);
        Vector3 world = MineMap.CanvasSpaceToWorld(fixedPosition);

        if (eventData.PointerEvent.button == PointerEventData.InputButton.Left && TeleportButton != null)
        {
            //ClearSelectedTeleportPoint();
            //foreach (var data in _activeTeleportPoints)
            //{                
            //    var dist = Vector3.Distance(world, data.POI.transform.position);
            //    if (dist < 5)
            //    {
            //        _selectedTeleportPoint = data;
            //        TeleportButton.gameObject.SetActive(true);
            //        if (data.Symbol != null && _symbolManager != null)
            //        {
            //            data.Symbol.Color = Color.red;
            //            _symbolManager.UpdateSymbolColor(data.Symbol);
            //        }
            //        break;
            //    }
            //}

            world.y = 0;
            ClearSelectedTeleportPoint();

            float minDist = 5.5f;
            POIData closestPOI = null;

            foreach (var data in _activeTeleportPoints)
            {
                if (data.POI == null)
                    continue;

                var poiPos = data.POI.transform.position;
                poiPos.y = 0;

                var dist = Vector3.Distance(world, poiPos);
                if (dist < minDist)
                {
                    closestPOI = data;
                    minDist = dist;
                }
            }

            if (closestPOI != null)
            {
                //SetSelectedTeleportPoint(closestPOI);
                _selectedTeleportPoint = closestPOI;
                TeleportButton.gameObject.SetActive(true);
                if (closestPOI.Symbol != null && _symbolManager != null)
                {
                    closestPOI.Symbol.Color = Color.red;
                    _symbolManager.UpdateSymbolColor(closestPOI.Symbol);
                }
            }
        }


    }
}
