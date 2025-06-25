using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ComponentInspector_Lifeline : ComponentInspector<ComponentInfo_Lifeline>
{
    
    //public int index;
    public TMP_Text headerText;
    public TMP_Text MarkerSpacingHeader;
    public TMP_InputField MarkerSpacingField;
    public Button AddDirectionalMarkersButton;
    public Button AddTagMarkersButton;
    public Button FlipDirectionalMarkersButton;
    public Button FlipTagMarkersButton;

    public IndexedColorList TagColors;
    public TMP_Dropdown ColorDropdown;

    public string componentName = "Cable";
    //public ComponentInfo_Lifeline TargetComponentInfo;
    
    private ObjectInfo objectInfo;


    private RuntimeMarkerEditor markerEditor;
    //private Inspector inspector;


    public override void Start()
    {
        base.Start();

        InitializeReferences();
        InitializeEvents();
        InitializeDropdown();
        InitializeValues();

        MarkerSpacingField.onEndEdit.AddListener(TargetComponentInfo.SetMarkerSpacing);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (markerEditor) AddDirectionalMarkersButton.onClick?.RemoveListener(markerEditor.PopulateDirection);
        if (markerEditor) AddTagMarkersButton.onClick?.RemoveListener(markerEditor.PopulateTag);

    }

    void InitializeReferences()
    {
        markerEditor = FindObjectOfType<RuntimeMarkerEditor>();
        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_Lifelines[index];
    }

    void InitializeValues()
    {
        if (TargetComponentInfo == null)
            return;
        MarkerSpacingField.text = TargetComponentInfo.GetMarkerSpacing().ToString();
        SetMarkerSpacingHeader();
        ColorDropdown.value = TargetComponentInfo.ColorIndex;

    }

    void InitializeEvents()
    {
        AddDirectionalMarkersButton.onClick.AddListener(markerEditor.PopulateDirection);
        AddTagMarkersButton.onClick.AddListener(markerEditor.PopulateTag);
        FlipTagMarkersButton.onClick.AddListener(TargetComponentInfo.FlipAllTags);
        FlipDirectionalMarkersButton.onClick.AddListener(TargetComponentInfo.FlipAllDirectionalMarkers);
        ColorDropdown.onValueChanged.AddListener(TargetComponentInfo.ChangeAllTagColors);
    }

    private void SetMarkerSpacingHeader()
    {
        if (TargetComponentInfo == null)
            return;

        if (TargetComponentInfo.IsMetric) { MarkerSpacingHeader.text = "Marker Spacing (M)"; } // To Do : retarget bool to global variable
        else { MarkerSpacingHeader.text = "Marker Spacing (F)"; }
    }

    void InitializeDropdown()
    {
        ColorDropdown.ClearOptions();
        List<string> colorNames = new List<string>();
        colorNames.Add(TagColors.DefaultColorData.Name);
        foreach (IndexedColorData c in TagColors.IndexedColors)
        {
            colorNames.Add(c.Name);
        }
        ColorDropdown.AddOptions(colorNames);
    }

}
