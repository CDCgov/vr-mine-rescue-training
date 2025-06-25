using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIVRSceneLoadButtons : MonoBehaviour
{
    public SceneLoadManager SceneLoadManager;
    public NetworkManager NetworkManager;

    public GameObject ButtonPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);


        AddButton("CRT", () => { LoadScene("TheConstruct"); }) ;
        AddButton("S1A", () => { LoadScene("MineRescueScenario1A"); });
        AddButton("S1B", () => { LoadScene("MineRescueScenario1B"); });
        AddButton("S1C", () => { LoadScene("MineRescueScenario1C"); });
        AddButton("TRA", () => { LoadScene("MineRescueScenario-TrainingA"); });
        AddButton("TRB", () => { LoadScene("MineRescueScenario-TrainingB"); });
        AddButton("TRC", () => { LoadScene("MineRescueScenario-TrainingC"); });
        AddButton("STN", () => { LoadScene("StoneMineDemoA"); });
        AddButton("EXT", () => { LoadScene("VRExitScene"); });
    }

    void LoadScene(string sceneName)
    {
        NetworkManager.SendLoadScene(sceneName, true);
    }

    void AddButton(string name, UnityAction handler)
    {
        try
        {
            var obj = GameObject.Instantiate<GameObject>(ButtonPrefab);
            var button = obj.GetComponent<Button>();
            var text = obj.GetComponentInChildren<TextMeshProUGUI>();

            button.onClick.AddListener(handler);
            text.text = name;

            obj.transform.SetParent(transform, false);
            obj.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error creating button {name} : {ex.Message}");
        }
    }

}
