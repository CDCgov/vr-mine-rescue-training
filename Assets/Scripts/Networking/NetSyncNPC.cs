using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using Google.Protobuf;

//public enum NPC_Status
//{
//    Sitting,
//    Stopped,
//    Following,
//    ExitInterior,
//    ExitExterior,
//    CustomAnimation,
//}

[System.Obsolete]
public class NetSyncNPC : MonoBehaviour
{
    /*
    public const float MaxSyncInterval = 0.5f;

    public bool SmoothTranslationUpdate = true;
    public Animator NPCAnimator;
    public string TargetToFollow;
    public bool StandingStatus = false;
    public bool FollowingStatus = false;
    public int TargetIDToFollow = 0;
    public float PlayerToFollow = -1;
    public NPC_Status NPCStatus = NPC_Status.Sitting;
    public NetworkManager NetworkManager;
    
    private VRNTransformData _NPCTransform;
    private VRNFloatData _NPCAnimatorForward;
    private VRNFloatData _NPCAnimatorLateral;
    private VRNNPCData _NPCData;
    

    private Vector3 _targetPos;
    private Quaternion _targetRot;
    private bool _receivingData = false;
    private Vector3 _lastSyncPos;
    private Quaternion _lastSyncRot;
    private float _lastSyncTime = -500;
    private float _forward;
    private float _lateral;
    private bool _isStanding;
    private NavMeshAgent _agent;
    private string _target;

    public bool NeedsUpdate()
    {
        if (Time.time - _lastSyncTime > MaxSyncInterval)
            return true;

        Vector3 posDelta = transform.position - _lastSyncPos;
        float angleDelta = Quaternion.Angle(transform.rotation, _lastSyncRot);

        if (posDelta.magnitude > 0.005f || angleDelta > 0.5f)
            return true;

        return false;
    }

    public void SyncObjState(CodedInputStream reader)
    {
        _receivingData = true;
        //_NPCTransform = VRNTransformData.Parser.ParseDelimitedFrom(reader);
        //_NPCAnimatorForward = VRNFloatData.Parser.ParseDelimitedFrom(reader);
        //_NPCAnimatorLateral = VRNFloatData.Parser.ParseDelimitedFrom(reader);
        //if(_NPCTransform.Position != null)
        //{
        //    _targetPos = _NPCTransform.Position.ToVector3();
        //}
        //if (_NPCTransform.Rotation != null)
        //{
        //    _targetRot = _NPCTransform.Rotation.ToQuaternion();
        //}
        //_forward = _NPCAnimatorForward.FloatData;
        //_lateral = _NPCAnimatorLateral.FloatData;

        //bool state = false;
        //var txtMessage = VRNTextMessage.Parser.ParseDelimitedFrom(reader);
        //if (txtMessage != null && txtMessage.Message == "true")
        //    state = true;

        //_isStanding = state;
        //var target = VRNFloatData.Parser.ParseDelimitedFrom(reader);
        //PlayerToFollow = target.FloatData;
        _NPCData.TargetPlayerID = 0;
        //_NPCData.NpcPosition = Vector3.zero.ToVRNVector3();
        if (_NPCData.NpcPosition == null)
            _NPCData.NpcPosition = new VRNVector3();
        _NPCData.NpcPosition.ResetData();
        _NPCData.IsStanding = false;
        _NPCData.IsFollowing = false;
        _NPCData.AnimatorState = 0;

        //_NPCData.MergeDelimitedFrom(reader);
        reader.ReadMessage(_NPCData);

        _targetPos = _NPCData.NpcPosition.ToVector3();
        TargetIDToFollow = _NPCData.TargetPlayerID;
        StandingStatus = _NPCData.IsStanding;
        FollowingStatus = _NPCData.IsStanding;
        NPCStatus = (NPC_Status)_NPCData.AnimatorState;
    }

    public void WriteObjState(CodedOutputStream writer)
    {
        _receivingData = false;
        //VRNTransformData xform = new VRNTransformData
        //{
        //    Rotation = transform.rotation.ToVRNQuaternion(),
        //    Position = transform.position.ToVRNVector3(),
        //};

        //xform.WriteDelimitedTo(writer);
        //VRNFloatData animatorForward = new VRNFloatData();
        //VRNFloatData animatorLateral = new VRNFloatData();
        //float forward = NPCAnimator.GetFloat("Forward");
        //float lateral = NPCAnimator.GetFloat("Lateral");
        //animatorForward.FloatData = forward;
        //animatorLateral.FloatData = lateral;
        //animatorForward.WriteDelimitedTo(writer);
        //animatorLateral.WriteDelimitedTo(writer);
        //_lastSyncTime = Time.time;
        //_lastSyncPos = transform.position;
        //_lastSyncRot = transform.rotation;

        //bool isStanding = NPCAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walking Blend Tree");
        //VRNTextMessage text = new VRNTextMessage
        //{
        //    Message = isStanding ? "true" : "false",
        //};
        //text.WriteDelimitedTo(writer);


        //VRNFloatData vRNFloatData = new VRNFloatData
        //{
        //    FloatData = PlayerToFollow
        //};
        //vRNFloatData.WriteDelimitedTo(writer);
        _NPCData.TargetPlayerID = TargetIDToFollow;
        _NPCData.NpcPosition = transform.position.ToVRNVector3();
        _NPCData.IsStanding = StandingStatus;
        _NPCData.IsFollowing = FollowingStatus;
        _NPCData.AnimatorState = (int)NPCStatus;

        //_NPCData.WriteDelimitedTo(writer);
        writer.WriteMessage(_NPCData);
    }

    // Start is called before the first frame update
    void Start()
    {
        _NPCTransform = new VRNTransformData();
        _NPCData = new VRNNPCData();
        _targetPos = transform.position;
        _targetRot = transform.rotation;
        if(NPCAnimator == null)
        {
            NPCAnimator = GetComponentInChildren<Animator>();
        }
        _agent = GetComponentInChildren<NavMeshAgent>();
        if(NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    //if (_receivingData)
    //    //{
    //    //    if (FollowingStatus)
    //    //    {
    //    //        if (Vector3.Distance(_targetPos, transform.position) > 10)
    //    //        {
    //    //            Vector3 dir = (transform.position - _targetPos).normalized;
    //    //            Vector3 telepos = _targetPos + 2 * (dir);
    //    //            _agent.Warp(telepos);
    //    //            transform.rotation = _targetRot;
    //    //        }
    //    //        else
    //    //        {
    //    //            if (Vector3.Distance(transform.position, _targetPos) > 1.0)
    //    //            {
    //    //                _agent.Warp(_targetPos);
    //    //            }
    //    //            //if (Quaternion.Angle(transform.rotation, _targetRot) > 45)
    //    //            //{
    //    //            //    transform.rotation = _targetRot;
    //    //            //}
    //    //        }
    //    //    }



    //        //NPCAnimator.SetFloat("Forward", _forward);
    //        //NPCAnimator.SetFloat("Lateral", _lateral);
    //        //if(StandingStatus && !NPCAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walking Blend Tree"))
    //        //{
    //        //    NPCAnimator.Play("Walking Blend Tree", 0);
    //        //}

    //        //StandingStatus = _isStanding;
    //        //TargetToFollow = _target;
    //    //}
    //}
    */
}
