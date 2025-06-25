using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class UIListDebriefCategoryToggles : MonoBehaviour
{
    public SessionPlaybackControl SessionPlaybackControl;
    public SessionEventManager SessionEventManager;
    public GameObject EventToggleButtonPrefab;
    public Transform ListParentTransform;

    private struct ToggleData
    {
        public Toggle Toggle;
        public DebriefMarkers.DebriefMarkerData CategoryData;
    }

    private List<ToggleData> _activeToggles = new List<ToggleData>();

    // Start is called before the first frame update
    void Start()
    {
        if (SessionPlaybackControl == null)
            SessionPlaybackControl = SessionPlaybackControl.GetDefault(gameObject);
        if (SessionEventManager == null)
            SessionEventManager = SessionEventManager.GetDefault(gameObject);

        if (ListParentTransform == null)
            ListParentTransform = transform;


        if (SessionPlaybackControl.IsSessionLoaded)
            InitializeCategoryToggleButtons();

        SessionPlaybackControl.SessionLoaded += OnSessionLoaded;
        SessionEventManager.CategoryVisibilityChanged += OnCategoryVisibilityChanged;
    }

    private void OnDestroy()
    {
        SessionPlaybackControl.SessionLoaded -= OnSessionLoaded;
        SessionEventManager.CategoryVisibilityChanged -= OnCategoryVisibilityChanged;
    }

    private void OnCategoryVisibilityChanged()
    {
        UpdateToggleState();
    }

    private void OnSessionLoaded()
    {
        InitializeCategoryToggleButtons();
    }

    private void UpdateToggleState()
    {
        foreach (var data in _activeToggles)
        {
            if (data.Toggle == null || data.Toggle.gameObject == null || data.CategoryData == null)
                continue;

            data.Toggle.SetIsOnWithoutNotify(data.CategoryData.CategoryVisible);
            var toggleShow = data.Toggle.GetComponent<ToggleShowWhenNotSelected>();
            if (toggleShow != null)
                toggleShow.UpdateObjectVisibility();
        }
    }

    private void InitializeCategoryToggleButtons()
    {
        _activeToggles.Clear();

        //remove any existing buttons
        foreach (Transform xform in ListParentTransform)
        {
            Destroy(xform.gameObject);
        }

        var eventCategories = SessionEventManager.DebriefMarkerCategories.EventCategoryData;

        for (int markerCatIndex = 0; markerCatIndex < eventCategories.Count; markerCatIndex++)
        {
            var markerCat = eventCategories[markerCatIndex];

            //create category toggle button object
            GameObject actionToggleObj = Instantiate(EventToggleButtonPrefab, ListParentTransform);

            SVGImage svImage;
            var catIconObj = actionToggleObj.transform.Find("EventCategoryIcon");
            if (catIconObj != null)
                svImage = catIconObj.GetComponent<SVGImage>();
            else
                svImage = actionToggleObj.GetComponentInChildren<SVGImage>();

            MenuTooltip menuTooltip = actionToggleObj.GetComponent<MenuTooltip>();
            Toggle actionToggle = actionToggleObj.GetComponent<Toggle>();

            if (svImage == null || menuTooltip == null || actionToggle == null)
                continue;

            //initialize object
            svImage.sprite = markerCat.MarkerSprite;

            if (menuTooltip != null)
            {
                menuTooltip.SetTooltipText(markerCat.Description);
            }

            actionToggle.SetIsOnWithoutNotify(SessionEventManager.IsCategoryDisplayed(markerCatIndex));

            var catIndex = markerCatIndex; //capture marker category index
            actionToggle.onValueChanged.AddListener((val) =>
            {                
                SessionEventManager.ShowEventCategory(catIndex, val);
            });

            _activeToggles.Add(new ToggleData
            {
                Toggle = actionToggle,
                CategoryData = markerCat,
            });

            //add event?            
            //actionToggleBtn.onValueChanged.AddListener((value) => ActionsHandler.ActionItemVisibilty(j, value));
            //ActionVisButton ActionButton = actionToggleBtn.GetComponent<ActionVisButton>();
            //ActionButton.Index = markerCatIndex;
            //ActionButton.ActionsVisibilityHandler = ActionsHandler;

        }
    }
}
