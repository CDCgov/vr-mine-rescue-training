using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    [SerializeField]
    List<CameraLogic> cameraLogics = new List<CameraLogic>();
    
    CameraLogic activeCameraLogic;

    Action<bool> onCameraSwap;
    int activeLogicIndex;


    public void Start()
    {
        if(activeCameraLogic == null)
        {
            if(cameraLogics.Count > 0)
            {
                activeCameraLogic = cameraLogics[0];
            }
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.X))
        {
           // CycleCameraLogic();
        }
    }

    public void CycleCameraLogic()
    {
        int prevIndex = activeLogicIndex;
        activeLogicIndex++;
        if(activeLogicIndex > cameraLogics.Count-1)
        {
            activeLogicIndex = 0;
        }
        activeCameraLogic = cameraLogics[activeLogicIndex];
        cameraLogics[activeLogicIndex].Activate();
        cameraLogics[prevIndex].Deactivate();
    }

    public void SetCameraPosition(GameObject obj)
    {
        SetCameraPosition(obj.transform.position);
    }

    public void SetCameraPosition(Vector3 pos)
    {
        Camera.main.transform.position = pos;
    }

    public CameraLogic GetActiveCameraLogic()
    {
        return activeCameraLogic;
    }

    public void SetCameraTarget(Transform obj)
    {

    }

}
