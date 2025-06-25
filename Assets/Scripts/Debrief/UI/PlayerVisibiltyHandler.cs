using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVisibiltyHandler : MonoBehaviour
{
    public Dictionary<int, List<GameObject>> PlayerDebriefItems;
    public List<bool> ActivePlayers;
    public ActionsVisibilityHandler Actions;
    public List<Toggle> PlayerToggleBtns;

    private void Start()
    {
        PlayerToggleBtns = new List<Toggle>();
    }
    public void SetActivePlayerReference(int length)
    {
        ActivePlayers = new List<bool>();
        for(int i = 0; i<length; i++)
        {
            ActivePlayers.Add(true);
        }
    }
    public void PlayerItemVisibilty(int index, bool visible)
    {        
        //ActivePlayers[index] = visible;
        Debug.Log($"{index} is a key?");
        if(PlayerDebriefItems.TryGetValue(index, out List<GameObject> objectsToHide))
        {
            Debug.Log($"Player {index}: Items to hide, {objectsToHide.Count}");
            foreach(GameObject obj in objectsToHide)
            {
                DebriefEventItem dbItem = obj.GetComponent<DebriefEventItem>();
                if (dbItem.EventActive)
                {
                    if (!dbItem.IsHiddenAction)
                        obj.SetActive(visible);
                }
                dbItem.IsHiddenPlayer = !visible;
            }
        }
    }
    public void HideAll()
    {
        foreach (KeyValuePair<int, List<GameObject>> pair in PlayerDebriefItems)
        {
            PlayerItemVisibilty(pair.Key, false);
        }
        foreach (Toggle tg in PlayerToggleBtns)
        {
            tg.SetIsOnWithoutNotify(false);
        }
    }
    public void ShowAll()
    {
        foreach (KeyValuePair<int, List<GameObject>> pair in PlayerDebriefItems)
        {
            PlayerItemVisibilty(pair.Key, true);
        }
        foreach (Toggle tg in PlayerToggleBtns)
        {
            tg.SetIsOnWithoutNotify(true);
        }
    }
}
