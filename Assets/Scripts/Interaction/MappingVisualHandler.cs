using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.HID;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.EventSystems;

public class MappingVisualHandler : MonoBehaviour
{
    //public Transform WorldBottomLeftAnchor;
    //public Transform WorldTopRightAnchor;
    //public Transform MapBottomLeftAnchor;
    //public Transform MapTopRightAnchor;

    public Transform PencilSocketTransform;
    public RectTransform DragAndDropMenuTransform;
    public Transform ClockTransform;
    //public Material ItemMaterial;
    //public GameObject MapText;
    //public GameObject MapItem;
    public GameObject CanvasGameObject;
    public GameObject CanvasMapItem;
    public GameObject RadialButtonPrefab;
    public GameObject HorizontalFallPrefab;
    public GameObject VerticalFallPrefab;
    public GameObject CurtainItemPrefab;
    public GameObject GasReadingItemPrefab;
    public GameObject PowerCenterItemPrefab;
    public GameObject RefugeItemPrefab;
    public GameObject XItemPrefab;
    public Transform Controller;
    public Canvas MapCanvas;
    public AudioSource PencilSfx;
    public AudioSource EraserSfx;

    public VectorMineMap VecMineMap;
    public RectTransform AllowedDragDropArea;
    public MineMapSymbolManager SymbolManager;
    public VentilationManager VentilationManager;
    public float DeleteSymbolRange = 1;
    public PencilSnapBehavior PencilBehavior;
    public Color SelectedColor = Color.green;
    public GameObject ErrorBox;
    //public TextMeshProUGUI ErrorBoxText;
    //public AudioSource ErrorBoxAudio;

    public string DragAndDropAddress = "Intentionally left blank";

    //public MineNetwork _MineNetwork;
    //private MineAtmosphere _Atmosphere;

    private GameObject _CurrentRadialButton;
    //private Vector3 _positionToSpawnMapItem;
    private MineMapSymbol _symbolSelected;
    private Vector3 _dragAndDropLoc;

    private NetworkedObject _netObj;
    private int _selectedSymbolIndex = 0;
    //MineMapSymbol symToSelect = null;
    private List<MineMapSymbol> _cachedSymbols;
    private float _gasReadingMaxDistance = Mathf.Infinity;
    private TeleportManager TeleportManager;
    private GameObject _spawnedErrorBox;
    private Color _cachedColor = Color.white;

    private void Awake()
    {
        _cachedSymbols = new List<MineMapSymbol>();
    }

    // Start is called before the first frame update
    void Start()
    {

        var systemManager = SystemManager.GetDefault();
        if (systemManager != null && systemManager.SystemConfig != null)
        {
            transform.localScale = systemManager.SystemConfig.MapBoardScale.ToVector3();
            _gasReadingMaxDistance = systemManager.SystemConfig.MapManMaxDistance;
            if(_gasReadingMaxDistance < 0)
            {
                _gasReadingMaxDistance = Mathf.Infinity;
            }
        }
       
        if (VentilationManager == null)
            VentilationManager = VentilationManager.GetDefault(gameObject);

        if(TeleportManager == null)
        {
            TeleportManager = TeleportManager.GetDefault(gameObject);
        }

        _netObj = GetComponent<NetworkedObject>();
      
        bool instantiateNetworkedSymbols = false;
        if (_netObj == null || _netObj.HasAuthority)
            instantiateNetworkedSymbols = true;

        SymbolManager.AddSceneSymbols(instantiateNetworkedSymbols);

        if (_netObj != null)
        {
            if (!_netObj.HasAuthority)
            {
                //DragAndDropMenuTransform.gameObject.SetActive(false);
                if (VecMineMap.TryGetComponent<MapmanGraphicsRaycastManager>(out var raycastManager))
                    raycastManager.enabled = false;

                if (VecMineMap.TryGetComponent<Button>(out var button))
                    button.enabled = false;

                if (CanvasGameObject.TryGetComponent<GraphicRaycaster>(out var graphicRaycaster))
                    graphicRaycaster.enabled = false;

                if (CanvasGameObject.TryGetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>(out var trackedDevRaycaster))
                    trackedDevRaycaster.enabled = false;

                if (CanvasGameObject.TryGetComponent<Canvas>(out var canvas))
                    canvas.worldCamera = null;

                //Turn off interactions for the drag and drop buttons but still display them
                foreach (Transform child in DragAndDropMenuTransform)
                {
                    child.TryGetComponent<Button>(out var btn);
                    if(btn != null)
                    {
                        btn.enabled = false;
                    }
                    child.TryGetComponent<MineMapDragAndDrop>(out var dd);
                    if(dd != null)
                    {
                        dd.enabled = false;
                    }
                }

                //return;
            }
        }
        
    }


