using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MappingMappableItem : MonoBehaviour
{
    public MappingVisualHandler MineMap;
    public string ItemName;
    public Sprite ItemMapSprite;

    private bool _isMappable = true;
    
    public void OnMap()
    {
        if (_isMappable)
        {
            //MineMap.AddPointOfInterest(transform.position, ItemName, ItemMapSprite);
            _isMappable = false;
        }
    }
}
