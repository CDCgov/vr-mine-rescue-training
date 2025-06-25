using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public enum PlayerViewpoint
{
    FirstPerson,
    ThirdPerson,
}

public class PIPController : MonoBehaviour, IMinimizableWindow
{
    private class PIPOption : TMP_Dropdown.OptionData
    {
        public PlayerRepresentation Player;
    }

    private class PIPRoleOption : TMP_Dropdown.OptionData
    {
        public VRNPlayerRole PlayerRole
        {
            get { return _vrnPlayerRole; }
            set 
            { 
                _vrnPlayerRole = value;
                text = value.ToString();
            }
        }

        private VRNPlayerRole _vrnPlayerRole;

    }

    public PlayerManager PlayerManager;
    public NetworkManager NetworkManager;
    public DMCameraController DMCamera;

    public GameObject PlayerDetailsWindowPrefab;

    public TextMeshProUGUI TitleText;
    public PIPRenderer PIPRenderer;
    public TMP_Dropdown PlayerDropdown;
    public TMP_Dropdown RoleDropdown;
    public Toggle FirstPersonToggle;
    public Toggle ThirdPersonToggle;
    public Image ColorBorder;
    public Button PlayerDetailsButton;

    private Dictionary<int, TMP_Dropdown.OptionData> _playerOptions = new Dictionary<int, TMP_Dropdown.OptionData>();

    private PlayerRepresentation _selectedPlayer = null;
    private Transform _selectedPlayerHead = null;
    private PlayerViewpoint _playerViewpoint = PlayerViewpoint.ThirdPerson;
    private GameObject _playerDetailsWindow = null;
    private bool _initialized = false;

    public event Action<string> TitleChanged;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;

        PlayerDropdown.onValueChanged.AddListener(OnSelectionChanged);

        PIPRenderer.Clicked += OnClickedPIPRenderer;

        if (PlayerDetailsButton != null)
            PlayerDetailsButton.onClick.AddListener(OnPlayerDetails);

        FirstPersonToggle.onValueChanged.AddListener((selected) =>
        {
            Debug.Log($"1st toggle: {selected}");
            if (selected) SetViewpoint(PlayerViewpoint.FirstPerson);
        });
        ThirdPersonToggle.onValueChanged.AddListener((selected) =>
        {
            Debug.Log($"3rd toggle: {selected}");
            if (selected) SetViewpoint(PlayerViewpoint.ThirdPerson);
        });

        if (RoleDropdown != null)
        {
            RoleDropdown.options.Clear();

            //var names = Enum.GetNames(typeof(VRNPlayerRole));
            //foreach (var name in names)
            foreach (var role in Enum.GetValues(typeof(VRNPlayerRole)))
            {
                //var opt = new TMP_Dropdown.OptionData(name);
                //RoleDropdown.options.Add(opt);
                RoleDropdown.options.Add(new PIPRoleOption { PlayerRole = (VRNPlayerRole)role });
            }

            RoleDropdown.onValueChanged.AddListener(OnRoleSelectionChanged);
        }

        _initialized = true;

