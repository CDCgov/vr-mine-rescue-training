using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlownCurtainInteraction : MonoBehaviour, IInteractableObject
{
    private NetworkedObject _netObj;
    private NetworkManager NetworkManager;
    public string RolledCurtainAddress = "";
    private void Start()
    {
        //if(NetworkManager == null)
        //{
        //    NetworkManager = NetworkManager.GetDefault(gameObject);
        //}
        if (_netObj == null)
        {
            _netObj = GetComponentInParent<NetworkedObject>();
            
        }
    }

    public void RollUpCurtain()
    {
        if (_netObj == null)
            return;

        RaycastHit hit;
        Vector3 spawnPoint = transform.position;
        if(Physics.Raycast(transform.position, -transform.up, out hit, 5, LayerMask.NameToLayer("Floor")))
        {
            spawnPoint = hit.point;
        }
        Quaternion rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + 90, transform.eulerAngles.z);
        _netObj.NetManager.SpawnObject(RolledCurtainAddress, System.Guid.NewGuid(), spawnPoint, rotation, true);
        _netObj.NetManager.DestroyObject(_netObj.uniqueID);
    }


    public void OnJoystickPressed(Transform interactor, bool pressed)
    {

    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {

    }

    public void OnPickedUp(Transform interactor)
    {
        RollUpCurtain();
    }

    public void OnDropped(Transform interactor)
    {

    }

    public ActivationState CanActivate => ActivationState.Ready;

    public void OnActivated(Transform interactor)
    {
        RollUpCurtain(); 
    }

    public void OnDeactivated(Transform interactor)
    {

    }

}
