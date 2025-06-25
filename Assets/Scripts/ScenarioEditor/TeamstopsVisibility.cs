using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamstopsVisibility : MonoBehaviour
{
    public GameObject IsInvisibleIconOverlay;
    private static bool _visible = true;
    private Button _uiButton;
    

    private void Start()
    {
        _uiButton = GetComponent<Button>();

        _uiButton.onClick.AddListener(ToggleTeamstops);
        NIOSH_EditorLayers.LayerManager.Instance.layerChanged += LayerChanged;
    }

    private void OnDestroy()
    {
        _uiButton.onClick.RemoveListener(ToggleTeamstops);
        NIOSH_EditorLayers.LayerManager.Instance.layerChanged -= LayerChanged;
    }

    private void LayerChanged(NIOSH_EditorLayers.LayerManager.EditorLayer editorLayer)
    {
        if(editorLayer == NIOSH_EditorLayers.LayerManager.EditorLayer.SceneControls)
        {
            _visible = true;
            gameObject.SetActive(true);
            SetVisiblityOnTeamstops(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public static void ToggleTeamstops()
    {
        _visible = !_visible;
        SetVisiblityOnTeamstops(_visible);
    }

    public static void SetVisiblityOnTeamstops(bool visible)
    {
        TeamstopEditorVisiblityHandler[] handlers = FindObjectsOfType<TeamstopEditorVisiblityHandler>();

        foreach (var item in handlers)
        {
            item.SetVisbility(visible);
        }

        TeamstopsVisibility vi = FindObjectOfType<TeamstopsVisibility>();
        if (vi != null)
        {
            vi.IsInvisibleIconOverlay.SetActive(!visible);
        }
        else
        {
            Debug.Log("Teamstops visibilty was null???");
        }
    }


    public static void Show()
    {
        SetVisiblityOnTeamstops(true);
    }

    public static void Hide()
    {
        SetVisiblityOnTeamstops(false);
    }

    public static bool GetVisbility()
    {
        return _visible;
    }
}
