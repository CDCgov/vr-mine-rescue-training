using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInfo_DeadNPC : ModularComponentInfo, ISaveableComponent, IInspectableComponent
{
    public string BodyName = "Fred";
    public bool BG4Enabled = false;
    public List<TextTexture> NameTextures = new List<TextTexture>();
    //public GameObject CapLight;
    public BodyBehavior BodyBehavior;
    public string ComponentInspectorTitle => "Body Name";

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
            return;

        Name = component.GetParamValueString("BodyName", "Fred");
        BG4Enabled = component.GetParamValueBool("BG4Enabled", false);
    }

    public string[] SaveInfo()
    {
        return new string[]
        {
            "BodyName|" + BodyName,
            "BG4Enabled|" + BG4Enabled,
        };
    }

    public string SaveName()
    {
        return "Body";
    }

    [InspectableStringProperty("Name")]
    public string Name
    {
        get => BodyName;
        set
        {
            BodyName = value;
            foreach (var item in NameTextures)
            {
                item.Text = BodyName;
            }
            BodyBehavior.BodyName = value;
        }
    }

    [InspectableBoolProperty("BG4 Enabled")]
    public bool IsBG4Enabled
    {
        get => BG4Enabled;
        set
        {
            BG4Enabled = value;
            BodyBehavior.EnableBG4(value);
        }
    }
}
