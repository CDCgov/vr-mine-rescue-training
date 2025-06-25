using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CineForceVC))]
public class CycleForcedVC : MonoBehaviour
{
    public float Duration = 7;
    public List<CinemachineVirtualCameraBase> VCList;


    private CineForceVC _forceVC;
    private int _currentCam = -1;

    void Start()
    {

    }

    private void OnEnable()
    {
        _forceVC = GetComponent<CineForceVC>();

        StartCoroutine(CycleVCs());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator CycleVCs()
    {
        yield return new WaitForEndOfFrame();

        if (VCList != null && VCList.Count > 0 && _forceVC != null)
        {
            _currentCam++;
            if (_currentCam >= VCList.Count)
                _currentCam = 0;

            _forceVC.SetVirtualCamera(VCList[_currentCam]);
        }

        yield return new WaitForSeconds(Duration);
    }

}
