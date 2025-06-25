using NIOSH_EditorLayers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCommand : ICommand
{
    Transform assetContainer;
    Vector3 spawnedLocation;
    Vector3 spawnedScale;
    Quaternion spawnedRotation;
    PlacablePrefab placeable;
    //HierarchyContainer container;

    public DestroyCommand(PlacablePrefab place, Transform asset, Vector3 location, Quaternion rotation, Vector3 scale)
    {
        placeable = place;
        assetContainer = asset;
        spawnedLocation = location;
        spawnedRotation = rotation;
        spawnedScale = scale;
        //container = placeable.GetObjectInfo().HierarchyItem.lastContainer;
    }

    public DestroyCommand(GameObject obj, Vector3 location, Quaternion rotation, Vector3 scale)
    {
        placeable = obj.GetComponent<PlacablePrefab>();
        if(placeable == null ) { Debug.LogError("DELETED SOMETHING WITHOUT A PLACEABLE PREFAB"); return; }

        //if(placeable.GetObjectInfo().HierarchyItem.lastContainer != null)
        //{
        //    container = placeable.GetObjectInfo().HierarchyItem.lastContainer;
        //}
        assetContainer = GameObject.Find("Assets").transform;
        if (assetContainer == null) { Debug.LogError("NO ASSET CONTAINER FOUND"); return; }
        spawnedLocation = location;
        spawnedRotation = rotation;
        spawnedScale = scale;
    }

    public void Execute()
    {
        if (placeable.gameObject != null)
        {
            placeable.gameObject.SetActive(true);
        }
        DeactivateObject();
    }

    public void UnExecute()
    {
        if (placeable.gameObject != null)
        {
            placeable.gameObject.SetActive(false);
        }
        ActivateObject();
    }

    void DeactivateObject()
    {

        placeable.SetIgnoreSave(true);
        Placer scenePlacer = GameObject.FindObjectOfType<Placer>();
        //scenePlacer.DeselectSpecificGameObject(placeable.gameObject);
        scenePlacer.DeselectObject();
        //if (placeable.GetObjectInfo().HierarchyItem != null && placeable.GetObjectInfo().HierarchyItem.lastContainer != null)
        //{
        //    container = placeable.GetObjectInfo().HierarchyItem.lastContainer;
        //}
        //if (placeable.GetObjectInfo().HierarchyItem != null)
        //{
        //    placeable.GetObjectInfo().HierarchyItem.StartDestroy();
        //}
        placeable.gameObject.transform.position = Vector3.zero;
    }

    void ActivateObject()
    {
        ObjectInfo info = placeable.GetObjectInfo();

        if (info.editorLayer == LayerManager.EditorLayer.Mine)
        {
            GameObject _simParent = new GameObject();
            _simParent.name = info.DisplayName + "_SimParent: " + info.DisplayName;

            info.simParent = _simParent.AddComponent(typeof(SimulatedParent)) as SimulatedParent;
            info.simParent.transform.parent = assetContainer;
            info.simParent.transform.position = info.transform.position;
            info.simParent.objectInfo = info;

        }
        if (placeable.gameObject.name != "VentilationLayerNode")
        {
            placeable.SetIgnoreSave(false);
        }

        Placer scenePlacer = GameObject.FindObjectOfType<Placer>();
        //scenePlacer.SelectSpecificGameObject(placeable.gameObject);
        scenePlacer.DeselectObject();

        //if (container != null)
        //{
        //    container.CreateItemFromPlacedObject(placeable.gameObject, 0, true);
        //    info.HierarchyItem.InitializeFromPlacedObject(placeable.gameObject, container);
        //}

        placeable.gameObject.transform.localPosition = spawnedLocation;
        placeable.gameObject.transform.localRotation = spawnedRotation;
        placeable.gameObject.transform.localScale = spawnedScale;
    }
}
