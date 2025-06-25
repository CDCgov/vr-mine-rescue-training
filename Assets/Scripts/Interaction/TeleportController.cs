using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
//using UnityEngine.Rendering.PostProcessing;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;

public class TeleportController : MonoBehaviour
{
    public NetworkManager NetworkManager;
    public POIManager POIManager;
    public TeleportManager TeleportManager;
    public PlayerManager PlayerManager;

    public bool AutomaticTeleportOffset = false;
    public Transform TeleportRelativeTo;

    public TextMeshProUGUI TeleportTextBox;
    public GameObject TeleportTextObject;

    public bool ResetRotationOnTeleport = true;

    //public bool IsOnLinkLine
    //{
    //    get { return _isOnLinkLine; }
    //}

    private const string OverrideObjectName = "TeleportVolumeOverride";

    //	private AutoExposure _autoExposure = null;

    //May want to make the public for the multiplayer teleport DM control to check the link line status.
    //private bool _isOnLinkLine = false;

    private Vector3 _teleportOffset;
    //public UnityEngine.Rendering.Vector4Parameter _gainParam;


    private LiftGammaGain _liftGammaGain;

    // Start is called before the first frame update
    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        //NetworkManager.TeleportAllCommand.AddListener(OnTeleportAll);
        //NetworkManager.TeleportAllCommand += OnTeleportAll;
        TeleportManager.Teleporting += OnTeleporting;

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;

        //Add in listener NetworkManager.ClientStateChanged.AddListener(OnClientStateChanged); with updating TeleportTextBox to say "Loading"
        //NetworkManager.ClientStateChanged.AddListener(OnClientStateChanged);
        //NetworkManager.ClientStateChanged += OnClientStateChanged;

        if (TeleportRelativeTo == null && AutomaticTeleportOffset && POIManager != null)
        {
            var spawnPoint = POIManager.GetSpawnPoint();
            if (spawnPoint != null)
            {
                TeleportRelativeTo = spawnPoint.transform;
            }
        }


        //compute offset relative to telport origin
        if (TeleportRelativeTo != null)
        {
            _teleportOffset = transform.position - TeleportRelativeTo.position;
        }
        else
        {
            //VRPointOfInterest[] pois = GameObject.FindObjectsOfType<VRPointOfInterest>();
            //VRPointOfInterest playerSpawn = null;
            //foreach(VRPointOfInterest poi in pois)
            //{
            //    if(poi.POIType == POIType.SpawnPoint)
            //    {
            //        playerSpawn = poi;
            //    }
            //}
            //if (playerSpawn != null)
            //{
            //    Debug.Log($"New Link Line Offset: {transform.position - playerSpawn.transform.position}");
            //    _teleportOffset = transform.position - playerSpawn.transform.position;
            //    TeleportRelativeTo = playerSpawn.transform;
            //}
            //else
            //{
            //    _teleportOffset = Vector3.zero;
            //}

            _teleportOffset = Vector3.zero;
        }

        PlayerManager.RegisterPlayerMessageHandler(OnPlayerMessage);

        //if (EnableFade)
        //{			
        //	_liftGammaGain = GetGainOverride();
        //	_liftGammaGain.active = true;
        //}

        //_liftGammaGain.gain = _gainParam;
        //_liftGammaGain.gain.value = new Vector4(1, 1, 1, -0.7f);

        ResetTeleportOffset(ResetRotationOnTeleport);

