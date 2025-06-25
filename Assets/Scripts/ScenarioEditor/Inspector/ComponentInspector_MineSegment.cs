using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

public class ComponentInspector_MineSegment : ComponentInspector<ComponentInfo_MineSegment>
{
    //Inspector inspector;
    public TMP_Text headerText;
    


    //[SerializeField] ComponentInfo_MineSegment TargetComponentInfo;
    [SerializeField] Toggle IsTeamstopToggle;
    [SerializeField] Toggle IsMappedToggle;

    //public int index;

    public override void Start()
    {
        base.Start();
        //inspector = Inspector.instance;
        //targetMineSegmentInfo = inspector.targetInfo.componentInfo_MineSegments[index];
        InitializeValues();

        IsTeamstopToggle.onValueChanged.AddListener(SetTeamstop);
        IsMappedToggle.onValueChanged.AddListener(SetMapped);
        //inspector.SizeContainerContent(true);
    }

    public void InitializeValues()
    {                
        IsTeamstopToggle.SetIsOnWithoutNotify(TargetComponentInfo.IsTeamstop);
        IsMappedToggle.SetIsOnWithoutNotify(TargetComponentInfo.IsMapped);
    }


    private void SetTeamstop(bool isTeamstop)
    {
        TargetComponentInfo.IsTeamstop = isTeamstop;
        TargetComponentInfo.ConfigureTeamstop(isTeamstop);
    }

    private void SetMapped(bool isMapped)
    {
        TargetComponentInfo.IsMapped = isMapped;
        TargetComponentInfo.SetMapped(isMapped);
    }

    

    private void OnDestroy()
    {
        IsTeamstopToggle.onValueChanged.RemoveListener(SetTeamstop);
        IsMappedToggle.onValueChanged.RemoveListener(SetMapped);
    }
}
