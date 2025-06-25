using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsCheck : MonoBehaviour
{
    public Transform ArrowTransform;
    public Transform XRRigBase;
    public Camera vrCam;
    public AudioSource AudioSource;
    public PlayerManager PlayerManager;
    private bool _active = false;

    private int _origCullingMask;
    private int _cullWorldMask;
    private int _cullWorldMask2;
    
    // Start is called before the first frame update
    void Start()
    {
        _origCullingMask = vrCam.cullingMask;
        _cullWorldMask = LayerMask.NameToLayer("Guardian");
        _cullWorldMask2 = LayerMask.NameToLayer("Player");
        Debug.Log("mask 1: " + _cullWorldMask + "mask 2: " + _cullWorldMask2);
        Debug.Log((1 << _cullWorldMask | 1 << _cullWorldMask2) + ", bitwise or");

        if(PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }
        //_cullWorldMask = LayerMask.NameToLayer("Guardian");        
    }

    // Update is called once per frame
    void Update()
    {
        if (_active)
        {
            //Vector3 position = transform.localPosition + (transform.forward.normalized * 0.5f);

            ArrowTransform.position = transform.position + (transform.forward * 0.5f);
            
            ArrowTransform.LookAt(XRRigBase);
            Vector3 rot = ArrowTransform.localEulerAngles;
            rot.x = 0;
            rot.z = 0;
            ArrowTransform.localEulerAngles = rot;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.CurrentPlayer.TranslationEnabled || PlayerManager.CurrentPlayer.DebugInterfaceEnabled)
        {
            return;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Guardian"))
        {
            ArrowTransform.gameObject.SetActive(true);
            _active = true;
            vrCam.cullingMask = 1 << _cullWorldMask | 1 << _cullWorldMask2;
            //vrCam.cullingMask = 1 << _cullWorldMask;
            AudioSource.Play();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Guardian"))
        {
            ArrowTransform.gameObject.SetActive(false);
            _active = false;
            vrCam.cullingMask = _origCullingMask;
            AudioSource.Stop();
        }
    }
}
