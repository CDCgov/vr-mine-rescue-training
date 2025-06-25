using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortDisplayHandler : MonoBehaviour
{
    public DebriefFileController FileController;
    public Button FileNameBtn;
    public Button SessionNameBtn;
    public Button SceneBtn;
    public Button DateBtn;
    public Button TimeBtn;

    private List<Transform> Buttons;
    // Start is called before the first frame update
    void Start()
    {
        Buttons = new List<Transform>();
        if (FileController == null)
        {
            FileController = FindObjectOfType<DebriefFileController>();
        }
        FileNameBtn.onClick.AddListener(() => OnClicked(FileNameBtn.transform));
        Buttons.Add(FileNameBtn.transform);
        if (SessionNameBtn != null)
        {
            SessionNameBtn.onClick.AddListener(() => OnClicked(SessionNameBtn.transform));
            Buttons.Add(SessionNameBtn.transform);
        }
        if(SceneBtn != null)
        {
            SceneBtn.onClick.AddListener(() => OnClicked(SceneBtn.transform));
            Buttons.Add(SceneBtn.transform);
        }

        DateBtn.onClick.AddListener(() => OnClicked(DateBtn.transform));
        Buttons.Add(DateBtn.transform);
        if (TimeBtn != null)
        {
            TimeBtn.onClick.AddListener(() => OnClicked(TimeBtn.transform));
            Buttons.Add(TimeBtn.transform);
        }
 
    }

    void OnClicked(Transform btnObj)
    {
        //foreach(Transform button in Buttons)
        //{
        //    button.GetChild(0).gameObject.SetActive(false);
        //}
        //btnObj.GetChild(0).gameObject.SetActive(true);
    }
}
