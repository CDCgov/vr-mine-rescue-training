using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class UIQuitButton : MonoBehaviour
{
    private Button _button;
    public PlayerManager PlayerManager;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        _button = GetComponent<Button>();
        _button.onClick.AddListener(QuitButton);
    }

    private void QuitButton()
    {
        //Log end of session code here?
        StartCoroutine(QuitDelay());
        //DestroyAllManagers();
        //SceneManager.LoadScene(0,LoadSceneMode.Single);
    }

    IEnumerator QuitDelay()
    {
        DestroyAllManagers();
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    private void DestroyAllManagers()
    {
        GameObject[] managers = GameObject.FindGameObjectsWithTag("Manager");
        foreach(GameObject obj in managers)
        {
            if(obj != null)
                Destroy(obj);
        }
    }
}
