using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
//using UnityEditor.Experimental.GraphView;

enum GasReadingState
{
    Off,
    O2Reading,
    COReading,
    CH4Reading,
    H2SReading,
    Complete
}
public class RadialMenu : MonoBehaviour
{
    public Vector3 MapPosition;
    public MappingVisualHandler MapVis;
    public TextMeshProUGUI DisplayLabel;
    public TextMeshProUGUI InfoLabel;
    public TextMeshProUGUI NumpadOutputDisplay;
    public TextMeshProUGUI MenuLabel;
    public GameObject[] NumberButtons;
    public GameObject[] InterfaceButtons;
    public GameObject OKButton;
    public GameObject AutoGasButton;
    public GameObject CloseButton;
    public GameObject ClearButton;
    public GameObject GasButton;
    public GameObject CurtainButton;
    public GameObject PowerCenterButton;
    public GameObject RefugeButton;
    public GameObject RooffallButton;
    public GameObject HorizontalFallButton;
    public GameObject VerticalFallButton;
    public GameObject NumberDisplay;
    public GameObject ItemButtons;
    public GameObject DeleteButton;
    public GameObject UpdateButton;
    public GameObject AnnotateButton;

    public DragDropRegion SymbolMoveRegion;
    
    public GameObject Numpad;
    //public RectTransform BackgroundTransform;

    public Transform AnnotationsVertLayout;
    public GameObject RotateButtons;
    public GameObject ColorButtons;
    public GameObject FlipButton;

    [System.NonSerialized]
    public MineMapSymbol SelectedSymbol;

    private int _displayIndex = 0;
    private char[] _displayCharArray;
    private string _gasEntry;
    private GasReadingState GasReadingStatus;
    //Delay created, to prevent an observed double click behavior
    private float _clickDelayTime = 0;
    private CanvasGroup _canvasGroup;


    private void Start()
    {
        DisplayLabel.text = "";
        _displayCharArray = new char[3];
        for (int i = 0; i < _displayCharArray.Length; i++)
        {
            _displayCharArray[i] = '0';
        }
        if(MapVis == null)
        {
            Debug.LogError($"RadialMenu: MapVis reference not set on {gameObject.name}");
            MapVis = GameObject.FindObjectOfType<MappingVisualHandler>();
        }

        _gasEntry = "";
        GasReadingStatus = GasReadingState.Off;

        if (SymbolMoveRegion != null)
        {
            SymbolMoveRegion.BeginDrag += OnSymbolMoveBeginDrag;
            SymbolMoveRegion.EndDrag += OnSymbolMoveEndDrag;
            SymbolMoveRegion.Drag += OnSymbolMoveDrag;
        }

        TryGetComponent<CanvasGroup>(out _canvasGroup);
    }

    private void OnSymbolMoveDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (SelectedSymbol == null || MapVis == null || MapVis.VecMineMap == null)
            return;

        var worldPos = eventData.pointerCurrentRaycast.worldPosition;

        if (!MapVis.PointInDragDropBounds(worldPos))
            return;

        //_rectTransform.position = worldPos;
        //_priorPos = eventData.position;

        var rt = MapVis.VecMineMap.transform as RectTransform;

        Vector2 canvPos = rt.InverseTransformPoint(worldPos);
        canvPos.x -= rt.rect.x;
        canvPos.y -= rt.rect.y;

