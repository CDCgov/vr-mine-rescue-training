# VR-MRT
## Overview
VR-MRT is a training platform designed to supplement current mine rescue training. VR-MRT is intended to improve mine rescue team members' procedural, collaborative, and problem-solving skills for an underground emergency response. VR-MRT consists of five modules:

* Scenario Editor: Allows users to create, modify and share mine rescue scenarios. Using a desktop computer setup, users can place the objects in the scene, set up the ventilation, and change properties.
* Simulation Module: Allows users to work through the NIOSH provided or user created scenarios. Users will join using head mounted displays (HMDs) either remotely or co-located in the same space.
* Director Module: Allows the trainer to set up, record, monitor, and control the simulation. Using a desktop computer setup, the trainer can change users' settings (e.g., roles, handedness), load scenarios, move the team around the mine (i.e., teleport), and provide them with additional items.
* Spectator Module: Allows additional people to view what is going on in a simulation. Using a desktop computer setup, this module includes an overhead view as well as a chase camera view following the captain.
* Debrief Module: Allows users to play back the log file that was collected in the director module for an after-action review of the simulation.
  
## External Dependencies 

### Main Project
Purchase and import the following packages to restore project's functionality:

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
* Link to texture pack: [Here](https://centersfordiseasecontrol.sharefile.com/d-s01688ced18ab4e0082c9d239f1786cf8)

### Other Licenses
* BAH - License Needed for Miner Avatar, DO NOT DELETE [BAH License](/External/BAH/license.txt)
* [AsyncIO](/Packages/VRMineExternal/Runtime/Plugins/AsyncIOLICENSE.md)
* [CsvHelper](/Packages/VRMineExternal/Runtime/Plugins/CsvHelper-License-Apache-2.0.mhtml)
* [Delaunator](/Packages/Delaunator/LICENSE.md)
* [EzySlice](/Licenses/EzySlice.license)
* [geometry3sharp](/Licenses/geometry3sharp.license)
* [glTFast](/Packages/glTFast/LICENSE.md)
* [Google Protobuf](/Packages/VRMineExternal/Runtime/Plugins/Google.ProtobufLICENSE.md)
* [KDTree](/Packages/VRMineExternal/Runtime/KDTree/LICENSE)
* [MathNet Numerics](/Packages/VRMineExternal/Runtime/Plugins/MathNetNumericsLICENSE.md)
* [Microsoft-BCL-AsyncInterfaces](/Packages/VRMineExternal/Runtime/Plugins/Microsoft-BCL-AsyncInterfaces-License.txt)
* [MonitoredUndo](/Packages/VRMineExternal/Runtime/Plugins/MonitoredUndolicense.md)
* [Protobuf Net](/Packages/VRMineExternal/Runtime/Plugins/protobuf-net-licence.txt)
* [OpusDotNet](/Licenses/OpusDotNet.license.md)
* [Unity Octree](/Packages/VRMineExternal/Runtime/UnityOctree/LICENSE)
* [Yaml Dot Net](/Packages/VRMineExternal/Runtime/Plugins/YamlDotNetLICENSE.txt)
* [zlib](/Packages/VRMineExternal/Runtime/Plugins/zlib%20License.html)

## Public Domain Standard Notice

This repository constitutes a work of the United States Government and is not
subject to domestic copyright protection under 17 USC § 105. This repository is in
the public domain within the United States, and copyright and related rights in
the work worldwide are waived through the [CC0 1.0 Universal public domain dedication](https://creativecommons.org/publicdomain/zero/1.0/).
All contributions to this repository will be released under the CC0 dedication. By
submitting a pull request you are agreeing to comply with this waiver of
copyright interest.

## License Standard Notice

The repository utilizes code licensed under the terms of the Apache Software
License and therefore is licensed under ASL v2 or later.

This source code in this repository is free: you can redistribute it and/or modify it under
the terms of the Apache Software License version 2, or (at your option) any
later version.

This source code in this repository is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the Apache Software License for more details.

You should have received a copy of the Apache Software License along with this
program. If not, see (<http://www.apache.org/licenses/LICENSE-2.0.html>)

The source code forked from other open source projects will inherit its license.

## Privacy Standard Notice

This repository contains only non-sensitive, publicly available data and
information. All material and community participation is covered by the
[Disclaimer](https://github.com/CDCgov/template/blob/master/DISCLAIMER.md)
and [Code of Conduct](https://github.com/CDCgov/template/blob/master/code-of-conduct.md).
For more information about CDC's privacy policy, please visit [http://www.cdc.gov/other/privacy.html](https://www.cdc.gov/other/privacy.html).

## Contributing Standard Notice

Anyone is encouraged to contribute to the repository by [forking](https://help.github.com/articles/fork-a-repo)
and submitting a pull request. (If you are new to GitHub, you might start with a
[basic tutorial](https://help.github.com/articles/set-up-git).) By contributing
to this project, you grant a world-wide, royalty-free, perpetual, irrevocable,
non-exclusive, transferable license to all users under the terms of the
[Apache Software License v2](http://www.apache.org/licenses/LICENSE-2.0.html) or
later.

All comments, messages, pull requests, and other submissions received through
CDC including this GitHub page may be subject to applicable federal law, including but not limited to the Federal Records Act, and may be archived. Learn more at [http://www.cdc.gov/other/privacy.html](http://www.cdc.gov/other/privacy.html).

## Records Management Standard Notice

This repository is not a source of government records, but is a copy to increase
collaboration and collaborative potential. All government records will be
published through the [CDC web site](http://www.cdc.gov).

## Additional Standard Notices

Please refer to [CDC's Template Repository](https://github.com/CDCgov/template)
for more information about [contributing to this repository](https://github.com/CDCgov/template/blob/master/CONTRIBUTING.md),
[public domain notices and disclaimers](https://github.com/CDCgov/template/blob/master/DISCLAIMER.md),
and [code of conduct](https://github.com/CDCgov/template/blob/master/code-of-conduct.md).
