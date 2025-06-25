using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
public class ComponentInfo_Lifeline : ModularComponentInfo, ISaveableComponent
{
    
    public string ComponentName = "Lifeline";
    public bool IsMetric = true; //To Do - set this or retarget it to a global/static bool elsewhere
    public LifelineMarkerGenerator Component;
    public List<LifelineItem> lifelineItems = new List<LifelineItem>();
    public List<LifelineItem> Tags = new List<LifelineItem>();
    public List<LifelineItem> DirectionalMarkers = new List<LifelineItem>();

    public bool TagsFlipped;
    public bool DirFlipped;

    public LifelineMarkerGenerator LGenerator;
    public Transform IndividualMarkerContainer;
    public HangingGeometry Cable;
    public ComponentInfo_Cable CableInfo;
    public Dictionary<Transform, int> MarkerPointDictionary = new Dictionary<Transform, int>();

    public IndexedColorList TagColors;
    public int ColorIndex;

    //private ObjectInfo _objectInfo;
    private int _lifelineItemsCount;
    private GlobalCableData _globalCableData;
    private float _markerSpacing = 1;
    //private RuntimeMarkerEditor _markerEditor;



    public void Awake()
    {
        _markerSpacing = 1;
        _globalCableData = FindObjectOfType<GlobalCableData>();

        //_markerEditor = FindObjectOfType<RuntimeMarkerEditor>();
        //if(_markerEditor)_markerEditor._info = this;
        Cable.MeshGenerated += RepositionMarkersWithCable;

        // add this to object info
        //_objectInfo = GetComponent<ObjectInfo>();
        //if (_objectInfo == null) _objectInfo = GetComponentInParent<ObjectInfo>();
        //if (!_objectInfo.componentInfo_Lifelines.Contains(this)) _objectInfo.componentInfo_Lifelines.Add(this);
        //InitializeMarkerDirection();
    }

    private void OnDestroy()
    {
        Cable.MeshGenerated -= RepositionMarkersWithCable;
    }

    public void ResetValues()
    {
        foreach(KeyValuePair<Transform,int> k in MarkerPointDictionary)
        {
            var t = k.Key.gameObject;
            //markers.Remove(k.Key);
            Destroy(k.Key.gameObject);
        }
        MarkerPointDictionary.Clear();
        
        foreach (LifelineItem item in lifelineItems)
        {
            Destroy(item.gameObject);
        }
        lifelineItems.Clear();
        Tags.Clear();
        DirectionalMarkers.Clear();
    }

    public string SaveName()
    {
        return ComponentName;
    }

    public string[] SaveInfo()
    {
        List<string> data = LifelineDataList();
        
        return data.ToArray();
    }

    List<string> LifelineDataList()
    {
        
        List<string>data = new List<string>();

        data.Add("LifelineItemsCount|" + lifelineItems.Count);
        data.Add("MarkerSpacing|" + _markerSpacing);
        
        for (int i = 0; i < lifelineItems.Count; i++)
        {
            LifelineItem item = lifelineItems[i];
            data.Add($"ItemType_{i}|" + (int)item.itemType);
            data.Add($"FlippedDirection_{i}|" + item.flippedDirection);
            data.Add($"PositionIndex_{i}|" + item.ClosestPointIndex);

            // save directional data
            data.Add($"DirectionFlipped_{i}|" + item.flippedDirection); 
            
            // save tag info
            if (item.itemType == LifelineItem.ItemType.Tag)
            {
                LifelineTag tag = item as LifelineTag;
                data.Add($"TagColor_{i}|" + tag.TagColorIndex);
            }
        }

        return data;
    }

    public void LoadInfo(SavedComponent component)
    {
        if (component == null) { Debug.Log("Failed to load lifleine component info. Saved component is null for " + gameObject.name); return; }

        // get component
        ComponentName = component.GetComponentName();

        int.TryParse(component.GetParamValueAsStringByName("LifelineItemsCount"), out _lifelineItemsCount);
        float.TryParse(component.GetParamValueAsStringByName("MarkerSpacing"), out _markerSpacing);
        
        //int.TryParse(component.GetParamValueAsStringByName("LifelineType"), out lifelineType);
        LoadLifelineData(component);
        //InitializeMarkerDirection();
    }

