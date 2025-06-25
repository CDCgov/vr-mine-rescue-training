using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class FreezeVfx : MonoBehaviour
{
    [SerializeField] float _timeScale = 0;
    [SerializeField] VisualEffect VFX;
    [SerializeField] bool NeedActivate = false;
    [SerializeField] int AdvanceFrames = 0;

    
    // Start is called before the first frame update
    void Start()
    {
        VFX = GetComponent<VisualEffect>();
        VFX.pause = true;
    }
}
