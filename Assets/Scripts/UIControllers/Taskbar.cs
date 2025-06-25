using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace ScenarioEditor
{

    /// <summary>
    /// Stripped down version of DMGUIController for taskbar functionality. Consider refactoring DMGUI for modularity.
    /// </summary>
    public class Taskbar : MonoBehaviour
    {

        public GameObject TaskBarItemPrefab;
        public RectTransform TaskBarPanel;
        public List<GameObject> taskWindows;
        
        private Dictionary<GameObject, GameObject> _taskBarItems;

        // Start is called before the first frame update
        void Start()
        {
            _taskBarItems = new Dictionary<GameObject, GameObject>();
            if (TaskBarItemPrefab != null && taskWindows != null && taskWindows.Count > 0)
            {
                foreach (var winobj in taskWindows)
                {
                    AddWindow(winobj);
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

            PassAssignTaskbarButton(win, btn);
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

        void TaskBarClicked(IMinimizableWindow win)
        {
            win.ToggleMinimize();
            
        }

        void PassAssignTaskbarButton(IMinimizableWindow win, Button btn)
        {
            win.AssignTaskbarButton(btn);
        }
    }
}
