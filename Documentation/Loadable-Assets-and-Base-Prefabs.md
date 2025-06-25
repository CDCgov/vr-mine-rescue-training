# Overview
Assets that can be loaded by the scenario editor consist of a collection of scriptable objects containing the IDs, prefab references, and other data. These data classes are used to locate and instantiate the correct prefabs when loading custom scenarios or adding objects to scenarios. 

# Creating a new object using base prefabs

## Create Geometry Object
The first step is to create a geometry prefab that contains the geometry, material references, and colliders for the object. Currently mesh colliders should not be used unless the object is to be static only. Additional ModularComponentInfo derived classes e.g. ComponentInfo_Light may be present for objects that should have adjustable settings (e.g. lights) (note: this needs further testing) 

The pivot of the geometry prefab should be located at the correct position for the object. If necessary this can be adjusted by having the geometry as a child object to the prefab root object. 

In some cases the geometry prefab can be the imported FBX. 

## Create LoadableAsset object

Create a LoadableAsset object in the editor (Create -> Scenario Editor -> LoadableAsset) and name it to match the object name. Set a unique, short, and descriptive AssetID. Leave the EditorObject and SceneObject set to none. Set the geometry object to the one you just created. Add one or more LoadableBasePrefabData references to the list of base prefabs. Note the first one in the list will be the default - use the same ordering as any similar objects that exist e.g. 1) StaticBasePrefab 2) InteractableBasePrefab. 

Set the AssetWindowName, Icon, tooltip, editor layer, etc. as appropriate for the object. 

## Add LoadableAsset to LoadableAssetCollection

In the LoadableAssetCollection, add a new entry to the list and assign the LoadableAsset you just created.

## LoadableVariantSet

If the object is part of rotation/variant set, a new set can be created via Create -> Scenario Editor -> Loadable Variant Set. It should then be added to that set (or an existing set's) list of LoadableAssets. The LoadableVariantSet should be added the the LoadableAssetCollection's list of LoadableVariantSets.

## Must-have components
- **Collider:**  Must be on prefab. Must not be on a LOD
- ...

# Creating a new base prefab 

Base prefabs consist of a LoadableBasePrefabData object, a scenario editor prefab, and a scene/runtime prefab. Ideally the scene prefab should be a variant of the editor prefab, or vice versa (whichever is more convenient for a given object). Both prefabs should have any component info scripts needed to store custom settings. The scenario editor prefab should not have any runtime scripts (NetworkedObject, etc.) and should have it's rigidbody set to kinematic. The prefabs should be placed in Assets/Prefabs/LoadableAssets/BasePrefabs.

The LoadableBasePrefabData object can be created via Create -> Scenario Editor -> LoadableBasePrefabData. It should be placed in Assets/Prefabs/LoadableAssets/BasePrefabData. The prefab should be given a unique, short, and descriptive name for its PrefabID. The display name should be set to what is shown to the user, and the EditorBaseObject and SceneBaseObject should be set to the prefabs you just created. 

Once created it can be added to the list of base prefabs for any appropriate LoadableAsset.
 
# Loadable Asset Components and Classes

The assets loadable by the scenario editor consist of multiple objects and components:

* **LoadableAsset** Scriptable Object
  * One of these should exist for each object that can be loaded into the scenario editor
  * Contains all information needed to instantiate the object:
    * **AssetID** The unique string ID for the object e.g. FIRE_EXTINGUISHER. Note this is what is serialized into the saved scenarios and should not be changed. It should also not be serialized onto any other prefabs e.g. ObjectInfo receives 
    * **EditorObject** If a custom object (not using base prefabs) the prefab to use in the scenario editor
    * **SceneObject** If a custom object the prefab to use in a real scenario
    * **GeometryObject** For base prefab objects, the prefab containing the geometry and potentially some components such as lights
    * **BasePrefabs** A list of all allowed base prefabs (list of LoadableBasePrefabData objects)
    * AssetWindowName, icon, editor layer, and tooltip
  * In the future will likely contain common object properties such as the sound/collision material 

* **LoadableVariantSet** Scriptable Object
  * Currently used for rotatable tile sets, contains a list of LoadableAssets that should appear as a single item in the asset window
  * Order of prefabs in list determines rotation order/sequence 

* **LoadableBasePrefabData** Scriptable Object
  * Contains the information needed to identify a base prefab
    * **PrefabID** The unique string ID of the base prefab
    * **DisplayName** The name shown to the user when selecting a base prefab
    * **EditorBaseObject** The base object used when instantiating the prefab in the scenario editor
    * **SceneBaseObject** The base object used when instantiating the prefab in the actual scenario 

* **ObjectInfo** component
  * This component must exist on any base prefab or custom prefab (scenario editor & scene)
  * Is created by the scenario editor load process, has a runtime copy of the asset ID and prefab ID but these are not serialized or stored in the prefab
  * As it exists in both the scenario editor and running scenarios, can contain any object specific data that is not needed for instantiation but is needed at runtime

* **PlacablePrefab** component
  * Must exist on all scenario editor base prefabs / editor objects. Should not exist on scene base prefabs / objects. 
  * Should be used to contain scenario editor only options that are not needed at runtime
  * Will be added with default values if not present on the prefab

* **LoadableAssetCollection** Scriptable Object
  * Currently used to store the main list of objects available in the editor
* **LoadableAssetManager** Scriptable Object Manager
  * The manager responsible for instantiating loadable assets
* **SavedAsset** Serializable Class
  * The serialized data stored in the scenario file for each object

# How objects are created

The general flow for how an object is created in the scenario editor & scenes is as follows: 

* the LoadableAssetManager is given an AssetID either directly or from a SavedAsset
* The manager finds the LoadableAsset data using the AssetID currently by searching the Assets/Prefabs/LoadableAssets/Objects/LoadableAssetCollection list of objects
* The object is instantiated depending on what properties are set on the loadable assets:
  * Instantiates the scenario editor or scene prefab if set
  * Instantiates the base prefab and then instantiates and parents the geometry prefab to it.
* If a saved asset is used, the data is then restored to the components

