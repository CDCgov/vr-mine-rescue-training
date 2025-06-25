using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine.Events;

public class ResearcherControlsController : MonoBehaviour 
{
    public Dropdown ViewpointDropdown;
    public Button LoadMineBtn;
    public Dropdown LoadMineDropdown;
    public Slider AdditionalLightSlider;
    public Transform ObjPropsContent;
    public Text ObjPropsTitle;
    public Text ObjPropsText;
    public Text PlayerPositionText;
    public RectTransform ContextButtonsParent;
    

    private StringBuilder _stringBuilder;
    ISelectableObject _lastSelectedObject;

    void Start () 
    {
        MasterControl.SceneChanged += OnSceneChanged;

        _stringBuilder = new StringBuilder(250);
        ObjPropsText.text = "";
    }

    void Update()
    {
        _stringBuilder.Length = 0; //clear buffer

        if (MasterControl.SceneControl != null && MasterControl.SceneControl.ActiveActors != null)
        {
            _stringBuilder.AppendLine("Player Positions:");
            foreach (ActorHost actor in MasterControl.SceneControl.ActiveActors)
            {
                _stringBuilder.AppendFormat("{0}: ", actor.ActorName);
                _stringBuilder.AppendLine(actor.transform.position.GetColoredText());
            }
        }

        PlayerPositionText.text = _stringBuilder.ToString();
        
        if (MasterControl.ResearcherCamera != null && MasterControl.ResearcherCamera.SelectedObject != null)
        {
            ISelectableObject selectedObj = MasterControl.ResearcherCamera.SelectedObject;
            if (_lastSelectedObject != selectedObj)
            {
                SelectedObjectChanged(selectedObj);
                _lastSelectedObject = selectedObj;
            }


            _stringBuilder.Length = 0;
            MasterControl.ResearcherCamera.SelectedObject.GetObjectInfo(_stringBuilder);

            ObjPropsTitle.text = "Properties of " + MasterControl.ResearcherCamera.SelectedObject.GetObjectDisplayName();// MasterControl.ResearcherCamera.SelectedObjectName;
            ObjPropsText.text = _stringBuilder.ToString();
        }

    }

    private void SelectedObjectChanged(ISelectableObject newObj)
    {		
        ClearContextButtons();


        /*
        int count = Random.Range(2, 6);
        for (int i = 0; i < count; i++)
        {
            AddContextButton();
        }*/

        System.Type objType = newObj.GetType();
        var methods = objType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var methodInfo in methods)
        {
            var attribs = methodInfo.GetCustomAttributes(typeof(ResearcherActionAttribute), true);

            if (attribs != null && attribs.Length >= 1)
            {
                ResearcherActionAttribute attrib = (ResearcherActionAttribute)attribs[0];

                System.Delegate d = System.Delegate.CreateDelegate(typeof(UnityAction), newObj, methodInfo);
                AddContextButton(attrib.CommandName, (UnityAction)d);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }

    private void AddContextButton(string buttonName, UnityAction clickHandler)
    {
        GameObject prefab = Resources.Load<GameObject>("SimpleButton");
        GameObject obj = GameObject.Instantiate<GameObject>(prefab);

        obj.transform.SetParent(ContextButtonsParent, false);
        Text txt = obj.GetComponentInChildren<Text>();
        txt.text = buttonName;

        Button button = obj.GetComponent<Button>();
        button.onClick.AddListener(clickHandler);
    }

    private void ClearContextButtons()
    {
        if (ContextButtonsParent == null)
            return;

        foreach (Transform child in ContextButtonsParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnSceneChanged()
    {
        if (MasterControl.SceneControl != null)
        {
            MasterControl.SceneControl.ActiveActorsChanged += OnActiveActorsChanged;
        }
    }

    void OnEnable()
    {
        if (MasterControl.SceneControl != null)
        {
            MasterControl.SceneControl.ActiveActorsChanged += OnActiveActorsChanged;
        }
    }

    void OnDisable()
    {
        if (MasterControl.SceneControl != null)
        {
            MasterControl.SceneControl.ActiveActorsChanged -= OnActiveActorsChanged;
        }
    }

    private void OnActiveActorsChanged()
    {
        PopulateViewpointList();
    }

    private void PopulateViewpointList()
    {
        Dropdown.OptionData option;

        ViewpointDropdown.options.Clear();

        option = new Dropdown.OptionData("Free Camera");
        ViewpointDropdown.options.Add(option);

        if (MasterControl.SceneControl != null)
        {
            int actorCount = MasterControl.SceneControl.ActiveActors.Count;
            for (int i = 0; i < actorCount; i++)
            {
                option = new Dropdown.OptionData(MasterControl.SceneControl.ActiveActors[i].ActorName);
                ViewpointDropdown.options.Add(option);
            }
        }
    }

    public void OnLoadMine()
    {
        MasterControl.RequestSceneLoad("TestMine1");
    }

    public void OnLightIntensityChanged()
    {
        MasterControl.SetResearcherLightIntensity(AdditionalLightSlider.value);
    }

    public void OnEndSimulation()
    {
        MasterControl.RequestLoadMainMenu();
    }

    public void OnChangeViewpoint()
    {
        Debug.Log("Changed to viewpoint " + ViewpointDropdown.value);

        int selected = ViewpointDropdown.value;

        if (selected == 0)
        {
            //free camera
            MasterControl.ResearcherCamera.FollowTransform(null);
        }
        else
        {
            //follow viewpoint
            ActorHost targetActor = MasterControl.SceneControl.ActiveActors[selected - 1];
            MasterControl.ResearcherCamera.FollowTransform(targetActor.HeadTransform);
        }

    }

    public void OnProxVisOff()
    {
        if (MasterControl.ResearcherCamera != null && MasterControl.ResearcherCamera.SelectedObject != null)
        {
            ProxSystemController proxSystem = ((Component)MasterControl.ResearcherCamera.SelectedObject).GetComponent<ProxSystemController>();
            if (proxSystem != null)
            {
                proxSystem.DisableZoneVisualization();
            }
        }
    }
    
    public void OnProxVisYellow()
    {
        if (MasterControl.ResearcherCamera != null && MasterControl.ResearcherCamera.SelectedObject != null)
        {
            ProxSystemController proxSystem = ((Component)MasterControl.ResearcherCamera.SelectedObject).GetComponent<ProxSystemController>();
            if (proxSystem != null)
            {
                proxSystem.EnableZoneVisualization(new ProxSystem.VisOptions(false, true));
            }
        }
    }

    public void OnProxVisRed()
    {
        if (MasterControl.ResearcherCamera != null && MasterControl.ResearcherCamera.SelectedObject != null)
        {
            ProxSystemController proxSystem = ((Component)MasterControl.ResearcherCamera.SelectedObject).GetComponent<ProxSystemController>();
            if (proxSystem != null)
            {
                proxSystem.EnableZoneVisualization(new ProxSystem.VisOptions(true, false));
            }
        }
    }
}