// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.WSA;

public class AnchorModuleScript : MonoBehaviour, IAnchorModuleScript
{
    private readonly Queue<Action> dispatchQueue = new Queue<Action>();

    private readonly Dictionary<string, CloudSpatialAnchor> locatedAnchors =
        new Dictionary<string, CloudSpatialAnchor>();

    private AnchorLocateCriteria anchorLocateCriteria;
    private SpatialAnchorManager cloudManager;
    private CloudSpatialAnchorWatcher currentWatcher;

    private float distanceInMeters;
    private int maxResultCount;
    private int expiration;
    public IASACallBackManager CallBackManager { get; set; }

#region Public Events

    public event AnchorModuleProxy.FeedbackDescription OnFeedbackDescription;

#endregion

#region Internal Methods and Coroutines

    private void QueueOnUpdate(Action updateAction)
    {
        lock (dispatchQueue)
        {
            dispatchQueue.Enqueue(updateAction);
        }
    }

#endregion

#region Unity Lifecycle

    public void Start()
    {
        // Get a reference to the SpatialAnchorManager component (must be on the same gameobject)
        cloudManager = GetComponent<SpatialAnchorManager>();

        // Register for Azure Spatial Anchor events
        cloudManager.AnchorLocated += CloudManager_AnchorLocated;
        cloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
        anchorLocateCriteria = new AnchorLocateCriteria();
    }

    public void Update()
    {
        lock (dispatchQueue)
        {
            if (dispatchQueue.Count > 0)
            {
                dispatchQueue.Dequeue()();
            }
        }
    }

    public void OnDestroy()
    {
        if (cloudManager != null && cloudManager.Session != null)
        {
            cloudManager.DestroySession();
        }

        if (currentWatcher != null)
        {
            currentWatcher.Stop();
            currentWatcher = null;
        }
    }

#endregion

#region Public Methods

    public async Task StartAzureSession()
    {
        Debug.Log("\nAnchorModuleScript.StartAzureSession()");

        OutputLog("Starting Azure session... please wait...");

        if (cloudManager.Session == null)
        {
            // Creates a new session if one does not exist
            await cloudManager.CreateSessionAsync();
        }

        // Starts the session if not already started
        await cloudManager.StartSessionAsync();

        OutputLog("Azure session started successfully");
    }

    public void OutputLog(string startSessionLog, LogType logType = LogType.Log, bool isOverWrite = false,
        bool isReset = false)
    {
        OnFeedbackDescription?.Invoke(startSessionLog, isOverWrite, isReset);
        switch (logType)
        {
            case LogType.Log:
                Debug.Log(startSessionLog);
                break;
            case LogType.Error:
                Debug.LogError(startSessionLog);
                break;
            case LogType.Warning:
                Debug.LogError(startSessionLog);
                break;
            default:
                Debug.Log(startSessionLog);
                break;
        }
    }

    public async Task StopAzureSession()
    {
        Debug.Log("\nAnchorModuleScript.StopAzureSession()");

        OutputLog("Stopping Azure session... please wait...");

        // Stops any existing session
        cloudManager.StopSession();

        // Resets the current session if there is one, and waits for any active queries to be stopped
        await cloudManager.ResetSessionAsync();

        OutputLog("Azure session stopped successfully", isOverWrite: true);
    }

    public async void UpdatePropertiesAll(string key, string val, bool replace = true)
    {
        OutputLog("Trying to update AppProperties of Azure anchors");
        foreach (var info in locatedAnchors.Values)
        {
            await UpdateProperties(info, key, val, replace);
        }
    }

    public void SetDistanceInMeters(float distanceInMeters)
    {
        this.distanceInMeters = distanceInMeters;
    }

    public void SetMaxResultCount(int maxResultCount)
    {
        this.maxResultCount = maxResultCount;
    }
    public void SetExpiration(int expiration)
    {
        this.expiration = expiration;
    }