        if (_selectedPlayer != null)
        {
            SetPlayer(_selectedPlayer);
        }
    }

    void OnDestroy()
    {
        ClearPlayer();
    }

    private void OnPlayerDetails()
    {
        if (PlayerDetailsWindowPrefab != null)
        {
            if (_playerDetailsWindow != null)
            {
                Destroy(_playerDetailsWindow);
                _playerDetailsWindow = null;
            }

            var win = Instantiate<GameObject>(PlayerDetailsWindowPrefab);
            var selPlayerViews = win.GetComponentsInChildren<ISelectedPlayerView>();
            foreach (var selPlayerView in selPlayerViews)
            {
                selPlayerView.SetPlayer(_selectedPlayer);
            }

            var detailsParent = transform.parent.parent;
            win.transform.SetParent(detailsParent, false);

            var pos = transform.position;
            pos = detailsParent.InverseTransformPoint(pos); //convert to local space
            pos.y = pos.y - 280; //offset down
            win.transform.localPosition = pos;

            _playerDetailsWindow = win;
        }
    }

    private void UpdateComponents()
    {
        var playerViews = GetComponentsInChildren<ISelectedPlayerView>();
        foreach (var playerView in playerViews)
        {
            playerView.SetPlayer(_selectedPlayer);
        }
    }

    private void OnRoleSelectionChanged(int selection)
    {
        if (selection < 0 || selection >= RoleDropdown.options.Count)
            return;

        if (_selectedPlayer != null)
        {
            //_selectedPlayer.PlayerRole = (VRNPlayerRole)selection;

            _selectedPlayer.PlayerRole = ((PIPRoleOption)RoleDropdown.options[selection]).PlayerRole;
            PlayerManager.AssignPlayerRole(_selectedPlayer.PlayerID, _selectedPlayer.PlayerRole, _selectedPlayer.PlayerDominantHand);
        }
    }

    private void OnClickedPIPRenderer()
    {
        if (DMCamera != null && _selectedPlayer != null)
        {
            switch (_playerViewpoint)
            {
                case PlayerViewpoint.FirstPerson:
                    DMCamera.SwitchToFirstPerson(_selectedPlayer);
                    break;

                case PlayerViewpoint.ThirdPerson:
                    DMCamera.SwitchToThirdPerson(_selectedPlayer.ClientID);
                    break;
            }
        }
    }

    public void ClearPlayer()
    {
        if (_selectedPlayer != null)
        {
            _selectedPlayer.PlayerRoleChanged -= OnPlayerRoleChanged;
        }

        _selectedPlayer = null;
        _selectedPlayerHead = null;
    }

    public void SetPlayer(PlayerRepresentation player)
    {
        if (_selectedPlayer != null)
        {
            ClearPlayer();
        }

        _selectedPlayer = player;
        _selectedPlayerHead = null;

        if (!_initialized) //if start hasn't been called, wait until Start() to finish
            return;

        UpdateComponents();

        if (_selectedPlayer != null)
        {
            //TitleText.text = $"{_selectedPlayer.Name} ({player.PlayerRole.ToString()})";
            TitleText.text = $"{_selectedPlayer.Name}";

            _selectedPlayer.PlayerRoleChanged += OnPlayerRoleChanged;
        }

        if (ColorBorder != null)
        {
            ColorBorder.color = player.PlayerColor;

            player.PlayerColorChanged += (playerColor) =>
            {
                ColorBorder.color = playerColor;
            };
        }
        

        if (RoleDropdown != null && player != null)
        {

            //RoleDropdown.SetValueWithoutNotify((int)player.PlayerRole);
            RoleDropdown.SetValueWithoutNotify(GetRoleOptionIndex(player.PlayerRole));

            //Debug.Log($"RoleDropdown: setting to {player.PlayerRole.ToString()}: {(int)player.PlayerRole}");

            //player.PlayerRoleChanged += (player, playerRole) =>
            //{
            //    //Debug.Log($"RoleDropdown: setting to {playerRole.ToString()}: {(int)playerRole}");
            //    //RoleDropdown.value = (int)playerRole;

            //    //RoleDropdown.SetValueWithoutNotify((int)playerRole);
            //    RoleDropdown.SetValueWithoutNotify(GetRoleOptionIndex(playerRole));
            //};
        }

        UpdatePlayerData();
    }

    private void OnPlayerRoleChanged(PlayerRepresentation player, VRNPlayerRole playerRole)
    {
        if (RoleDropdown == null)
            return;

        RoleDropdown.SetValueWithoutNotify(GetRoleOptionIndex(playerRole));
    }

    private int GetRoleOptionIndex(VRNPlayerRole role)
    {
        if (RoleDropdown == null)
            return -1;

        for (int i = 0; i < RoleDropdown.options.Count; i++)
        {
            var roleOpt = (PIPRoleOption)RoleDropdown.options[i];
            if (roleOpt.PlayerRole == role)
                return i;
        }

        return -1;
    }

    private void UpdatePlayerData()
    {
        if (_selectedPlayer.Head.Object != null && _selectedPlayer.Head.Object.TryGetComponent<Animator>(out var anim))
        {
            //var anim = _selectedPlayer.Head.Object.GetComponent<Animator>();
            _selectedPlayerHead = anim.GetBoneTransform(HumanBodyBones.Head);

            //if (_selectedPlayerHead != null)
            //{
            //    Debug.Log($"Found head transform {_selectedPlayerHead.name}");
            //}
        }
    }

    public void SetViewpoint(PlayerViewpoint viewpoint)
    {
        _playerViewpoint = viewpoint;
    }

    private void UpdateViewpointSelection(bool selected)
    {
        //if (!selected)
        //    return;

        if (FirstPersonToggle.isOn)
            SetViewpoint(PlayerViewpoint.FirstPerson);
        else
            SetViewpoint(PlayerViewpoint.ThirdPerson);
    }

    private void OnPlayerLeft(PlayerRepresentation player)
    {
        TMP_Dropdown.OptionData data;
        if (_playerOptions.TryGetValue(player.ClientID, out data))
        {

            PlayerDropdown.options.Remove(data);
        }
    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {
        //TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
        PIPOption data = new PIPOption();
        data.Player = player;
        data.text = player.Name;
        _playerOptions[player.ClientID] = data;

        PlayerDropdown.options.Add(data);
    }

    private void OnSelectionChanged(int value)
    {
        if (value < 0 || value >= PlayerDropdown.options.Count)
        {
            _selectedPlayer = null;
            return;
        }

        var selected = PlayerDropdown.options[value] as PIPOption;
        if (selected == null)
        {
            _selectedPlayer = null;
            return;
        }

        Debug.Log($"Selected {selected.Player.Name}");
        SetPlayer(selected.Player);
    }

    // Update is called once per frame
    void Update()
    {
        if (_selectedPlayerHead == null)
        {
            UpdatePlayerData();
        }

        if (_selectedPlayer != null && PIPRenderer != null && _selectedPlayerHead != null)
        {
            Quaternion rot = Quaternion.identity;
            Vector3 pos = Vector3.zero;

            switch (_playerViewpoint)
            {
                case PlayerViewpoint.ThirdPerson:

                    //var xform = _selectedPlayer.Head.Object.transform;
                    var xform = _selectedPlayerHead;
                    pos = xform.position;
                    //Quaternion rot = _selectedPlayer.Head.Object.transform.rotation;

                    pos -= xform.forward * 3.5f;
                    pos += Vector3.up * 3.5f;
                    rot = Quaternion.LookRotation(xform.position - pos, Vector3.up);
                    break;

                case PlayerViewpoint.FirstPerson:                    
                    rot = _selectedPlayer.HeadTransform.rotation;
                    pos = _selectedPlayer.HeadTransform.position + _selectedPlayer.HeadTransform.forward * 0.3f;
                    //rot = _selectedPlayer.Head.Rotation;
                    //pos = _selectedPlayer.Head.Position + rot * (Vector3.forward * 0.3f);
                    break;
            }

            //Debug.Log($"Moving PIP Camera {pos.ToString()} {rot.ToString()}");

            PIPRenderer.PositionCamera(pos, rot);
        }
    }


    public string GetTitle()
    {
        if (_selectedPlayer != null)
        {
            return _selectedPlayer.Name;
        }
        else
            return "Player View";
    }

    public void Minimize(bool minimize)
    {
        gameObject.SetActive(minimize);
    }

    public void ToggleMinimize()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void AssignTaskbarButton(Button button)
    {

    }
}
