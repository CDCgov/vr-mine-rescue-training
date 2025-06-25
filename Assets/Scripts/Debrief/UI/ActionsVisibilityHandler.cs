using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionsVisibilityHandler : MonoBehaviour
{

    //public Dictionary<int, List<GameObject>> DebriefActionItems;
    //public List<bool> ActiveActions;
    //public PlayerVisibiltyHandler PlayerVisibiltyHandler;
    //public List<Toggle> ActionToggleBtns;

    //private void Start()
    //{
    //    ActionToggleBtns = new List<Toggle>();
    //}

    //public void SetActiveActionsReference(int length)
    //{
    //    ActiveActions = new List<bool>();
    //    for (int i = 0; i < length; i++)
    //    {
    //        ActiveActions.Add(true);
    //    }
    //}

    //public void ActionItemVisibilty(int index, bool visible)
    //{        
    //    //Debug.Log($"{index} is a key?");
    //    if (DebriefActionItems.TryGetValue(index, out List<GameObject> objectsToHide))
    //    {
    //        Debug.Log($"Player {index}: Items to hide, {objectsToHide.Count}");

    //        foreach (GameObject obj in objectsToHide)
    //        {
    //            DebriefEventItem dbItem = obj.GetComponent<DebriefEventItem>();
    //            if (dbItem.EventActive)
    //            {
    //                if (!dbItem.IsHiddenPlayer)
    //                    obj.SetActive(visible);
    //            }
    //            dbItem.IsHiddenAction = !visible;
    //        }
    //    }
    //}

    //public void HideAll()
    //{
    //    foreach(KeyValuePair<int, List<GameObject>> pair in DebriefActionItems)
    //    {
    //        ActionItemVisibilty(pair.Key, false);
    //    }

    //    foreach(Toggle tg in ActionToggleBtns)
    //    {
    //        tg.SetIsOnWithoutNotify(false);
    //    }
    //}
    //public void ShowAll()
    //{
    //    foreach (KeyValuePair<int, List<GameObject>> pair in DebriefActionItems)
    //    {
    //        ActionItemVisibilty(pair.Key, true);
    //    }
    //    foreach (Toggle tg in ActionToggleBtns)
    //    {
    //        tg.SetIsOnWithoutNotify(true);
    //    }
    //}
}
