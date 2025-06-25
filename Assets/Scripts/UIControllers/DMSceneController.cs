using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;
using TMPro;

public class DMSceneController : UIContextBase, IMinimizableWindow
{
    public NetworkManager NetworkManager;

    public Button PreloadButton;
    public Button ActivateButton;
    public ToggleGroup SceneListGroup;
    public TMP_InputField FilterInputField;
    public Toggle FavoriteToggle;


    private string _selectedScene;
    private const string sceneContextVariable = "SELECTED_SCENE";

    public event Action<string> TitleChanged;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        if (NetworkManager == null)
            NetworkManager = NetworkManager.GetDefault(gameObject);

        if (SceneListGroup == null)
        {
            Debug.LogError("No scene list group on DMSceneController");
            return;
        }

        foreach (Transform xform in SceneListGroup.transform)
        {
            var toggle = xform.gameObject.GetComponent<Toggle>();
            if (toggle == null)
                continue;

            toggle.onValueChanged.AddListener((isSelected) =>
            {
                OnSelectionChanged(toggle.name);
            });
        }

        _context.ContextDataChanged += OnContextDataChanged;

        if (PreloadButton != null)
            PreloadButton.onClick.AddListener(OnPreload);

        if (ActivateButton != null)
            ActivateButton.onClick.AddListener(OnActivate);

        if (FavoriteToggle != null)
            FavoriteToggle.onValueChanged.AddListener(OnFavorite);

        if (FilterInputField != null)
            FilterInputField.onValueChanged.AddListener(FilterField);
    }

    private void OnContextDataChanged(string obj)
    {
        var selectedScene = _context.GetStringVariable(sceneContextVariable);

        if (ActivateButton != null && selectedScene != null && selectedScene.Length > 0)
        {
            ActivateButton.interactable = true;
        }
        else
        {
            ActivateButton.interactable = false;
        }
    }

    private void OnFavorite(bool favorite)
    {
        RectTransform rectTransform = SceneListGroup.GetComponent<RectTransform>();

        if (FilterInputField != null)
            FilterInputField.SetTextWithoutNotify("");

        foreach (RectTransform child in rectTransform)
        {
            UIBtnSelectScene uIBtnSelectScene = child.GetComponentInChildren<UIBtnSelectScene>();
            if (uIBtnSelectScene == null)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            if (!favorite && !uIBtnSelectScene.IgnoreAtRuntime)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                UIBtnSelectScene childScene = child.GetComponentInChildren<UIBtnSelectScene>();
                if (childScene != null)
                {
                    child.gameObject.SetActive(childScene.IsFavorite());
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    void OnSelectionChanged(string selectedScene )
    {
        _selectedScene = selectedScene;
        Debug.Log($"SceneSelected: {selectedScene}");
    }

    void OnPreload()
    {
        if (_selectedScene == null)
            return;

        NetworkManager.SendLoadScene(_selectedScene, false);
    }

    void OnActivate()
    {
        _selectedScene = _context.GetStringVariable(sceneContextVariable);
        Debug.Log("SELECTED SCENE: " + _selectedScene);
        if (_selectedScene == null)
            return;

        if (_selectedScene.Contains(".json"))
        {
            /*
            ScenarioInitializer init = ScenarioInitializer.Instance;
            if (init == null) 
                init = gameObject.AddComponent<ScenarioInitializer>();

            StartCoroutine(init.LoadCustomScenario(_selectedScene)); */

            NetworkManager.SendLoadScene("CustomScenario:" + _selectedScene, true);
            _context.SetVariable(sceneContextVariable, null);
        }
        else
        {
            NetworkManager.SendLoadScene(_selectedScene, true);
            _context.SetVariable(sceneContextVariable, null);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public string GetTitle()
    {
        return "Scenes";
    }

    public void Minimize(bool minimize)
    {
        gameObject.SetActive(minimize);
    }

    public void ToggleMinimize()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    public void AssignTaskbarButton(Button button)
    {

    }

    public void FilterField(string entry)
    {
        string lowerCase = entry.ToLower();
        RectTransform rectTransform = SceneListGroup.GetComponent<RectTransform>();
        foreach (RectTransform child in rectTransform)
        {
            string childLowerCaseName = child.GetComponentInChildren<TextMeshProUGUI>().text.ToLower();
            UIBtnSelectScene uIBtnSelectScene = child.GetComponentInChildren<UIBtnSelectScene>();
            if (string.IsNullOrEmpty(entry) && !uIBtnSelectScene.IgnoreAtRuntime)
            {
                if (FavoriteToggle.isOn)
                {
                    if (uIBtnSelectScene.IsFavorite())
                    {
                        child.gameObject.SetActive(true);
                    }
                }
                else
                {
                    child.gameObject.SetActive(true);
                }
            }
            else
            {
                if (childLowerCaseName.Contains(lowerCase) && !uIBtnSelectScene.IgnoreAtRuntime)
                {
                    if (FavoriteToggle.isOn)
                    {
                        if (uIBtnSelectScene.IsFavorite())
                        {
                            child.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        child.gameObject.SetActive(true);
                    }
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

}
