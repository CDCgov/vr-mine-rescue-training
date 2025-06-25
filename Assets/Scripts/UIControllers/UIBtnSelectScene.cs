using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnSelectScene : UIContextBase
{
    public string SceneName;
    public Image BackgroundImage;
    public bool IgnoreAtRuntime = false;
    public SystemManager SystemManager;

    private Button _button;

    private const string sceneContextVariable = "SELECTED_SCENE";

    private Color _selectedColor = new Color(0, 0.5f, 0);
    private Color _normalColor = Color.black;
    private MenuTooltip _menuTooltip;
    private Toggle _favoriteToggle;
    private bool _isFavorite = false;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnButtonClicked);
        _context.ContextDataChanged += OnContextDataChanged;

        _menuTooltip = GetComponent<MenuTooltip>();
        _favoriteToggle = GetComponentInChildren<Toggle>();
        if (_favoriteToggle != null)
            _favoriteToggle.onValueChanged.AddListener(SetFavorite);

        if(_menuTooltip != null)
        {
            _menuTooltip.SetTooltipText(SceneName);
        }

        OnContextDataChanged(null);

        if(SystemManager == null)
        {
            SystemManager = SystemManager.GetDefault();
        }

        if (SystemManager.SystemConfig.FavoriteScenes.Contains(SceneName))
        {
            _isFavorite = true;
            _favoriteToggle.SetIsOnWithoutNotify(true);
        }
    }

    private void OnContextDataChanged(string obj)
    {
        if (BackgroundImage == null)
            return;

        var selectedScene = _context.GetStringVariable(sceneContextVariable);

        if (selectedScene == SceneName)
            BackgroundImage.color = _selectedColor;
        else
            BackgroundImage.color = _normalColor;
    }

    private void OnButtonClicked()
    {
        _context.SetVariable(sceneContextVariable, SceneName);
    }

    private void SetFavorite(bool isFavorite)
    {
        _isFavorite = isFavorite;

        if (isFavorite)
        {
            if (!SystemManager.SystemConfig.FavoriteScenes.Contains(SceneName))
            {
                SystemManager.SystemConfig.FavoriteScenes.Add(SceneName);
                SystemManager.SystemConfig.SaveConfig();
            }

        }
        else
        {
            if (SystemManager.SystemConfig.FavoriteScenes.Contains(SceneName))
            {
                SystemManager.SystemConfig.FavoriteScenes.Remove(SceneName);
                SystemManager.SystemConfig.SaveConfig();
            }
        }
    }

    public bool IsFavorite()
    {
        return _isFavorite;
    }

    void OnDestroy()
    {
        if (_favoriteToggle != null)
            _favoriteToggle.onValueChanged.RemoveListener(SetFavorite);
    }
}
