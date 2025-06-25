using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

public class ComponentInspector_Decal : ComponentInspector<ComponentInfo_Decal>
{
    public TMP_Text headerText;
    public TMP_Text pathText;
    public Button SelectFileBtn;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        SelectFileBtn.onClick.AddListener(PerformFileSelect);
        InitializeValues(TargetComponentInfo);
    }

    public override void OnDestroy()
    {
        SelectFileBtn.onClick.RemoveListener(PerformFileSelect);
    }

    private void PerformFileSelect()
    {
        if(TargetComponentInfo == null)
        {
            return;
        }
        var path = StandaloneFileBrowser.OpenFilePanel("Select Decal File", Application.dataPath, "png", false);
        Debug.Log($"Path: {System.IO.Path.Combine(path)}");
        if (path != null)
        {
            pathText.text = path[path.Length - 1];
            TargetComponentInfo.DecalPath = System.IO.Path.Combine(path);
            TargetComponentInfo.PerformLoadDecal();
        }
    }

    private void InitializeValues(ComponentInfo_Decal component)
    {
        if(component == null)
        {
            return;
        }
        pathText.text = component.DecalPath;
        component.PerformLoadDecal();
    }
}
