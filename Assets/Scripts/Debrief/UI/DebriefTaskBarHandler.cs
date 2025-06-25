using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebriefTaskBarHandler : MonoBehaviour
{
    public SceneLoadManager SceneLoadManager;

    public GameObject QuitPanel;
    public bool QuitPanelOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);

        Application.wantsToQuit += WantsToQuit;
    }

    // Update is called once per frame
    void Update()
    {
        QuitPanelOpen = QuitPanel.activeSelf;
    }

    bool WantsToQuit()
    {
        if (QuitPanelOpen)
            return true;
        QuitPanel.SetActive(true);
        QuitPanelOpen = true;
        return false;
    }

    public void QuitButton()
    {
        QuitPanel.SetActive(true);
        QuitPanelOpen = true;
    }

    public void MainMenuButton()
    {
        //ugly brute force. . .
        //GameObject[] all = FindObjectsOfType<GameObject>();
        //foreach (GameObject obj in all)
        //{
        //    if (obj.scene.buildIndex == -1)
        //    {
        //        Destroy(obj);
        //    }
        //}
        SceneManager.LoadScene("BAH_VRMineLaunch", LoadSceneMode.Single);
    }
}
