using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class MinerFinalIK : MonoBehaviour
{
    public VRIK VRIK;
    //public AimIK AimIK;
    public BAHDOL.PlayerAnimator PlayerAnimator;
    public Transform HeadBone;
    public PlayerRepresentation Player;
    public Transform PlayerHead;
    public Transform ChestGoal;
    public Transform LeftHandTarget;
    public Transform RightHandTarget;

    private float _initHeight = 0;
    private float _playerHeight = PlayerRepresentation.BasePlayerHeight;
    private Vector3 _baseScale;
    private Transform _staticHead;

    private void Start()
    {
        _baseScale = VRIK.references.root.localScale;//Base scale is one. This seems redundant.
        if (PlayerAnimator == null)
        {
            PlayerAnimator.GetComponent<BAHDOL.PlayerAnimator>();
        }

        if(VRIK == null)
        {
            VRIK = GetComponent<VRIK>();
        }

        //if(AimIK == null)
        //{
        //    AimIK = GetComponent<AimIK>();
        //}
        _initHeight = transform.InverseTransformPoint(HeadBone.position).y;
    }

    private void Update()
    {
        if(Player != null)
        {
            //Debug.Log("Player height = " + Player.PlayerHeight);
            if (Player.PlayerMode == 0 || Player.PlayerMode == 1)
            {
                if (Player.PlayerHeight != _playerHeight)
                {
                    Debug.Log("Player height changed! " + Player.Name + ", " + Player.PlayerHeight);
                    _playerHeight = Player.PlayerHeight;
                    SetPlayerHeight(_playerHeight);
                }
            }
        }

    }

    public void SetHead(Transform staticHead, Transform trueHead)
    {        
        _staticHead = staticHead;
        PlayerHead = trueHead;

        SetPlayerScale(1.0f, false);
    }
   

    public void SetRightHand(Transform rightHand)
    {
        VRIK.solver.rightArm.target = rightHand;
        RightHandTarget = rightHand;
    }
    public void SetLeftHand(Transform leftHand)
    {
        VRIK.solver.leftArm.target = leftHand;
        LeftHandTarget = leftHand;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerHeight"></param>
    /// <returns>scale factor applied</returns>
    public float SetPlayerHeight(float playerHeight)
    {
        if (playerHeight <= 0)
        {
            SetPlayerScale(1, true);
            return 1.0f;
        }

        float scaleFactor = playerHeight / PlayerRepresentation.BasePlayerHeight;//1.58 is the default height of the model
        SetPlayerScale(scaleFactor, false);
        return scaleFactor;
    }

    public void SetPlayerScale(float scaleFactor, bool useStaticHead = false)
    {
        if (useStaticHead)
        {
            SetHeadTransform(_staticHead);
        }
        else
        {
            SetHeadTransform(PlayerHead);
        }

        Debug.Log($"Setting player scale to {scaleFactor:F1} on {gameObject.name}");

        VRIK.references.root.localScale = scaleFactor * Vector3.one;
        PlayerAnimator.PlayerScale = scaleFactor;
        UpdateDecalProjectorScale(scaleFactor);
    }


    private void SetHeadTransform(Transform head)
    {
        VRIK.solver.spine.headTarget = head;
        if (PlayerAnimator != null)
        {
            PlayerAnimator.HeadTransform = head;
        }

        ChestGoal.SetParent(head);
        ChestGoal.localPosition = new Vector3(0, 1, 0);
    }

    private void UpdateDecalProjectorScale(float scaleFactor)
    {
        DecalProjectorParentChange[] decalProjectors = gameObject.GetComponentsInChildren<DecalProjectorParentChange>();
        if (decalProjectors.Length > 0)
        {
            foreach (DecalProjectorParentChange dcp in decalProjectors)
            {
                dcp.UpdateProjectorSize(scaleFactor);
            }
        }
    }

}
