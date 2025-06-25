## Non-serialized fields

**SourceFolder (string):** The folder/subfolder containing the asset files (e.g. %userprofile%/VRMine/ExternalAssets/TestObject1)

**SourceFile (string):** The filename of the metadata file this was loaded from without folder/path (e.g. asset.yaml)

## Basic information

**AssetID (string):** The unique ID for the asset

**AssetName (string):** The name of the asset when displayed to the user

**IconFilename (string):** The filename (jpg or png) of the icon file (wtihout path, should exist inside the SourceFolder)

## Geometry information
**GeometryFilename (string):** The filename of the glTF or glb file (without path, should exist inside the SourceFolder)

**MeshColliderName (string):** The name of the mesh in the geometry file to use for the mesh collider (typically the lowest or near-lowest LOD mesh)

## Material information
Some of this may need to be added to or changed - specifically how meshes with multiple materials should be handled, if we need the ability to override using sets of materials, etc. Dependent on how well changing/assigning materials in the glTF file works, etc.

**MeshMaterialOverrides (dictionary):** List of mesh names and the material ID to use as an override for that mesh's material e.g. RockMesh123: RockMaterial12

**CustomMaterialDefinitions (dictionary):** Definitions for custom materials - key is the material ID, and contents is the associated maps and other properties (ExternalAssetMat struct - exact contents to be expanded on as needed)

## LoadableAsset data

**BasePrefabs (list):** List of string base prefab IDs that the asset can use

**PlacementOptions (LoadablePlacementOptions):** class containing the placement options such as layer, etc.

**PhysicalProperties (LoadablePhysicalProperties):** class containing the physical property overrides such as mass, etc. Negative or missing values should be ignored.