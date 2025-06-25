using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class MineMapController : MonoBehaviour, IMinimizableWindow
{
    public POIManager POIManager;
    public NetworkManager NetworkManager;
    public TeleportManager TeleportManager;

    public VectorMineMap MineMap;
    public Button TeleportButton;
    public string TeleportSymbol;
    public string TeleportSelectedSymbol;

    protected MineMapSymbolManager _symbolManager;
    private Color _normalSymbolColor = Color.white;
    private TMPro.TMP_Text _teleportButtonText;

    protected class POIData
    {
        public PointOfInterest POI;
        public MineMapSymbol Symbol;
    }

    protected List<POIData> _activeTeleportPoints;

    protected POIData _selectedTeleportPoint;
    //private POIData _currentPOIPoint;

    // Use this for initialization
    protected virtual void Start()
    {
        _activeTeleportPoints = new List<POIData>();

        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);

        //use the mine map's symbol manager if set
        if (MineMap != null)
            _symbolManager = MineMap.MineMapSymbolManager;
        if (_symbolManager == null)
            _symbolManager = MineMapSymbolManager.GetDefault(gameObject);

        if (TeleportSymbol == null || TeleportSelectedSymbol == null)
        {
            Debug.LogError("Error : teleport symbols not set on mine map controller");
            return;
        }

        if (TeleportButton != null)
        {
            TeleportButton.gameObject.SetActive(false);
            TeleportButton.onClick.AddListener(OnTeleportClicked);

            _teleportButtonText = TeleportButton.GetComponentInChildren<TMPro.TMP_Text>();
        }

        MineMap.MapClicked += OnMapClicked;

        POIManager.POIAdded += OnPOIAdded;
        POIManager.POIRemoved += OnPOIRemoved;

        TeleportManager.AfterTeleport += OnAfterTeleport;

        foreach (var poi in POIManager.ActivePOIs)
        {
            OnPOIAdded(poi);
        }
    }

    void OnDestroy()
    {
        if (POIManager != null)
        {
            POIManager.POIAdded -= OnPOIAdded;
            POIManager.POIRemoved -= OnPOIRemoved;
        }

        if (TeleportManager != null)
        {
            TeleportManager.AfterTeleport -= OnAfterTeleport;
        }
    }

    private void OnAfterTeleport(Transform obj)
    {
        ClearSelectedTeleportPoint();
    }

    private void OnPOIRemoved(PointOfInterest poi)
    {
        if (poi.POIType == POIType.CameraPosition)
        {


            for (int i = 0; i < _activeTeleportPoints.Count; i++)
            {
                var data = _activeTeleportPoints[i];

                if (_activeTeleportPoints[i].POI == poi)
                {
                    _activeTeleportPoints.RemoveAt(i);
                    if (data.Symbol != null)
                        _symbolManager.RemoveSymbol(data.Symbol);
                    break;
                }
            }


        }

    }

    private async void OnPOIAdded(PointOfInterest poi)
    {
        if (poi.POIType == POIType.CameraPosition)
        {
            //_symbolManager.InstantiateSymbol(TeleportSymbol, poi.transform.position, Quaternion.identity);
            var symbol = await _symbolManager.InstantiateSymbolAsync(TeleportSymbol, poi.transform.position, Quaternion.identity);

            POIData data = new POIData
            {
                POI = poi,
                Symbol = symbol,
            };
            _normalSymbolColor = symbol.Color;

            _activeTeleportPoints.Add(data);
        }
    }

    protected void ClearSelectedTeleportPoint()
    {
        if (TeleportButton != null)
        {
            TeleportButton.gameObject.SetActive(false);
        }

        _selectedTeleportPoint = null;
        var activePOI = TeleportManager.ActivePOIName;

        if (string.IsNullOrEmpty(activePOI) && TeleportManager.ActiveTeleportTarget != null)
        {
            Debug.Log("MineMapController: Highlighting closest POI to start");
            var closestPOI = FindClosestPOI(TeleportManager.ActiveTeleportTarget.position);
            if (closestPOI != null)
            {
                activePOI = closestPOI.POI.ID;
            }
        }

        foreach (var data in _activeTeleportPoints)
        {
            if (data.Symbol == null)
                    continue;

            var color = _normalSymbolColor;

            if (data.POI.ID == activePOI)
                color = Color.green;

            data.Symbol.Color = color;
            _symbolManager.UpdateSymbolColor(data.Symbol);
        }

        //if (_currentPOIPoint != null)
        //{
        //    _currentPOIPoint.Symbol.Color = Color.green;
        //    _symbolManager.UpdateSymbolColor(_currentPOIPoint.Symbol);
        //}
    }

    protected void SetSelectedTeleportPoint(POIData data)
    {
        _selectedTeleportPoint = data;
        TeleportButton.gameObject.SetActive(true);
        if (data.Symbol != null && _symbolManager != null)
        {
            data.Symbol.Color = Color.red;
            _symbolManager.UpdateSymbolColor(data.Symbol);
        }

        if (_teleportButtonText != null && data.POI != null)
        {
            _teleportButtonText.text = $"Teleport: {data.POI.Name}";
        }
    }

    private void OnTeleportClicked()
    {
        
        if (_selectedTeleportPoint != null)
        {
            //NetworkManager.SendTeleportAll(_selectedTeleportPoint.POI.name, Time.time);
            //_currentPOIPoint = _selectedTeleportPoint;
            TeleportManager.TeleportToPOI(_selectedTeleportPoint.POI);
        }

        ClearSelectedTeleportPoint();
    }

    private int _testSymbolIndex = 0;

    public event Action<string> TitleChanged;

    protected virtual void OnMapClicked(MineMapClickedEventData eventData)
    {
        //Vector2 pos = eventData.WorldSpacePosition;
        //Debug.Log($"Map Clicked {pos.ToString()}");

        //var rt = MineMap.GetComponent<RectTransform>();

        //Vector2 size = Vector2.Scale(rt.rect.size, rt.transform.lossyScale);
        //var r = new Rect((Vector2)rt.transform.position - (size * 0.5f), size);
        //pos -= r.min;

        //Debug.Log(pos);

        //Vector3 world = MineMap.CanvasSpaceToWorld(pos);
        Vector3 world = eventData.WorldSpacePosition;

        if (eventData.PointerEvent.button == PointerEventData.InputButton.Left)
        {
            ClearSelectedTeleportPoint();

            //if (ResearcherCam != null)
            //{
            //    //var dmCam = ResearcherCam.GetComponent<DMCameraController>();
            //    //ResearcherCam.transform.position = new Vector3(worldPos.x, ResearcherCam.transform.position.y, worldPos.z);
            //    ResearcherCam.transform.position = new Vector3(world.x, ResearcherCam.transform.position.y, world.z);

            //    var rcam = ResearcherCam.GetComponent<ResearcherCamController>();
            //    if (rcam != null)
            //        rcam.FollowTransform(null);
            //}
        }
        else if (eventData.PointerEvent.button == PointerEventData.InputButton.Right && TeleportButton != null)
        {
            world.y = 0;
            ClearSelectedTeleportPoint();

            var closestPOI = FindClosestPOI(world);

            if (closestPOI != null)
                SetSelectedTeleportPoint(closestPOI);
            
        }
    }

    private POIData FindClosestPOI(Vector3 world)
    {
        float minDist = 5.5f;
        POIData closestPOI = null;

        foreach (var data in _activeTeleportPoints)
        {
            if (data.POI == null)
                continue;

            var pos = data.POI.transform.position;
            pos.y = 0;

            var dist = Vector3.Distance(world, pos);
            if (dist < minDist)
            {
                closestPOI = data;
                minDist = dist;
            }
        }

        return closestPOI;
    }

    public void Minimize(bool minimize)
    {
        gameObject.SetActive(minimize);
    }

    public string GetTitle()
    {
        return "Mine Map";
    }

    public void ToggleMinimize()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void AssignTaskbarButton(Button button)
    {

    }
}
