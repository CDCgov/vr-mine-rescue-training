using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DMCameraController : MonoBehaviour
{
    NetworkManager NetworkManager;
    PlayerManager PlayerManager;
    POIManager POIManager;
    TeleportManager TeleportManager;
    SceneLoadManager SceneLoadManager;

    //private ResearcherCamController _researchCam = null;
    private bool _trackPlayer = false;
    private PlayerRepresentation _player;
    private PlayerViewpoint _viewpoint;
    private ISceneCamera _sceneCam;

    private int  _sceneChangedCounter = -1;

    void Start()
    {
        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);
        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);
        if (POIManager == null)
            POIManager = POIManager.GetDefault(gameObject);
        if (TeleportManager == null)
            TeleportManager = TeleportManager.GetDefault(gameObject);
        if (SceneLoadManager == null)
            SceneLoadManager = SceneLoadManager.GetDefault(gameObject);

        //_researchCam = GetComponent<ResearcherCamController>();
        _sceneCam = GetComponent<ISceneCamera>();

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        //NetworkManager.TeleportAllCommand += OnTeleportAll;
        TeleportManager.Teleporting += OnTeleporting;

        if (SceneLoadManager != null)
        {
            SceneLoadManager.SceneLoadFinalized += OnSceneLoadFinalized;
        }

        MoveToSpawnPoint();
    }

    private void OnSceneLoadFinalized()
    {
        MoveToSpawnPoint();
    }

    private void OnDestroy()
    {
        if (TeleportManager != null)
            TeleportManager.Teleporting -= OnTeleporting;

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        if (SceneLoadManager != null)
            SceneLoadManager.SceneLoadFinalized -= OnSceneLoadFinalized;
    }

    private void OnDisable()
    {
        _sceneChangedCounter = -1;
    }

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        if (!isActiveAndEnabled)
            return;

        MoveToSpawnPoint();
        _sceneChangedCounter = 2;
    }

    //private void LateUpdate()
    //{
    //    if (_sceneChangedCounter >= 0)
    //    {
    //        _sceneChangedCounter--;
    //        if (_sceneChangedCounter == 0)
    //        {
    //            MoveToSpawnPoint();
    //        }
    //    }
    //}

    void OnTeleporting(Transform dest)
    {
        if (_sceneCam == null)
            return;

        var teleDest = dest.position + new Vector3(0, 5, -5);

        _trackPlayer = false;
        //transform.position = teleDest;
        var rot = Quaternion.LookRotation(dest.position - teleDest, Vector3.up);

        _sceneCam.PositionCamera(teleDest, rot);

        //if (_researchCam != null)
        //{
        //    _researchCam.SetLookRotation(rot);
        //}
        //else
        //    transform.rotation = rot;
    }

    private void MoveToSpawnPoint()
    {
        if (POIManager == null || _sceneCam == null)
            return;

        foreach (var poi in POIManager.GetPOIs())
        {
            if (poi.POIType == POIType.DMSpawnPoint)
            {
                Debug.Log($"Moving to {poi.transform.position.ToString()}");
                _sceneCam.PositionCamera(poi.transform.position, poi.transform.rotation);
                //transform.position = poi.transform.position;
                //transform.rotation = poi.transform.rotation;
                //if (_researchCam != null)
                //{
                //    _researchCam.SetLookRotation(poi.transform.rotation);
                //}
                //else
                //    transform.rotation = poi.transform.rotation;
                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (_trackPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                _trackPlayer = false;
            }
            else if (_player != null)
            {
                switch (_viewpoint)
                {
                    case PlayerViewpoint.FirstPerson:
                        MoveCameraToPlayer(_player, PlayerViewpoint.FirstPerson);
                        break;

                    case PlayerViewpoint.ThirdPerson:
                        MoveCameraToPlayer(_player, PlayerViewpoint.ThirdPerson);
                        break;
                }
            }
        }

        if (_sceneChangedCounter >= 0)
        {
            _sceneChangedCounter--;
            if (_sceneChangedCounter == 0)
            {
                MoveToSpawnPoint();
            }
        }
    }

    public void SwitchToFirstPerson(int playerid)
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            if (player.ClientID == playerid)
            {
                SwitchToFirstPerson(player);
            }
        }
    }

    public void SwitchToFirstPerson(PlayerRepresentation player)
    {
        //var anim = player.Head.Object.GetComponent<Animator>();
        //var headTransform = anim.GetBoneTransform(HumanBodyBones.Head);

        _trackPlayer = true;
        _player = player;
        _viewpoint = PlayerViewpoint.FirstPerson;

        MoveCameraToPlayer(_player, _viewpoint);
    }

    public void SwitchToThirdPerson(int playerid)
    {
        foreach (var player in PlayerManager.PlayerList.Values)
        {
            if (player.ClientID == playerid)
            {
                if (player.Head.Object == null)
                    continue;

                SwitchToThirdPerson(player);
            }
        }
    }

    public void SwitchToThirdPerson(PlayerRepresentation player)
    {
        _trackPlayer = true;
        _player = player;
        _viewpoint = PlayerViewpoint.ThirdPerson;

        MoveCameraToPlayer(_player, _viewpoint);
    }

    private void MoveCameraToPlayer(PlayerRepresentation player, PlayerViewpoint viewpoint)
    {
        if (_sceneCam == null)
            return;

        Quaternion rot = Quaternion.identity;
        Vector3 pos = Vector3.zero;

        switch (viewpoint)
        {
            case PlayerViewpoint.FirstPerson:
                if (player.HeadTransform != null)
                {
                    rot = player.HeadTransform.rotation;
                    pos = player.HeadTransform.position + player.HeadTransform.forward * 0.3f;
                }
                break;

            case PlayerViewpoint.ThirdPerson:
                if (player.Head.Object != null)
                {
                    pos = player.Head.Object.transform.position + new Vector3(0, 5, -5);
                    Vector3 dir = (player.Head.Object.transform.position - pos).normalized;
                    rot = Quaternion.LookRotation(dir, Vector3.up);
                }
                break;
        }

        _sceneCam.PositionCamera(pos, rot);
        //transform.position = pos;
        //if (_researchCam != null)
        //{
        //    _researchCam.SetLookRotation(rot);
        //}
        //else
        //    transform.rotation = rot;
    }

}
