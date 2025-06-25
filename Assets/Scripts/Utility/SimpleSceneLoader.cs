using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    public RectTransform ScrollContent;
    public GameObject ButtonPrefab;
    private Canvas _canvas;

    public List<string> SceneNames;

    // Start is called before the first frame update
    void Start()
    {
        _canvas = GetComponent<Canvas>();    

        foreach (var sceneName in SceneNames)
        {
            GameObject btnObj = Instantiate<GameObject>(ButtonPrefab);
            btnObj.name = $"Btn{sceneName}";
            var btn = btnObj.GetComponent<Button>();
            var txt = btnObj.GetComponentInChildren<Text>();

            txt.text = sceneName;
            btn.onClick.AddListener(() => OnLoadClicked(sceneName));

            btnObj.transform.SetParent(ScrollContent);
        }
    }

    void OnLoadClicked(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
