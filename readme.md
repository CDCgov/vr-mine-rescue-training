# VR-MRT

## External Dependencies 

### Main Project
Purchase and import the following packages to restore original project as originally committed:

* BAH - DO NOT DELETE, Needed for Miner Avatar [BAH License](/External/BAH/license.txt)
* EasyButtons - Used in BAH editor buttons [Easy Buttons](https://github.com/madsbangh/EasyButtons)
* CamelotVFX_Fire&Smoke - [Camelot](https://assetstore.unity.com/packages/vfx/particles/fire-explosions/camelotvfx-fire-smoke-11105)
* Cross Platform Input - Deprecated, not currently used due to HMD focus. [CI Blog Post](https://discussions.unity.com/t/crossplatforminput-deprecated/707231/3)
* geometry3sharp - [geometry3sharp](https://github.com/gradientspace/geometry3Sharp)
* GoogleMatDesignIcons - [Google Assets](https://developers.google.com/fonts/faq)
* Lens Flares - Unity Standard assets, deprecated
* MGS-Machinery - [MGS Machinery](https://github.com/mogoson/MGS.Machinery)
* Skybox - Deprecated, Legacy Unity skybox, not available on asset store, replace HDRI sky field in the HDRP scene properties
* StandaloneFileBrowser - [StandaloneFileBrowser](https://github.com/gkngkc/UnityStandaloneFileBrowser)
* TextMesh Pro - Legacy, now in Packages
* Vehicle - Unity Standard Assets, legacy, deprecated. Investigate replacement to restore Vehicle functionality of older VR Mine builds. Not needed for current scenario editor work - [Standard Assets Blog Post](https://assetstore.unity.com/packages/essentials/asset-packs/standard-assets-2018-4-check-out-starter-assets-first-person-thi-32351)
* FinalIK - Plugin Needed for 3rd Person player avatars. Must be purchased through the Unity Asset Store. [FinalIK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290)


### Packages Needed in Packages/VRMineExternal

* Flashlight asset in VRMineExternal/Runtime - [Flashlight](https://assetstore.unity.com/packages/3d/props/electronics/flashlight-18972)

### Plugins needed in Packages/VRMineExternal/Runtime/Plugins

* Microsoft Plugins:
  * System.Buffers
  * System.Collections.Immutable
  * System.Memory
  * System.Numerics.Vectors
  * System.Runtime.CompilerServices.Unsafe
  * System.ServiceModel.Primitives
  * Microsoft.Bcl.AsyncInterfaces

### Textures
* Before launching Unity, download and extract the VR-MRT Texture Pack into the Unity project "Assets" folder, it matches the folder structure of the Unity project and will place the large texture files into the VR-MRT 