# WayFindingSamplesUsingASA
this Sample is realize the 'way-finding' demo using Azure Spatial Anchors.

## Know-how to realize 'Way Finding' that is used Azure Spatial Anchors.

In order to help you understand and practice the session content of de:code 2020, we will introduce a technique to realize the use cases of Azure Spatial Anchors, "Way-Finding".

![Demo](./images/WayfindingUsingASA.gif)

This application provide the below contents using Azure Spatial Anchors.

* First step,this application create the base spatial anchor using Azure Spatial Anchors,and saved Anchor ID in local strage.
   * After the second time, this applicaiton get the base spatial anchor using saved anchor ID.
* Second step, register the spatial anchors to set the route.
   * we can set the points and the destination.
   * Do placement several points to the destination.The distance must be within 5m among the points.(search range is defult 5m)
   * we can set the name to the destination.
   * we can set multi route, but you need to restart the application.
*  Try to follow the route to your destination.
   * After doing the base spatial anchor, this application display the destination lists.
   * Select the destination you want to go, search next anchor nearby the base point and display it.
   * When you get close to the displayed anchor, the applications search next anchor.
   * Repeat do process over until the destination

### Development Environment

This Sample is confimred the below enviromnents.

#### Hardware
* PC
  * Windows 10 Pro(OS Version 20H2)
* HoloLens 2(HoloLens is available too)

#### Software
* [Unity 2019.4.X LTS](https://unity3d.com/jp/get-unity/download/archive)(this sample is used Unity 2019.4.19f1)
* [Visual Studio 2019(16.5.3)](https://visualstudio.microsoft.com/ja/downloads/)
* Azure Spatial anchors 2.8.1(set by Mixed Reality Feature Tool)
* [Mixed Reality Toolkit 2.3.0](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.3.0)
* [Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778)

### advance preparation
#### Basic understainding about Azure Spatial Anchors
This sample process is almost the same as the quick start of Azure Spatial Anchors.The prerequisites and how to create a Spatial Anchors resource is the below site as needed.
[Quickstart: Create a Unity HoloLens app that uses Azure Spatial Anchors](https://docs.microsoft.com/en-us/azure/spatial-anchors/quickstarts/get-started-unity-hololens?tabs=azure-portal)

#### About downloading the modules
This sample is used the modules and softwares of 'Software' section.
Please download the modules and softwares from Web site.

Azure Spatial Anchors SDK set to the sample using [Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778).
Please download tha below web site, and unzip the download file.

* [(Official)Welcome to the Mixed Reality Feature Tool](https://docs.microsoft.com/en-US/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool)

### How to deploy the application

1. Clone this ripogitory.
2. Set Azure Spatial Anchors SDK from [Mixed Reality Feature Tool](https://www.microsoft.com/en-us/download/details.aspx?id=102778)(MRTK can set from Mixed Reality Feature Tool, but we don't set MRTK using this tool in this time. This reason is that it can't select this sample required version.)
    1. Execute MixedRealityFeatureTool.exe
    2. Click [Start]
    3. In [Select project] section, select path [WayFindingSamplesUsingASA\Unity] from the cloned repogitory in Seq.1 and click [Discover Features].
    4. In [Discover features] section, check off the below components and click [Get Features]
        * Azure Spatial Anchors SDK for Android 2.8.1
        * Azure Spatial Anchors SDK Core 2.8.1
        * Azure Spatial Anchors SDK for iOS 2.8.1
        * Azure Spatial Anchors SDK for Windows 2.8.1
    5. In [Import features] section, you confirm the selected component as above four components and click [Import]
    6. In [Review and Approve] section, click [Approve]
    7. Click [Exit], so exit the tool.
2. Start Unity engine, and open the sample project WayFindingSamplesUsingASA\Unity.
3. Select [Asset]-[Import Package]-[Custom Package], and import [Microsoft.MixedReality.Toolkit.Unity.Foundation.2.3.0.unitypackage](https://github.com/microsoft/MixedRealityToolkit-Unity/releases/tag/v2.3.0)
4. Open [MRTK Project Configurator] dialog, and check off all and apply.
5. Open the sample scene(WayFindingSamplesUsingASA\Unity\Assets\ASA.Samples.WayFindings\Scenes\SampleScene.unity)
6. Select [AzureSpatialAnchors.SDK\Resources\SpatialAnchorConfig] in [Project] panel, and set the Azure Spatial Anchors Credentials in [Inspector] panel.
7. Open [File]-[Build Settings], and set the below items.
    * Set Platform to Universal Windows Platform.
    * Open [MRTK Project Configurator] dialog, and check off all and apply.
    * Open [Edit]-[Project Settings], set the below items in [Player] category
       * Check off [Virtual Reality Supported] in [XR Settings]
       * Add [Windows Mixed Reality SDK] in [XR Settings]-[Virtual Reality SDKs], and change 16bit to [Depth Format]
       * Check off the below items in [Publish Settings]-[Capabilities]
           * InternetClient
           * InternetClientServer
           * RemovableStorage
           * SpatialPerception 
8. Execute the build, and select any folder.
9. After Unity build, open build resouces in Visual studio and deploy to Hololens.

### Operaions the application

Execute the deployed application in Hololens, oparate the below steps.

    1. Place the base anchor(first placement anchor is used start point.)
        1. First time execution.
            1. Do placement the cube of orange color to the base point that you want.
            2. push or select 'Start Azure Sesion'
            3. push or select 'Create Azure Anchor', if this step is succeeded, this application saved Anchor ID in local strage.
        2. After the second time
            1. This applications load Anchor ID in local strage.
            2. push or select 'Start Azure Sesion'
            3. push or select 'Find Azure Sesion'
        3. If the spatial anchor placement is suceeded, you can select next two steps.
    2. Set the route.
        1. Set the route and the destination. About route nodes, do placement the cube of blue color to the point that you want, and push or select 'Create Azure Anchor'(repeat this step over until nearby the destination).
        2. Finally set the destination. push or select 'Set Destination'. Display input text field, so you set the any destination name.
        3. push or select 'Create Anchor', register the spatial anchor. Then the application set the destination information to spatial anchors.
    3. Way finding
        1. Display the destination list menu, push or select the destination you want to go.
        2. the applications search next anchor nearby the base point. If search anchor is succeeded, visualize the next anchor.
        3. When you get close to the visualized anchor, the applications search next anchor.
        4. Repeat this step.3 over until the destination 
        
<div style="text-align: center;">
© 2020 Takahiro Miyaura All rights reserved.<br />
The copyright of this content, as well as trademarks, organization names, logos, products, and services appearing in this content, belong to their respective rights holders.
</div>
