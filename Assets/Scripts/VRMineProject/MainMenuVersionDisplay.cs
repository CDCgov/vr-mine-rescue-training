using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuVersionDisplay : MonoBehaviour 
{
	void Start () 
	{
		GitVersion.ReadVersion();
		Text versionTextBox = gameObject.GetComponent<Text>();
		TextMeshProUGUI tmproText = gameObject.GetComponent<TextMeshProUGUI>();

		string versionText = "Version " + GitVersion.Version;

		if (versionTextBox != null)
			versionTextBox.text = versionText;
		if (tmproText != null)
			tmproText.text = versionText;

		//SceneManager.activeSceneChanged += (oldScene, newScene) => {
		//	if (newScene.name != "MainMenuBackground")
		//		gameObject.SetActive(false);
		//};

	}

    //void OnSceneChanged()

    private void OnDestroy()
    {
        //eneManager.activeSceneChanged -= 
    }
}