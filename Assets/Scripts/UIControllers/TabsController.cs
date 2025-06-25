using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TabsController : MonoBehaviour
{
    public List<Button> Tabs;
    public int StartTab = 0;

    protected int _activeTab;

    protected virtual void Start()
    {
        for (int i = 0; i < Tabs.Count; i++)
        {
            int tabIndex = i;
            Tabs[tabIndex].onClick.AddListener(delegate { ChangeTab(tabIndex); });
        }
        ChangeTab(StartTab);
    }

    protected virtual void OnEnable()
    {
        ChangeTab(_activeTab);
    }

    /// <summary>
    /// Disables interaction on the current active tab, and activates interaction on all others. Override to extend with switch case.
    /// </summary>
    /// <param name="tabIndex"></param>
    protected virtual void ChangeTab(int tabIndex)
    {
        if (Tabs == null || tabIndex < 0 || tabIndex >= Tabs.Count)
            return;

        _activeTab = tabIndex;
        
        foreach (Button btn in Tabs)
        {
            btn.interactable = true;
        }

        Tabs[_activeTab].interactable = false;
    }
}
