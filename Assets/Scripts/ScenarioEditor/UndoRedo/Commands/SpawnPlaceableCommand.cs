using NIOSH_EditorLayers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NIOSH_EditorLayers.LayerManager;

public class SpawnPlaceableCommand : ICommand
{
    Transform assetContainer;
    Vector3 spawnedLocation;
    Quaternion spawnedRotation;
    Vector3 spawnedScale;
    PlacablePrefab placeable;
    HierarchyContainer container;

    public SpawnPlaceableCommand(PlacablePrefab place, Transform container, Vector3 location, Quaternion rotation, Vector3 scale)
    {
        placeable = place;
        assetContainer = container;
        spawnedLocation = location;
        spawnedRotation = rotation;
        spawnedScale = scale;
    }

    public SpawnPlaceableCommand(GameObject obj, Vector3 location, Quaternion rotation, Vector3 scale)
    {
        placeable = obj.GetComponent<PlacablePrefab>();
        Debug.Log(placeable + " IS OUR PLACeABLE");
        if (placeable == null) { Debug.LogError("SPAWNED SOMETHING WITHOUT A PLACEABLE PREFAB"); return; }
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
        ActivateObject();
    }

    public void UnExecute()
    {
        if(placeable.gameObject != null)
        {
            placeable.gameObject.SetActive(false);
        }
        DeactivateObject();
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
        scenePlacer.SelectObject(placeable.gameObject);
        if (container != null)
        {
            //container.CreateItemFromPlacedObject(placeable.gameObject, 0, true);
            //info.HierarchyItem.InitializeFromPlacedObject(placeable.gameObject, container);
        }
        placeable.gameObject.transform.localPosition = spawnedLocation;
        placeable.gameObject.transform.localRotation = spawnedRotation;
        placeable.gameObject.transform.localScale = spawnedScale;
    }
}
