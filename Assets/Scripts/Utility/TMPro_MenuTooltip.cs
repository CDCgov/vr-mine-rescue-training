using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPro_MenuTooltip : MenuTooltip
{
    public TextMeshProUGUI LabelToMakeTooltip;
    // Start is called before the first frame update
    protected override void Start()
    {
        if(LabelToMakeTooltip == null)
        {
            LabelToMakeTooltip = GetComponent<TextMeshProUGUI>();
        }
        TooltipTextString = LabelToMakeTooltip.text;        
        base.Start();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (LabelToMakeTooltip == null)
        {
            LabelToMakeTooltip = GetComponent<TextMeshProUGUI>();
        }
        TooltipTextString = LabelToMakeTooltip.text;
        if(TooltipText != null)
        {
            TooltipText.text = TooltipTextString;
            TooltipText.enableWordWrapping = true;
            TooltipText.overflowMode = TextOverflowModes.Overflow;
        }
        base.OnPointerEnter(eventData);
    }
}
