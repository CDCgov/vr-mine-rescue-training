using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TestSceneLoader : MonoBehaviour 
{
    void Start () 
    {
        GameObject.DontDestroyOnLoad(gameObject);
    }
    
    void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SceneManager.LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Application.Quit();
        }
    }
}