using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Google.Protobuf;
using System;
using UnityEngine.Serialization;

[RequireComponent(typeof(NPCController))]
public class NPCMinerResponse : MonoBehaviour
{
    public const float NPCResponseDelay = 3.0f;

    [FormerlySerializedAs("VocalResposneAudioSource")]
    public AudioSource VocalResponseAudioSource;
    //public NPCController RefugeNPCBehaviors;
    //public Animator Animator;

    public bool AudioPlayed = false;
    //public bool DebugIgnoreSingleAudioEvent = false;
    public float Delay = 30f;
    public bool Standing = false;
    public bool RespondToNearbyKnocks = false;

    private NPCController _npcController;
    private NetworkedObject _netObj;
    private float _delayTimer = 0;
    //private bool _canPlayClip = false;
    private ComponentInfo_NPC _componentInfo_NPC;
    private List<Knock> _nearbyKnockSources;
    //private int _randomIndex = 0;
    //private bool _isDead = false;

    public bool CanRespond
    {
        get
        {
            if (!RespondToNearbyKnocks)
                return false;

            if (VocalResponseAudioSource == null || _componentInfo_NPC == null)
                return false;

            if (_npcController == null || !_npcController.IsConscious || _npcController.IsFollowing)
                return false;

            if (Time.time <= _delayTimer)
                return false;

            return true;
        }
    }


    public void Start()
    {
        _componentInfo_NPC = GetComponent<ComponentInfo_NPC>();
        RespondToNearbyKnocks = _componentInfo_NPC.RespondsToNearbyKnocks;
        _nearbyKnockSources = new List<Knock>();
        //if(Animator == null)
        //{
        //    Animator = GetComponent<Animator>();
        //}
        _netObj = GetComponent<NetworkedObject>();
        if (_netObj != null)
            _netObj.RegisterMessageHandler(OnNetObjMessage);

        if (!TryGetComponent<NPCController>(out _npcController))
        {
            Debug.LogError($"NPCMinerResponse: NPC {gameObject.name} missing NPCController");
        }
    }

    //private void Update()
    //{
    //    if(Animator.GetCurrentAnimatorStateInfo(0).IsName("Dead") || Animator.GetCurrentAnimatorStateInfo(0).IsName("Dying") || Animator.GetCurrentAnimatorStateInfo(0).IsName("Unconscious") || Animator.GetCurrentAnimatorStateInfo(0).IsName("Dizzy"))
    //    {
    //        //_isDead = true;
    //        //RespondToNearbyKnocks = false;
    //        return;
    //    }

    //    if (RefugeNPCBehaviors != null)
    //    {
    //        if (_canPlayClip && !RefugeNPCBehaviors.IsStanding)
    //        {
    //            _delayTimer = Time.time + Delay;
    //            PlayClip();
    //            _canPlayClip = false;
    //        }
    //        return;
    //    }

    //    if (_canPlayClip)
    //    {
    //        _delayTimer = Time.time + Delay;
    //        PlayClip();
    //        _canPlayClip = false;
    //    }

    //    if (RespondToNearbyKnocks)
    //    {            
    //        Collider[] cols = Physics.OverlapSphere(transform.position, 6);
    //        List<Knock> foundKnocks = new List<Knock>();
    //        foreach (var col in cols)
    //        {
    //            Knock knock;
    //            //who's there?
    //            if(col.TryGetComponent<Knock>(out knock))
    //            {
    //                if (!_nearbyKnockSources.Contains(knock))
    //                {
    //                    knock.AllowNPCResponse = true;
    //                    knock.knockEvent.AddListener(PlayVerbalResponse);
    //                    _nearbyKnockSources.Add(knock);
    //                    foundKnocks.Add(knock);
    //                    Debug.Log($"Added knock to {gameObject.name}! Source: {knock.name}");
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        if(_nearbyKnockSources.Count > 0)
    //        {
    //            foreach (var knock in _nearbyKnockSources)
    //            {
    //                knock.knockEvent.RemoveListener(PlayVerbalResponse);
    //            }
    //            _nearbyKnockSources.Clear();
    //        }
    //    }
    //}

    //public void SetCanPlay(bool canPlay)
    //{
    //    _canPlayClip = canPlay;
    //}

    //public void SetDead(bool dead)
    //{
    //    _isDead = dead;
    //}