    public async Task UpdateProperties(CloudSpatialAnchor currentCloudAnchor, string key, string val,
        bool replace = true)
    {
        OutputLog($"anchor properties.id:{currentCloudAnchor.Identifier} -- key:{key},val:{val}....");
        if (currentCloudAnchor != null)
        {
            if (currentCloudAnchor.AppProperties.ContainsKey(key))
            {
                if (replace || currentCloudAnchor.AppProperties[key].Length == 0)
                {
                    currentCloudAnchor.AppProperties[key] = val;
                }
                else
                {
                    currentCloudAnchor.AppProperties[key] = currentCloudAnchor.AppProperties[key] + "," + val;
                }
            }
            else
            {
                currentCloudAnchor.AppProperties.Add(key, val);
            }
            // Start watching for Anchors
            if (cloudManager != null && cloudManager.Session != null)
            {
                foreach (var ke in currentCloudAnchor.AppProperties.Keys)
                {
                    OutputLog($"@id{currentCloudAnchor.Identifier} -> key:{ke},val:{currentCloudAnchor.AppProperties[ke]}");
                }
                await cloudManager.Session.UpdateAnchorPropertiesAsync(currentCloudAnchor);
                foreach (var ke in currentCloudAnchor.AppProperties.Keys)
                {
                    OutputLog($"@@id{currentCloudAnchor.Identifier} -> key:{ke},val:{currentCloudAnchor.AppProperties[ke]}");
                }

                var result = await cloudManager.Session.GetAnchorPropertiesAsync(currentCloudAnchor.Identifier);
                foreach (var ke in result.AppProperties.Keys)
                {
                    OutputLog($"@@@id{result.Identifier} -> key:{ke},val:{result.AppProperties[ke]}");
                }

                OutputLog(
                    $"anchor properties.id:{currentCloudAnchor.Identifier} -- key:{key},val:{val}....successfully",
                    isOverWrite: true);
            }
            else
            {
                OutputLog("Attempt to create watcher failed, no session exists", LogType.Error);
            }
        }
    }

    public async Task<string> CreateAzureAnchor(GameObject theObject, IDictionary<string, string> appProperties)
    {
        Debug.Log("\nAnchorModuleScript.CreateAzureAnchor()");

        // Notify AnchorFeedbackScript
        OutputLog("Creating Azure anchor");

        // First we create a native XR anchor at the location of the object in question
        theObject.CreateNativeAnchor();

        // Notify AnchorFeedbackScript
        OutputLog("Creating local anchor");

        // Then we create a new local cloud anchor
        var localCloudAnchor = new CloudSpatialAnchor();

        foreach (var key in appProperties.Keys)
        {
            localCloudAnchor.AppProperties.Add(key, appProperties[key]);
        }


        // Now we set the local cloud anchor's position to the native XR anchor's position
        localCloudAnchor.LocalAnchor = theObject.FindNativeAnchor().GetPointer();

        // Check to see if we got the local XR anchor pointer
        if (localCloudAnchor.LocalAnchor == IntPtr.Zero)
        {
            OutputLog("Didn't get the local anchor...", LogType.Error);
            return null;
        }

        Debug.Log("Local anchor created");

        // In this sample app we delete the cloud anchor explicitly, but here we show how to set an anchor to expire automatically
        localCloudAnchor.Expiration = DateTimeOffset.Now.AddDays(expiration);

        // Save anchor to cloud
        OutputLog("Move your device to capture more environment data: 0%");

        while (!cloudManager.IsReadyForCreate)
        {
            await Task.Delay(330);
            var createProgress = cloudManager.SessionStatus.RecommendedForCreateProgress;
            QueueOnUpdate(() => OutputLog($"Move your device to capture more environment data: {createProgress:0%}",
                isOverWrite: true));
        }

        try
        {
            OutputLog("Creating Azure anchor... please wait...");

            // Actually save
            await cloudManager.CreateAnchorAsync(localCloudAnchor);

            // Success?
            var success = localCloudAnchor != null;
            if (success)
            {
                OutputLog($"Azure anchor with ID '{localCloudAnchor.Identifier}' created successfully");
                locatedAnchors.Add(localCloudAnchor.Identifier, localCloudAnchor);
                return localCloudAnchor.Identifier;
            }

            OutputLog("Failed to save cloud anchor to Azure", LogType.Error);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }

        return null;
    }


