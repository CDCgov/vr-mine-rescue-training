using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using BAHDOL;
using Google.Protobuf;
using UnityEditor;

[System.Flags]
public enum MinerEquipmentFlags : uint
{
    None = 0,
    SafetyGlasses = 0b1,
    BG4 = 0b10,

    All = 0xFFFFFFFF
}

[System.Serializable]
public class NPCEquipmentData
{
    public MinerEquipmentFlags EquipmentType;
    public List<GameObject> EquipmentObjects;
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CustomXRInteractable))]
[RequireComponent(typeof(NPC_Animator))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(FollowCharacter))]
public class NPCController : MonoBehaviour, INetSync, IInteractableObject, ISelectableObjectAction
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;

    public string FollowTargetString;
    public AudioSource FollowSource;
    public AudioClip FollowYouClip;
    public SoundCollection FollowResponseCollection;
    public AudioClip WaitHereClip;
    public SoundCollection WaitResponseCollection;
    public SoundCollections soundCollections;       // SO of all sound collections for loading purposes.
    public string NPCName = "Han";
    public GameObject IndicatorLight;
    public Transform RagdollParent;

    public bool IsDebug = false;
    public bool YellowIndicatorOn = false;
    public bool GreenIndicatorOn = false;
    public float InteractorCooldown = 2;
    public float NPCHeight = 1.78f;
    public float NPCCrouchHeight = 1.5f;

    public bool IsAlive 
    {
        get 
        {
            if (_npcAnimState == null)
                return true;

            if (_npcAnimState.State == NPCState.Dead)
                return false;

            return true;
        }
    }
    public bool SyncLocalPosition = false;

    public List<NPCEquipmentData> EquipmentData;

    public bool IsStanding
    {
        get { return _standing; }
    }

    public bool IsConscious
    {
        get
        {
            if (_npcAnimState == null)
            {
                bool unconscious = _animator.GetCurrentAnimatorStateInfo(0).IsName("Unconscious") || _animator.GetCurrentAnimatorStateInfo(0).IsName("FallOverBackwards");
                return !unconscious;
            }
            else if (_npcAnimState.State == NPCState.Alive)
                return true;

            return false;
        }
    }

    public bool IsFollowing
    {
        get
        {
            if (_targetToFollow >= 0)
                return true;

            //if (_followCharacter != null && _followCharacter.enabled  && 
            //    _followCharacter.shouldFollowTarget)
            //    return true;

            return false;
        }
    }

    public bool CanMove
    {
        get
        {
            if (!IsAlive)
                return false;
            if (!IsConscious)
                return false;

            if (_npcAnimState != null)
                return _npcAnimState.CanMove;
            else
                return true;
        }
    }

    public bool HasBG4
    {
        get
        {
            return (_equipmentFlags & MinerEquipmentFlags.BG4) > 0;

            //if (_compInfoNPC == null)
            //    TryGetComponent<ComponentInfo_NPC>(out _compInfoNPC);

            //if (_compInfoNPC != null)
            //    return _compInfoNPC.isBG4;

            //return false;
        }
    }

    public bool IsRagdoll
    {
        get { return _ragdollEnabled; }
    }

    public MinerEquipmentFlags EquipmentFlags
    {
        get { return _equipmentFlags; }
        //set
        //{
        //    _equipmentFlags = value;
        //}
    }

    public MineNPCInfo MineNPCInfo;

    private NPC_Animator _npcAnimator;
    private Animator _animator;
    private FollowCharacter _followCharacter;
    private CustomXRInteractable _customXRInteractable;

    private bool _standing = false;
    //public NetSyncSocketTarget NetSyncSocketTarget;
    private NavMeshAgent _navMeshAgent;

    private NetworkedObject _netObj;
    private bool _isStanding = false;
    //private NetSyncNPC _NetSyncNPC;
    private MineNPCHost _mineNPCHost;
    private Vector3 _destination;

    private VRNNPCData _npcData;
    private float _clickCooldown = 0;
    private int _followSelection = 0;
    private int _waitSelection = 0;
    private int _targetToFollow = -1;
    private NPCMinerResponse _npcMinerResponse;
    private NPCTriggerDeath _npcTriggerDeath;
    private MinerEquipmentFlags _equipmentFlags;
    private ComponentInfo_NPC _compInfoNPC;
    private bool _hasBeenAssessed = false;

    private bool _bg4Enabled = false;
    private int _heightRaycastMask;
    private int _deadStateHash;
    private NPCAnimState _npcAnimState;
    private int _animStateHash;
    private bool _ragdollEnabled = false;
    private Rigidbody[] _ragdollBodies;
    private int _cachedPlayerID = -1;
    //private bool _ownerFollowingPlayer = false;
    //private PlayerRepresentation _cachedPlayer;

    //private MinerEquipmentFlags _currentEquipment = 0;

    void Awake()
    {
        _npcAnimator = GetComponent<NPC_Animator>();
        _animator = GetComponent<Animator>();
        _followCharacter = GetComponent<FollowCharacter>();
        _customXRInteractable = GetComponent<CustomXRInteractable>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _npcMinerResponse = GetComponent<NPCMinerResponse>();
        //_NetSyncNPC = GetComponentInChildren<NetSyncNPC>();
        _npcTriggerDeath = GetComponent<NPCTriggerDeath>();
        TryGetComponent<ComponentInfo_NPC>(out _compInfoNPC);

        _mineNPCHost = GetComponent<MineNPCHost>();
        if(MineNPCInfo == null)
        {
            MineNPCInfo = GetComponent<MineNPCInfo>();
        }

        MineNPCInfo.NPCName = NPCName;

        _heightRaycastMask = LayerMask.GetMask("Guardian", "Floor");

        _deadStateHash = Animator.StringToHash("Base Layer.Dead");
    }

    public void Start()
    {
        _netObj = GetComponent<NetworkedObject>();
        if (_netObj != null)
            _netObj.RegisterMessageHandler(OnNetObjMessage);

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        
        _destination = _navMeshAgent.destination;

        if (RagdollParent != null)
            _ragdollBodies = RagdollParent.GetComponentsInChildren<Rigidbody>();
        else
            _ragdollBodies = null;

        _animator.keepAnimatorStateOnDisable = true;
        _npcTriggerDeath.OnDeath += _npcTriggerDeath_OnDeath;
        _npcTriggerDeath.OnUnconscious += _npcTriggerDeath_OnUnconscious;
    }

    private void _npcTriggerDeath_OnUnconscious()
    {
        _hasBeenAssessed = false;
    }

    private void _npcTriggerDeath_OnDeath()
    {
        _hasBeenAssessed = false;
    }

    private void OnDestroy()
    {
        _npcTriggerDeath.OnDeath -= _npcTriggerDeath_OnDeath;
        _npcTriggerDeath.OnUnconscious -= _npcTriggerDeath_OnUnconscious;
    }

    private void Update()
    {
        var height = CheckAvailableHeight();
        height = (height - NPCCrouchHeight) / (NPCHeight - NPCCrouchHeight);
        height = Mathf.Clamp(height, 0, 1);
        _animator.SetFloat("Vertical", height);
        if (_hasBeenAssessed)
        {
            if (PlayerManager.CurrentPlayer != null)
            {
                float dist = Vector3.Distance(PlayerManager.CurrentPlayer.RigTransform.position, transform.position);
                if(dist > 9.144f)
                {
                    _hasBeenAssessed = false;//Reset assessment if distance is greater than 30 feet
                }
            }
        }   
    }

    private float CheckAvailableHeight()
    {
        const float offset = 0.5f;

        var pos = transform.position + new Vector3(0, offset, 0);
        if (!Physics.Raycast(pos, Vector3.up, out var hit, NPCHeight - offset, _heightRaycastMask, QueryTriggerInteraction.Collide))
        {
            return NPCHeight;
        }

        var height = (hit.point.y - pos.y) + offset;
        return height;
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (messageType == "NPC_STAND")
        {
            _standing = true;
            //ActivateStand();
            //MineNPCInfo.Status = "Stopped";
        }

        if (messageType == "START_FOLLOW")
        {
            //_npcMinerResponse.RespondToNearbyKnocks = false;
            Debug.Log("Received Start Follow");
            //_ownerFollowingPlayer = true;
            
            FollowSource.clip = FollowResponseCollection.Sounds[_followSelection];
            FollowSource.Play();
            
            _followSelection++;
            if(_followSelection >= FollowResponseCollection.Sounds.Length)
            {
                _followSelection = 0;
            }
        }

        if (messageType == "STOP_FOLLOW")
        {
            Debug.Log("Received Stop Follow");
            //_ownerFollowingPlayer = false;

            FollowSource.clip = WaitResponseCollection.Sounds[_waitSelection];
            FollowSource.Play();
            
            _waitSelection++;
            if(_waitSelection >= WaitResponseCollection.Sounds.Length)
            {
                _waitSelection = 0;
            }
        }

        if (messageType == "NPC_DEAD")
        {
            Debug.Log("Received Dead");
            //_ownerFollowingPlayer = false;

            //if (_customXRInteractable != null)
            //{
            //    _customXRInteractable.enabled = false;
            //}
        }

        if (messageType == "NPC_SIT")
        {
            //NPC_Animator.JumpToAnimationState(BAHDOL.AnimationState.Seated);
        }
    }
    public void ActivateStand()
    {
        _isStanding = true;
        _animator.SetTrigger("StandUp");
    }

    public void StartFollow(Transform target)
    {
        //_npcMinerResponse.RespondToNearbyKnocks = false;
        //FollowCharacter.characterToFollow = playerToFollow;
        if (Time.time < _clickCooldown)
        {
            return;
        }
        else
        {
            _clickCooldown = Time.time + InteractorCooldown;
        }

        if (_netObj != null)
        {
            _netObj.RequestOwnership();
        }

        int playerID = 0;
        if (PlayerManager.CurrentPlayer != null)
            playerID = PlayerManager.CurrentPlayer.PlayerID;

        if (playerID < 0)
            playerID = 0;

     
        if (_followCharacter.characterToFollow != target)
        {
            _npcAnimator.TransitionToAnimation(BAHDOL.AnimationState.Walking);
            //int ID = interactor.PlayerManager.CurrentPlayer.PlayerID;

            _followCharacter.characterToFollow = target;
            _followCharacter.shouldFollowTarget = true;
            _followCharacter.HasAuthority = true;
            _navMeshAgent.enabled = true;
            _targetToFollow = playerID;
            if (_netObj != null)
            {
                _netObj.SendMessage("START_FOLLOW", new VRNTextMessage());
            }
            //int selection = Random.Range(0, FollowResponseCollection.Sounds.Length - 1);
            FollowSource.clip = FollowResponseCollection.Sounds[_followSelection];
            FollowSource.Play();
            _followSelection++;
            if (_followSelection >= FollowResponseCollection.Sounds.Length)
            {
                _followSelection = 0;
            }
            //IndicatorLight.SetActive(true);
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.NpcstartFollow,
                ObjectName = NPCName,
                Position = target.position.ToVRNVector3(),
                Rotation = transform.rotation.ToVRNQuaternion(),
                SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
                PositionMetadata = NPCName
            });
        }
        else
        {
            ClearFollowTarget();

            if (_netObj != null)
            {
                _netObj.SendMessage("STOP_FOLLOW", new VRNTextMessage());
            }
            //int selection = Random.Range(0, WaitResponseCollection.Sounds.Length - 1);
            FollowSource.clip = WaitResponseCollection.Sounds[_waitSelection];
            FollowSource.Play();
            if (_waitSelection >= WaitResponseCollection.Sounds.Length)
            {
                _waitSelection = 0;
            }
            //IndicatorLight.SetActive(false);
            //_NetSyncNPC.NPCStatus = NPC_Status.Stopped;
            NetworkManager.LogSessionEvent(new VRNLogEvent
            {
                EventType = VRNLogEventType.NpcstopFollow,
                ObjectName = NPCName,
                Position = target.position.ToVRNVector3(),
                Rotation = transform.rotation.ToVRNQuaternion(),
                SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID,
                PositionMetadata = NPCName
            });
        }
    }

    private void ClearFollowTarget()
    {
        _followCharacter.characterToFollow = null;
        _followCharacter.shouldFollowTarget = false;
        _followCharacter.HasAuthority = false;

        _navMeshAgent.enabled = false;
        _targetToFollow = -1;
    }

    //public void NPCDeath()
    //{
    //    _followCharacter.characterToFollow = null;
    //    _followCharacter.shouldFollowTarget = false;
    //    _followCharacter.HasAuthority = false;
       
    //    _navMeshAgent.enabled = false;
    //    _targetToFollow = -1;
    //    if (_netObj != null)
    //    {
    //        _netObj.SendMessage("NPC_DEAD", new VRNTextMessage());
    //    }

      
    //}

    public void StartStandAnimation()
    {
        if (!_standing)
        {
            //Standing = true;
            StartCoroutine(StandAnimationDelayed());
            //ActivateStand();
            //if (_netObj != null)
            //{
            //    _netObj.SendMessage("NPC_STAND", new VRNTextMessage());
            //}
        }
    }

    IEnumerator StandAnimationDelayed()
    {
        yield return new WaitForSeconds(1);
        if (!_standing)
        {
            _standing = true;
            //if (_netObj.HasAuthority)
            //    _NetSyncNPC.StandingStatus = true;
            ActivateStand();
            if (_netObj != null)
            {
                _netObj.SendMessage("NPC_STAND", new VRNTextMessage());
            }
        }
    }

    public void UpdateFollowTarget(string guid)
    {
        NetworkedObject netObjTar = NetworkManager.GetNetworkObject(System.Guid.Parse(guid));
        if (netObjTar != null)
        {
            _followCharacter.characterToFollow = netObjTar.transform;
            _followCharacter.shouldFollowTarget = true;
        }
    }

    public string GetStatus()
    {
        if (!IsAlive)
            return "Dead";

        bool unconscious = !IsConscious;
        if (unconscious)
            return "Unconscious";

        string output = "";
        var animStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"Path Hash: {animStateInfo.fullPathHash}, {Animator.StringToHash("Base Layer.Seated")}");
        if (animStateInfo.fullPathHash == Animator.StringToHash("Base Layer.Seated"))
        {
            output = "Sitting";
        }
        else
        {
            if(_targetToFollow == -1)
            {
                output = "Idle";
            }
            else
            {
                output = $"Following {PlayerManager.GetPlayer(_targetToFollow).Name}";
            }
        }


        return output;
    }


    public bool NeedsUpdate()
    {
        return true;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        if (_npcData == null)
            _npcData = new VRNNPCData();

        _followCharacter.enabled = true;
        _npcAnimator.enabled = true;
        //_ownerFollowingPlayer = false;

        var animStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        int animatorState = -1;

        if (_npcAnimState != null)
        {
            animatorState = _animStateHash;
        }
        else
        {
            animatorState = animStateInfo.fullPathHash;
        }

        //Debug.Log($"AnimatorState: {animatorState}");
        //if (!IsAlive)
        //    animatorState = _deadStateHash;



        _npcData.AnimatorState = animatorState;
        _npcData.AnimTime = animStateInfo.normalizedTime;
        _npcData.Walking = _animator.GetBool("walking");
        _npcData.Running = _animator.GetBool("running");
        _npcData.AtDestination = _animator.GetBool("atDestination");
        _npcData.Crouching = _animator.GetBool("crouching");
        _npcData.ForwardVelocity = _animator.GetFloat("Forward");
        _npcData.LateralVelocity = _animator.GetFloat("Lateral");

        _npcData.IsFollowing = _navMeshAgent.enabled;
        _npcData.NavDestination = _navMeshAgent.destination.ToVRNVector3();

        if (SyncLocalPosition)
        {
            _npcData.NpcPosition = transform.localPosition.ToVRNVector3();
            _npcData.NpcRotation = transform.localRotation.ToVRNQuaternion();
        }
        else
        {
            _npcData.NpcPosition = transform.position.ToVRNVector3();
            _npcData.NpcRotation = transform.rotation.ToVRNQuaternion();
        }

        _npcData.TargetPlayerID = _targetToFollow;
        _npcData.EquipmentFlags = (uint)_equipmentFlags;

        //_npcData.WriteDelimitedTo(writer);
        writer.WriteMessage(_npcData);
    }

    public void SyncObjState(CodedInputStream reader)
    {
        //if (_npcData == null)
        //    _npcData = VRNNPCData.Parser.ParseDelimitedFrom(reader);
        //else
        //    _npcData.MergeDelimitedFrom(reader);
        if (_npcData == null)
            _npcData = new VRNNPCData();

        _npcData.Crouching = false;
        _npcData.AtDestination = false;
        _npcData.AnimTime = 0;
        _npcData.AnimatorState = 0;
        _npcData.ForwardVelocity = 0;
        _npcData.IsFollowing = false;
        _npcData.IsStanding = false;
        _npcData.LateralVelocity = 0;
        _npcData.Running = false;
        _npcData.TargetPlayerID = -1;
        _npcData.Walking = false;
        _npcData.EquipmentFlags = 0;

        if (_npcData.NavDestination == null)
            _npcData.NavDestination = new VRNVector3();
        _npcData.NavDestination.X = 0;
        _npcData.NavDestination.Y = 0;
        _npcData.NavDestination.Z = 0;

        if (_npcData.NpcPosition == null)
            _npcData.NpcPosition = new VRNVector3();
        _npcData.NpcPosition.X = 0;
        _npcData.NpcPosition.Y = 0;
        _npcData.NpcPosition.Z = 0;

        if (_npcData.NpcRotation == null)
            _npcData.NpcRotation = new VRNQuaternion();
        _npcData.NpcRotation.X = 0;
        _npcData.NpcRotation.Y = 0;
        _npcData.NpcRotation.Z = 0;
        _npcData.NpcRotation.W = 0;
        

        reader.ReadMessage(_npcData);

        _followCharacter.enabled = false;
        _npcAnimator.enabled = false;

        var navDest = _npcData.NavDestination.ToVector3();
        var npcPos = _npcData.NpcPosition.ToVector3();

        Vector3 dir = navDest - npcPos;
        //var rot = Quaternion.LookRotation(dir, Vector3.up);
        var rot = _npcData.NpcRotation.ToQuaternion();

        //if (_npcData.IsFollowing)
        //{
        //    _navMeshAgent.enabled = false;


        //    transform.position = npcPos;
        //    transform.rotation = rot;
        //    _targetToFollow = _npcData.TargetPlayerID;
        //}
        //else
        //{
        //    _navMeshAgent.enabled = false;
        //    transform.position = npcPos;
        //    transform.rotation = rot;
        //    _targetToFollow = -1;
        //}

        _navMeshAgent.enabled = false;
        if (SyncLocalPosition)
        {
            transform.localPosition = npcPos;
            transform.localRotation = rot;
        }
        else
        {
            transform.position = npcPos;
            transform.rotation = rot;
        }
        _targetToFollow = _npcData.TargetPlayerID;

        _animator.SetBool("walking", _npcData.Walking);
        _animator.SetBool("running", _npcData.Running);
        _animator.SetBool("atDestination", _npcData.AtDestination);
        _animator.SetBool("crouching", _npcData.Crouching);

        _animator.SetFloat("Forward", _npcData.ForwardVelocity);
        _animator.SetFloat("Lateral", _npcData.LateralVelocity);

        var animStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        //if (animStateInfo.fullPathHash != _npcData.AnimatorState && !_animator.IsInTransition(0))
        if (_animStateHash != _npcData.AnimatorState)
        {
            //_animator.Play(_npcData.AnimatorState, 0, _npcData.AnimTime);

            //var transition = _animator.GetAnimatorTransitionInfo(0);
            //if (transition.)

            _animator.enabled = true;

            Debug.Log($"NPCController: Cross fading from {animStateInfo.fullPathHash} to {_npcData.AnimatorState}");
            _animator.CrossFadeInFixedTime(_npcData.AnimatorState, 0.25f, 0, 0);
            _animStateHash = _npcData.AnimatorState;
        }

        //bool isAlive = true;
        //if (_npcData.AnimatorState == _deadStateHash)
        //    isAlive = false;

        //Debug.Log($"NPC Alive: {IsAlive} State Alive: {isAlive}");

        //var equipFlags = (MinerEquipmentFlags)_npcData.EquipmentFlags;
        //if (!_bg4Enabled && ((equipFlags & MinerEquipmentFlags.BG4) > 0))
        //{
        //    if (_compInfoNPC != null)
        //    {
        //        _compInfoNPC.SetBG4(true);
        //    }
        //    _bg4Enabled = true;
        //}
        SetEquipment((MinerEquipmentFlags)_npcData.EquipmentFlags);

        //if (IsRagdoll && isAlive)
        //{
        //    Ragdoll(false);
        //}

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
        
    }

    public void OnDropped(Transform interactor)
    {
        
    }

    public ActivationState CanActivate
    {
        get
        {
            bool unconscious = !IsConscious;
            if (PlayerManager.CurrentPlayer != null)
                _cachedPlayerID = PlayerManager.CurrentPlayer.PlayerID;
            if (!_hasBeenAssessed)
            {
                string condition;
                if (!IsAlive)
                    condition = "Dead";
                else if (unconscious)
                    condition = "Unconscious";
                else
                    condition = "Conscious";

                _hasBeenAssessed = true;
                NetworkManager.LogSessionEvent(VRNLogEventType.NpcstatusUpdate, $"Condition: {condition}", 
                    transform.position, transform.rotation, NPCName);
                //_cachedPlayer = PlayerManager.CurrentPlayer;                
            }

            if (!IsAlive)
                return ActivationState.Error;

            if (!unconscious)
                return ActivationState.Ready;

            return ActivationState.Unknown;
        }
    }

    public string SelectableActionName => "Test NPC Animation";

    public void OnActivated(Transform interactor)
    {
        if (!IsAlive)
            return;

        if (interactor.TryGetComponent<CustomXRInteractor>(out var xrInteractor) && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Unconscious"))
        {
            if (xrInteractor.HeadCamera != null)
                StartFollow(xrInteractor.HeadCamera.transform);
        }        
    }

    public void OnDeactivated(Transform interactor)
    {
        
    }

    public void PerformSelectableObjectAction()
    {
        var obj = new GameObject("NPCAnimationTestObject");
        obj.transform.position = transform.position + new Vector3(-5, 0, 0);

        StartFollow(obj.transform);
    }

    public void SetAnimStateData(NPCAnimState data, AnimatorStateInfo stateInfo)
    {
        Debug.Log($"NPCController: {gameObject.name} animator changed state {data.ToString()}");

        _npcAnimState = data;
        _animStateHash = stateInfo.fullPathHash;
        _hasBeenAssessed = false; //if animation state gets changed, flag the assessment
        //if (_customXRInteractable != null)
        //    _customXRInteractable.enabled = data.CanInteractWith;

        if (_npcAnimState.Ragdoll)
        {
            if (!_ragdollEnabled)
                Ragdoll(true);
        }
        else
        {
            if (_ragdollEnabled)
                Ragdoll(false);
        }


        if (_npcAnimState.CanMove)
        {
            if (_followCharacter != null && _followCharacter.characterToFollow != null)
            {
                _followCharacter.StartFollow();
            }
        }
        else
        {
            if (_followCharacter != null)
                _followCharacter.StopFollow();
        }
    }


    public void Kill()
    {
        _npcAnimator.TransitionToAnimation(BAHDOL.AnimationState.Dying);
        //NPCAnimator.JumpToAnimationState(BAHDOL.AnimationState.Dying);

        //if (_customXRInteractable != null)
        //{
        //    _customXRInteractable.enabled = false;
        //}
        //if(_npcMinerResponse != null)
        //    _npcMinerResponse.SetDead(true);

        ClearFollowTarget();

        //if (_netObj == null || !_netObj.HasAuthority)
        //    return;


        //_netObj.SendMessage("NPC_DEAD", new VRNTextMessage());
    }

    public void Revive()
    {
        Ragdoll(false);
        _animator.enabled = true;
        _npcAnimator.TransitionToAnimation(BAHDOL.AnimationState.Idle);
        //_npcMinerResponse.SetDead(false);
        _hasBeenAssessed = false;
    }

    public void ForceAnimationState(BAHDOL.AnimationState state)
    {
        Ragdoll(false);
        _animator.enabled = true;
        _npcAnimator.TransitionToAnimation(state);
        _hasBeenAssessed = false;
    }

    public void ForceAnimationState(string stateName)
    {
        Ragdoll(false);
        _animator.enabled = true;
        _animator.CrossFadeInFixedTime(stateName, 0.25f, 0);
        _hasBeenAssessed = false;
    }

    public void Ragdoll(bool enableRagdoll)
    {
        if (_ragdollBodies == null)
            return;

        _ragdollEnabled = enableRagdoll;
        _animator.enabled = !enableRagdoll;

        foreach (var rb in _ragdollBodies)
        {
            rb.isKinematic = !enableRagdoll;
        }

    }

    public void SetUnconscious()
    {
        //Debug.Log("Setting NPC to unconscious");
        _npcAnimator.TransitionToAnimation(BAHDOL.AnimationState.Unconscious);
        _hasBeenAssessed = false;
    }

    public void SetEquipment(MinerEquipmentFlags equip)
    {
        if (equip == _equipmentFlags)
            return;

        foreach (var data in EquipmentData)
        {
            bool equipEnabled = ((data.EquipmentType & equip) > 0);
            EnableEquipment(data, equipEnabled);
        }

        _equipmentFlags = equip;
    }

    private void EnableEquipment(NPCEquipmentData data, bool enable)
    {
        if (data.EquipmentObjects == null || data.EquipmentObjects.Count <= 0)
            return;

        foreach (var obj in data.EquipmentObjects)
        {
            if (obj == null)
                continue;

            obj.SetActive(enable);
        }
    }
}