    void LoadLifelineData(SavedComponent component)
    {

        lifelineItems.Clear();
        Tags.Clear();
        DirectionalMarkers.Clear();
        MarkerPointDictionary.Clear();

        Tags = new List<LifelineItem>();
        DirectionalMarkers = new List<LifelineItem>();
        lifelineItems = new List<LifelineItem>();
        MarkerPointDictionary =  new Dictionary<Transform, int>();


        for (int i = 0; i < _lifelineItemsCount; i++)
        {
            //create marker from item type
            int itemType = 0;
            int.TryParse(component.GetParamValueAsStringByName($"ItemType_{i}"), out itemType);
            GameObject marker = null;
            
            switch ((LifelineItem.ItemType)itemType)
            {
                case LifelineItem.ItemType.Branchline:
                    marker = Instantiate(_globalCableData.branchLinePrefab, IndividualMarkerContainer);
                    break;
                case LifelineItem.ItemType.Directional:
                    marker = Instantiate(_globalCableData.directionPrefab, IndividualMarkerContainer);
                    break;
                case LifelineItem.ItemType.Mandoor:
                    marker = Instantiate(_globalCableData.manDoorPrefab, IndividualMarkerContainer);
                    break;
                case LifelineItem.ItemType.Refuge:
                    marker = Instantiate(_globalCableData.refugeChamberPrefab, IndividualMarkerContainer);
                    break;
                case LifelineItem.ItemType.SCSRCache:
                    marker = Instantiate(_globalCableData.SCSRCachePrefab, IndividualMarkerContainer);
                    break;
                case LifelineItem.ItemType.Tag:
                    marker = Instantiate(_globalCableData.tagPrefab, IndividualMarkerContainer);
                    break;
            }

            if (marker)
            {
                LifelineItem item = marker.GetComponent<LifelineItem>();

                //Set basic values
                int.TryParse(component.GetParamValueAsStringByName($"PositionIndex_{i}"), out item.ClosestPointIndex);
                item.LifeLineGenRef = Component;
                int closestPointIndex = item.ClosestPointIndex;



                //Set Tag Color
                if (item.itemType == LifelineItem.ItemType.Tag) 
                {
                    int tagColor = 0;
                    LifelineTag tag = item as LifelineTag;
                    int.TryParse(component.GetParamValueAsStringByName($"TagColor_{i}"), out tagColor);
                    ChangeTagColor(tag, tagColor);
                    ColorIndex = tagColor;

                    if (!Tags.Contains(item)) { Tags.Add(item); }
                }

                //Set Directional
                if(item.itemType == LifelineItem.ItemType.Directional)
                {
                    if (!DirectionalMarkers.Contains(item)) { DirectionalMarkers.Add(item); }
                }

                //Set flip geometry
                bool.TryParse(component.GetParamValueAsStringByName($"FlippedDirection_{i}"), out item.flippedDirection);
                if (item.flippedDirection)
                {
                    FlipMarkerGeometery(item);
                }

                //Add to List/Dictionary
                lifelineItems.Add(item);
                MarkerPointDictionary.Add(marker.transform, item.ClosestPointIndex);

                SetMarkerPosition(marker.transform, item.ClosestPointIndex);
                SetMarkerRotation(marker.transform, item.ClosestPointIndex);
                //Set position and rotation
                //IEnumerator coroutine;
                //coroutine = PositionMarkers(marker.transform, item.ClosestPointIndex);
                //StartCoroutine(coroutine);
                //Debug.Log("Loaded marker to index point : " + i);

            }
        }
    }

    //IEnumerator PositionMarkers(Transform xfm, int i)
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(1);
    //        _globalCableData.SetMarkerPosition(xfm, i, this);
    //        _globalCableData.SetMarkerRotation(xfm, i , this);
    //    }
    //}

    public void SetMarkerSpacing(string value_s)
    {
        float value_f = 0;
        float.TryParse(value_s, out value_f);
        if (IsMetric) { _markerSpacing = value_f; }
        // convert from feet to meters
        else
        {
            _markerSpacing = value_f * .3048f; 
        }

    }

