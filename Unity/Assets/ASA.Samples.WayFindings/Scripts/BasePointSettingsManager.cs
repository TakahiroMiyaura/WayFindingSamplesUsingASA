// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.IO;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#if WINDOWS_UWP
using Windows.Storage;
#endif

public class BasePointSettingsManager : MonoBehaviour, IASACallBackManager
{
    public GameObject AnchorObject;
    private IDictionary<string, string> appProperties;
    private SimpleDialog dialog;

    public SimpleDialog Dialog;
    private bool isLocateAnchors;
    private GameObject nextStepButtons;
    public GameObject Operations;
    public string BasePointAnchorId { get; private set; }


    private void Start()
    {
        AnchorModuleProxy.Instance.SetASACallBackManager(this);
        dialog = Instantiate(Dialog);
        if (!GetAzureAnchorIdFromDisk())
        {
            dialog.SetDialog("Not load Anchor Id on this device.",
                "Please set object and create base point.First, open session.", new[] {"Ok"},
                new[] {new UnityAction(() =>
                {
                    IsContentsStart(true);
                    AnchorObject.SetActive(true);
                })});
        }
        else
        {
            dialog.SetDialog("Anchor Id is loaded.", "Please open session and find Azure Anchor.", new[] {"Ok"},
                new[] { new UnityAction(() =>
                {
                    IsContentsStart(true);
                    AnchorObject.SetActive(false);
                })});
            
        }

        nextStepButtons = Operations.transform.GetChild(1).gameObject;
        nextStepButtons.SetActive(false);

    }

    private void Update()
    {
        nextStepButtons.SetActive(isLocateAnchors);
    }

    public async void StartAzureSession()
    {
        await AnchorModuleProxy.Instance.StartAzureSession();
    }

    public async void StopAzureSession()
    {
        await AnchorModuleProxy.Instance.StopAzureSession();
    }

    private void SaveAzureAnchorIdToDisk()
    {
        Debug.Log("\nAnchorModuleScript.SaveAzureAnchorIDToDisk()");

        var filename = "SavedAzureAnchorID.txt";
        var path = Application.persistentDataPath;

#if WINDOWS_UWP
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        path = storageFolder.Path.Replace('\\', '/') + "/";
#endif

        var filePath = Path.Combine(path, filename);
        File.WriteAllText(filePath, BasePointAnchorId);

        Debug.Log($"Current Azure anchor ID '{BasePointAnchorId}' successfully saved to path '{filePath}'");
    }

    private bool GetAzureAnchorIdFromDisk()
    {
        Debug.Log("\nAnchorModuleScript.LoadAzureAnchorIDFromDisk()");

        var filename = "SavedAzureAnchorID.txt";
        var path = Application.persistentDataPath;

#if WINDOWS_UWP
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        path = storageFolder.Path.Replace('\\', '/') + "/";
#endif

        var filePath = Path.Combine(path, filename);
        if (File.Exists(filePath))
        {
            BasePointAnchorId = File.ReadAllText(filePath);
            Debug.Log(
                $"Current Azure anchor ID successfully updated with saved Azure anchor ID '{BasePointAnchorId}' from path '{path}'");
            return true;
        }

        Debug.Log($"File Not Founded.'{filePath}'");
        return false;
    }

    /// <summary>
    /// Locate the Anchor that set  Destination property.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="appProperties"></param>
    /// <returns></returns>
    public GameObject OnLocatedAnchorObject(string identifier,
        IDictionary<string, string> appProperties)
    {
        this.appProperties = appProperties;
        this.BasePointAnchorId = identifier;

        AnchorObject.SetActive(true);
        DisabledAnchorEffects(AnchorObject);
        AnchorObject.name = "Anchor:" + identifier;

        isLocateAnchors = true;

        return AnchorObject;
    }

    public void OnLocatedAnchorComplete()
    {
    }

    public void FindAzureAnchor()
    {

        AnchorModuleProxy.Instance.FindAzureAnchorById(BasePointAnchorId);

    }

    public async void CreateLocalCloudAnchor()
    {

        // Then we create a new local cloud anchor
        appProperties = new Dictionary<string, string>();

        appProperties.Add(RouteGuideInformation.ANCHOR_TYPE, RouteGuideInformation.ANCHOR_TYPE_POINT);
        appProperties.Add(RouteGuideInformation.PREV_ANCHOR_ID, RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED);

        BasePointAnchorId = await AnchorModuleProxy.Instance.CreateAzureAnchor(AnchorObject, appProperties);
        SaveAzureAnchorIdToDisk();

        DisabledAnchorEffects(AnchorObject);

        isLocateAnchors = true;
    }

    private static void DisabledAnchorEffects(GameObject theObject)
    {
        theObject.GetComponent<ManipulationHandler>().enabled = false;
        theObject.GetComponent<NearInteractionGrabbable>().enabled = false;
        theObject.GetComponentInChildren<HandInteractionHint>()?.gameObject.SetActive(false);
        theObject.GetComponentInChildren<ArrowAnimte>()?.transform.parent.gameObject.SetActive(false);
        theObject.GetComponentInChildren<ToolTip>()?.transform.parent.gameObject.SetActive(false);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
    }

    public void NextStepRouteCreate()
    {
        IsContentsStart(false);
        SetRouteGuideManager.Instance.IsContentsStart(true, BasePointAnchorId, AnchorObject, appProperties);
    }

    public void NextStepWayFinding()
    {
        IsContentsStart(false);
        WayFindingManager.Instance.IsContentsStart(true, BasePointAnchorId, AnchorObject, appProperties);
    }

    public void IsContentsStart(bool enabled)
    {
        transform.GetChild(0).gameObject.SetActive(enabled);
        transform.GetChild(1).gameObject.SetActive(enabled);

    }


}