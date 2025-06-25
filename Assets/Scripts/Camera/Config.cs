using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class Config : MonoBehaviour
{

    // public Camera360 camScript;

    // public static string ipAddress = "";
    // public static float BufferTime = 1;
    // public static float TimeoutTime = 1;
    // public static float RestTime = 1;
    // public static float ExitTime = 1;

    // private StreamReader _file;
    // private string _filePath;
    // // Use this for initialization
    // void Awake () {
    // 	_filePath = Path.Combine(Application.dataPath, "config.txt");
    // 	_file = new StreamReader(_filePath);

    // 	string line;
    // 	while ((line = _file.ReadLine()) != null)
    // 	{
    // 		if (string.IsNullOrEmpty(line) || line[0] == '#')
    // 		{
    // 			continue;
    // 		}
    // 		if (line.Contains("\t"))
    // 		{
    // 			Debug.Log("Found the tab!");
    // 		}
    // 		string[] strSplit = line.Split('\t');
    // 		switch (strSplit[0])
    // 		{
    // 			case "Overlap":
    // 				float fltOverlap = 0;
    // 				if (float.TryParse(strSplit[1], out fltOverlap))
    // 				{
    // 					camScript.Overlap = fltOverlap;
    // 					Debug.Log("Setting overlap: " + fltOverlap);
    // 				}
    // 				break;
    // 			case "CamPos":
    // 				float fltCamPos = 0;
    // 				if(float.TryParse(strSplit[1], out fltCamPos))
    // 				{
    // 					camScript.CamPos = fltCamPos;
    // 					Debug.Log("Setting camera position: " + fltCamPos);
    // 				}
    // 				break;
    // 			case "ipAddress":
    // 				//LabViewComm.ipAddress = strSplit[1];
    // 				Config.ipAddress = strSplit[1];
    // 				break;
    // 		}
    // 	}

    // 	camScript.Config();
    // }	
}
