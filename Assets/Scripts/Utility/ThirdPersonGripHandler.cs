using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonGripHandler : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public PlayerRepresentation PlayerRep;
    public Animator PlayerAnimator;
    public BAHDOL.VrHandController vrHandController;
    public int ID = -1;

    private bool _priorRGrip = false;
    private bool _priorLGrip = false;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
        {
            PlayerManager.GetDefault(gameObject);
        }
        
    }

    private void Update()
    {
        if(PlayerRep.LeftGrip != _priorLGrip)
        {
            LeftGrip(PlayerRep.LeftGrip);
            _priorLGrip = PlayerRep.LeftGrip;
        }
        if(PlayerRep.RightGrip != _priorRGrip)
        {
            RightGrip(PlayerRep.RightGrip);
            _priorRGrip = PlayerRep.RightGrip;
        }
    }

    private void OnDestroy()
    {
        PlayerRep.PlayerLGripChanged -= LeftGrip;
        PlayerRep.PlayerRGripChanged -= RightGrip;
    }

    public void SetPlayerRep(PlayerRepresentation player)
    {
        PlayerRep = player;
        //PlayerRep.PlayerLGripChanged += LeftGrip;
        //PlayerRep.PlayerRGripChanged += RightGrip;
    }

    private void LeftGrip(bool value)
    {
        vrHandController.LeftHandClosed = value;
    }

    private void RightGrip(bool value)
    {
        Debug.Log("RGrip message received " + value);
        vrHandController.RightHandClosed = value;
    }

    private void OnPlayerMessage(VRNPlayerMessageType messageType, VRNPlayerMessage msg)
    {
        if(messageType == VRNPlayerMessageType.PmLGripOn || messageType == VRNPlayerMessageType.PmRGripOn)
        {
            Debug.Log("Grip message received " + msg.PlayerID);
            
        }
        if (msg.PlayerID != ID)
        {
            return;
        }
        switch (messageType)
        {
            case VRNPlayerMessageType.PmLGripOn:
                //PlayerAnimator.SetBool("isLeftHandClosed", msg.BoolData);
                vrHandController.LeftHandClosed = msg.BoolData;
                break;
            case VRNPlayerMessageType.PmRGripOn:
                //PlayerAnimator.SetBool("isRightHandClosed", msg.BoolData);
                vrHandController.RightHandClosed = msg.BoolData;
                break;
            default:
                break;
        }
    }

    public void ConfigurePlayerManagerAndEventHandler(PlayerManager pm)
    {
        PlayerManager = pm;
        PlayerManager.RegisterPlayerMessageHandler(OnPlayerMessage);
    }

}
