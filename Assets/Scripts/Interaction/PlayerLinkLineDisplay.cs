using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerLinkLineDisplay : MonoBehaviour
{
    public TextMeshProUGUI TeleportTextBox;
    public GameObject TeleportTextObject;
    public PlayerManager PlayerManager;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        TeleportTextBox.text = "Linked\nStay In Place";
        if(TeleportTextObject == null)
        {
            Debug.LogError("PlayerLinkLineDisplay: Text box reference is null");
        }

        if (PlayerManager.CurrentPlayer != null)
            PlayerManager.CurrentPlayer.OnLinkLineChanged += OnLinkLineChanged;
        else
            Debug.LogError("PlayerLinkLineDisplay: CurrentPlayer is null");
    }

    private void OnDestroy()
    {
        if (PlayerManager != null && PlayerManager.CurrentPlayer != null)
            PlayerManager.CurrentPlayer.OnLinkLineChanged -= OnLinkLineChanged;
    }

    //private void Update()
    //{
    //    if(TeleportTextObject.activeSelf != PlayerManager.CurrentPlayer.OnLinkLine)
    //        TeleportTextObject.SetActive(PlayerManager.CurrentPlayer.OnLinkLine);
    //}

    private void OnLinkLineChanged(bool obj)
    {
        if (TeleportTextObject == null)
        {
            TeleportTextObject = gameObject.GetComponentInChildren<TextMeshProUGUI>().gameObject;
            //Debug.LogError("Text box is null");
        }

        if (TeleportTextBox == null)
            return;

        //TeleportTextObject.SetActive(PlayerManager.CurrentPlayer.OnLinkLine);
    }

}
