using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CustomXRInteractor))]
public class ControllerBehaviors : MonoBehaviour
{
    public GameObject ControllerObject;
    public bool PencilActive = false;
    //[HideInInspector]
    //public bool InteractorActive = true;

    private CustomXRInteractor _customXRInteractor;
    private bool _appQuitting = false;

    private void Start()
    {
        _customXRInteractor = GetComponent<CustomXRInteractor>();

        _customXRInteractor.ObjectPickedUp += OnObjectPickedUp;
        _customXRInteractor.ObjectDropped += OnObjectDropped;

        Application.quitting += () => { _appQuitting = true; };
    }

    private void OnObjectDropped(CustomXRInteractable obj)
    {
        ShowController();
    }

    private void OnObjectPickedUp(CustomXRInteractable obj)
    {
        HideController();
    }

    public void HideController()
    {
        //Debug.Log("Hide controller activated: " + (ControllerObject != null) + "," + (!PencilActive));
        if (ControllerObject != null && !PencilActive)
            ControllerObject.SetActive(false);
    }

    public void ShowController()
    {
        if (_appQuitting)
            return;

        if (!gameObject.scene.isLoaded)
            return;

        if (ControllerObject != null && !PencilActive)
            ControllerObject.SetActive(true);
    }

    public void PencilHideController()
    {
        ControllerObject.SetActive(false);
    }

    public void PencilShowController()
    {
        ControllerObject.SetActive(true);
    }

    //public void DisableInteractor()
    //{
    //    if(_customXRInteractor != null)
    //    {
    //        _customXRInteractor.AllowGrip = false;
    //        InteractorActive = false;
    //    }
    //}
    //public void EnableInteractor()
    //{
    //    if (_customXRInteractor != null)
    //    {
    //        _customXRInteractor.AllowGrip = true;
    //        InteractorActive = true;
    //    }
    //}

    public void PencilShake()
    {
        _customXRInteractor.HapticShake();
    }
}
