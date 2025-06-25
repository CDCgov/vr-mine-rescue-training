using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CustomXRInteractable))]
public class SocketSfx : MonoBehaviour, IInteractableObject, ISocketableObject
{
    public CustomXRInteractable CustXR;
    public AudioSource SocketAudio;
    public SoundingStickSounds MaterialSounds;

    private float _playDelay = 0.5f;
    private NetworkedObject _netObj;

    private void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        if (CustXR == null)
        {
            CustXR = GetComponent<CustomXRInteractable>();
        }
        //CustXR.OwnerChanged += CustXR_OwnerChanged;
        if(TryGetComponent<NetworkedObject>(out _netObj))
        {
            _netObj.RegisterMessageHandler(OnSfx);
        }
        
    }

    private void OnSfx(string messageType, CodedInputStream reader)
    {
        if(messageType == "SOCKETSFX")
        {
            if (Time.time < _playDelay)
            {
                return;
            }
            SocketAudio.Play();
        }
    }

    private void CustXR_OwnerChanged(XRObjectController obj)
    {
        //Debug.Log($"Socket SFx owner change event: {obj.name}, Is socket? {obj.GetComponent<CustomXRSocket>() != null}");
        //PlaySocketSfx(obj);
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        _playDelay += Time.time;
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        
    }

    public void OnDeactivated(Transform interactor)
    {
        
    }

    public void OnDropped(Transform interactor)
    {
        
    }

    public void OnJoystickPressed(Transform interactor, bool pressed)
    {
        
    }

    public void OnPickedUp(Transform interactor)
    {
        //PlaySocketSfx();
    }

    public void OnPrimaryButtonPressed(Transform interactor, bool pressed)
    {
        
    }

    public void OnSecondaryButtonPressed(Transform interactor, bool pressed)
    {
        
    }

    public void PlaySocketSfx(XRObjectController obj = null)
    {
        if(Time.time < _playDelay)
        {
            return;
        }
        
        //if(CustXR.CurrentOwner != null)
        if(obj.GetComponent<CustomXRSocket>() != null)
        {
            //var interactor = CustXR.CurrentOwner as CustomXRInteractor;
            //if (!SocketAudio.isPlaying && interactor != null && interactor.IsTrackedController)
            //{                    
            //    if (MaterialSounds != null)
            //    {
            //        SocketAudio.clip = MaterialSounds.Sounds[Random.Range(0, MaterialSounds.Sounds.Length)];
            //    }
            //    SocketAudio.Play();
            //    Debug.Log("Played socket sfx!");
            //}

               
            if (!SocketAudio.isPlaying)
            {
                if (MaterialSounds != null)
                {
                    SocketAudio.clip = MaterialSounds.Sounds[Random.Range(0, MaterialSounds.Sounds.Length)];
                }
                SocketAudio.Play();
                Debug.Log("Played socket sfx!");
            }
            
        }
        
    }

    public void OnSocketed(CustomXRSocket socket)
    {
        if (Time.time < _playDelay)
        {
            return;
        }
        SocketAudio.Play();
        if(_netObj != null && _netObj.HasAuthority)
        {
            _netObj.SendMessage("SOCKETSFX", new VRNTextMessage());
        }
    }

    public void OnRemovedFromSocket(CustomXRSocket socket)
    {
        
    }
}
