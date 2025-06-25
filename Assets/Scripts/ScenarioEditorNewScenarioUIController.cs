using UnityEngine;

public class ScenarioEditorNewScenarioUIController : MonoBehaviour
{
    public SceneLoader _sceneLoader;
    private const string ScenarioEditorSceneName = "BAH_ScenarioEditor";

    private void Start()
    {
        if(_sceneLoader == null) { _sceneLoader = FindObjectOfType<SceneLoader>(); }
    }
    // load scenario editor while also checking with scenarioinitializer to see if we need to generate a mine or not
    public void LoadScenarioEditor(bool generateMineOnStart)
    {
        if(!ScenarioInitializer.Instance || _sceneLoader == null)
        {
            Debug.LogError("Missing scenario init: " + ScenarioInitializer.Instance + " or missing scene loader: " + _sceneLoader);
            return;
        }

        ScenarioInitializer.Instance.GenerateMineOnStart = generateMineOnStart;
        _sceneLoader.LoadScene(ScenarioEditorSceneName);
    }
}
