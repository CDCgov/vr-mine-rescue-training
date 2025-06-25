using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandedness : MonoBehaviour
{
    public PlayerDominantHand Handedness = PlayerDominantHand.RightHanded;

    
    public List<GameObject> Hands_RHMode;
    public List<GameObject> Hand_LHMode;

    private void Start()
    {
        switch (Handedness)
        {
            case PlayerDominantHand.RightHanded:                
                for (int i = 0; i < Hands_RHMode.Count; i++)
                {
                    Hands_RHMode[i].SetActive(true);
                    Hand_LHMode[i].SetActive(false);
                }
                break;
            case PlayerDominantHand.LeftHanded:                
                for (int i = 0; i < Hands_RHMode.Count; i++)
                {
                    Hands_RHMode[i].SetActive(false);
                    Hand_LHMode[i].SetActive(true);
                }
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Backspace))
        {
            OnSwitchHand();
        }
    }

    public void OnSwitchHand()
    {        
        switch (Handedness)
        {
            case PlayerDominantHand.RightHanded:
                Handedness = PlayerDominantHand.LeftHanded;
                for (int i = 0; i < Hands_RHMode.Count; i++)
                {
                    Hands_RHMode[i].SetActive(false);
                    Hand_LHMode[i].SetActive(true);
                }
                break;
            case PlayerDominantHand.LeftHanded:
                Handedness = PlayerDominantHand.RightHanded;
                for (int i = 0; i < Hands_RHMode.Count; i++)
                {
                    Hands_RHMode[i].SetActive(true);
                    Hand_LHMode[i].SetActive(false);
                }
                break;
            default:
                break;
        }
    }
}