    public void FindNearByAnchor(string anchorId)
    {
        anchorLocateCriteria.Identifiers = new string[0];
        Debug.Log("\nAnchorModuleScript.FindAzureAnchor()");
        OutputLog("Trying to find near by Azure anchor");

        if (!locatedAnchors.ContainsKey(anchorId))
        {
            OutputLog($"Not found anchor.id:{anchorId}.", LogType.Error);
            return;
        }
        anchorLocateCriteria.NearAnchor = new NearAnchorCriteria();
        anchorLocateCriteria.NearAnchor.SourceAnchor = locatedAnchors[anchorId];
        anchorLocateCriteria.NearAnchor.DistanceInMeters = distanceInMeters;
        anchorLocateCriteria.NearAnchor.MaxResultCount = maxResultCount;
        anchorLocateCriteria.RequestedCategories = AnchorDataCategory.Properties;

        Debug.Log(
            $"Anchor locate criteria configured to Search Near by Azure anchor ID '{anchorLocateCriteria.NearAnchor.SourceAnchor.Identifier}'");

        // Start watching for Anchors
        if (cloudManager != null && cloudManager.Session != null)
        {
            currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);
            Debug.Log("Watcher created");
            OutputLog("Looking for Azure anchor... please wait...");
        }
        else
        {
            OutputLog("Attempt to create watcher failed, no session exists", LogType.Error);
            currentWatcher = null;
        }
    }

    public void FindAzureAnchorById(params string[] azureAnchorIds)
    {
        Debug.Log("\nAnchorModuleScript.FindAzureAnchor()");

        // Notify AnchorFeedbackScript
        OutputLog("Trying to find Azure anchor");

        // Set up list of anchor IDs to locate
        var anchorsToFind = new List<string>();

        if (azureAnchorIds != null && azureAnchorIds.Length > 0)
        {
            anchorsToFind.AddRange(azureAnchorIds);
        }
        else
        {
            OutputLog("Current Azure anchor ID is empty", LogType.Error);
            return;
        }

        anchorLocateCriteria = new AnchorLocateCriteria();
        anchorLocateCriteria.Identifiers = anchorsToFind.ToArray();
        anchorLocateCriteria.BypassCache = false;

        // Start watching for Anchors
        if (cloudManager != null && cloudManager.Session != null)
        {
            currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);

            Debug.Log("Watcher created");
            OutputLog("Looking for Azure anchor... please wait...");
        }
        else
        {
            OutputLog("Attempt to create watcher failed, no session exists", LogType.Error);

            currentWatcher = null;
        }
    }

    public async void DeleteAllAzureAnchor()
    {
        Debug.Log("\nAnchorModuleScript.DeleteAllAzureAnchor()");

        // Notify AnchorFeedbackScript
        OutputLog("Trying to find Azure anchor...");

        foreach (var AnchorInfo in locatedAnchors.Values)
        {
            // Delete the Azure anchor with the ID specified off the server and locally
            await cloudManager.DeleteAnchorAsync(AnchorInfo);
        }

        locatedAnchors.Clear();

        OutputLog("Trying to find Azure anchor...Successfully");
    }

    public void SetASACallBackManager(IASACallBackManager iasaCallBackManager)
    {
        CallBackManager = iasaCallBackManager;
    }

    #endregion

    #region Event Handlers

        private static object lockObj = new object();
    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        QueueOnUpdate(() => Debug.Log("Anchor recognized as a possible Azure anchor"));

        if (args.Status == LocateAnchorStatus.Located || args.Status == LocateAnchorStatus.AlreadyTracked)
        {
            QueueOnUpdate(async () =>
            {
                 
                Debug.Log("Azure anchor located successfully");

                var currentCloudAnchor = args.Anchor;
                GameObject point = null;
                
                lock (lockObj)
                {
                    point = CallBackManager?.OnLocatedAnchorObject(currentCloudAnchor.Identifier,
                        currentCloudAnchor.AppProperties);
                    point?.SetActive(true);

                    if (!locatedAnchors.ContainsKey(currentCloudAnchor.Identifier))
                    {
                        locatedAnchors.Add(currentCloudAnchor.Identifier, currentCloudAnchor);
                    }
                }

                if (point == null)
                {
                    OutputLog("Not Anchor Object", LogType.Error);
                    return;
                }

                // Notify AnchorFeedbackScript
                OutputLog("Azure anchor located");

#if UNITY_ANDROID || UNITY_IOS
                Pose anchorPose = Pose.identity;
                anchorPose = currentCloudAnchor.GetPose();
#endif

#if WINDOWS_UWP || UNITY_WSA
                // HoloLens: The position will be set based on the unityARUserAnchor that was located.

                // Create a local anchor at the location of the object in question
                point.CreateNativeAnchor();

                // Notify AnchorFeedbackScript
                OutputLog("Creating local anchor");

                // On HoloLens, if we do not have a cloudAnchor already, we will have already positioned the
                // object based on the passed in worldPos/worldRot and attached a new world anchor,
                // so we are ready to commit the anchor to the cloud if requested.
                // If we do have a cloudAnchor, we will use it's pointer to setup the world anchor,
                // which will position the object automatically.
                if (currentCloudAnchor != null)
                {
                    Debug.Log("Local anchor position successfully set to Azure anchor position");

                    point.GetComponent<WorldAnchor>().SetNativeSpatialAnchorPtr(currentCloudAnchor.LocalAnchor);
                }
#else
                Debug.Log($"Setting object to anchor pose with position '{anchorPose.position}' and rotation '{anchorPose.rotation}'");
                point.transform.position = anchorPose.position;
                point.transform.rotation = anchorPose.rotation;

                // Create a native anchor at the location of the object in question
                point.CreateNativeAnchor();
                
#endif
            });
        }
        else
        {
            QueueOnUpdate(() =>
                OutputLog(
                    $"Attempt to locate Anchor with ID '{args.Identifier}' failed, locate anchor status was not 'Located' but '{args.Status}'",
                    LogType.Error));
        }
    }

    private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
        QueueOnUpdate(() => OutputLog("Locate Azure anchors Complete."));

        if (!args.Cancelled)
        {
            CallBackManager?.OnLocatedAnchorComplete();
        }
        else
        {
            QueueOnUpdate(() => OutputLog("Attempt to locate Anchor Complete failed.", LogType.Error));
        }
    }

#endregion
}