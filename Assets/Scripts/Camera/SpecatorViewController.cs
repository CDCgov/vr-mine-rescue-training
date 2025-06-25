using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpecatorViewController : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public PlayerManager PlayerManager;
    public SpectatorManager SpectatorManager;

    public KeyCode EnableSpectatorMapHotkey = KeyCode.M;
    public KeyCode Map1Hotkey = KeyCode.B;
    public KeyCode Map2Hotkey = KeyCode.N;
    public KeyCode EnableSpectatorDirectionalLightHotkey = KeyCode.L;
    public bool UseControlKey = true;

    //public GameObject MineMap;
    public GameObject[] MineMaps;
    public GameObject DirectionalLight;

    private Light _directionalLight;
    private bool _mineMapsDisplayed = false;
    //private Dictionary<int, Transform> _playerPositions;

    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (SpectatorManager == null)
            SpectatorManager = SpectatorManager.GetDefault(gameObject);

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;

        NetworkManager.ClientIDAssigned += OnClientIDAssigned;

        if (DirectionalLight != null)
            _directionalLight = DirectionalLight.GetComponent<Light>();

    }

    private void OnClientIDAssigned(int obj)
    {
        SpectatorManager.SendSpectatorJoinRequest();
    }

    private void OnPlayerLeft(PlayerRepresentation player)
    {

    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {

    }

    public void FollowPlayer(PlayerRepresentation player)
    {

    }

    private void ShowMineMaps(bool bShow)
    {

        if (MineMaps == null || MineMaps.Length <= 0)
            return;

        _mineMapsDisplayed = bShow;

        foreach (var map in MineMaps)
        {
            map.SetActive(bShow);
        }
    }

    private void ToggleMineMap(int index)
    {
        if (MineMaps == null || (MineMaps.Length - 1) < index)
            return;

        MineMaps[index].SetActive(!MineMaps[index].activeSelf);
    }

    void Update()
    {
        if (UseControlKey && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            return;

        if (Input.GetKeyDown(EnableSpectatorMapHotkey))
        {
            ShowMineMaps(!_mineMapsDisplayed);
        }

        if (Input.GetKeyDown(Map1Hotkey))
        {
            ToggleMineMap(0);
        }

        if (Input.GetKeyDown(Map2Hotkey))
        {
            ToggleMineMap(1);
        }

        if (DirectionalLight != null && _directionalLight != null && Input.GetKeyDown(EnableSpectatorDirectionalLightHotkey))
        {
            DirectionalLight.SetActive(!DirectionalLight.activeSelf);
            if (DirectionalLight.activeSelf && !_directionalLight.enabled)
            {
                _directionalLight.enabled = true;
            }
        }
    }
}