        var symbolPos = MapVis.VecMineMap.CanvasSpaceToWorld(canvPos);        
        MapVis.VecMineMap.PositionSymbol(SelectedSymbol, symbolPos, SelectedSymbol.WorldRotation, true);
    }

    private void OnSymbolMoveEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        ShowRadialMenu(true);

        if (SelectedSymbol == null || MapVis == null || MapVis.VecMineMap == null)
            return;


        //var worldPos = eventData.pointerCurrentRaycast.worldPosition;
        
        if (MapVis.VecMineMap.TryFindSymbolObjectWorldPosition(SelectedSymbol, out var worldPos))
            MapVis.MoveRadialMenu(worldPos);
    }

    private void OnSymbolMoveBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        ShowRadialMenu(false);
    }

    public void ShowRadialMenu(bool bShow)
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = bShow ? 1 : 0;
        _canvasGroup.interactable = bShow;
        _canvasGroup.blocksRaycasts = bShow;
    }

    private void OnDestroy()
    {        
        if (SymbolMoveRegion != null)
        {
            SymbolMoveRegion.BeginDrag -= OnSymbolMoveBeginDrag;
            SymbolMoveRegion.EndDrag -= OnSymbolMoveEndDrag;
            SymbolMoveRegion.Drag -= OnSymbolMoveDrag;
        }
    }

    public void AddNumber(int num)
    {        
        if(Time.time < _clickDelayTime)
        {
            return;
        }
        if (Numpad.activeSelf)
        {
            if (NumpadOutputDisplay.text.Length < 4)
            {
                NumpadOutputDisplay.text += num.ToString();
                _clickDelayTime = Time.time + 0.5f;
            }
        }
        else
        {
            if (num >= 0 && num <= 9)
            {
                DisplayLabel.text += num;
            }
            if (num == 10)
            {
                DisplayLabel.text += ".";
            }
        }
    }

    public void AcceptNumpad()
    {
        Numpad.SetActive(false);
        if (!string.IsNullOrEmpty(NumpadOutputDisplay.text))
        {
            MapVis.AddText(NumpadOutputDisplay.text, "MineMapSymbols/NumpadSymbol");
        }
        else
        {
            MapVis.AddText("#", "MineMapSymbols/NumpadSymbol");
        }
    }

    public void BackspaceButton()
    {
        if(NumpadOutputDisplay.text == null)
        {
            return;
        }
        if (Time.time < _clickDelayTime)
        {
            return;
        }
        if (NumpadOutputDisplay.text.Length > 0)
        {
            string sub = NumpadOutputDisplay.text.Substring(0, NumpadOutputDisplay.text.Length - 1);
            NumpadOutputDisplay.text = sub;
            _clickDelayTime = Time.time + 0.5f;
        }
    }

    public void OnClearButton()
    {
        DisplayLabel.text = "";
    }

    public void CloseMenu()
    {
        MapVis.DestroyRadialButton();
    }

    public void StartGasEntry()
    {
        foreach(GameObject ob in NumberButtons)
        {
            ob.SetActive(true);
        }
        //CurtainButton.SetActive(false);
        //RooffallButton.SetActive(false);
        //GasButton.SetActive(false);
        //PowerCenterButton.SetActive(false);
        //RefugeButton.SetActive(false);
        //HorizontalFallButton.SetActive(false);
        ItemButtons.SetActive(false);
        OKButton.SetActive(true);
        NumberDisplay.SetActive(true);
        ClearButton.SetActive(true);
        AutoGasButton.SetActive(true);
        InfoLabel.text = "O2 Value";
        GasReadingStatus = GasReadingState.O2Reading;         
    }

    public void StartNumberEntry()
    {
        //BackgroundTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, )
        ItemButtons.SetActive(false);
        Numpad.SetActive(true);
        MenuLabel.transform.parent.gameObject.SetActive(false);
        //DisplayLabel.text = "Number Entry";
    }

    public void OnAutoGasButton()
    {
        MapVis.AddGasReadFromNetwork("MFire Gas");
    }
    public void OkButton()
    {
        string labelVal = DisplayLabel.text;
        if(labelVal == "")
        {
            labelVal = "0";
        }
        switch (GasReadingStatus)
        {
            case GasReadingState.Off:
                break;
            case GasReadingState.O2Reading:
                _gasEntry += "O2: " + labelVal + "%\n";
                GasReadingStatus = GasReadingState.COReading;
                InfoLabel.text = "CO Value";
                DisplayLabel.text = "";
                break;
            case GasReadingState.COReading:
                _gasEntry += "CO: " + labelVal + " ppm\n";
                GasReadingStatus = GasReadingState.CH4Reading;
                InfoLabel.text = "CH4 Value";
                DisplayLabel.text = "";
                break;
            case GasReadingState.CH4Reading:
                _gasEntry += "CH4: " + labelVal + "%\n";
                GasReadingStatus = GasReadingState.H2SReading;
                GasReadingStatus = GasReadingState.Complete;
                DisplayLabel.text = "";
                //InfoLabel.text = "CO Value";
                foreach (GameObject ob in NumberButtons)
                {
                    ob.SetActive(false);
                }
                //MapVis.AddGasReadingFromMenu(_gasEntry);
                //MapVis.AddMapItem(_gasEntry, true);
                MapVis.AddGasReading(_gasEntry);
                break;
            case GasReadingState.H2SReading:
                //_gasEntry += "H2S: " + labelVal + "%";
                //GasReadingStatus = GasReadingState.Complete;
                //DisplayLabel.text = "";
                ////InfoLabel.text = "CO Value";
                //foreach (GameObject ob in NumberButtons)
                //{
                //    ob.SetActive(false);
                //}
                //MapVis.AddGasReadingFromMenu(_gasEntry);
                break;
            case GasReadingState.Complete:
                break;
            default:
                break;
        }
    }

    public void OnCurtainButton()
    {
        //Sprite sp = CurtainButton.GetComponent<Image>().sprite;
        //MapVis.AddMapItem("Curtain", false, sp);
        MapVis.AddMapItem("Curtain", "MineMapSymbols/CheckCurtain");
    }

    public void OnPowerCenterButton()
    {
        //Sprite sp = PowerCenterButton.GetComponent<Image>().sprite;
        //MapVis.AddMapItem("Power", false, sp);
        MapVis.AddMapItem("Power Center", "MineMapSymbols/PowerCenter");        
    }
    public void OnRefugeButton()
    {
        //Sprite sp = RefugeButton.GetComponent<Image>().sprite;
        //MapVis.AddMapItem("Refuge Alternative", false, sp);
        MapVis.AddMapItem("Refuge Chamber", "MineMapSymbols/RefugeAlternative");
    }

    public void OnGasTest()
    {
        MapVis.AddMapItem("Gas Test", "MineMapSymbols/GasTest");
    }

    public void OnLivePerson()
    {
        MapVis.AddMapItem("Live Person", "MineMapSymbols/LivePerson");
    }

    public void OnFireButton()
    {
        MapVis.AddMapItem("Fire", "MineMapSymbols/Fire");
    }

    public void OnFPAButton()
    {
        MapVis.AddMapItem("FPA", "MineMapSymbols/FurthestPointAdvanced");
    }

    public void OnObjectButton()
    {
        MapVis.AddMapItem("Power Center", "MineMapSymbols/Object");
    }

    public void OnFaceButton()
    {
        MapVis.AddMapItem("Face", "MineMapSymbols/Face");
    }

    public void OnDoorButton()
    {
        MapVis.AddMapItem("Face", "MineMapSymbols/Door");
    }

    public void AddMapItem(string address)
    {
        MapVis.AddMapItem("Mapman", address);
    }

    public void OnHorizontalRooffall()
    {
        MapVis.AddHorizontalFall();
    }

    public void OnVerticalRooffall()
    {
        MapVis.AddVerticalFall();
    }

    public void AddRoofFall()
    {
        MapVis.AddMapItem("Refuge Chamber", "MineMapSymbols/Caved");
    }

    public void OnRoofFallButton()
    {
        CurtainButton.SetActive(false);
        RooffallButton.SetActive(false);
        GasButton.SetActive(false);
        PowerCenterButton.SetActive(false);
        RefugeButton.SetActive(false);

        HorizontalFallButton.SetActive(true);
        VerticalFallButton.SetActive(true);
    }

    public void OnDeleteButton()
    {
        MapVis.DeleteMapItem();
    }

    public void SetupDeleteButton()
    {
        ItemButtons.SetActive(false);
        DeleteButton.SetActive(true);
    }

    public void GasSelectedOptions()
    {
        ItemButtons.SetActive(false);
        DeleteButton.SetActive(true);
        UpdateButton.SetActive(true);
    }

    public void GasSelectionOff()
    {
        UpdateButton.SetActive(false);
    }

    public void SetupColorButtons()
    {
        ColorButtons.SetActive(true);
    }

    public void OnUpdateGasButton()
    {
        MapVis.DeleteMapItemWithoutClosing();
        MapVis.AddGasReadFromNetwork("MFire Gas");
    }

    public void OnAnnotateButton(MineMapSymbol symbol, GameObject src)
    {
        TextMeshProUGUI text = src.GetComponentInChildren<TextMeshProUGUI>();
        //MapVis.AddAnnotation()
        MapVis.AddAnnotation(text.text, symbol);
    }

    public void ClearAnnotations()
    {
        if(AnnotationsVertLayout.childCount == 0)
        {
            return;
        }
        foreach (Transform item in AnnotationsVertLayout)
        {
            Destroy(item.gameObject);
        }
    }

    public void AddAnnotations(MineMapSymbol sym)
    {
        if(sym == null)
        {
            return;
        }
        
        List<string> annotations = sym.AvailableAnnotations;
        if (annotations == null)
            return;
        for (int i = 0; i < annotations.Count; i++)
        {
            GameObject newAnnotationBtn = Instantiate(AnnotateButton, AnnotationsVertLayout);
            newAnnotationBtn.SetActive(true);
            Button annotation = newAnnotationBtn.GetComponent<Button>();
            TextMeshProUGUI annotationText = annotation.GetComponentInChildren<TextMeshProUGUI>();

            annotationText.text = annotations[i];
            annotation.onClick.AddListener(delegate { OnAnnotateButton(sym, newAnnotationBtn); });
        }
    }

    public void SetMenuTitle(MineMapSymbol sym)
    {
        if(sym == null && sym.DisplayName != null)
        {
            return;
        }

        if (sym.DisplayName != "")
        {
            if (sym.ParentSymbol != null)
            {
                MenuLabel.text = $"{sym.ParentSymbol.DisplayName}:{sym.SymbolText}";
            }
            else if(sym.DisplayName == "Annotation")
            {
                MenuLabel.text = $"Annotation:{sym.SymbolText}";
            }
            else
            {
                MenuLabel.text = sym.DisplayName;
            }
        }
        else
        {
            string text = sym.AddressableKey;
            string subTex = text.Substring(text.LastIndexOf('/'));
            subTex = string.Concat(subTex.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            MenuLabel.text = subTex;
        }
    }

    public void OnRotateCWBtn()
    {
        MapVis.RotateCW();
    }

    public void OnRotateCCWBtn()
    {
        MapVis.RotateCCW();
    }

    public void SetColor(GameObject obj)
    {
        obj.TryGetComponent<Image>(out var image);
        Color color = Color.magenta;
        if(image != null)
        {
            color = image.color;
        }
        MapVis.UpdateSymbolColor(color);
    }

    public void OnFlipBtn()
    {
        MapVis.FlipSymbol();
    }

    public void CheckAndAcceptNumpad()
    {
        if (Numpad.activeSelf)
        {
            if (!string.IsNullOrEmpty(NumpadOutputDisplay.text))
            {
                MapVis.AddTextNoDestroy(NumpadOutputDisplay.text, "MineMapSymbols/NumpadSymbol");
            }
            else
            {
                MapVis.AddTextNoDestroy("#", "MineMapSymbols/NumpadSymbol");
            }
        }
    }
}
