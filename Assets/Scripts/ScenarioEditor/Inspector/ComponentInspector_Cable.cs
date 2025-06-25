using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class ComponentInspector_Cable : ComponentInspector<ComponentInfo_Cable>
{
    //public int Index;
    public TMP_Text HeaderText;
    public SliderField CableSlackSliderField;
    public TMP_Dropdown CableTypeDropdown;//, HangerTypeDropdown;
    public Button ResetSlopeButton, DeleteCableButton;

    //private Inspector _inspector;
    private RuntimeCableEditor _cableEditor;
    //private ComponentInfo_Cable TargetComponentInfo;
    private HangingGeometry _component;
    private ComponentInspector_Lifeline _lifelineInspector;
    private ContextMenuController _contextMenu;
    private Placer _placer;
    private GameObject _cableDeletionPrompt;
    private Button _confirmDeleteCableButton, _cancelDeleteCableButton;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        InitializeReferences();
        InitializeValues();
        InitializeEvents();

        //set lifeline component state
        if (TargetComponentInfo && !TargetComponentInfo.IsLifeline && _lifelineInspector != null)
        {
            _lifelineInspector.gameObject.SetActive(false);
        }
    }
   
    void InitializeReferences()
    {
        _contextMenu = FindObjectOfType<ContextMenuController>();
        _lifelineInspector = FindObjectOfType<ComponentInspector_Lifeline>();
        _cableEditor = _contextMenu.CableEditor;
        _placer = _contextMenu.Placer;
        //_inspector = Inspector.instance;
        //TargetComponentInfo = _inspector.targetInfo.componentInfo_Cables[Index];
        _component = TargetComponentInfo.Component;
        _cableDeletionPrompt = _contextMenu.CableDeletionPrompt;
        _confirmDeleteCableButton = _contextMenu.ConfirmDeleteCableButton;
        _cancelDeleteCableButton = _contextMenu.CancelDeleteCableButton;
    }
    
    void InitializeValues()
    {
        Debug.Log("____________________________Init Dropdown");

        if (CableTypeDropdown != null)
            CableTypeDropdown.value = (int)TargetComponentInfo.CableType;

        //if (HangerTypeDropdown != null)
        //    HangerTypeDropdown.value = TargetComponentInfo.CableHangerIndex;
        
        if (_component != null)
        {
            //targetCableInfo.intensity = targetCableComponent.intensity;
            //targetCableInfo.InstantiateNodeGizmos();
            //_component.CableNodes = _info.CableNodes;

            //set cable dropdown
           

            //set hanger type
            

            // set cable slack
            CableSlackSliderField.ForceValue(TargetComponentInfo.CableSlack);
        }
    }
    
    void InitializeEvents()
    {
        if (CableTypeDropdown != null)
            CableTypeDropdown.onValueChanged.AddListener(ChangeCableTypeInspector);        
        //CableTypeDropdown.onValueChanged.AddListener(TargetComponentInfo.SetCableType);

        //if (HangerTypeDropdown != null)
        //    HangerTypeDropdown.onValueChanged.AddListener(TargetComponentInfo.SetHangerType);

        if (ResetSlopeButton != null)
            ResetSlopeButton.onClick.AddListener(_cableEditor.ResetAllCableSlack);

        if (CableSlackSliderField != null)
            CableSlackSliderField.onSubmitValue.AddListener(_cableEditor.SetAllCableSlack);

        if (DeleteCableButton != null)
            DeleteCableButton.onClick.AddListener(StartDeleteCable);

        _confirmDeleteCableButton.onClick.AddListener(ConfirmDeleteCable);
        _cancelDeleteCableButton.onClick.AddListener(CancelDeleteCable);
    }

    ///change inspector when cable type changes
    void ChangeCableTypeInspector( int value)
    {
        TargetComponentInfo.SetCableType(value);

        if(_lifelineInspector == null)_lifelineInspector = FindObjectOfType<ComponentInspector_Lifeline>();

        if (value == 0) ///activate lifeline
        {
            _lifelineInspector.gameObject.SetActive(true);
        }
        else /// clear and deactivate lifeline
        {
            if (_lifelineInspector.TargetComponentInfo != null)
            {
                _lifelineInspector.TargetComponentInfo.ResetValues();// TO DO: this deletes everything. WOuld be good to backup or offer prompt before erasing marker data
            }

            _lifelineInspector.gameObject.SetActive(false);
            _contextMenu.lifelineActive = false;
        }
    }

    void StartDeleteCable()
    {
        _cableDeletionPrompt.SetActive(true);
    }

    void CancelDeleteCable()
    {
        _cableDeletionPrompt.SetActive(false);
    }

    void ConfirmDeleteCable()
    {
        _cableDeletionPrompt.SetActive(false);

        //if (_placer == null) _placer = FindObjectOfType<Placer>();

        //CablePlacerLogic cablePlacer = _placer.activeLogic as CablePlacerLogic;
        //cablePlacer.ForceDelete();
        if (_placer == null)
            return;

        _placer.DestroySelectedObject();
    }







}
