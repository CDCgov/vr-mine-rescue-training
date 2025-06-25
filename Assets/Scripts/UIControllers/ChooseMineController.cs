using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChooseMineController : MonoBehaviour 
{
    public Transform ListParent;

    public UnityAction<string> MapSelectedCallback;

    void Start () 
    {
        foreach (MapData map in MasterControl.Instance.MineMaps)
        {
            /*if (Application.isEditor)
                AddMapToList(map.SceneName, map.DisplayName);
            else */

            if (IsSceneNameValid(map.SceneName))
                AddMapToList(map.SceneName, map.DisplayName);
        }
    }

    private bool IsSceneNameValid(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);

            if (scenePath.Contains(name))
                return true;
        }

        return false;
    }

    private void AddMapToList(string sceneName, string displayName)
    {
        GameObject obj = Util.InstantiateResource("GUI/GenericButton");
        obj.name = sceneName;

        Text txt = obj.GetComponentInChildren<Text>();
        Button btn = obj.GetComponent<Button>();

        btn.onClick.AddListener(OnMapClicked);
        txt.text = displayName;

        obj.transform.SetParent(ListParent, false);
    }

    private void OnMapClicked()
    {
        string selectedScene = EventSystem.current.currentSelectedGameObject.name;

        if (MapSelectedCallback != null)
            MapSelectedCallback(selectedScene);
    }
    
}