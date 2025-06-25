using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UILabelSceneName : MonoBehaviour
{
    public SceneLoadManager SceneLoadManager;
    public TMP_Text Label;    

    // Start is called before the first frame update
    void Start()
    {
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);

        if (Label == null)
            Label = GetComponent<TMP_Text>();

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneLoadManager != null)
            SceneLoadManager.EnteredSimulationScene += OnEnteredSimulationScene;

        UpdateText(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (SceneLoadManager != null)
            SceneLoadManager.EnteredSimulationScene -= OnEnteredSimulationScene;
    }

    private void OnEnteredSimulationScene()
    {
        if (SceneLoadManager == null)
            return;

        UpdateText(SceneLoadManager.ScenarioName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateText(scene);
    }

    void UpdateText(string name)
    {
        if (Label == null)
            return;

        Label.text = name;
    }

    void UpdateText(Scene scene)
    {
        if (Label == null || scene.name == null || scene.name.Length <= 0)
            return;

        Label.text = scene.name;
    }

}
