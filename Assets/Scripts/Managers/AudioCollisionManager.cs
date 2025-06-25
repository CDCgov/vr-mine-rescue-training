using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class AudioCollisionManager : SceneManagerBase
{
    public const int MAX_AUDIO_POOL = 100;

    public NetworkManager NetworkManager;

    public static AudioCollisionManager GetDefault(GameObject self) { return self.GetDefaultManager<AudioCollisionManager>("AudioCollisionManager", false); }
    
    public AudioSource CollisionAudioSource;
    public Transform AudioSourcePoolParent;
    public AudioMaterialList AMList;
    public NetworkedObject _netObj;
    List<AudioSource> _collisionAudioPool;

    private AudioSource _playingSource;

    VRNTextMessage vRNTextMessage;
    VRNTextMessage vRNTextMessage2;
    VRNVector3 vRNPositionMessage;
    VRNVector3 vRNAudioDataMessage;

    VRNAudioCollisionData _vRNAudioCollisionData;

    private string _cachedMaterialName = "";
    private string _cachedCollisionType = "";
    private Vector3 _cachedPosition = new Vector3();
    private Vector3 _cachedAudioData = new Vector3();
    private System.Random _randomizer;
    private int _NextSync = 0;
    private float _timeSyncVal = 2000;

    bool _messageComplete = false;
    bool _blackout = true;
    bool _updateLoop = true;
    private float _blackoutTime;

    private void Awake()
    {        
        _vRNAudioCollisionData = new VRNAudioCollisionData();
        _vRNAudioCollisionData.Position = new VRNVector3();
        _vRNAudioCollisionData.AudioName = "";
        _vRNAudioCollisionData.AudioType = 0;
        _vRNAudioCollisionData.ClipIndex = -1;
        _vRNAudioCollisionData.Volume = 0;
        _vRNAudioCollisionData.Pitch = 0;

        _cachedAudioData.x = -1;
    }

    private void Start()
    {
        _collisionAudioPool = new List<AudioSource>(MAX_AUDIO_POOL);
        if(NetworkManager == null)
        {
            NetworkManager = NetworkManager.GetDefault(gameObject);
        }

        //NetworkManager.RegisterHandler(VRNPacketType.PlayAudioClip, PlayAudioClipHandler);
        //NetworkManager.RegisterHandler(VRNPacketType.PlayAudioClip, UpdateClipSync);

        int firstSeed = DateTime.Now.Year + DateTime.Now.Day;
        _randomizer = new System.Random(firstSeed);
        NetworkManager.RegisterHandler(VRNPacketType.RandomSeed, UpdateSeed);
        //if (_netObj == null)
        //{
        //    _netObj = GetComponentInChildren<NetworkedObject>();
        //}

        //_netObj.RegisterMessageHandler(OnNetObjMessage);
        _updateLoop = true;
        if (NetworkManager.IsServer)
        {
            StartCoroutine(UpdateSeedLoop());
        }
        SceneManager.activeSceneChanged += Blackout;
    }
    private void OnDestroy()
    {
        //if (_netObj != null)
        //    _netObj.UnregisterMessageHandler(OnNetObjMessage);
        _updateLoop = false;
        if (NetworkManager != null)
        {
            NetworkManager.UnregisterHandler(VRNPacketType.RandomSeed, UpdateSeed);
            if (NetworkManager.IsServer)
            {
                StopAllCoroutines();
            }
        }

        foreach (var source in _collisionAudioPool)
        {
            if (source.gameObject != null)
                Destroy(source.gameObject);
        }

        _collisionAudioPool.Clear();
    }

    private void Blackout(Scene current, Scene next)
    {
        _blackoutTime = Time.time + 3f;
    }

    private void UpdateSeed(VRNHeader header, CodedInputStream reader, int clientID)
    {
        VRNRandomSeedData seed = new VRNRandomSeedData();
        reader.ReadMessage(seed);
        _randomizer = new System.Random(seed.Seed);
        //Debug.Log($"Received audio collision random seed of {seed.Seed} at {System.DateTime.Now.ToLongTimeString()}");
    }

    private void UpdateClipSync(VRNHeader header, CodedInputStream reader, int clientID)
    {
        if (_netObj.HasAuthority)
        {
            VRNFloatData newSync = new VRNFloatData();
            reader.ReadMessage(newSync);
            int updatedNext = (int)newSync.FloatData;
            while(_NextSync < updatedNext)
            {
                _randomizer.Next();
                _NextSync++;
            }
        }
    }
    private void PlayAudioClipHandler(VRNHeader header, CodedInputStream reader, int clientID)
    {
        _vRNAudioCollisionData.Position = new VRNVector3();
        _vRNAudioCollisionData.AudioName = "";
        _vRNAudioCollisionData.ClipIndex = -1;
        _vRNAudioCollisionData.AudioType = 0;
        _vRNAudioCollisionData.Volume = 0;
        _vRNAudioCollisionData.Pitch = 0;

        reader.ReadMessage(_vRNAudioCollisionData);
        AudioClip clip = AMList.GetClipByMaterialNameAndIndex(_vRNAudioCollisionData.AudioName, (AudioMaterialType)_vRNAudioCollisionData.AudioType, _vRNAudioCollisionData.ClipIndex);

        PlayCollisionSound(_vRNAudioCollisionData.Position.ToVector3(), clip, _vRNAudioCollisionData.Volume, _vRNAudioCollisionData.Pitch);
        Debug.Log($"Audio Collision Message Received! {_vRNAudioCollisionData.Position.ToVector3()}, {_vRNAudioCollisionData.AudioName}, {_vRNAudioCollisionData.Volume}");

    }

    //private void OnNetObjMessage(string messageType, CodedInputStream reader)
    //{
    //    switch (messageType)
    //    {
    //        case "ACOL":
    //            var audioCollision = new VRNAudioCollisionData();
    //            audioCollision.Position = new VRNVector3();
    //            audioCollision.AudioName = "";
    //            audioCollision.ClipIndex = -1;
    //            audioCollision.AudioType = 0;
    //            audioCollision.Volume = 0;
    //            audioCollision.Pitch = 0;

    //            reader.ReadMessage(audioCollision);
    //            AudioClip clip = AMList.GetClipByMaterialNameAndIndex(audioCollision.AudioName, (AudioMaterialType)audioCollision.AudioType, audioCollision.ClipIndex);

    //            PlayCollisionSound(audioCollision.Position.ToVector3(), clip, audioCollision.Volume, audioCollision.Pitch);
    //            Debug.Log($"Audio Collision Message Received! {audioCollision.Position.ToVector3()}, {audioCollision.AudioName}, {audioCollision.Volume}");
    //            break;
    //        default:
    //            break;
    //    }
        
    //}

    public void ProcessCollision(GameObject source, GameObject target, Vector3 collisionPoint, Vector3 collisionNormal, Vector3 collisionRelativeVelocity)
    {
        //if (!NetworkManager.IsServer)
        //{
        //    return;
        //}
        if (source == null)
        {
            return;
        }
        AudioMaterial materialToPlay;
        CollisionSoundEffect colSfx = source.GetComponent<CollisionSoundEffect>();        
        if(colSfx == null)
        {
            return;
        }
        float vol;
        float pitch;

        target.TryGetComponent<CollisionSoundEffect>(out var targetSfx);

        if (targetSfx == null)
        {     
            if(target.TryGetComponent<Collider>(out var collider))
            {
                if (collider.isTrigger)
                {
                    return;
                }
            }
            materialToPlay = colSfx.SoundMaterial;
            if(materialToPlay == null)
            {
                return;
            }
            float mass;
            if(colSfx.CollisionRigidbody != null)
            {
                mass = colSfx.CollisionRigidbody.mass;
            }
            else
            {
                if (colSfx.SoundMaterial != null)
                    mass = colSfx.SoundMaterial.ReferenceMass;
                else
                    mass = 1;
            }
            vol = materialToPlay.GetImpactVolume(collisionRelativeVelocity, collisionNormal, mass);
            //pitch = 1 + materialToPlay.GetRandomPitch();
            //pitch = materialToPlay.GetScaleModPitch(source.transform.localScale);
            pitch = materialToPlay.GetUnifiedModifiedPitch(colSfx.ObjectBounds, collisionRelativeVelocity, mass, colSfx.DefaultPitch);
            AudioSoundSet fallbackSoundSet = materialToPlay.GetSoundSet(materialToPlay.FallbackCollisionType);
            //int index = UnityEngine.Random.Range(0, fallbackSoundSet.Sounds.Count);
            int index = _randomizer.Next(fallbackSoundSet.Sounds.Count);
            _NextSync++;
            //if (vol < 0.5f)
            //    vol = 0.5f;
            //Do not do audio collisions during scene load!
            if (Time.time < _blackoutTime)
            {
                return;
            }
            Debug.Log($"{Time.frameCount}: Playing collision default audio for {source.name}:{target.name} collision. Velocity {collisionRelativeVelocity:F1} vol {vol:F1}, pitch {pitch:F1} for scale {source.transform.localScale.magnitude}, at index: {index}");
            CollisionAudioClip collisionAudioClip = materialToPlay.GetCollisionAudio(materialToPlay.FallbackCollisionType, index);
            AudioClip clip = collisionAudioClip.CollisionClip;            
            pitch = collisionAudioClip.PitchRange.Clamp(pitch);
            PlayCollisionSound(collisionPoint, clip, vol, pitch);
            //SendNetworkSound(collisionPoint, materialToPlay.AudioMaterialName, materialToPlay.FallbackCollisionType, index, vol, pitch);
        }
        else
        {
            //CollisionSoundEffect targetSfx = target.GetComponent<CollisionSoundEffect>();
            materialToPlay = colSfx.SoundMaterial;
            if(materialToPlay == null)
            {
                return;
            }
            float mass;
            if (colSfx.CollisionRigidbody != null)
            {
                mass = colSfx.CollisionRigidbody.mass;
            }
            else
            {
                mass = colSfx.SoundMaterial.ReferenceMass;
            }
            vol = materialToPlay.GetImpactVolume(collisionRelativeVelocity, collisionNormal, mass);
            //pitch = materialToPlay.GetScaleModPitch(source.transform.localScale);
            pitch = materialToPlay.GetUnifiedModifiedPitch(colSfx.ObjectBounds, collisionRelativeVelocity, mass,colSfx.DefaultPitch);
            AudioSoundSet audioSoundSet = materialToPlay.GetSoundSet(targetSfx.SoundMaterial.MaterialType);

            //if (vol < 0.5f)
            //    vol = 0.5f;

            

            //materialToPlay.
            //int index = UnityEngine.Random.Range(0, audioSoundSet.Sounds.Count);
            int index = _randomizer.Next(audioSoundSet.Sounds.Count);
            Debug.Log($"{Time.frameCount}: Playing collision audio for {source.name}:{target.name} collision. Velocity {collisionRelativeVelocity:F1} vol {vol:F1}, pitch {pitch:F1} for scale {source.transform.localScale.magnitude}, at index: {index}");
            //_NextSync++;
            //Do not do audio collisions during scene load!
            if (Time.time < _blackoutTime)
            {
                return;
            }

            CollisionAudioClip collisionAudioClip = materialToPlay.GetCollisionAudio(targetSfx.SoundMaterial.MaterialType, index);
            AudioClip clip = collisionAudioClip.CollisionClip;
            pitch = collisionAudioClip.PitchRange.Clamp(pitch);

            PlayCollisionSound(collisionPoint, clip, vol, pitch);
            //SendNetworkSound(collisionPoint, materialToPlay.AudioMaterialName, targetSfx.SoundMaterial.MaterialType, index, vol, pitch);
        }
        //Debug.Log($"Tracking Next Sync Count: {_NextSync} at {System.DateTime.Now.ToShortTimeString()}");
    }

    private AudioSource CreatePooledAudioSource()
    {
        AudioSource newSource;

        GameObject newSourceObj = new GameObject("PoolAudio");

        if (AudioSourcePoolParent != null)
            newSourceObj.transform.SetParent(AudioSourcePoolParent, false);
        else
            newSourceObj.transform.SetParent(transform, false);
        
        newSource = newSourceObj.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.spatialBlend = 1;
        newSource.maxDistance = CollisionAudioSource.maxDistance;
        newSource.outputAudioMixerGroup = CollisionAudioSource.outputAudioMixerGroup;

        _collisionAudioPool.Add(newSource);

        Debug.Log($"AudioCollisionManager: Expanded audio source pool to {_collisionAudioPool.Count}");

        return newSource;
    }

    void PlayCollisionSound(Vector3 collisionPosition, AudioClip clip, float volume = 1, float pitch = 1)
    {
        if (clip == null)
        {
            var trace = new System.Diagnostics.StackTrace(true);
            Debug.LogError($"AudioCollisionManager: Attempted to play null audio clip: {trace}");
            return;
        }

        AudioSource sourceToPlay = null;

        foreach(AudioSource audio in _collisionAudioPool)
        {
            if (!audio.isPlaying)
            {
                sourceToPlay = audio;
            }
        }

        if (sourceToPlay == null) 
        {
            if (_collisionAudioPool.Count >= MAX_AUDIO_POOL)
            {
                Debug.LogError("AudioCollisionManager: Attempted to play more than MAX_AUDIO_POOL sounds simultaneously");
                return;
            }

            sourceToPlay = CreatePooledAudioSource();
        }

        sourceToPlay.transform.position = collisionPosition;
        sourceToPlay.clip = clip;
        sourceToPlay.pitch = pitch;
        sourceToPlay.volume = volume;
        sourceToPlay.Play();

        //StartCoroutine(EndClip(sourceToPlay, clip.length));
    }

    void SendNetworkSound(Vector3 collisionPosition, string audioMaterialName, AudioMaterialType audioMaterialType, int clipIndex, float volume = 1, float pitch = 1)
    {
        VRNAudioCollisionData aColData = new VRNAudioCollisionData();
        aColData.Position = new VRNVector3();
        aColData.Position.X = collisionPosition.x;
        aColData.Position.Y = collisionPosition.y;
        aColData.Position.Z = collisionPosition.z;
        aColData.AudioName = audioMaterialName;
        aColData.ClipIndex = clipIndex;
        aColData.AudioType = (int)audioMaterialType;        
        aColData.Volume = volume;
        aColData.Pitch = pitch;

        NetworkManager.SendNetMessage(VRNPacketType.PlayAudioClip, aColData, false, true);

        //_netObj.SendMessage("ACOL", aColData);
        
        
        
        //vRNTextMessage.Message = audioMaterialName;

        //vRNPositionMessage.X = collisionPosition.x;
        //vRNPositionMessage.Y = collisionPosition.y;
        //vRNPositionMessage.Z = collisionPosition.z;
        //vRNAudioDataMessage.X = clipIndex;
        //vRNAudioDataMessage.Y = volume;
        //vRNAudioDataMessage.Z = pitch;

        //_netObj.SendMessage("ACOLNAME", vRNTextMessage);
        //vRNTextMessage2.Message = audioMaterialType;
        //_netObj.SendMessage("ACOLTYPE", vRNTextMessage2);
        //_netObj.SendMessage("ACOLPOS", vRNPositionMessage);
        //_netObj.SendMessage("ACOLDATA", vRNAudioDataMessage);
    }

    bool TargetHasSfx(GameObject go)
    {
        CollisionSoundEffect cSFx = go.GetComponent<CollisionSoundEffect>();
        bool nullCheck = false;
        if(cSFx != null)
        {
            nullCheck = true;
        }
        return nullCheck;
    }

    //IEnumerator EndClip(AudioSource source, float clipLength)
    //{
    //    yield return new WaitForSeconds(clipLength);
    //    source.Stop();
    //}

    IEnumerator UpdateSeedLoop()
    {
        while (_updateLoop)
        {
            yield return new WaitForSeconds(5);
            VRNRandomSeedData seed = new VRNRandomSeedData();
            seed.Seed = UnityEngine.Random.Range(0, 100000);
            _randomizer = new System.Random(seed.Seed);
            NetworkManager.SendNetMessage(VRNPacketType.RandomSeed, seed, false, true);
            //Debug.Log($"Sent update seed of {seed.Seed} at time {DateTime.Now.ToLongTimeString()}");
        }
    }
} 