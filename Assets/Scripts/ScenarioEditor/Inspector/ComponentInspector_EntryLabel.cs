using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

[System.Obsolete]
public class ComponentInspector_EntryLabel : ComponentInspector<ComponentInfo_EntryLabel>
{
    // need to store npc name, located in MineNPCHost, RefugeNPCBehaviors,MineNPCInfo,TextTexture on NameProjector child object
    // NPC_Animator component starting animation
    // NavMeshAgent follow speed, follow distance(stopping distance)
    // RefugeNPCBehaviors component, soundcollection, follow response collection
    // RefugeNPCBehaviors component, soundcollection, wait response collection
    // show/hide bG4, boolean 


    //Inspector inspector;
    public TMP_Text headerText;
    [SerializeField] TMP_Dropdown SymbolDropdown;
    [SerializeField] TMP_InputField SymbolInput;
    //protected ComponentInfo_EntryLabel TargetComponentInfo;

    //public int index;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //TargetComponentInfo = inspector.targetInfo.componentInfo_EntryLabels[index];
        InitializeValues();
        //SymbolDropdown.onValueChanged.AddListener(SetEntrySymbol);
        SymbolInput.onValueChanged.AddListener(SetEntryText);
        //inspector.SizeContainerContent(true);
    }

    public void InitializeValues()
    {
        //if(TargetComponentInfo.symbol != null)
        //{
        //    //SymbolDropdown.value = SymbolDropdown.options.FindIndex(option => option.text == targetEntryLabelInfo.symbol.name);
        //    SymbolInput.text = TargetComponentInfo.EntryValue;
        //    Debug.Log($"Setting input text: {TargetComponentInfo.EntryValue}");
        //}

    }

   
    private void SetEntrySymbol(int index)
    {
        TargetComponentInfo.SetSymbol(TargetComponentInfo.symbolList.GetCollectionByString(SymbolDropdown.captionText.text));
    }

    private void SetEntryText(string text)
    {
        //TargetComponentInfo.SetText(SymbolInput.text);
    }

    private void OnDestroy()
    {
        //SymbolDropdown.onValueChanged.RemoveListener(SetEntrySymbol);
        SymbolInput.onValueChanged.RemoveListener(SetEntryText);

    }
}
