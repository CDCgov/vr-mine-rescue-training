using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowObjectActiveNamedScene : MonoBehaviour
{
    public string NamedScene = "BAH_ScenarioEditor";
    public Renderer[] MeshesToHide;
    public Image[] ImagesToHide;

    private bool _visible = true;
    
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        
        if(SceneManager.GetActiveScene().name != NamedScene)
        {
            ShowHideMeshes(false);
        }
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        if(arg1.name != NamedScene)
        {
            ShowHideMeshes(false);
        }
        else
        {
            ShowHideMeshes(true);
        }
    }

    private void ShowHideMeshes(bool showMesh)
    {
        foreach (Renderer ren in MeshesToHide)
        {
            ren.enabled = showMesh;
        }

        foreach (Image image in ImagesToHide)
        {
            image.enabled = showMesh;
        }
        _visible = showMesh;
    }

    public void ToggleShowHide()
    {
        _visible = !_visible;
        ShowHideMeshes(_visible);
    }

    public void Show()
    {
        ShowHideMeshes(true);
        _visible = true;
    }

    public void Hide()
    {
        ShowHideMeshes(false);
        _visible = false;
    }
    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }
}
