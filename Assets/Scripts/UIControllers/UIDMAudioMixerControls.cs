using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UIDMAudioMixerControls : MonoBehaviour
{
    public AudioMixer GameAudioMixer;
    public Slider MasterAudioSlider;
    public Slider FireMasterSlider;
    public Slider FireBurningSlider;
    public Slider FireExtinguishedSlider;
    public Slider FireExtinguisherSlider;
    public Slider FootfallsSlider;
    public Slider CollisionsMasterSlider;
    public Slider GeneralCollisionsSlider;
    public Slider StretcherCollisionSlider;

    public SystemManager SystemManager;
    public PlayerManager PlayerManager;
    public SceneLoadManager SceneLoadManager;

    private float _masterVol = 0;
    private float _collisionsMasterVol = 0;
    private float _generalCollisionVol = 0;
    private float _fireMasterVol = 0;
    private float _footfallsVol = 0;
    private float _fireBurningVol = 0;
    private float _fireExtinguisherVol = 0;
    private float _fireExtinguishedVol = 0;
    private float _stretcherCollisionVol = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);
        if (SystemManager == null)
            SystemManager = SystemManager.GetDefault();

        if (SceneLoadManager != null)   
        {
            SceneLoadManager.EnteredSimulationScene += OnEnteredSimulationScene;
        }

        if(PlayerManager == null)
        {
            PlayerManager = PlayerManager.GetDefault(gameObject);
        }
        if(GameAudioMixer.GetFloat("masterVol", out _masterVol))
        {
            MasterAudioSlider.SetValueWithoutNotify(_masterVol);
            MasterAudioSlider.onValueChanged.AddListener(delegate { ValueChange(MasterAudioSlider.value, "masterVol", out _masterVol); });
        }
        else
        {
            MasterAudioSlider.enabled = false;
        }
        
        if (GameAudioMixer.GetFloat("collisionsMasterVol", out _collisionsMasterVol))
        {
            CollisionsMasterSlider.SetValueWithoutNotify(_collisionsMasterVol);
            CollisionsMasterSlider.onValueChanged.AddListener(delegate { ValueChange(CollisionsMasterSlider.value, "collisionsMasterVol", out _collisionsMasterVol); });
        }
        else
        {
            CollisionsMasterSlider.enabled = false;
        }

        if (GameAudioMixer.GetFloat("fireMasterVol", out _fireMasterVol))
        {
            FireMasterSlider.SetValueWithoutNotify(_fireMasterVol);
            FireMasterSlider.onValueChanged.AddListener(delegate { ValueChange(FireMasterSlider.value, "fireMasterVol", out _fireMasterVol); });
        }
        else
        {
            FireMasterSlider.enabled = false;
        }

        if (GameAudioMixer.GetFloat("fireBurningVol", out _fireBurningVol))
        {
            FireBurningSlider.SetValueWithoutNotify(_fireBurningVol);
            FireBurningSlider.onValueChanged.AddListener(delegate { ValueChange(FireBurningSlider.value, "fireBurningVol", out _fireBurningVol); });
        }
        else
        {
            FireBurningSlider.enabled = false;
        }

        if (GameAudioMixer.GetFloat("fireExtinguishedVol", out _fireExtinguishedVol))
        {
            FireExtinguishedSlider.SetValueWithoutNotify(_fireExtinguishedVol);
            FireExtinguishedSlider.onValueChanged.AddListener(delegate { ValueChange(FireExtinguishedSlider.value, "fireExtinguishedVol", out _fireExtinguishedVol); });
        }
        else
        {
            FireExtinguishedSlider.enabled = false;
        }

        if (GameAudioMixer.GetFloat("fireExtinguisherVol", out _fireExtinguisherVol))
        {
            FireExtinguisherSlider.SetValueWithoutNotify(_fireExtinguisherVol);
            FireExtinguisherSlider.onValueChanged.AddListener(delegate { ValueChange(FireExtinguisherSlider.value, "fireExtinguisherVol", out _fireExtinguisherVol); });
        }
        else
        {
            FireExtinguisherSlider.enabled = false;
        }

        if (GameAudioMixer.GetFloat("footfallsVol", out _footfallsVol))
        {
            FootfallsSlider.SetValueWithoutNotify(_footfallsVol);
            FootfallsSlider.onValueChanged.AddListener(delegate { ValueChange(FootfallsSlider.value, "footfallsVol", out _footfallsVol); });
        }
        else
        {
            FootfallsSlider.enabled = false;
        }

        if (GameAudioMixer.GetFloat("generalCollisionVol", out _generalCollisionVol))
        {
            GeneralCollisionsSlider.SetValueWithoutNotify(_generalCollisionVol);
            GeneralCollisionsSlider.onValueChanged.AddListener(delegate { ValueChange(GeneralCollisionsSlider.value, "generalCollisionVol", out _generalCollisionVol); });
        }
        else
        {
            GeneralCollisionsSlider.enabled = false;
        }

        if (GameAudioMixer.GetFloat("stretcherCollisionVol", out _stretcherCollisionVol))
        {
            StretcherCollisionSlider.SetValueWithoutNotify(_stretcherCollisionVol);
            StretcherCollisionSlider.onValueChanged.AddListener(delegate { ValueChange(StretcherCollisionSlider.value, "stretcherCollisionVol", out _stretcherCollisionVol); });
        }
        else
        {
            StretcherCollisionSlider.enabled = false;
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {

        if (SceneLoadManager != null)
        {
            SceneLoadManager.EnteredSimulationScene -= OnEnteredSimulationScene;
        }
    }

    private void OnEnteredSimulationScene()
    {
        if (ScenarioSaveLoad.Settings != null)  
        {
            _masterVol = RatioToMixerSetting(ScenarioSaveLoad.Settings.MasterVolume);
            ValueChange(_masterVol, "masterVol", out var _);
            MasterAudioSlider.SetValueWithoutNotify(_masterVol);
        }
    }

    private float RatioToMixerSetting(float ratio)
    {
        return Mathf.Lerp(-80, 0, Mathf.Clamp01(ratio));
    }
    

    private void ValueChange(float value, string param, out float mixerSet)
    {
        
        GameAudioMixer.SetFloat(param, value);
        mixerSet = value;
        //Do multiplayer notify, will need a network listener class to do this same stuff
        VRNPlayerMessageType vRNPlayerMessageType = VRNPlayerMessageType.PmUnknown;

        switch (param)
        {
            case "masterVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetMasterVolume;
                break;
            case "collisionsMasterVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetCollisionMasterVolume;
                break;
            case "fireMasterVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetFireMasterVolume;
                break;
            case "fireBurningVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetFireBurningVolume;
                break;
            case "fireExtinguishedVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetFireExtinguished;
                break;
            case "fireExtinguisherVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetFireExtinguisher;
                break;
            case "footfallsVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetFootfallsVolume;
                break;
            case "generalCollisionVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetGeneralCollisionsVolume;
                break;
            case "stretcherCollisionVol":
                vRNPlayerMessageType = VRNPlayerMessageType.PmSetStretcherCollisionVolume;
                break;
            default:
                break;
        }

        Dictionary<int, PlayerRepresentation>.ValueCollection playerRepresentations = PlayerManager.PlayerList.Values;
        foreach (PlayerRepresentation player in playerRepresentations)
        {
            float setHeight = player.RigTransform.InverseTransformPoint(player.HeadTransform.position).y;
            //Debug.Log("Player manager updated: " + PlayerManager.CurrentPlayer)
            PlayerManager.SendPlayerMessage(player.PlayerID, vRNPlayerMessageType, (float)value);
        }
    }
}
