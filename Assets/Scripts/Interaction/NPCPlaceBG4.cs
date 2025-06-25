using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPlaceBG4 : MonoBehaviour
{
    public NetworkedObject NetObj;    
    public StretcherController NPCSocketHelper;
    public bool IsStaticBody = false;

    private NPCController _npc;
    private BodyBehavior _bodyBehavior;
    // Start is called before the first frame update
    void Start()
    {
        if (NetObj == null)
        {
            NetObj = GetComponentInParent<NetworkedObject>();
        }
        if (NetObj != null)
        {
            NetObj.RegisterMessageHandler(OnNetObjMessage);
        }
        _npc = GetComponent<NPCController>();
        if (NPCSocketHelper == null)
        {
            TryGetComponent(out NPCSocketHelper);
        }
        _bodyBehavior = GetComponentInChildren<BodyBehavior>();
    }

    private void OnNetObjMessage(string messageType, CodedInputStream reader)
    {
        if (messageType == "APPLY_BG4")
        {
            if (IsStaticBody)
            {
                //if (TryGetComponent<BodyBehavior>(out var bodyBehavior))
                //{
                //    if(!bodyBehavior.BG4Active)
                //        bodyBehavior.EnableBG4();
                //}
                if(_bodyBehavior != null)
                {
                    if (!_bodyBehavior.BG4Active)
                    {
                        _bodyBehavior.EnableBG4();
                    }
                }
            }
            if (_npc.HasBG4)
            {
                return;
            }
            _npc.SetEquipment(MinerEquipmentFlags.BG4);
            
        }

        if (messageType == "STRETCHER_BG4")
        {
            NPCSocketHelper.RequestSetBG4Active(true);
        }
    }

    public bool CanApplyBG4()
    {
        if (_npc != null && !_npc.HasBG4)
        {
            return true;
        }
        else if (NPCSocketHelper != null && !NPCSocketHelper.GetNPCBG4State())
        {
            return true;
        }

        return false;
    }

    public bool RequestEnableBG4()
    {
        if (_npc != null)
        {
            if (_npc.HasBG4)
            {
                return false;
            }

            if (NetObj.HasAuthority)
                _npc.SetEquipment(MinerEquipmentFlags.BG4);
            else
                NetObj.SendMessage("APPLY_BG4", new VRNTextMessage());

            return true;
        }
        else if (NPCSocketHelper != null)
        {
            if (NPCSocketHelper.GetNPCBG4State())
            {
                return false;
            }
            NPCSocketHelper.RequestSetBG4Active(true);
            //NetObj.SendMessage("STRETCHER_BG4", new VRNTextMessage());
            return true;
        }
        else if (IsStaticBody)
        {
            //if (TryGetComponent<BodyBehavior>(out var bodyBehavior))
            //{
            //    return bodyBehavior.EnableBG4();
            //}
            if (_bodyBehavior != null)
            {
                if (NetObj.HasAuthority)
                {
                    return _bodyBehavior.EnableBG4();
                }
                else
                {
                    NetObj.SendMessage("APPLY_BG4", new VRNTextMessage());
                }
                return true;
            }
        }
        return false;
    }
}
