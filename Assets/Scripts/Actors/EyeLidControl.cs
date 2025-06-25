using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EyeState
{
    Open,
    Closed,
    Blink,
    Dizzy
}
public class EyeLidControl : MonoBehaviour
{
    public BAHDOL.NPC_Animator NPCAnimator;
    public Animator Animator;
    public SkinnedMeshRenderer SkinnedMesh;
    public float BlinkMinTime = 3;
    public float BlinkMaxTime = 6;
    public EyeState EyeState = EyeState.Blink;

    private float _blinkTime = 3;
    private bool _blinkActive = false;
    private bool _lidDown = true;
    private float t = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        if(Animator == null)
        {
            Animator = GetComponentInParent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Animator == null || SkinnedMesh == null)
        {
            return;
        }

        ////States where we want to hard set the eyes as either forced Open (no blinking) or Closed (unconscious)
        //if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Dead") || Animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
        //{
        //    SkinnedMesh.SetBlendShapeWeight(0, 0);
        //    SkinnedMesh.SetBlendShapeWeight(1, 0);
        //    return;
        //}
        //if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Dizzy"))
        //{
        //    SkinnedMesh.SetBlendShapeWeight(0, 0);
        //    SkinnedMesh.SetBlendShapeWeight(1, 100);
        //    return;
        //}
        //if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Unconscious"))
        //{
        //    SkinnedMesh.SetBlendShapeWeight(0, 100);
        //    SkinnedMesh.SetBlendShapeWeight(1, 0);
        //    return;
        //}

        switch (EyeState)
        {
            case EyeState.Open:
                SkinnedMesh.SetBlendShapeWeight(1, 0);
                SkinnedMesh.SetBlendShapeWeight(0, 0);
                break;
            case EyeState.Closed:
                SkinnedMesh.SetBlendShapeWeight(1, 0);
                SkinnedMesh.SetBlendShapeWeight(0, 100);
                break;
            case EyeState.Blink:
                SkinnedMesh.SetBlendShapeWeight(1, 0);
                if (_blinkActive)
                {
                    if (_lidDown)
                    {
                        t += Time.deltaTime * 4;
                        if (t > 1)
                        {
                            t = 1;
                            //SkinnedMesh.SetBlendShapeWeight(0, 1);
                            _lidDown = false;
                        }
                        SkinnedMesh.SetBlendShapeWeight(0, Mathf.Lerp(0, 100, t));
                    }
                    else
                    {
                        t -= Time.deltaTime * 4;
                        if (t < 0)
                        {
                            _lidDown = true;
                            _blinkActive = false;
                            t = 0;
                        }
                        SkinnedMesh.SetBlendShapeWeight(0, Mathf.Lerp(0, 100, t));
                    }

                }

                if (Time.time > _blinkTime)
                {
                    _blinkActive = true;

                    _blinkTime = Time.time + Random.Range(BlinkMinTime, BlinkMaxTime);
                }
                break;
            case EyeState.Dizzy:
                SkinnedMesh.SetBlendShapeWeight(0, 0);
                SkinnedMesh.SetBlendShapeWeight(1, 100);
                break;
            default:
                break;
        }
        
        //Blink every 3-4 seconds (human norm is average 15-20 per minute)
        
    }
}
