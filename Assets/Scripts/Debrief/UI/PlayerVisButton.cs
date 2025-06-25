using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVisButton : MonoBehaviour
{
    public PlayerVisibiltyHandler PlayerVisibiltyHandler;
    public int Index;
    public Toggle EventToggle;
    public Toggle PathToggle;
    public Image BGImage;
    
    public void OnToggle()
    {
        PlayerVisibiltyHandler.PlayerItemVisibilty(Index, EventToggle.isOn);
    }

    public void SetButtonColor(Color col)
    {
        BGImage.color = col;
    }
}
