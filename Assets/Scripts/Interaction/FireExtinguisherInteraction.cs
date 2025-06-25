using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.VFX;
using Google.Protobuf;


public class FireExtinguisherInteraction : MonoBehaviour, ISelectableObjectInfo, 
    ISelectableObjectAction, INetSync, IInteractableObject, IResetableObject
{
    public const int MAX_RAYCAST_HITS = 30;
    public const int MAX_COLLIDER_HITS = 10;

    private bool _isGrabbed = true;
    private bool _isOn = false;
    private bool _activatedRemotely = false;

    public float PercentFull = 1; //Scale between zero and 1
    public float TargetAngle = -80;
    //public ParticleSystem ExtinguisherEmitter;
    //public ParticleSystem FirstPuffEmitter;
    public VisualEffect FireExtinguishEmitter;

    public AudioSource ExtinguisherAudio;
    public AudioClip StartExtinguishClip;
    public AudioClip LoopExtinguishClip;
    public AudioClip EndExtinguishClip;
    public Transform EmissionPoint;
    public Transform IndicatorTransform;
    public PlayerManager PlayerManager;

    public bool IsActivated
    {
        get => _isOn;
    }

    private RaycastHit _rHit;
    private float _indicatorAngle = 0;
    //private NetSyncFloatValue _netFloat;
    //private FireInteraction _fireInteract;
    private NetworkedObject _netObj;
    private bool _endExtinguisherAudioFlag = false;    
    private VRNFireExtinguisherStatus _vrnStatus;
    private CustomXRInteractable _interactable;
    private float _priorImpulseTime = 0;

    private RaycastHit[] _raycastHits;
    private Collider[] _colliderHits;
    private HashSet<FireInteraction> _firesAffected;


    public string SelectableActionName
    {
        get { return "Refill"; }
    }

    private void Awake()
    {
        _raycastHits = new RaycastHit[MAX_RAYCAST_HITS];
        _colliderHits = new Collider[MAX_COLLIDER_HITS];
        _firesAffected = new HashSet<FireInteraction>();
        _vrnStatus = new VRNFireExtinguisherStatus();
    }


    private void Start()
    {
        FireExtinguishEmitter.Stop();
        float indicator = Mathf.Lerp(TargetAngle, 0, PercentFull);
        IndicatorTransform.localEulerAngles = new Vector3(indicator, 0, indicator);//changed prefab structure, moved indicator from z manipulated to x manipulated
        _interactable = GetComponent<CustomXRInteractable>();
        //_netFloat = GetComponent<NetSyncFloatValue>();
        //if (_netFloat != null)
        //{
        //    _netFloat.ValueToSync = PercentFull;
        //}

        _netObj = GetComponent<NetworkedObject>();
        ExtinguisherAudio.clip = StartExtinguishClip;

        if(PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }
    }

    void OnEnable()
    {
        //visual effect resumes playing on enable
        if (!_isOn && FireExtinguishEmitter != null)
            FireExtinguishEmitter.Stop();
    }

    public void SetPercentFull(float value)
    {
        PercentFull = value;
        float indicator = Mathf.Lerp(TargetAngle, 0, PercentFull);
        IndicatorTransform.localEulerAngles = new Vector3(indicator, 0, indicator);//changed prefab structure, moved indicator from z manipulated to x manipulated
    }


    private void Update()
    {
        if (_isOn)
        {
            if (PercentFull > 0)
            {
                ApplyFireExtinguisherEffect();
            }
            else
            {
                if (_endExtinguisherAudioFlag)
                {
                    ExtinguisherAudio.Stop();
                    ExtinguisherAudio.loop = false;
                    ExtinguisherAudio.clip = EndExtinguishClip;
                    ExtinguisherAudio.Play();
                    _endExtinguisherAudioFlag = false;

                }
                FireExtinguishEmitter.Stop();
                PercentFull = 0;
            }
        }
        //else
        //{
        //    if (_fireInteract != null)
        //    {
        //        _fireInteract.StopExtinguish();
        //        _fireInteract = null;
        //    }

        //}

        float indicator = Mathf.Lerp(TargetAngle, 0, PercentFull);
        IndicatorTransform.localEulerAngles = new Vector3(indicator, 0, 0);
    }

    private void ApplyFireExtinguisherEffect()
    {
        PercentFull -= Time.deltaTime * 0.05f;

        //only apply the effect if we have authority or are not networked
        if (_netObj != null && !_netObj.HasAuthority)
            return;


        var interactor = _interactable.CurrentOwner as CustomXRInteractor;
        if (interactor != null && Time.time > (_priorImpulseTime + 0.09f))
        {
            interactor.HapticShake(1, 0.1f);
            _priorImpulseTime = Time.time;
        }

        _firesAffected.Clear();

        var numHits = Physics.RaycastNonAlloc(EmissionPoint.position, EmissionPoint.forward, _raycastHits, 5);
        for (int i = 0; i < numHits; i++)
        {
            //var fi = _raycastHits[i].collider.GetComponent<FireInteraction>();
            //if (fi != null)
            if (_raycastHits[i].collider.TryGetComponent<FireInteraction>(out var fire))
            {
                _firesAffected.Add(fire);
            }
        }

        numHits = Physics.OverlapSphereNonAlloc(EmissionPoint.position, 0.15f, _colliderHits);
        for (int i = 0; i < numHits; i++)
        {
            if (_colliderHits[i].TryGetComponent<FireInteraction>(out var fire))
            {
                if (!_firesAffected.Contains(fire))
                    _firesAffected.Add(fire);
            }
        }

        foreach (var fire in _firesAffected)
        {
            ApplyFireExtinguisherEffect(fire);
        }
    }

    private void ApplyFireExtinguisherEffect(FireInteraction fire)
    {
        int player = -1;
        if (_netObj.HasAuthority)
        {
            player = PlayerManager.CurrentPlayer.PlayerID;
        }
        fire.ApplyFireExtinguish(Time.deltaTime, player);
    }

    public void ActivateFireExtinguisher()
    {
        //check for multiplayer float value?
        //set PercentFull to multiplayer float value
        //if (_netFloat != null)
        //{
        //    PercentFull = _netFloat.ValueToSync;
        //}
        if (!_isOn && PercentFull > 0)
        {
            //ExtinguisherEmitter.Play();
            ExtinguisherAudio.loop = false;
            //ExtinguisherAudio.clip = StartExtinguishClip;
            //ExtinguisherAudio.Play();
            StartCoroutine(PlayExtinguisherSound());
            //FirstPuffEmitter.Play();
            FireExtinguishEmitter.Play();
            var interactor = _interactable.CurrentOwner as CustomXRInteractor;
            if (interactor != null)
            {
                interactor.HapticShake(1, 0.1f);
                _priorImpulseTime = Time.time;
            }
            _isOn = true;
            _endExtinguisherAudioFlag = true;
        }
        //if (Physics.Raycast(EmissionPoint.position, EmissionPoint.forward, out _rHit, 2))
        //{
        //    if (_rHit.collider.tag == "Fire")
        //    {
        //        FireInteraction fI = _rHit.collider.GetComponent<FireInteraction>();
        //        if (fI != null)
        //        {
        //            fI.OnExtinguish();
        //        }
        //    }
        //}

    }

    public void DeactivateFireExtinguisher()
    {
        if (_isOn)
        {
            //ExtinguisherEmitter.Stop();
            ExtinguisherAudio.Stop();
            ExtinguisherAudio.loop = false;
            ExtinguisherAudio.clip = EndExtinguishClip;
            ExtinguisherAudio.Play();
            //_audioClipStatus = 2;
            //FirstPuffEmitter.Stop();
            FireExtinguishEmitter.Stop();

            //Sync fire extinguisher level to multiplayer here
            //if (_netFloat != null)
            //{
            //    _netFloat.ValueToSync = PercentFull;
            //}

        }
        _isOn = false;
    }

    private IEnumerator PlayExtinguisherSound()
    {
        ExtinguisherAudio.clip = StartExtinguishClip;
        ExtinguisherAudio.Play();
        yield return new WaitForSeconds(ExtinguisherAudio.clip.length - 0.001f);
        if (_isOn)
        {
            ExtinguisherAudio.clip = LoopExtinguishClip;
            ExtinguisherAudio.loop = true;
            ExtinguisherAudio.Play();
        }
    }

    private void OnDrawGizmos()
    {
        //Debug.DrawLine(transform.position, transform.position + new Vector3(0, 0, 2), Color.red);
        if (EmissionPoint != null)
        {
            Debug.DrawRay(EmissionPoint.position, EmissionPoint.forward, Color.red, 2);
        }
    }

    public void GetObjectInfo(StringBuilder sb)
    {
        sb.AppendLine($"Percent Full: {PercentFull * 100.0f:F1}%");
    }

    public void PerformSelectableObjectAction()
    {
        if (_netObj != null && !_netObj.HasAuthority)
            _netObj.RequestOwnership();

        PercentFull = 1;
        //_netFloat.ValueToSync = 1;
    }

    public bool NeedsUpdate()
    {
        return true;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        if (_activatedRemotely && _isOn)
        {
            DeactivateFireExtinguisher();
            _activatedRemotely = false;
        }

        _vrnStatus.PercentFull = PercentFull;
        _vrnStatus.IsActive = _isOn;

        writer.WriteMessage(_vrnStatus);
    }

    public void SyncObjState(CodedInputStream reader)
    {
        if (_vrnStatus == null)
            _vrnStatus = new VRNFireExtinguisherStatus();

        _vrnStatus.PercentFull = 0;
        _vrnStatus.IsActive = false;

        //_vrnStatus.MergeDelimitedFrom(reader);
        reader.ReadMessage(_vrnStatus);

        PercentFull = _vrnStatus.PercentFull;
        if (_vrnStatus.IsActive && !_isOn)
        {
            ActivateFireExtinguisher();
            _activatedRemotely = true;
        }
        else if (!_vrnStatus.IsActive && _isOn)
        {
            DeactivateFireExtinguisher();
            _activatedRemotely = false;
        }
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
        DeactivateFireExtinguisher();
    }

    public void OnDropped(Transform interactor)
    {
        DeactivateFireExtinguisher();
    }

    public ActivationState CanActivate => ActivationState.Unknown;

    public void OnActivated(Transform interactor)
    {
        ActivateFireExtinguisher();
    }

    public void OnDeactivated(Transform interactor)
    {
        DeactivateFireExtinguisher();
    }

    public void ResetObject()
    {
        if (_netObj != null && !_netObj.HasAuthority)
            _netObj.RequestOwnership();

        PercentFull = 1;
    }
}
