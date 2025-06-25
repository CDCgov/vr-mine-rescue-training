using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CinemachineBrain))]
public class CineForceVC : MonoBehaviour
{
    public  CinemachineVirtualCameraBase VirtualCamera;

    private CinemachineBrain _cineBrain;
    private int _overrideID;

    // Start is called before the first frame update
    void Start()
    {
        if (VirtualCamera == null)
        {
            Debug.LogError("No virtual camera set on CineForceVC");
            return;
        }

        _cineBrain = GetComponent<CinemachineBrain>();
        _overrideID = _cineBrain.SetCameraOverride(-1, VirtualCamera, VirtualCamera, 0, -1);
    }

    public void SetVirtualCamera(CinemachineVirtualCameraBase vc)
    {
        VirtualCamera = vc;

        _cineBrain.ReleaseCameraOverride(_overrideID);
        _overrideID = _cineBrain.SetCameraOverride(-1, VirtualCamera, VirtualCamera, 0, -1);
    }

    private void OnDestroy()
    {
        _cineBrain.ReleaseCameraOverride(_overrideID);
    }

}