    public void SetMapHandedness(PlayerDominantHand playerDominantHand)
    {

    }

    public void SetMapItemSpawnPosition()
    {
        Debug.LogError("Error: Call to SetMapItemSpawnPosition");
    }

    public void SpawnRadialButton(BaseEventData eventData)
    {
        if (_netObj != null && !_netObj.HasAuthority)
            return;


        var pointerEventData = (PointerEventData)eventData;

        SpawnRadialButton(pointerEventData.pointerCurrentRaycast.worldPosition, null);
    }

    private void ScaleToFixedMapSize(Transform xform)
    {
        ScaleToFixedMapSize(xform, Vector3.one);
    }

    private void ScaleToFixedMapSize(Transform xform, Vector3 baseScale)
    {
        if (VecMineMap == null || VecMineMap.transform.parent == null)
            return;

        //var scale = new Vector3(0.5f, 0.5f, 0.5f);
        var scale = baseScale;
        //var scaleCorrection = 1.0f / VecMineMap.transform.lossyScale.x;
        var scaleCorrection = 1.0f / VecMineMap.transform.parent.localScale.x;
        scale *= scaleCorrection;
        //_CurrentRadialButton.transform.localScale = scale;

        xform.localScale = scale;
    }

    private void SetMapUIAnchors(GameObject go)
    {
        var rt = go.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector3.zero;
            rt.anchorMax = Vector3.zero;
        }

    }

    public void SpawnRadialButton(Vector3 worldPos, MineMapSymbol symToSelect)
    {
        if(_netObj != null && !_netObj.HasAuthority)
            return;

        bool buttonInstantiated = false;
        if (_CurrentRadialButton != null)
        {
            if (Vector3.Distance(VecMineMap.WorldCanvasSpaceToWorld(worldPos), VecMineMap.WorldCanvasSpaceToWorld(_CurrentRadialButton.transform.position)) > 2)
            {
                //DestroyRadialButton(false);
                //_CurrentRadialButton = GameObject.Instantiate(RadialButtonPrefab, VecMineMap.transform);
                buttonInstantiated = true;
            }
        }
        else
        {
            _CurrentRadialButton = GameObject.Instantiate(RadialButtonPrefab, VecMineMap.transform);
            SetMapUIAnchors(_CurrentRadialButton);
            buttonInstantiated = true;
        }

        var world = VecMineMap.WorldCanvasSpaceToWorld(worldPos);

        if (symToSelect == null)
            symToSelect = GetNextSymbolToSelect(world, buttonInstantiated);

        if (symToSelect != null)
        {
            //VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
            SetSelectedSymbol(symToSelect);

            ((RectTransform)_CurrentRadialButton.transform).anchoredPosition = VecMineMap.WorldSpaceToCanvas(symToSelect.WorldPosition);

            _CurrentRadialButton.transform.localRotation = Quaternion.identity;
            ScaleToFixedMapSize(_CurrentRadialButton.transform, new Vector3(0.5f, 0.5f, 0.5f));

            RadialMenu radialMenu = _CurrentRadialButton.GetComponent<RadialMenu>();
            radialMenu.MapVis = this;
            radialMenu.SelectedSymbol = _symbolSelected;
            radialMenu.ClearAnnotations();
            radialMenu.SetMenuTitle(_symbolSelected);
            if (_symbolSelected.AllowManualRotations)
            {
                radialMenu.RotateButtons.SetActive(true);
            }
            else
            {
                radialMenu.RotateButtons.SetActive(false);
            }

            if (_symbolSelected.AllowFlipSymbol)
            {
                radialMenu.FlipButton.SetActive(true);
            }
            else
            {
                radialMenu.FlipButton.SetActive(false);
            }


            if (_symbolSelected.MineSymbolType != MineMapSymbol.SymbolType.GasCheck)
            {
                radialMenu.SetupDeleteButton();
                radialMenu.GasSelectionOff();
                if (_symbolSelected.AvailableAnnotations != null)
                {
                    radialMenu.AddAnnotations(_symbolSelected);
                }
                if (_symbolSelected.AllowColorChange)
                {
                    radialMenu.SetupColorButtons();
                }
                
            }
            else
            {
                radialMenu.GasSelectedOptions();
            }

        }
        else
        {
            DestroyRadialButton();
        }
    }

    private MineMapSymbol GetNextSymbolToSelect(Vector3 worldPos, bool updateNearestSymbolList)
    {
        MineMapSymbol symToSelect = null;
        if (updateNearestSymbolList || _cachedSymbols.Count <= 0)
        {
            VecMineMap.MineMapSymbolManager.GetAllSymbolsWithinRange(worldPos, 2.5f, _cachedSymbols, false);
            _selectedSymbolIndex = -1;
        }

        if (_cachedSymbols != null && _cachedSymbols.Count > 0)
        {
            _selectedSymbolIndex++;

            if (_selectedSymbolIndex >= _cachedSymbols.Count)
            {
                _selectedSymbolIndex = 0;
            }

            symToSelect = _cachedSymbols[_selectedSymbolIndex];

        }
        else
        {
            symToSelect = null;
        }

        return symToSelect;
    }

    public void MoveRadialMenu(Vector3 worldPos)
    {
        if (_CurrentRadialButton == null)
            return;

        _CurrentRadialButton.transform.localPosition = VecMineMap.transform.InverseTransformPoint(worldPos);
        //_positionToSpawnMapItem = transform.InverseTransformPoint(worldPos);

        _cachedSymbols.Clear();
    }

    public void SpawnRadialButtonDragAndDrop(Vector3 worldPos)
    {
        if (_CurrentRadialButton != null)
        {
            DestroyRadialButton();
        }
       
        _CurrentRadialButton = GameObject.Instantiate(RadialButtonPrefab, VecMineMap.transform);
        SetMapUIAnchors(_CurrentRadialButton);
     
        _CurrentRadialButton.transform.localPosition = VecMineMap.transform.InverseTransformPoint(worldPos);
        
        _CurrentRadialButton.transform.localEulerAngles = Vector3.zero;

        ScaleToFixedMapSize(_CurrentRadialButton.transform, new Vector3(0.5f, 0.5f, 0.5f));

        var world = VecMineMap.WorldCanvasSpaceToWorld(_CurrentRadialButton.transform.position);

        RadialMenu radialMenu = _CurrentRadialButton.GetComponent<RadialMenu>();
        radialMenu.MapVis = this;
        radialMenu.StartNumberEntry();
            
    }


    public void RotateCW()
    {
        //Debug.Log($"Rotating symbol: {_symbolSelected.AddressableKey}");
        if(!string.IsNullOrEmpty(_symbolSelected.ClockWiseSymbolKey) && _symbolSelected.ClockWiseSymbolKey != "")
        {
            VecMineMap.MineMapSymbolManager.InstantiateSymbol(_symbolSelected.ClockWiseSymbolKey, _symbolSelected.WorldPosition, Quaternion.identity);
            VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
        }
        else
        {
            VecMineMap.RotateSymbol(_symbolSelected, 90);
            //Quaternion rot = _symbolSelected.WorldRotation;
            
            //rot *= Quaternion.Euler(0, 90, 0);
            //if (_symbolSelected.IsAnnotation)
            //{
            //    VecMineMap.MineMapSymbolManager.InstantiateTextAnnotation(_symbolSelected.AddressableKey, _symbolSelected.WorldPosition, rot, _symbolSelected.SymbolText, _symbolSelected.ParentSymbol);
            //}
            //else
            //{
            //    VecMineMap.MineMapSymbolManager.InstantiateSymbol(_symbolSelected.AddressableKey, _symbolSelected.WorldPosition, rot, overrideRotation: true);
            //}
        }
       
        
        //VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
        SetSelectedSymbol(null);
        DestroyRadialButton();
    }

    public void RotateCCW()
    {
        if(!string.IsNullOrEmpty(_symbolSelected.CounterClockwiseSymbolKey) && _symbolSelected.CounterClockwiseSymbolKey != "")
        {
            VecMineMap.MineMapSymbolManager.InstantiateSymbol(_symbolSelected.CounterClockwiseSymbolKey, _symbolSelected.WorldPosition, Quaternion.identity);
            VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
        }
        else
        {
            VecMineMap.RotateSymbol(_symbolSelected, -90);
            //Quaternion rot = _symbolSelected.WorldRotation;
            //rot *= Quaternion.Euler(0, -90, 0);
            //if (_symbolSelected.IsAnnotation)
            //{
            //    VecMineMap.MineMapSymbolManager.InstantiateTextAnnotation(_symbolSelected.AddressableKey, _symbolSelected.WorldPosition, rot, _symbolSelected.SymbolText, _symbolSelected.ParentSymbol);
            //}
            //else
            //{
            //    VecMineMap.MineMapSymbolManager.InstantiateSymbol(_symbolSelected.AddressableKey, _symbolSelected.WorldPosition, rot, overrideRotation: true);
            //}
        }
       
        
        //VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
        SetSelectedSymbol(null);
        DestroyRadialButton();
    }

    public void AddGasReadingFromMenu(string value)
    {
       
        //TODO: Need a text field
        AddMapItem("Gas Reading", "MineMapSymbols/GasReading");
        DestroyRadialButton();
    }


    public void AddGasReading(string values)
    {
        var world = VecMineMap.WorldCanvasSpaceToWorld(_CurrentRadialButton.transform.position);
        world.y = 0;


        //VecMineMap.MineMapSymbolManager.InstantiateTextSymbol("MineMapSymbols/GasReadingText", world, Quaternion.identity, values);
        VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/GasReadingText", world, Quaternion.identity, values);
        //Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if(PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
        DestroyRadialButton();
    }

    public void AddText(string text, string address)
    {
        var world = VecMineMap.WorldCanvasSpaceToWorld(_CurrentRadialButton.transform.position);
        world.y = 0;

        VecMineMap.MineMapSymbolManager.InstantiateTextSymbol(address, world, Quaternion.identity, text);
        //Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
        DestroyRadialButton();
    }
    public void AddTextNoDestroy(string text, string address)
    {
        var world = VecMineMap.WorldCanvasSpaceToWorld(_CurrentRadialButton.transform.position);
        world.y = 0;

        VecMineMap.MineMapSymbolManager.InstantiateTextSymbol(address, world, Quaternion.identity, text);
        //Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
    }

    public void AddAnnotation(string annotation, MineMapSymbol symbolToAnnotate)
    {
        GameObject go;
        VecMineMap.GetSymbolObject(symbolToAnnotate, out go);
        if (symbolToAnnotate.ChildSymbol != null)
        {
            if (annotation != "Removed")
            {
                VecMineMap.MineMapSymbolManager.RemoveSymbol(symbolToAnnotate.ChildSymbol);
            }
            
        }
        Vector3 world = symbolToAnnotate.WorldPosition;
        Vector2 symPosOnMap = VecMineMap.WorldSpaceToCanvas(world);
        Quaternion rot = Quaternion.identity;

        
        if (annotation != "Removed")
        {
            if (symbolToAnnotate.SpanEntry)
            {
                if (symbolToAnnotate.WorldRotation == Quaternion.identity)
                {
                    symPosOnMap.x += symbolToAnnotate.Size.x + 5;
                    rot = Quaternion.Euler(0, 270, 0);
                }
                else
                {
                    symPosOnMap.y -= symbolToAnnotate.Size.x + 5;
                }
            }
            else
            {
                if (symbolToAnnotate.WorldRotation == Quaternion.identity)
                {
                    symPosOnMap.y -= (symbolToAnnotate.Size.y / 2) + 4;
                }
                else
                {
                    symPosOnMap.y -= (symbolToAnnotate.Size.x / 2) + 4;
                }
            }
            world = VecMineMap.CanvasSpaceToWorld(symPosOnMap);
            VecMineMap.MineMapSymbolManager.InstantiateTextAnnotation("MineMapSymbols/Annotation", world, rot, annotation, symbolToAnnotate);
        }
        else
        {
            if (symbolToAnnotate.SpanEntry)
            {
                RectTransform rect = null;
                if (go != null)
                {
                    go.TryGetComponent<RectTransform>(out rect);
                    Debug.Log($"Rect Rotation?: {rect.localEulerAngles}");
                }

                if (rect != null)
                {
                    if (rect.localRotation == Quaternion.identity)
                    {
                        symPosOnMap.y -= (rect.rect.height / 2) + 4;
                    }
                    else
                    {
                        symPosOnMap.x -= (rect.rect.height / 2) + 27;
                    }
                }
                else
                {
                    if(symbolToAnnotate.WorldRotation == Quaternion.identity)
                    {
                        symPosOnMap.y -= symbolToAnnotate.Size.y + 2;
                    }
                    else
                    {
                        symPosOnMap.x -= symbolToAnnotate.Size.x + 37;
                    }
                }
                //if (rect.localRotation == Quaternion.identity)
                //{
                    
                //    if (go != null)
                //    {
                //        symPosOnMap.y -= (rect.rect.height / 2) + 4;
                //    }
                //    else
                //    {
                //        symPosOnMap.y -= symbolToAnnotate.Size.y + 2;
                //    }

                //}
                //else
                //{
                //    if (go != null)
                //    {                        
                //        symPosOnMap.x -= (rect.rect.height / 2) + 27;
                //    }
                //    else
                //    {
                //        symPosOnMap.x -= symbolToAnnotate.Size.x + 37;
                //    }
                //}
            }
            else
            {
                //if (symbolToAnnotate.WorldRotation == Quaternion.identity)
                //{                    
                //    symPosOnMap.x -= (symbolToAnnotate.Size.x / 2) + 13;//half of the annotation symbol's width, dunno about calling it up via code like this tho
                //}
                //else
                //{
                //    symPosOnMap.x -= (symbolToAnnotate.Size.y / 2) + 13;
                //}
                symPosOnMap.y -= (symbolToAnnotate.Size.y / 2) + 4;
            }
            world = VecMineMap.CanvasSpaceToWorld(symPosOnMap);
            VecMineMap.MineMapSymbolManager.InstantiateTextAnnotation("MineMapSymbols/RemovedAnnotation", world, rot, annotation, symbolToAnnotate);
        }



        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
        DestroyRadialButton();
    }

    public void AddDoorDragDrop(string values, Vector3 worldPos)
    {
        worldPos.y = 0;

        MineMapSymbol symToDel;
        float dist;
        VecMineMap.MineMapSymbolManager.GetNearestSymbol(worldPos, out symToDel, out dist);
        bool replaceStopping = false;
        bool doorSpawned = false;
        float doorDelRange = DeleteSymbolRange + 6;
        Vector3 cachedSymPos = symToDel.WorldPosition;
        if (symToDel != null)
        {
            if (dist <= doorDelRange)
            {
                switch (symToDel.MineSymbolType)
                {                    
                    case MineMapSymbol.SymbolType.PermanentStopping:
                        VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                        VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Mandoor", cachedSymPos, Quaternion.identity, values);
                        doorSpawned = true;
                        break;
                    case MineMapSymbol.SymbolType.Refuge:
                        Vector3 diff = worldPos - symToDel.WorldPosition;
                        Debug.Log($"Refuge pos diff: {diff.ToString()}");
                        float diffXAbs = Mathf.Abs(diff.x);
                        float diffZAbs = Mathf.Abs(diff.z);
                        VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                        if (diffXAbs > diffZAbs)
                        {
                            if(diff.x < 0)
                            {
                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/RA-DoorLeft", cachedSymPos, Quaternion.identity, values);
                            }
                            else
                            {
                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/RA-DoorRight", cachedSymPos, Quaternion.identity, values);
                            }
                        }
                        else
                        {
                            if (diff.z < 0)
                            {

                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/RA-DoorBottom", cachedSymPos, Quaternion.identity, values);
                            }
                            else
                            {
                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/RA-DoorTop", cachedSymPos, Quaternion.identity, values);
                            }
                        }
                        doorSpawned = true;
                        break;
                    case MineMapSymbol.SymbolType.Prefab:
                        Vector3 difference = worldPos - symToDel.WorldPosition;
                        Debug.Log($"Door to {symToDel.AddressableKey}");
                        switch (symToDel.AddressableKey)
                        {
                            case "MineMapSymbols/Overcast_EW":
                                VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                                if (difference.z > 0)
                                {
                                    VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_EW_N_Door", cachedSymPos, Quaternion.identity, values);
                                }
                                else
                                {
                                    VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_EW_S_Door", cachedSymPos, Quaternion.identity, values);
                                }
                                doorSpawned = true;
                                break;
                            case "MineMapSymbols/Overcast_NS":
                                VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                                if (difference.x > 0)
                                {
                                    VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_NS_E_Door", cachedSymPos, Quaternion.identity, values);
                                }
                                else
                                {
                                    VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_NS_W_Door", cachedSymPos, Quaternion.identity, values);
                                }
                                doorSpawned = true;
                                break;
                            case "MineMapSymbols/Overcast_EW_N_Door":
                                VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_EW_TwoDoor", cachedSymPos, Quaternion.identity, values);
                                doorSpawned = true;
                                break;
                            case "MineMapSymbols/Overcast_EW_S_Door":
                                VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_EW_TwoDoor", cachedSymPos, Quaternion.identity, values);
                                doorSpawned = true;
                                break;
                            case "MineMapSymbols/Overcast_NS_E_Door":
                                VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_NS_TwoDoor", cachedSymPos, Quaternion.identity, values);
                                doorSpawned = true;
                                break;
                            case "MineMapSymbols/Overcast_NS_W_Door":
                                VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                                VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Overcast_NS_TwoDoor", cachedSymPos, Quaternion.identity, values);
                                doorSpawned = true;
                                break;                            
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                
            }
        }

        if (!doorSpawned)
        {
            VecMineMap.MineMapSymbolManager.InstantiateTextSymbol("MineMapSymbols/Door", worldPos, Quaternion.identity, "D");
        }
        
        //Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
    }


    public void AddGasReadingDragDrop(string values, Vector3 worldPos)
    {
       
        worldPos.y = 0;
        
        if(Vector3.Distance(worldPos, TeleportManager.ActiveTeleportTarget.position) > _gasReadingMaxDistance)
        {
            //Run error!
            DoMappingError("Gas check out of range", worldPos);
            return;
        }

        MineMapSymbol symToDel;
        float dist;
        VecMineMap.MineMapSymbolManager.GetNearestSymbol(worldPos, out symToDel, out dist);
        if (symToDel != null)
        {            
            if (dist <= DeleteSymbolRange)
            {
                if (symToDel.MineSymbolType == MineMapSymbol.SymbolType.GasCheck)
                {
                    VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                }
            }
        }
        //VecMineMap.MineMapSymbolManager.InstantiateTextSymbol("MineMapSymbols/GasReadingText", world, Quaternion.identity, values);
        VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/GasReadingText", worldPos, Quaternion.identity, values);
        //Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
    }

    public void AddTimeDragDrop(string values, Vector3 worldPos)
    {
        worldPos.y = 0;

        MineMapSymbol symToDel;
        float dist;
        VecMineMap.MineMapSymbolManager.GetNearestSymbol(worldPos, out symToDel, out dist);
        
        //VecMineMap.MineMapSymbolManager.InstantiateTextSymbol("MineMapSymbols/GasReadingText", world, Quaternion.identity, values);
        VecMineMap.MineMapSymbolManager.InstantiateSymbol("MineMapSymbols/Time", worldPos, Quaternion.identity, values);
        //Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
    }

    public void DeleteItemDragAndDrop(Vector3 worldPos)
    {
        worldPos.y = 0;

        MineMapSymbol symToDel;
        float dist;
        VecMineMap.MineMapSymbolManager.GetNearestSymbol(worldPos, out symToDel, out dist);
        if (symToDel != null && !symToDel.DoNotDelete)
        {
            if (dist <= DeleteSymbolRange)
            {                
                VecMineMap.MineMapSymbolManager.RemoveSymbol(symToDel);
                if (EraserSfx != null)
                {
                    EraserSfx.Play();
                }                
            }
        }
    }


    public void AddMapItem(string label, GameObject mapItemPrefab)
    {
        GameObject item = GameObject.Instantiate(mapItemPrefab, CanvasGameObject.transform);
        //item.transform.localScale = new Vector3(1, 1, 1);
        //Vector3 posFix = _positionToSpawnMapItem;
        //posFix.y = 0.001f;
        item.transform.localPosition = _CurrentRadialButton.transform.localPosition;
        item.transform.localEulerAngles = new Vector3(0, 0, 0);
        //item.transform.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);
        item.name = label;
        MapUIItem mapIt = item.GetComponent<MapUIItem>();
        if (mapIt != null)
        {
            mapIt.Label.gameObject.SetActive(false);
        }
        DestroyRadialButton();
    }

    public void AddMapItem(string label, string address)
    {
        var world = VecMineMap.WorldCanvasSpaceToWorld(_CurrentRadialButton.transform.position);
        world.y = 0;

        VecMineMap.MineMapSymbolManager.InstantiateSymbol(address, world, Quaternion.identity);
        Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
        DestroyRadialButton();
    }

    public void AddMapItemDragAndDrop(string label, string address, Vector3 worldPos)
    {
        worldPos.y = 0;

        if (label == "")
            label = null;

        VecMineMap.MineMapSymbolManager.InstantiateSymbol(address, worldPos, Quaternion.identity, label, -1, false, (symbol) =>
        {
            //update symbol position using mine map layout
            //this is done primarily to set spanned symbols position to be centered in the entry/crosscut
            VecMineMap.PositionSymbol(symbol, worldPos, Quaternion.identity, true);
        });

        //Debug.Log("Spawn pos: " + new Vector2(_CurrentRadialButton.transform.localPosition.x, _CurrentRadialButton.transform.localPosition.y) + ", World pos: " + world);
        
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
    }

    public void DeleteMapItem()
    {
        if (_symbolSelected != null && !_symbolSelected.DoNotDelete)
        {
            VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
            SetSelectedSymbol(null);
            if (EraserSfx != null)
            {
                EraserSfx.Play();
            }
        }
        DestroyRadialButton();
    }

    public void DeleteMapItemWithoutClosing()
    {
        if (_symbolSelected != null && !_symbolSelected.DoNotDelete)
        {
            VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
            SetSelectedSymbol(null);
            if (EraserSfx != null)
            {
                EraserSfx.Play();
            }
        }
    }






    public void AddGasReadFromNetworkDragAndDrop(string label, Vector3 worldPos, bool isGasReading = true, Sprite spriteToUse = null)
    {
        worldPos.y = 0;

        MineAtmosphere atmosphere;
        VentilationManager.GetMineAtmosphere(new Vector3(worldPos.x, 1, worldPos.z), out atmosphere);
       
        //TODO: Add text item for Will's map
        //AddMapItem("Gas Reading", "MineMapSymbols/GasReading");
        var carbonMonixide = atmosphere.CarbonMonoxide * 1000000.0f;
        if (carbonMonixide > GasMeterDisplay.GasMeterMaxCOReading)
            carbonMonixide = GasMeterDisplay.GasMeterMaxCOReading;
        carbonMonixide = Mathf.Round(carbonMonixide);
        string text = "CH4: " + (atmosphere.Methane * 100).ToString("F1") + "%\n" + "CO: " + ((int)(carbonMonixide)) + "ppm\n" + "O2: " + (atmosphere.Oxygen * 100).ToString("F1") + "%";//NOTE: Removed 'ppm' temporarily from CO to make writing more narrow
        AddGasReadingDragDrop(text, worldPos);

    }

    public void AddTimeDragAndDrop(string label, Vector3 worldPos, bool isGasReading = true, Sprite spriteToUse = null)
    {
        worldPos.y = 0;

        int hour = System.DateTime.Now.Hour;
        int correctedHour = hour;
        if(hour > 12)
        {
            correctedHour = hour - 12;
        }

        string text = $"{correctedHour.ToString("00")}:{System.DateTime.Now.Minute.ToString("00")}";
        AddTimeDragDrop(text, worldPos);
    }


    public void AddGasReadFromNetwork(string label, bool isGasReading = true, Sprite spriteToUse = null)
    {
        var world = VecMineMap.WorldCanvasSpaceToWorld(_CurrentRadialButton.transform.position);
        world.y = 0;
        

        MineAtmosphere atmosphere;
        VentilationManager.GetMineAtmosphere(new Vector3(world.x, 1, world.z), out atmosphere);
       
        //TODO: Add text item for Will's map
        //AddMapItem("Gas Reading", "MineMapSymbols/GasReading");
        var carbonMonixide = atmosphere.CarbonMonoxide * 1000000.0f;
        if (carbonMonixide > GasMeterDisplay.GasMeterMaxCOReading)
            carbonMonixide = GasMeterDisplay.GasMeterMaxCOReading;
        carbonMonixide = Mathf.Round(carbonMonixide);
        string text = "CH4: " + (atmosphere.Methane * 100).ToString("F1") + "%\n" + "CO: " + ((int)(carbonMonixide)) + "ppm\n" + "O2: " + (atmosphere.Oxygen * 100).ToString("F1") + "%";//NOTE: Removed 'ppm' temporarily from CO to make writing more narrow
        AddGasReading(text);

        DestroyRadialButton();
    }

    public void AddHorizontalFall()
    {
        GameObject item = Instantiate(HorizontalFallPrefab, CanvasGameObject.transform);
        //item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localPosition = _CurrentRadialButton.transform.localPosition;
        item.transform.localEulerAngles = Vector3.zero;
        //item.transform.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);
        DestroyRadialButton();
    }

    public void AddVerticalFall()
    {
        GameObject item = Instantiate(VerticalFallPrefab, CanvasGameObject.transform);
        //item.transform.localScale = new Vector3(1, 1, 1);
        item.transform.localPosition = _CurrentRadialButton.transform.localPosition;
        item.transform.localEulerAngles = Vector3.zero;
        //item.transform.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);
        DestroyRadialButton();
    }

    public void DestroyRadialButton()
    {
        //do this better
        if (_CurrentRadialButton != null && _CurrentRadialButton.TryGetComponent<RadialMenu>(out var menu))
        {
            menu.CheckAndAcceptNumpad();
        }
        if (_symbolSelected != null)
            VecMineMap.SetSymbolColor(_symbolSelected, _cachedColor);
        Destroy(_CurrentRadialButton);
        _CurrentRadialButton = null;
        SetSelectedSymbol(null);
        VecMineMap.SelectedSymbol = null;
        //}
    }

    private void SetSelectedSymbol(MineMapSymbol symbol)
    {
        if (_symbolSelected != null && VecMineMap != null)
        {
            VecMineMap.SetSymbolColor(_symbolSelected, _cachedColor);
        }

        _symbolSelected = symbol;

        if (_symbolSelected != null && VecMineMap != null)
        {
            _cachedColor = _symbolSelected.Color;
            VecMineMap.SelectedSymbol = symbol;
            VecMineMap.SetSymbolColor(_symbolSelected, SelectedColor);
        }

    }

    public void SetUICanvasCamera(Camera cam)
    {
        MapCanvas.worldCamera = cam;
    }

    public void AddDragAndDropMapItem()
    {
        Debug.Log($"Address noted: {DragAndDropAddress}");
    }

    public void DoMappingError(string msg, Vector2 position)
    {
        if(_spawnedErrorBox != null)
        {
            Destroy(_spawnedErrorBox);
        }
        GameObject go = GameObject.Instantiate(ErrorBox, VecMineMap.transform);
        SetMapUIAnchors(go);
        ScaleToFixedMapSize(go.transform);
        
        MappingErrorBox mappingErrorBox;
        if (go.TryGetComponent<MappingErrorBox>(out mappingErrorBox))
        {
            _spawnedErrorBox = go;
            var boxText = mappingErrorBox.ErrorTextBox;
            boxText.text = msg;
            var rectTransform = mappingErrorBox.ErrorBoxRectTransform;
            
            Vector3 finalPos = rectTransform.localPosition;
            finalPos.x = position.x;
            finalPos.y = position.y;
            rectTransform.localPosition = finalPos;

            mappingErrorBox.OkButton.onClick.AddListener(CloseErrorBox);
            //ErrorBox.SetActive(true);
            //ErrorBoxAudio.Play();
            Invoke("CloseErrorBox", 2);
        }
    }

    public void CloseErrorBox()
    {
        //ErrorBox.SetActive(false);
        if(_spawnedErrorBox != null)
        {
            Destroy(_spawnedErrorBox);
        }
    }

    public bool PointInDragDropBounds(Vector3 worldPos)
    {
        if (AllowedDragDropArea == null)
            return true;

        Vector2 pos = AllowedDragDropArea.InverseTransformPoint(worldPos);
        return AllowedDragDropArea.rect.Contains(pos);
    }

    public void UpdateSymbolColor(Color color)
    {
        VecMineMap.SetSymbolColor(_symbolSelected, color);
        _cachedColor = color;
        if (PencilSfx != null)
        {
            PencilSfx.Play();
            if (PencilBehavior != null)
            {
                PencilBehavior.PencilShake();
            }
        }
        DestroyRadialButton();
    }

    public void FlipSymbol()
    {
        if(_symbolSelected != null)
        {
            //VecMineMap.MineMapSymbolManager.
            if (string.IsNullOrEmpty(_symbolSelected.FlipSymbolKey))
            {
                //Quaternion rot = _symbolSelected.WorldRotation;
                //rot *= Quaternion.Euler(0, 180, 0);
                
                //VecMineMap.MineMapSymbolManager.InstantiateSymbol(_symbolSelected.AddressableKey, _symbolSelected.WorldPosition, rot, overrideRotation: true);
                VecMineMap.FlipSymbolXScale(_symbolSelected);
            }
            else
            {
                VecMineMap.MineMapSymbolManager.InstantiateSymbol(_symbolSelected.FlipSymbolKey, _symbolSelected.WorldPosition, _symbolSelected.WorldRotation);
                VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
                SetSelectedSymbol(null);
            }

            //VecMineMap.MineMapSymbolManager.RemoveSymbol(_symbolSelected);
            //SetSelectedSymbol(null);
            DestroyRadialButton();
            if (PencilSfx != null)
            {
                PencilSfx.Play();
                if (PencilBehavior != null)
                {
                    PencilBehavior.PencilShake();
                }
            }
        }
    }
}