    public void OnDestroy()
    {
        if (_netObj != null)
            _netObj.UnregisterMessageHandler(OnNetObjMessage);
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        var msg = new VRNFloatData();
        reader.ReadMessage(msg);
        if (Time.time > _delayTimer)
        {
            if (messageType == "PLAY_VERBAL_RESPONSE")
            {
                //PlayClip();
                //_randomIndex = (int)msg.FloatData;
                //_canPlayClip = true;

                int randomIndex = (int)msg.FloatData;
                PlayClip(randomIndex);
            }
        }
    }

    public string PlayVerbalResponse()
    {
        if (!CanRespond)
            return null;

        VRNFloatData num = new VRNFloatData();
        int randomIndex = -1;
        SoundCollection sounds = null;

        if (_componentInfo_NPC == null)
            return null;

        sounds = GetSoundCollection();
        if (sounds == null)
            return null;

        int length = sounds.Sounds.Length;
        if (length > 0)
        {
            randomIndex = UnityEngine.Random.Range(0, length);
        }

        num.FloatData = randomIndex;
        //send request to play on remote clients
        if (_netObj != null)
        {
            _netObj.SendMessage("PLAY_VERBAL_RESPONSE", num);
        }

        PlayClip(randomIndex);

        //_delayTimer = Time.time + NPCResponseDelay;

        return GetResponseTypeName();

    }

    private void PlayClip(int randomIndex = -1)
    {
        if (!CanRespond)
            return;

        if (VocalResponseAudioSource == null || VocalResponseAudioSource.isPlaying)
            return;

        var sounds = GetSoundCollection();
        if (sounds == null)
            return;

        int length = sounds.Sounds.Length;
        if (length <= 0)
            return;

        if (randomIndex < 0 || randomIndex >= sounds.Sounds.Length)
            randomIndex = UnityEngine.Random.Range(0, length);

        //int ran = UnityEngine.Random.Range(0, length);
        VocalResponseAudioSource.clip = sounds.Sounds[randomIndex];
        VocalResponseAudioSource.Play();
        _delayTimer = Time.time + NPCResponseDelay;

    }

    private SoundCollection GetSoundCollection()
    {
        SoundCollection sounds = null;
        switch (_componentInfo_NPC.KnockSelection)
        {
            case 1:
                sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.GenericResponses;
                break;
            case 2:
                sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.SoloResponses;
                break;
            case 3:
                sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.DuoResponses;
                break;
            case 4:
                sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.GroupResponses;
                break;
            case 5:
                sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.AirtightResponses;
                break;
            default:
                break;
        }

        return sounds;
    }

    private string GetResponseTypeName()
    {
        string typeName = null;
        switch (_componentInfo_NPC.KnockSelection)
        {
            case 1:
                //sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.GenericResponses;
                typeName = "Generic";
                break;
            case 2:
                //sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.SoloResponses;
                typeName = "Solo";
                break;
            case 3:
                //sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.DuoResponses;
                typeName = "Duo";
                break;
            case 4:
                //sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.GroupResponses;
                typeName = "Group";
                break;
            case 5:
                //sounds = _componentInfo_NPC.NPCVoicePackCollection.NPCVoicePacks[_componentInfo_NPC.VoicePackSelection].NPCResponses.AirtightResponses;
                typeName = "Airtight";
                break;
            default:
                break;
        }

        return typeName;
    }



    //public bool NeedsUpdate()
    //{
    //    if (Time.time > _lastUpdateTime + _updateRate)
    //    {
    //        _lastUpdateTime = Time.time;
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}

    //private void Update()
    //{
    //    if (_receivingData)
    //    {
    //        if (_remotePlayAudio)
    //        {
    //            PlayRemoteResponse();
    //        }
    //    }
    //}


    //private void PlayRemoteResponse()
    //{
    //    if (!_audioPlayed || DebugIgnoreSingleAudioEvent)
    //    {
    //        if (!VocalResposneAudioSource.isPlaying)
    //        {
    //            VocalResposneAudioSource.Play();
    //        }
    //        _audioPlayed = true;
    //    }
    //}

    //public void SyncObjState(Stream reader)
    //{
    //    _receivingData = true;
    //    bool state = false;
    //    if (bool.TryParse(VRNTextMessage.Parser.ParseDelimitedFrom(reader).Message, out state))
    //    {
    //        _remotePlayAudio = state;
    //    }
    //}

    //public void WriteObjState(Stream writer)
    //{
    //    _receivingData = false;
    //    VRNTextMessage text = new VRNTextMessage
    //    {
    //        Message = _remotePlayAudio.ToString()
    //    };
    //    text.WriteDelimitedTo(writer);
    //}
}
