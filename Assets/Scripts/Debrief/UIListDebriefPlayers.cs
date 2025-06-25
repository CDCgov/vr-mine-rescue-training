using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIListDebriefPlayers : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;
    public SessionEventManager SessionEventManager;
    public PlayerManager PlayerManager;

    public GameObject ControlPrefab;
    public Transform ListParentTransform;
    

    private struct ToggleData
    {
        public Toggle Toggle;
        public DebriefMarkers.DebriefMarkerData CategoryData;
    }

    private List<ToggleData> _activeToggles = new List<ToggleData>();

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        if (ListParentTransform == null)
            ListParentTransform = transform;


        if (SessionPlaybackControl.IsSessionLoaded)
            InitializePlayerTogglePrefabs();

        SessionPlaybackControl.SessionLoaded += OnSessionLoaded;
    }

    private void OnDestroy()
    {
        SessionPlaybackControl.SessionLoaded -= OnSessionLoaded;
    }

    private void OnSessionLoaded()
    {
        InitializePlayerTogglePrefabs();
    }

    private void InitializePlayerTogglePrefabs()
    {
        //remove any existing buttons
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
        

        var selectedPlayerInterfaces = obj.GetComponentsInChildren<ISelectedPlayerView>();
        foreach (var selPlayer in selectedPlayerInterfaces)
        {
            selPlayer.SetPlayer(player);
        }

    }

}
