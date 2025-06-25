using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIListPlayerControls : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public GameObject ControlPrefab;
    public Transform ListParentTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        if (ListParentTransform == null)
            ListParentTransform = transform;

        PlayerManager.PlayerJoined += OnPlayersChanged;
        PlayerManager.PlayerLeft += OnPlayersChanged;

        UpdatePlayerList();
    }

    private void OnDestroy()
    {
        PlayerManager.PlayerJoined -= OnPlayersChanged;
        PlayerManager.PlayerLeft -= OnPlayersChanged;
    }

    private void OnPlayersChanged(PlayerRepresentation obj)
    {
        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        foreach (Transform xform in ListParentTransform)
        {
            Destroy(xform.gameObject);
        }

        foreach (var player in PlayerManager.PlayerList.Values)
        {
            AddPlayerControl(player);
        }
    }

    private void AddPlayerControl(PlayerRepresentation player)
    {
        var obj = GameObject.Instantiate<GameObject>(ControlPrefab);
        obj.transform.SetParent(ListParentTransform, false);
        obj.SetActive(true);

        var selectedPlayerInterfaces = obj.GetComponentsInChildren<ISelectedPlayerView>();
        foreach (var selPlayer in selectedPlayerInterfaces)
        {
            selPlayer.SetPlayer(player);
        }
        
    }
}
