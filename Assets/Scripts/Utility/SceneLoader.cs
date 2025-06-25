using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        //if (ScenarioSaveLoad.Instance != null)
        //{
        //    //var closeAllowed = await ScenarioSaveLoad.Instance.IsScenarioCloseAllowed();
        //    //if (!closeAllowed)
        //    //    return;

        //    ScenarioSaveLoad.Instance.IsScenarioCloseAllowed((result) =>
        //    {
        //        if (result)
        //            SceneManager.LoadScene(sceneName);
        //    });
        //}
        //else
        //{
        //    SceneManager.LoadScene(sceneName);
        //}
    }

    public Button playButton;

    public void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        if (ScenarioInitializer.Instance == null)
            return;

        _ = ScenarioInitializer.Instance.PlayLoadedScenario();
    }

    //public void OnEnable()
    //{
    //    if(playButton != null)
    //    {
    //        StartCoroutine(FindButtonHookup());
    //    }
    //    //playButton.onClick.AddListener(ScenarioInitializer.Instance.PlayLoadedScenario);

    //}

    //IEnumerator FindButtonHookup()
    //{
    //    yield return new WaitForSeconds(1f);
    //    if(ScenarioInitializer.Instance != null )
    //    {
    //        playButton.onClick.AddListener(ScenarioInitializer.Instance.PlayLoadedScenario);
    //    }
    //}

}
