using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILinkLineIndicator : MonoBehaviour, ISelectedPlayerView
{
    public PlayerRepresentation Player;

    public void SetPlayer(PlayerRepresentation player)
    {

        ClearPlayer();

        Player = player;
        Player.OnLinkLineChanged += OnLinkLineChanged;

    }

    private void OnLinkLineChanged(bool obj)
    {
        Debug.Log("UILinkLine: LinkLine Status Changed");
        UpdatePlayerData();
    }

    void UpdatePlayerData()
    {
        if (Player == null)
            return;

        var onLinkLine = Player.OnLinkLine;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(onLinkLine);
        }
    }

    void ClearPlayer()
    {
        if (Player != null)
        {
            Player.OnLinkLineChanged -= OnLinkLineChanged;
            Player = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdatePlayerData();
        if(Player == null)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        ClearPlayer();
    }

    private void Update()
    {
        UpdatePlayerData();
    }

}
