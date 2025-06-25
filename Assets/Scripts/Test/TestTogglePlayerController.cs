using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is only for testing the phone during the sprint
/// </summary>
public class TestTogglePlayerController : MonoBehaviour {

	//public UnityStandardAssets.Characters.FirstPerson.FirstPersonController FPSScript;
	//FPS controller starts on
	bool IsOn = true;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.F5))
		{
			IsOn = !IsOn;            
			//FPSScript.enabled = IsOn;
			if (IsOn)
			{
				Cursor.lockState = CursorLockMode.Locked;                
			}
			else
			{
				Cursor.lockState = CursorLockMode.None;
				
				Cursor.visible = true;
			}
		}
	}
}
