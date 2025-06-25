using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// This script manages and stores the data of consolidated child UI objects
/// </summary>
[System.Obsolete]
public class HierarchyContainer : MonoBehaviour
{
    /*
    #region Variables
    public enum HierarchyTag
    {
        Root,
        Tile,
        Placeable,
    }

    public HierarchyTag hierarchyTag;
    [SerializeField]protected float uiSpacing;
    
    public RectTransform rt;
    public RectTransform contentRt;
    private RectTransform rootContentRt;
    //private HierarchyContainer rootContainer;
    [SerializeField] protected VerticalLayoutGroup layout;
    [SerializeField] protected RectTransform layoutRt;

    protected GameObject contentGO;
    public Transform prefabInSceneTfm;
    public GameObject mineParent;
    public bool toggleOpen;
    [SerializeField] bool isRoot; //assigned in inspector
    Toggle toggle;
    GameObject toggleGO;
    [SerializeField] GameObject arrowOpen;
    [SerializeField] GameObject arrowClosed;
    float originalHeight;

    //[SerializeField] ScenarioSaveLoad scenarioSaveLoad;

    #endregion Variables

    #region Initialize

    public void Awake()
    {
        //assign
        rt = GetComponent<RectTransform>();
        if (contentRt == null) contentRt = (RectTransform)rt.Find("Content");
        contentGO = contentRt.gameObject;
        if (layout == null) layout = GetComponentInParent<VerticalLayoutGroup>();
        if (layoutRt == null) layoutRt = layout.GetComponent<RectTransform>();
        toggle = GetComponentInChildren<Toggle>();
        if (toggle) toggleGO = toggle.gameObject;
        originalHeight = rt.sizeDelta.y;
    }


    public void Reinitialize()
    {
        //Debug.Log("Reinitialize");

        layout = GetComponentInChildren<VerticalLayoutGroup>();

        if (layout != null) Initialize();


    }
    public virtual void Initialize()
    {
        //Debug.Log("Initialize");
        //assign prefab in scene  
        if (isRoot)
        {
            rootContentRt = contentRt;
            prefabInSceneTfm = GameObject.Find("Assets").transform;
            mineParent = GameObject.Find("MineParent");
            //rootContainer = this;
            if (mineParent) GetSubObjects(mineParent.transform);
        }
        else
        {
            HierarchyItem myChildScript = GetComponent<HierarchyItem>();
            if (myChildScript != null) prefabInSceneTfm = myChildScript.prefabInScene.transform;
            VerticalLayoutGroup layout = GetComponentInParent<VerticalLayoutGroup>();
            rootContentRt = layout.GetComponent<RectTransform>();
            //rootContainer = layout.GetComponentInParent<HierarchyContainer>();
        }

        //assign sub objects
        if (prefabInSceneTfm)
        {
            if (isRoot) GetSubObjects(prefabInSceneTfm);
            else StartCoroutine(GetSubObjectsLate());
        }
        if (toggleGO != null && contentRt.childCount < 1) toggleGO.SetActive(false);
        if (!isRoot)SizeContainerLabel(true);
        SizeContainerContent(true);
    }

    IEnumerator GetSubObjectsLate()
    {
        yield return new WaitForSeconds(3);
        GetSubObjects(prefabInSceneTfm);
    }

    void GetSubObjects(Transform container)
    {
        // get initial sub objects in scene on generation deep
        foreach (Transform child in container)
        {
            ObjectInfo info = child.GetComponent<ObjectInfo>();
            if (info) { CreateItemFromPlacedObject(child.gameObject, 0, true); }

        }
    }

    #endregion Initialize

    #region Create Item

    /// <summary>
    /// Used for creating a UI element to represent an object that is already placed in the scene
    /// </summary>
    /// <param name="linkedGO"></param>
    /// <param name="index"></param> 
    public virtual void CreateItemFromPlacedObject(GameObject placedObject, int index = 0, bool add = false)
    {
        //Debug.Log("CreatItemFromPlacedObject");//
        //create
        GameObject itemPrefab = placedObject.GetComponent<ObjectInfo>().HierarchyItemPrefab;
        GameObject itemObj = Instantiate(itemPrefab, contentRt);
        
        //assign
        HierarchyItem item = itemObj.GetComponent<HierarchyItem>();

        //initialize
        //item.InitializeFromPlacedObject(placedObject, this);

        //add to hierarchy
        if(add)AddItemToContainer(item,0,false);
    }
   
    /// <summary>
    /// Used for creating both a UI item and an object from a prefab
    /// </summary>
    /// <param name="linkedGO"></param>
    /// <param name="index"></param>
    public virtual void CreateItemFromPrefab(GameObject PrefabToLoad, out HierarchyItem item)
    {
        //Debug.Log("CreatItemFromPrefab");
        
        //create
        GameObject uiItemPrefab = PrefabToLoad.GetComponent<ObjectInfo>().HierarchyItemPrefab;
        GameObject uiObj = Instantiate(uiItemPrefab, contentRt);
        
        //assign
        item = uiObj.GetComponent<HierarchyItem>();

        //initialize
        //item.InitializeFromPrefab(PrefabToLoad, this);

        //item.rt.parent = rootContentRt;

        //add to hierarchy
        //AddItemToContainer(item, 0, true);
    }

    #endregion Create Item

    #region Move Item

    /// <summary>
    /// Add an existing hierarchy item to a container 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    /// <param name="repositionPrefab"></param>
    public virtual void AddItemToContainer(HierarchyItem item, int index = 0, bool repositionPrefab = true)
    {
        //Debug.Log("Add Item Tag: " + this.hierarchyTag + " / " + prefabInSceneTfm.gameObject);
        // child UI should only be added if parent layer matches
        if (item.parentTag != this.hierarchyTag)
        {
            ////Debug.Log("Add Item Canceled - Invalid Tag: " + this.hierarchyTag + " / " + prefabInSceneTfm.gameObject);
            return;
        }

        item.rt.SetParent(contentRt, false);
        item.rt.SetSiblingIndex(index);

        if (item.lastContainer != null)
        {
            if (item.lastContainer == this)
            {
                ////Debug.Log("Add Item Canceled - Same Container: " + prefabInSceneTfm.gameObject);
                return;
            }
            item.lastContainer.RemoveItemFromContainer(item);
        }
        ////Debug.Log("Add " + item.prefabInScene + " to " + prefabInSceneTfm.gameObject);
        item.lastContainer = this;

        //move linked object to new parent and position
        if (prefabInSceneTfm != null && repositionPrefab)
        {
            Transform itemPrefabInSceneXFM = item.prefabInScene.transform;
            itemPrefabInSceneXFM.parent = prefabInSceneTfm;
            itemPrefabInSceneXFM.localPosition = Vector3.zero;
        }

        //activate and open toggle
        if (toggle)
        {
            if (!toggleGO.activeInHierarchy) toggleGO.SetActive(true);
            toggle.isOn = true;
        }
        toggleOpen = true; // redundant?

        //adjust hierarchy UI
        SizeContainerContent(false);
        StructureContentItems(true);
        if (!isRoot) SizeContainerLabel(toggleOpen);
    }
    
    /// <summary>
    /// Remove an existing hierarchy item from a container, either to be destroyed or to be added to another container
    /// </summary>
    /// <param name="item"></param>
    public void RemoveItemFromContainer(HierarchyItem item)
    {
        if (item.rt.parent == contentRt) item.rt.parent = null;

        //activate and open toggle
        if (contentRt.childCount < 1)
        {
            if (toggleGO != null)
            {
                toggle.isOn = false;
                if (toggleGO.activeInHierarchy) toggleGO.SetActive(false);
                toggleOpen = false; // redundant?
            }
            //Debug.Log("Remove " + item + " from " + this + " and clear");
        }
        else
        {
            //Debug.Log("Remove " + item + " from " + this + " with " + (contentRt.childCount) + " item(s) left");
        }

        if (item) item.rt.SetAsLastSibling();

        // rebuild layout
        SizeContainerContent(false);
        StructureContentItems(true);
        if (!isRoot) SizeContainerLabel(toggleOpen);

    }

    #endregion Move Item

    #region Sizing and Layout
    protected void SizeContainerLabel(bool open)
    {
        //change size of ui to accomodate sub heirarchy
        ////Debug.Log("SizeUI");
        Vector3 size = rt.sizeDelta;
        if (open)
        {
            // height is one spacing bigger than subObjects
            //float height = (contentRt.childCount * uiSpacing) + uiSpacing;
            float height = (contentRt.sizeDelta.y) + originalHeight;
            rt.sizeDelta = new Vector3(size.x, height, size.z);
        }
        else
        {
            // height is one spacing
            float height = originalHeight;
            rt.sizeDelta = new Vector3(size.x, height, size.z);
        }
        RebuildLayout();
    }
    
    public void SizeContainerContent(bool rebuildLayout = false)
    {
        if (contentRt == null) return;
        ////Debug.Log("SizeUIContent");
        Vector3 size = contentRt.sizeDelta;
        //float height = contentRt.childCount * uiSpacing;
        float height = 0;
        foreach(RectTransform child in contentRt)
        {
            height = height + child.sizeDelta.y + uiSpacing;
        }

        contentRt.sizeDelta = new Vector3(size.x, height, size.z);

       // //Debug.Log("Resize Container");
        // if not root, also resize root

        if (rebuildLayout) RebuildLayout();
    }

    protected void StructureContentItems(bool rebuildLayout = false)
    {
        ////Debug.Log("StructureUIContent");
        for (int i = 0; i < contentRt.childCount; i++)
        {
            RectTransform c = (RectTransform)contentRt.GetChild(i);
            float offset = c.sizeDelta.y + uiSpacing;
            c.anchoredPosition = new Vector3(0, i * -offset, 0);
        }

        if (rebuildLayout) RebuildLayout();

        
    }

    protected void RebuildLayout()
    {
        if (!isRoot) rootContainer.SizeContainerContent(false);
        layout.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRt);
        layout.enabled = false;
    }
    
    #endregion Sizing and Layout

    public void ToggleSubHierarchy(bool state)
    {
        // activate sub hierarchy object
        toggleOpen = state;
        contentGO.SetActive(toggleOpen);
        arrowOpen.SetActive(toggleOpen);
        arrowClosed.SetActive(!toggleOpen);

        if (!isRoot) SizeContainerLabel(toggleOpen);

    }
    */
}
