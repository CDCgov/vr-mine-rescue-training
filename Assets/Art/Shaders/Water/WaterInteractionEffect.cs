using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
[RequireComponent(typeof(Collider))]
public class WaterInteractionEffect : MonoBehaviour
{
    public float RippleDelay = 2.75f;
    public SoundEffectCollection SplashSounds;

    private VisualEffect _vfx;
    private Collider _collider;

    private VFXEventAttribute _vfxAttrib;

    private Dictionary<int, float> _lastRipple;


    public void SpawnMovmentEffect(Vector3 pos)
    {
        if (_vfx == null)
            return;

        pos = transform.InverseTransformPoint(pos);
        _vfxAttrib.SetVector3("position", pos);
        
        _vfx.SendEvent("SpawnMovementSplash", _vfxAttrib);
        //_vfx.AdvanceOneFrame();
    }

    // Start is called before the first frame update
    void Start()
    {
        _vfx = GetComponent<VisualEffect>();
        _collider = GetComponent<Collider>();

        _vfxAttrib = _vfx.CreateVFXEventAttribute();

        _lastRipple = new Dictionary<int, float>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //int numContacts = collision.contactCount;
        //for (int i = 0; i < numContacts; i++)
        //{
        //    var contact = collision.GetContact(i);
        //    var pos = contact.point;
        //    pos = transform.InverseTransformPoint(pos);

        //    VFXEventAttribute attr = _vfx.CreateVFXEventAttribute();
        //    attr.SetVector3("position", pos);

        //    Debug.Log($"Sending SpawnSplash at {pos}");
        //    _vfx.SendEvent("SpawnSplash", attr);

        //}
    }

    private Vector3 GetEffectPosition(Collider other)
    {
        var pos = other.transform.position;
        pos = _collider.ClosestPoint(pos);
        pos = transform.InverseTransformPoint(pos);

        return pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        //var pos = other.transform.position;
        //pos = _collider.ClosestPoint(pos);
        //pos = transform.InverseTransformPoint(pos);
        var pos = GetEffectPosition(other);

        //VFXEventAttribute attr = _vfx.CreateVFXEventAttribute();
        _vfxAttrib.SetVector3("position", pos);

        Debug.Log($"Sending SpawnSplash at {pos} caused by {other.name}");
        _vfx.SendEvent("SpawnSplash", _vfxAttrib);

        if (SplashSounds != null)
            SplashSounds.PlaybackRandomWithPitchVariation(transform.TransformPoint(pos), 1.0f, 0.8f, 1.5f);
    }

    private void OnTriggerStay(Collider other)
    {
        float lastRipple = 0;
        int instID = other.gameObject.GetInstanceID();
        _lastRipple.TryGetValue(instID, out lastRipple);
        //Debug.Log($"TriggerStay {other.name} {instID}");

        if (Time.time - lastRipple > RippleDelay)
        {
            _lastRipple[instID] = Time.time;
            var pos = GetEffectPosition(other);

            //var attrib = _vfx.CreateVFXEventAttribute();
            _vfxAttrib.SetVector3("position", pos);
            _vfx.SendEvent("SpawnRipple", _vfxAttrib);
            //attrib.Dispose();

            Debug.Log($"SpawnRipple {other.name} {instID} {pos}");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
