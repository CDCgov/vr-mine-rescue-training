using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MineMapSymbolManager))]
public class UILoadPlayerMineMap : MonoBehaviour
{
    public MineMapManager MineMapManager;
    public SessionPlaybackControl SessionPlaybackControl;

    public int PlayerID = -1;

    private MineMapSymbolManager _symbolManager;
    private VRNSymbolManagerState _currentState;

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        if (MineMapManager == null)
            MineMapManager = MineMapManager.GetDefault(gameObject);

        _symbolManager = GetComponent<MineMapSymbolManager>();

        SessionPlaybackControl.SessionScrubbed += OnSessionScrubbed;

    }

    private void OnSessionScrubbed()
    {
        if (MineMapManager == null || MineMapManager.SymbolManagers == null || MineMapManager.SymbolManagers.Values.Count <= 0)
            return;

        if (PlayerID < 0)
        {
            foreach (var data in MineMapManager.SymbolManagers.Values)
            {
                LoadMapData(data);
                break;
            }
        }
        else
        {
            var data = MineMapManager.GetMineMapData(PlayerID);
            if (data != null)
                LoadMapData(data);
        }
    }

    private void LoadMapData(MineMapData data)
    {
        if (_currentState != data.SymbolState)
        {
            _symbolManager.LoadFromSerializedState(data.SymbolState);
            _currentState = data.SymbolState;
        }
    }
}