    public float GetMarkerSpacing()
    {
        if (IsMetric) { return _markerSpacing; }
        //convert from meters to feet
        else
        {
            return _markerSpacing / .3048f;
        }
    }

    /// orient all markers according to thier sign flip  
    /// FixMe: this causes flip direction to break. Could be redundant code somewhere. Disabling for now
    /*
    public void InitializeMarkerDirection()
    {
        foreach(LifelineItem item in lifelineItems)
        {
           // if (item.flippedDirection) { FlipMarkerGeometery(item); }
           
        }
    }*/

    public void FlipMarker(LifelineItem marker)
    {
        marker.flippedDirection = !marker.flippedDirection;
        FlipMarkerGeometery(marker);
    }

    public void FlipMarkerGeometery(LifelineItem marker)
    {
        Transform geo = marker.transform.Find("Geometry");
        if (geo != null)
        {
            geo.Rotate(new Vector3(0, 180, 0), Space.Self);
        }
    }
    public void FlipAllTags()
    {
        TagsFlipped = !TagsFlipped;

        foreach (LifelineItem tag in Tags)
        {
            if(tag.flippedDirection != TagsFlipped) FlipMarker(tag);
        }
    }
    public void FlipAllDirectionalMarkers()
    {
        DirFlipped = !DirFlipped;

        foreach (LifelineItem marker in DirectionalMarkers)
        {
            if (marker.flippedDirection != DirFlipped) FlipMarker(marker);
        }
    }

    public void ChangeAllTagColors(int index)
    {
        Debug.Log("Change all Tag Colors");
        foreach (LifelineItem item in Tags)
        {
            LifelineTag tag = item as LifelineTag;
            ChangeTagColor(tag, index);
        }
    }

    //change specific tag color
    public void ChangeTagColor(LifelineTag tag, int index)
    {
        if (tag == null)
            return;
        if (index == 0)
        {
            tag.UpdateTagColor(TagColors.DefaultColorData.Color, index);
        }
        else
        {
            tag.UpdateTagColor(TagColors.IndexedColors[index - 1].Color, index);
        }
    }

    public void SetMarkerPosition(Transform t, int i)
    {

        if (i < 0) 
            i = 0;
        else if (i > Cable.GetSmoothedPoints().Count - 1) 
            i = Cable.GetSmoothedPoints().Count - 1;

        //t.position = data.transform.TransformPoint(data.cable.GetSmoothedPoints()[i]);
        t.position = IndividualMarkerContainer.TransformPoint(Cable.GetSmoothedPoints()[i]);
        //markerGizmo.transform.position = t.position;
    }
    //called from Gizmo Button


    public void SetMarkerRotation(Transform t, int i)
    {
        Vector3 direction;

        //if (i < 0) { i = 0; }
        //else if (i > Cable.GetSmoothedPoints().Count - 1) { i = Cable.GetSmoothedPoints().Count - 1; }

        //if (i < Cable.GetSmoothedPoints().Count - 1)
        //{
        //    direction = Cable.GetSmoothedPoints()[i + 1] - Cable.GetSmoothedPoints()[i];
        //}
        //else
        //{
        //    direction = Cable.GetSmoothedPoints()[i - 1] - Cable.GetSmoothedPoints()[i];
        //}

        var pt1 = Cable.GetSmoothedPoint(i - 1);
        var pt2 = Cable.GetSmoothedPoint(i + 1);
        direction = pt2 - pt1;

        t.rotation = Quaternion.LookRotation(direction, Vector3.up);
        //markerGizmo.transform.rotation = t.rotation;
    }

    public void RepositionMarkersWithCable()
    {
        foreach (KeyValuePair<Transform, int> pair in MarkerPointDictionary)
        {
            SetMarkerRotation(pair.Key, pair.Value);
            SetMarkerPosition(pair.Key, pair.Value);
        }
    }

    public void MoveLifelineItem(LifelineItem item, int newPointIndex)
    {
        item.ClosestPointIndex = newPointIndex;
        MarkerPointDictionary[item.transform] = item.ClosestPointIndex;
        SetMarkerPosition(item.transform, item.ClosestPointIndex);
        SetMarkerRotation(item.transform, item.ClosestPointIndex);
    }


}


