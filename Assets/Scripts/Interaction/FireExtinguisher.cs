using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisher : MonoBehaviour
{
    private bool _isGrabbed = true;
    private bool _isOn = false;
    public ParticleSystem ExtinguisherEmitter;
    public ParticleSystem FirstPuffEmitter;
    public AudioSource ExtinguisherAudio;

    private RaycastHit _rHit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_isGrabbed)
        {
            if (Input.GetButton("Fire1"))
            {
                if (!_isOn)
                {
                    ExtinguisherEmitter.Play();
                    ExtinguisherAudio.Play();
                    FirstPuffEmitter.Play();
                    _isOn = true;
                }
                if(Physics.Raycast(transform.position, transform.forward, out _rHit, 2))
                {
                    if(_rHit.collider.tag == "Fire")
                    {                        
                        FireInteraction fI = _rHit.collider.GetComponent<FireInteraction>();
                        if(fI != null)
                        {
                            fI.OnExtinguish();
                        }
                    }
                }
            }
            else
            {
                if (_isOn)
                {
                    ExtinguisherEmitter.Stop();
                    ExtinguisherAudio.Stop();
                    FirstPuffEmitter.Stop();              
                    _isOn = false;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        //Debug.DrawLine(transform.position, transform.position + new Vector3(0, 0, 2), Color.red);
        Debug.DrawRay(transform.position, transform.forward, Color.red, 2);
    }
}
