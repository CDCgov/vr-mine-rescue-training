using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using UnityEngine.Events;

public class Knock : MonoBehaviour
{
    public AudioSource KnockAudioPlayer;
    public SoundingStickSounds MaterialSounds;
    public SoundCollection KnockSounds;
    public SoundCollection SoundingStickKnockSounds;
    //public bool AllowPlayRequest = true;//Handle for ignoring the knock while, for example, a door is opening
    public bool AllowNPCResponse = true;
    //public UnityEvent oneTimeKnockEvent;
    //public UnityEvent knockEvent;
    public float KnockDelayTime = 2;
    //public static bool KnockLock = false;
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public string KnockName = "RA Knock";
    private bool _allowOneTimeEventResponse = true;
    public float _timeNextKnockAllowed = 0;

    private static Collider[] _colliders = null;

    private void Start()
    {
        if (NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }
        if (PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }

    }

    public string PlayKnock()
    {
        /*if (MaterialSounds != null)
        {
            KnockAudioPlayer.clip = MaterialSounds.Sounds[Random.Range(0, MaterialSounds.Sounds.Length - 1)];
        }
        else if(KnockSounds != null)
        {
            KnockAudioPlayer.clip = KnockSounds.Sounds[Random.Range(0, KnockSounds.Sounds.Length - 1)];
        }
        else
        {
            return false;
        }*/

        if (PlayKnockSound(MaterialSounds))
        {
        }
        else if (PlayKnockSound(KnockSounds))
        {
        }

        return PlayKnockResponse();

        //if (AllowPlayRequest && !KnockAudioPlayer.isPlaying)
        //{
        //    KnockAudioPlayer.Play();
        //    if (_allowOneTimeEventResponse && AllowNPCResponse)
        //    {
        //        oneTimeKnockEvent.Invoke();
        //    }
        //    if(AllowNPCResponse)
        //        knockEvent.Invoke();
        //    _timeOfLastKnock = Time.time + KnockDelayTime;

        //    return true;
        //}

        //return false;
    }

    public string SoundingStickKnock()
    {
        //if (SoundingStickKnockSounds != null)
        //{
        //    KnockAudioPlayer.clip = SoundingStickKnockSounds.Sounds[Random.Range(0, SoundingStickKnockSounds.Sounds.Length - 1)];
        //}
        //else if (KnockSounds != null)
        //{
        //    KnockAudioPlayer.clip = KnockSounds.Sounds[Random.Range(0, KnockSounds.Sounds.Length - 1)];
        //}

        if (PlayKnockSound(SoundingStickKnockSounds))
        {
        }
        else if (PlayKnockSound(MaterialSounds))
        {
        }
        else if (PlayKnockSound(KnockSounds))
        {
        }

        var knockResponse = PlayKnockResponse();

        return knockResponse;
    }

    public void LogKnockEvent(string objectName, string knockResponse, Vector3 position)
    {
        if (knockResponse == null)
            knockResponse = "No response";
        else
            knockResponse = $"Knock Response: {knockResponse}";

        NetworkManager.LogSessionEvent(new VRNLogEvent
        {
            EventType = VRNLogEventType.DoorKnock,
            ObjectName = objectName,
            Message = knockResponse,
            Position = position.ToVRNVector3(),
            Rotation = Quaternion.identity.ToVRNQuaternion(),
            SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
        }); 
    }

    private string PlayKnockResponse()
    {
        if (!AllowNPCResponse)
            return null;

        if ((Time.time < _timeNextKnockAllowed))
            return null;

        //rate limit physics check - real rate limit is in NPCMinerResponse
        _timeNextKnockAllowed = Time.time + 1;

        var responder = FindNearbyResponder();

        if (responder == null)
            return null;


        return responder.PlayVerbalResponse();

        //if (_allowOneTimeEventResponse && AllowNPCResponse)
        //{
        //    oneTimeKnockEvent.Invoke();
        //    //NetworkManager.LogSessionEvent(new VRNLogEvent
        //    //{
        //    //    EventType = VRNLogEventType.DoorKnock,
        //    //    ObjectName = KnockName,
        //    //    Position = transform.position.ToVRNVector3(),
        //    //    Rotation = transform.rotation.ToVRNQuaternion(),
        //    //    SourcePlayerID = PlayerManager.CurrentPlayer.PlayerID
        //    //});
        //}

        //if (AllowNPCResponse)
        //    knockEvent.Invoke();


        //return true;

    }

    private NPCMinerResponse FindNearbyResponder()
    {
        if (_colliders == null)
            _colliders = new Collider[250];

        NPCMinerResponse result = null;

        int numHits = Physics.OverlapSphereNonAlloc(transform.position, 6, _colliders, LayerMask.GetMask("Player", "Default"));
        if (numHits <= 0)
            return null;

        for (int i = 0; i < numHits; i++)
        {
            var col = _colliders[i];
            NPCMinerResponse npcResponse = null;

            if (col.TryGetComponent<NPCMinerResponse>(out npcResponse) && npcResponse.CanRespond)
            {
                result = npcResponse;
                break;
            }

            if (col.attachedRigidbody != null && col.attachedRigidbody.TryGetComponent<NPCMinerResponse>(out npcResponse) && npcResponse.CanRespond)
            {
                result = npcResponse;
                break;
            }
        }

        if (result == null)
            Debug.Log($"Knock found {numHits} nearby colliders, no responders");
        else
            Debug.Log($"Knock found {numHits} nearby colliders, responder: {result.name}");

        return result;
    }

    private bool PlayKnockSound(SoundCollection sounds)
    {
        if (sounds == null)
            return false;

        return PlayKnockSound(sounds.Sounds);
    }

    private bool PlayKnockSound(SoundingStickSounds sounds)
    {
        if (sounds == null)
            return false;

        return PlayKnockSound(sounds.Sounds);
    }

    private bool PlayKnockSound(AudioClip[] sounds)
    {
        if (sounds == null || sounds.Length <= 0 || KnockAudioPlayer == null)
            return false;

        if (KnockAudioPlayer.isPlaying)
            KnockAudioPlayer.Stop();

        KnockAudioPlayer.clip = sounds[Random.Range(0, sounds.Length)];
        KnockAudioPlayer.Play();

        return true;
    }
}
