using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DMGUIController : MonoBehaviour
{
    private class PlayerInfo
    {
        public GameObject PlayerWindow;
        public PIPRenderer PIPRenderer;
    }

    public PlayerManager PlayerManager;
    public DMCameraController DMCamera;

    public GameObject PIPWindowPrefab;
    public GameObject TaskBarItemPrefab;    

    public RectTransform TaskBarPanel;
    public RectTransform PIPWindowParent;
    public float PIPMaxFPS = 20;

    public List<GameObject> DMGUIWindows;

    //keep player info in a dictionary & a list for now (for sequential + random access)
    private Dictionary<PlayerRepresentation, PlayerInfo> _playerInfo;
    private List<PlayerInfo> _playerInfoList;
    //private Dictionary<PlayerRepresentation, PIPRenderer> _playerRenderers;

    private Dictionary<GameObject, GameObject> _taskBarItems;
    private float _pipRenderDelay;
    private float _nextPIPRenderTime;
    private int _lastPIPRenderIndex = 0;

    // Start is called before the first frame update
    void Start() 
    {
        _playerInfo = new Dictionary<PlayerRepresentation, PlayerInfo>();
        _playerInfoList = new List<PlayerInfo>();

        _taskBarItems = new Dictionary<GameObject, GameObject>();

        _pipRenderDelay = 1.0f / PIPMaxFPS;

        if (PlayerManager == null)
            PlayerManager = PlayerManager.GetDefault(gameObject);

        PlayerManager.PlayerJoined += OnPlayerJoined;
        PlayerManager.PlayerLeft += OnPlayerLeft;

        if (TaskBarItemPrefab != null && DMGUIWindows != null && DMGUIWindows.Count > 0)
        {
            foreach (var winobj in DMGUIWindows)
            {
                AddWindow(winobj);
            }
        }
    }

    private void Update()
    {
        if (Time.time >= _nextPIPRenderTime)
        {
            _nextPIPRenderTime = Time.time + _pipRenderDelay;

            if (_playerInfoList == null || _playerInfoList.Count <= 0)
                return;

            //find the next PIP renderer to update
            int index = _lastPIPRenderIndex + 1;
            int maxCount = _playerInfoList.Count;

            while (maxCount > 0)
            {
                maxCount--;

                if (index >= _playerInfoList.Count)
                    index = 0;

                var info = _playerInfoList[index];
                if (info.PIPRenderer != null && info.PIPRenderer.gameObject.activeInHierarchy)
                {
                    info.PIPRenderer.RenderCamera();
                    _lastPIPRenderIndex = index;
                    //Debug.Log($"Rendering PIP index {index} at time {Time.time:F3}");
                    break;
                }

                index = index + 1;
            }
        }
    }

    public void AddWindow(GameObject winobj)
    {
        var win = winobj.GetComponent<IMinimizableWindow>();
        if (win == null)
            return;

        var obj = Instantiate<GameObject>(TaskBarItemPrefab, TaskBarPanel, false);
        var btn = obj.GetComponent<Button>();
        var txt = obj.GetComponentInChildren<TextMeshProUGUI>();

        //obj.transform.SetParent(TaskBarPanel);

        txt.text = win.GetTitle();
        win.TitleChanged += (newtitle) =>
        {
            txt.text = newtitle;
        };

        btn.onClick.AddListener(() =>
        {
            TaskBarClicked(win);
        });

        _taskBarItems.Add(winobj, obj);
    }

    public void RemoveWindow(GameObject winobj)
    {
        GameObject taskObj = null;
        if (_taskBarItems.TryGetValue(winobj, out taskObj))
        {
            Destroy(taskObj);
            _taskBarItems.Remove(winobj);
        }
    }

    private void OnPlayerLeft(PlayerRepresentation player)
    {
        //GameObject obj;
        PlayerInfo info;
        if (_playerInfo.TryGetValue(player, out info))
        {
            Destroy(info.PlayerWindow);
            _playerInfo.Remove(player);
            _playerInfoList.Remove(info);

            RemoveWindow(info.PlayerWindow);
        }
    }

    private void OnPlayerJoined(PlayerRepresentation player)
    {
        if (PIPWindowPrefab == null)
            return;

        var obj = GameObject.Instantiate<GameObject>(PIPWindowPrefab, PIPWindowParent);

        var pip = obj.GetComponent<PIPController>();
        pip.SetPlayer(player);
        pip.DMCamera = DMCamera;
        

        PlayerInfo info = new PlayerInfo
        {
            PlayerWindow = obj,
            PIPRenderer = pip.PIPRenderer,
        };

        _playerInfo.Add(player, info);
        _playerInfoList.Add(info);
        AddWindow(obj);
    }

    void TaskBarClicked(IMinimizableWindow win)
    {
        win.ToggleMinimize();
    }

}