        if (TeleportManager.ActiveTeleportTarget != null)
        {
            ImmediateTeleport(TeleportManager.ActiveTeleportTarget);
        }
        else
        {
            TeleportToSpawn();
        }
    }

    private void OnPlayerMessage(VRNPlayerMessageType messageType, VRNPlayerMessage msg)
    {
        if (msg.PlayerID != PlayerManager.CurrentPlayer.PlayerID)
            return;

        if (messageType == VRNPlayerMessageType.PmResetToTeleport)
        {
            ResetTeleportOffset(ResetRotationOnTeleport);
        }
    }

    private void OnTeleporting(Transform dest)
    {
        ImmediateTeleport(dest);
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        //if (NetworkManager != null)
        //    NetworkManager.ClientStateChanged -= OnClientStateChanged;

        if (TeleportManager != null)
            TeleportManager.Teleporting -= OnTeleporting;

        if (PlayerManager != null)
            PlayerManager.UnregisterPlayerMessageHandler(OnPlayerMessage);
    }

    private void TeleportToSpawn()
    {
        var pois = POIManager.GetPOIs();

        foreach (var poi in pois)
        {
            if (poi.POIType == POIType.SpawnPoint)
            {
                //transform.position = poi.transform.position;
                //transform.rotation = poi.transform.rotation;
                ImmediateTeleport(poi.transform);
                break;
            }
        }
    }

    private void OnActiveSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        if (transform == null || gameObject == null)
            return; // ignore if we have been / are being destroyed

        Debug.Log("Teleport: Active Scene Changed");
        //TeleportToSpawn();
    }

    //void OnTeleportAll(VRNTeleportAll tele)
    //{
    //	Debug.Log($"Teleporting to {tele.TeleportTarget}");

    //	var poi = POIManager.GetPOI(tele.TeleportTarget);
    //	if (poi == null)
    //	{
    //		Debug.LogError($"Couldn't find teleport target {tele.TeleportTarget}");
    //		return;
    //	}

    //	//transform.position = poi.transform.position;
    //	StartTeleport(poi.transform);
    //}

    //async void StartTeleport(Transform dest)
    //{
    //       //_destination = dest;
    //       //_teleporting = true;
    //       //_haveMoved = false;
    //       //_teleportStartTime = Time.time;

    //       if (EnableFade)
    //       {
    //           Debug.Log("Fading out");
    //           await SceneFadeManager.FadeOut();
    //           ImmediateTeleport(dest);
    //           Debug.Log("Fading in");
    //           _ = SceneFadeManager.FadeIn();
    //       }
    //       else
    //           ImmediateTeleport(dest);
    //}

    public void ImmediateTeleport(Transform dest)
    {
        //Vector3 destPos = dest.position;
        //destPos += _teleportOffset;

        ////move main object
        //transform.position = destPos;
        //transform.rotation = dest.rotation;
        transform.SetParent(dest, false);

        if (TeleportManager.ResetToPOIOnTeleport)
            ResetTeleportOffset(false);
        if (TeleportManager.ResetRotationOnTeleport)
            transform.localRotation = Quaternion.identity;

        //ResetTeleportOffset();

        ////fix any rigidbodies so they don't fall through the floor
        //var rigidbodies = GetComponentsInChildren<Rigidbody>();
        //Vector3 offset = new Vector3(0, 0.25f, 0);
        //foreach (var rb in rigidbodies)
        //{
        //	if (!rb.isKinematic)
        //	{
        //		rb.transform.position = rb.transform.position + offset;
        //	}
        //}

        //reset any tensioned cables
        var cables = GetComponentsInChildren<TensionedCable>();
        foreach (var cable in cables)
        {
            cable.ResetCable();
        }
    }

    public void ResetTeleportOffset(bool resetRotation = true)
    {
        transform.localPosition = Vector3.zero;
        if (resetRotation)
            transform.localRotation = Quaternion.identity;

        if (TeleportRelativeTo != null)
        {
            transform.localPosition = _teleportOffset;
        }

        if (transform.parent != null)
        {
            //transform.parent.localPosition = Vector3.zero;
            //transform.parent.localRotation = Quaternion.identity;
        }
    }


    //void Update()
    //{

    //	if (!_teleporting || _destination == null)
    //		return;

    //	float progress = (Time.time - _teleportStartTime) / TeleportDuration;
    //	float fadeValue = 0;

    //	if (progress >= 0.5f && !_haveMoved)
    //	{
    //		ImmediateTeleport(_destination);
    //	}

    //	if (progress >= 1)
    //	{
    //		_teleporting = false;
    //		fadeValue = 1.0f;
    //	}
    //	else
    //	{
    //		fadeValue = FadeCurve.Evaluate(progress);
    //	}

    //	if (EnableFade)
    //	{
    //		if (_liftGammaGain != null)
    //		{
    //			var gain = fadeValue - 1.0f;
    //			_liftGammaGain.gain.value = new Vector4(1, 1, 1, gain);
    //		}
    //	}


    //}

    //public void LinkLineAttached()
    //{
    //    //if (TeleportTextBox != null && TeleportTextObject != null)
    //    //{
    //    //    TeleportTextBox.text = "Linked\nStay In Place";
    //    //    TeleportTextObject.SetActive(true);
    //    //}
    //    _isOnLinkLine = true;

    //}

    //public void LinkLineDetached()
    //{
    //    //if (TeleportTextObject != null)
    //    //    TeleportTextObject.SetActive(false);

    //    _isOnLinkLine = false;
    //}

    //To test with multiplayer in future
    //void OnClientStateChanged(VRNClientState state)
    //{
    //    if (TeleportTextObject == null || TeleportTextBox == null)
    //        return;

    //    // string status = "";
    //    //switch (state.SceneLoadState)
    //    //{
    //    //	case VRNSceneLoadState.Loading:
    //    //		TeleportTextBox.text = "Loading";
    //    //		TeleportTextObject.SetActive(true);
    //    //		break;

    //    //	case VRNSceneLoadState.ReadyToActivate:                
    //    //		TeleportTextObject.SetActive(false);
    //    //		break;
    //    //	default:
    //    //		TeleportTextObject.SetActive(false);
    //    //		break;
    //    //}
    //}
}
