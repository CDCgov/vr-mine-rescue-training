using BAHDOL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ComponentInfo_NPC : ModularComponentInfo, ISaveableComponent
{
    // need to store npc name, located in MineNPCHost, RefugeNPCBehaviors,MineNPCInfo,TextTexture on NameProjector child object
    // NPC_Animator component starting animation
    // NavMeshAgent follow speed, follow distance(stopping distance)
    // RefugeNPCBehaviors component, soundcollection, follow response collection
    // RefugeNPCBehaviors component, soundcollection, wait response collection
    // show/hide bG4, boolean TODO don't know what this is or where it is located


    public string componentName = "NPC";
    //protected ObjectInfo objectInfo;
    public Inspector.ExposureLevel volumeExposureLevel;

    public NavMeshAgent navMesh_Component;
    public MineNPCHost mineNPCHost_Component;
    public NPCController refugeNPCBehaviors_Component;
    public MineNPCInfo mineNPCInfo_Component;
    public List<TextTexture> textTexture_Components;
    public NPC_Animator npcAnimator_Component;
    public SoundCollections soundCollection;
    public NPCVoicePackCollection NPCVoicePackCollection;

    public bool HasBG4
    {
        get { return _hasBG4; }
        set
        {
            _hasBG4 = value;
            if (refugeNPCBehaviors_Component != null)
                refugeNPCBehaviors_Component.SetEquipment(_hasBG4 ? 
                    MinerEquipmentFlags.BG4 : MinerEquipmentFlags.SafetyGlasses);
        }
    }

    public float speed;
    public float stopDistance;
    public string npcName;
    public string startingAnimName;
    public string followCollectionName;
    public string waitCollectionName;
    public int VoicePackSelection;
    public int KnockSelection = 0;
    //public bool isBG4 = false;
    public bool RespondsToNearbyKnocks = false;
    public List<GameObject> BG4GameObjects = new List<GameObject>();
    public GameObject GlassesObject;

    private bool _hasBG4;

    private void Awake()
    {
        //objectInfo = GetComponent<ObjectInfo>();
        //if (objectInfo == null) objectInfo = GetComponentInParent<ObjectInfo>();
        //if (objectInfo != null)
        //{
        //    if (!objectInfo.componentInfo_NPCs.Contains(this)) objectInfo.componentInfo_NPCs.Add(this);
        //}

        navMesh_Component = GetComponent<NavMeshAgent>();
        mineNPCHost_Component = GetComponent<MineNPCHost>();
        refugeNPCBehaviors_Component = GetComponent<NPCController>();
        mineNPCInfo_Component = GetComponent<MineNPCInfo>();
        textTexture_Components = new List<TextTexture>(GetComponentsInChildren<TextTexture>()); // turn into array GetComponentsInChildren
        npcAnimator_Component = GetComponent<NPC_Animator>();
        if(refugeNPCBehaviors_Component != null)
        {
            soundCollection = refugeNPCBehaviors_Component.soundCollections;
            speed = navMesh_Component.speed;
            stopDistance = navMesh_Component.stoppingDistance;
            followCollectionName = refugeNPCBehaviors_Component.FollowResponseCollection.name;
            waitCollectionName = refugeNPCBehaviors_Component.WaitResponseCollection.name;
            npcName = refugeNPCBehaviors_Component.NPCName;
        }
        

        foreach (Transform t in transform)
        {
            if(t.name.Contains("BG4"))
            {
                BG4GameObjects.Add(t.gameObject);
            }
        }
    }
    public string[] SaveInfo()
    {
        //Debug.Log($"Saving NPC info");
        return new string[] { "Name|" + npcName,"StartingAnim|" + startingAnimName,
                              "FollowSpeed|" + speed,"StopDist|" + stopDistance,
                              "VoiceSelection|" + VoicePackSelection,
                              "RespondToKnocks|" + RespondsToNearbyKnocks,
                              "BG4|" + _hasBG4,
                              "KnockSelection|" + KnockSelection
                               };
    }
    public string SaveName()
    {
        return componentName;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null)
        {
            Debug.Log("Failed to load NPC component info. Saved component is null for " + gameObject.name); return;
        }
        componentName = component.GetComponentName();
        SetName(component.GetParamValueAsStringByName("Name"));

        startingAnimName = component.GetParamValueAsStringByName("StartingAnim");
        BAHDOL.AnimationState state;
        Enum.TryParse(startingAnimName, out state);
        SetStartAnim(state);

        float speed;
        float.TryParse(component.GetParamValueAsStringByName("FollowSpeed"), out speed);
        SetSpeed(speed);

        float stopDist;
        float.TryParse(component.GetParamValueAsStringByName("StopDist"), out stopDist);
        SetStopDistance(stopDist);

        if(refugeNPCBehaviors_Component != null)
        {
            //SetSoundCollection(refugeNPCBehaviors_Component.FollowResponseCollection, component.GetParamValueAsStringByName("FollowResponse"), true);
            //SetSoundCollection(refugeNPCBehaviors_Component.WaitResponseCollection, component.GetParamValueAsStringByName("WaitResponse"), false);
            string voice = component.GetParamValueAsStringByName("VoiceSelection");
            if(voice != null)
            {
                int.TryParse(voice, out VoicePackSelection);
            }
            else
            {
                VoicePackSelection = 0;
            }
            refugeNPCBehaviors_Component.FollowResponseCollection = NPCVoicePackCollection.NPCVoicePacks[VoicePackSelection].FollowResponses;
            refugeNPCBehaviors_Component.WaitResponseCollection = NPCVoicePackCollection.NPCVoicePacks[VoicePackSelection].WaitResponses;
        }
        else
        {
            string voice = component.GetParamValueAsStringByName("VoiceSelection");
            if(voice != null)
            {
                int.TryParse(voice, out VoicePackSelection);
            }
            else
            {
                VoicePackSelection = 0;
            }
        }

        bool.TryParse(component.GetParamValueAsStringByName("BG4"), out _hasBG4);
        //SetBG4(_hasBG4);

        if (refugeNPCBehaviors_Component != null)
        {
            if (_hasBG4)
                refugeNPCBehaviors_Component.SetEquipment(MinerEquipmentFlags.BG4);
            else
                refugeNPCBehaviors_Component.SetEquipment(MinerEquipmentFlags.SafetyGlasses);
        }

        string knocks = component.GetParamValueAsStringByName("RespondToKnocks");
        if(knocks != null)
        {
            bool.TryParse(knocks, out RespondsToNearbyKnocks);
        }
        else
        {
            RespondsToNearbyKnocks = false;
        }

        string response = component.GetParamValueAsStringByName("KnockSelection");
        if(response != null)
        {
            int.TryParse(response, out KnockSelection);
        }
        else
        {
            KnockSelection = 0;
        }

    }

    public void SetStartAnim(BAHDOL.AnimationState state, bool animateNow = true)
    {
        startingAnimName = state.ToString();
        if(animateNow && npcAnimator_Component)
        {
            npcAnimator_Component.stateToStartIn = state; // Net state npc component check for relation
            //npcAnimator_Component.TransitionToAnimation(state);
            npcAnimator_Component.JumpToAnimationState(state);
        }
    }

    public void SetStopDistance(float stop)
    {
        stopDistance = Mathf.Clamp(stop, 1, 5);
        if(navMesh_Component)
        {
            navMesh_Component.stoppingDistance = stopDistance;
        }
        FollowCharacter follow = GetComponent<FollowCharacter>();
        if(follow)
        {
            follow.distanceToKeep = stopDistance;
        }
    }

    public void SetSpeed(float spd)
    {
        speed = Mathf.Clamp(spd, 1, 10);
        if(navMesh_Component)
        {
            navMesh_Component.speed = speed;
        }
    }

    public void SetSoundCollection(SoundCollection collection, string choice, bool isFollow)
    {
        collection = soundCollection.GetCollectionByString(choice);
        if(isFollow)
        {
            refugeNPCBehaviors_Component.FollowResponseCollection = collection;
        }
        else
        {
            refugeNPCBehaviors_Component.WaitResponseCollection = collection;
        }
    }

    public void SetSoundSelect(int selection)
    {
        VoicePackSelection = selection;
    }

    public void SetName(string name)
    {
        if(mineNPCHost_Component) { mineNPCHost_Component.MineNPC.DisplayName = name; }
        if(refugeNPCBehaviors_Component) { refugeNPCBehaviors_Component.NPCName = name; }
        if(mineNPCInfo_Component) { mineNPCInfo_Component.NPCName = name; }
        int count = textTexture_Components.Count;
        if (count > 0) 
        {
            for (int i = 0; i < count; i++)
            {
                if(textTexture_Components[i] != null)
                {
                    textTexture_Components[i].Text = name;
                    textTexture_Components[i].UpdateTexture();
                }
            }
        }
        npcName = name;
    }


    //public void SetBG4(bool enable)
    //{
    //    isBG4 = enable;

    //    if (BG4GameObjects != null)
    //    {
    //        foreach (GameObject obj in BG4GameObjects)
    //        {
    //            obj.SetActive(enable);
    //        }
    //    }

    //    if(GlassesObject != null)
    //    {
    //        GlassesObject.SetActive(!enable);
    //    }

    //    if (refugeNPCBehaviors_Component != null)
    //    {
    //        if (enable)
    //            refugeNPCBehaviors_Component.EquipmentFlags = MinerEquipmentFlags.BG4;
    //        else
    //            refugeNPCBehaviors_Component.EquipmentFlags = MinerEquipmentFlags.SafetyGlasses;
    //    }
    //}
}
