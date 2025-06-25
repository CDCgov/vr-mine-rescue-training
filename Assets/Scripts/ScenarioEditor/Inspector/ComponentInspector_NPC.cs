using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

public class ComponentInspector_NPC : ComponentInspector<ComponentInfo_NPC>
{
    // need to store npc name, located in MineNPCHost, RefugeNPCBehaviors,MineNPCInfo,TextTexture on NameProjector child object
    // NPC_Animator component starting animation
    // NavMeshAgent follow speed, follow distance(stopping distance)
    // RefugeNPCBehaviors component, soundcollection, follow response collection
    // RefugeNPCBehaviors component, soundcollection, wait response collection
    // show/hide bG4, boolean 


    //Inspector inspector;
    public TMP_Text headerText;
    [SerializeField] TMP_InputField npcNameText;
    [SerializeField] SliderField followSpeedSlider; // between 1 and 10, default 2
    [SerializeField] SliderField stopDistanceSlider; // between 1 and 5, default 1.75 
    [SerializeField] TMP_Dropdown StartingAnimationDropdown;
    //[SerializeField] TMP_Dropdown FollowSoundDropdown;
    //[SerializeField] TMP_Dropdown WaitSoundDropdown;
    [SerializeField] TMP_Dropdown VoicePackDropdown;
    public TMP_Dropdown KnockResponseDropdown;
    //[SerializeField] ComponentInfo_NPC TargetComponentInfo;
    [SerializeField] Toggle BG4Toggle;
    public Toggle RespondToKnocksToggle;

    public NPCVoicePackCollection NPCVoicesPacks;

    //public int index;

    public override void Start()
    {
        base.Start();

        //inspector = Inspector.instance;
        //targetNPCInfo = inspector.targetInfo.componentInfo_NPCs[index];
        InitializeValues();

        followSpeedSlider.onSubmitValue.AddListener(SetFollowSpeed);
        stopDistanceSlider.onSubmitValue.AddListener(SetStopDistance);
        //FollowSoundDropdown.onValueChanged.AddListener(SetFollowCollection);
        //WaitSoundDropdown.onValueChanged.AddListener(SetWaitCollection);
        VoicePackDropdown.onValueChanged.AddListener(SetVoicePack);
        BG4Toggle.onValueChanged.AddListener(SetBG4);
        RespondToKnocksToggle.onValueChanged.AddListener(SetRespondToKnocks);
        StartingAnimationDropdown.onValueChanged.AddListener(SetStartingAnim);
        npcNameText.onValueChanged.AddListener(SetNPCName);
        KnockResponseDropdown.onValueChanged.AddListener(SetKnockResponseDrop);
        //inspector.SizeContainerContent(true);
    }

    private void SetKnockResponseDrop(int selection)
    {
        TargetComponentInfo.KnockSelection = selection;
        TargetComponentInfo.RespondsToNearbyKnocks = (selection != 0);
    }

    public void InitializeValues()
    {
        List<string> voicePackOptions = new List<string>();
        for (int i = 0; i < NPCVoicesPacks.NPCVoicePacks.Length; i++)
        {
            voicePackOptions.Add(NPCVoicesPacks.NPCVoicePacks[i].VoiceName);
        }
        VoicePackDropdown.AddOptions(voicePackOptions);
        Debug.Log($"VoicePackDropdownValue?? {TargetComponentInfo.VoicePackSelection} for player {TargetComponentInfo.npcName}");
        VoicePackDropdown.value = TargetComponentInfo.VoicePackSelection;
        
        followSpeedSlider.startValue = TargetComponentInfo.speed;
        stopDistanceSlider.startValue = TargetComponentInfo.stopDistance;
        //FollowSoundDropdown.value = FollowSoundDropdown.options.FindIndex(option => option.text == TargetComponentInfo.followCollectionName);
        //WaitSoundDropdown.value = WaitSoundDropdown.options.FindIndex(option => option.text == TargetComponentInfo.waitCollectionName);

        BG4Toggle.isOn = TargetComponentInfo.HasBG4;
        RespondToKnocksToggle.isOn = TargetComponentInfo.RespondsToNearbyKnocks;
        StartingAnimationDropdown.value = StartingAnimationDropdown.options.FindIndex(option => option.text == TargetComponentInfo.startingAnimName);
        npcNameText.text = TargetComponentInfo.npcName;
        KnockResponseDropdown.value = TargetComponentInfo.KnockSelection;
    }

    private void SetNPCName(string str)
    {
        TargetComponentInfo.SetName(str);
        
    }

    private void SetStartingAnim(int index)
    {
        BAHDOL.AnimationState state;
        Enum.TryParse(StartingAnimationDropdown.captionText.text, out state);
        TargetComponentInfo.SetStartAnim(state, true);
    }

    private void SetBG4(bool toggle)
    {
        TargetComponentInfo.HasBG4 = toggle;
    }

    private void SetRespondToKnocks(bool responds)
    {
        TargetComponentInfo.RespondsToNearbyKnocks = responds;
    }

    //private void SetWaitCollection(int index)
    //{
    //    TargetComponentInfo.SetSoundCollection(TargetComponentInfo.refugeNPCBehaviors_Component.WaitResponseCollection, WaitSoundDropdown.captionText.text,false);
    //    TargetComponentInfo.waitCollectionName = WaitSoundDropdown.captionText.text;
    //}

    private void SetVoicePack(int index)
    {
        //TargetComponentInfo.refugeNPCBehaviors_Component.FollowResponseCollection = NPCVoicesPacks.NPCVoicePacks[index].FollowResponses;
        //TargetComponentInfo.refugeNPCBehaviors_Component.WaitResponseCollection = NPCVoicesPacks.NPCVoicePacks[index].WaitResponses;

        TargetComponentInfo.SetSoundSelect(index);
    }

    //private void SetFollowCollection(int index)
    //{
    //    TargetComponentInfo.SetSoundCollection(TargetComponentInfo.refugeNPCBehaviors_Component.FollowResponseCollection, FollowSoundDropdown.captionText.text,true);
    //    TargetComponentInfo.followCollectionName = FollowSoundDropdown.captionText.text;
    //}

    private void SetStopDistance(float stop, bool bl)
    {
        TargetComponentInfo.SetStopDistance(stop);
    }

    private void SetFollowSpeed(float spd, bool bl)
    {
        TargetComponentInfo.SetSpeed(spd);
    }

    private void OnDestroy()
    {
        followSpeedSlider.onSubmitValue.RemoveListener(SetFollowSpeed);
        stopDistanceSlider.onSubmitValue.RemoveListener(SetStopDistance);
        //FollowSoundDropdown.onValueChanged.RemoveListener(SetFollowCollection);
        //WaitSoundDropdown.onValueChanged.RemoveListener(SetWaitCollection);
        VoicePackDropdown.onValueChanged.RemoveListener(SetVoicePack);
        BG4Toggle.onValueChanged.RemoveListener(SetBG4);
        StartingAnimationDropdown.onValueChanged.RemoveListener(SetStartingAnim);
        npcNameText.onValueChanged.RemoveListener(SetNPCName);
        RespondToKnocksToggle.onValueChanged.RemoveListener(SetRespondToKnocks);
        KnockResponseDropdown.onValueChanged.RemoveListener(SetKnockResponseDrop);
    }
}
