using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DebriefMarkers", menuName = "VRMine/DebriefMarkers", order = 1)]
public class DebriefMarkers : ScriptableObject
{
    [System.Serializable]
    public class DebriefMarkerData
    {
        public string Description;

        //public VRNLogEventType AssociatedEvent;
        public VRNLogEventType[] AssociatedEvents;

        public Sprite MarkerSprite;
        public SoundEffectCollection SoundEffect;

        [System.NonSerialized]
        public int EventCategoryIndex;
        [System.NonSerialized]
        public bool CategoryVisible = true;
    }


    [FormerlySerializedAs("DebriefMarkerList")]
    public List<DebriefMarkerData> EventCategoryData;

    public Sprite[] Markers;
    public string[] MarkerDescriptions;

    private Dictionary<VRNLogEventType, DebriefMarkerData> _markerMap;

    public DebriefMarkerData GetMarkerData(VRNLogEventType eventType)
    {
        if (_markerMap == null)
            BuildMap();

        DebriefMarkerData data = null;
        if (_markerMap.TryGetValue(eventType, out data))
            return data;

        return null;
    }

    private void BuildMap()
    {
        _markerMap = new Dictionary<VRNLogEventType, DebriefMarkerData>();

        if (EventCategoryData == null)
            return;

        //foreach (var data in EventCategoryData)
        for (int i = 0; i < EventCategoryData.Count; i++)
        {
            var data = EventCategoryData[i];
            data.EventCategoryIndex = i;

            foreach (var ev in data.AssociatedEvents)
                _markerMap[ev] = data;
        }
    }
}