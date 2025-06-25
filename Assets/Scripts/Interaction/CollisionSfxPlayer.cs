using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSfxPlayer : MonoBehaviour
{
    public AudioSource CollisionSoundPlayer;
    public AudioSource SocketPlayer;
    public SoundingStickSounds MaterialSounds;
    public bool PlaySfxOnEveryCollision = true;
    public NetworkedObject NetworkedObject;

    private void Start()
    {
        NetworkedObject = GetComponent<NetworkedObject>();
        if (NetworkedObject != null)
            NetworkedObject.RegisterMessageHandler(OnNetObjMessage);
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (messageType == "COLLIDESFX")
        {
            if (SocketPlayer != null)
            {
                if (SocketPlayer.isPlaying)
                    return;
            }
            if (!CollisionSoundPlayer.isPlaying)
            {
                if (MaterialSounds != null)
                {
                    CollisionSoundPlayer.clip = MaterialSounds.Sounds[Random.Range(0, MaterialSounds.Sounds.Length)];
                }
                Debug.Log($"Play collision: {gameObject.name}");
                CollisionSoundPlayer.Play();
            }
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //disable for now to use new audio system exclusively
        return;

        if(CollisionSoundPlayer == null)
        {
            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor") || PlaySfxOnEveryCollision)
        {
            if (SocketPlayer != null)
            {
                if (SocketPlayer.isPlaying)
                    return;
            }
            if (!CollisionSoundPlayer.isPlaying)
            {
                if (MaterialSounds != null)
                {
                    CollisionSoundPlayer.clip = MaterialSounds.Sounds[Random.Range(0, MaterialSounds.Sounds.Length)];
                }
                CollisionSoundPlayer.Play();

                if (NetworkedObject != null)
                {
                    if (NetworkedObject.HasAuthority)
                    {
                        NetworkedObject.SendMessage("COLLIDESFX", new VRNTextMessage());
                    }
                }
            }
        }
    }
}
