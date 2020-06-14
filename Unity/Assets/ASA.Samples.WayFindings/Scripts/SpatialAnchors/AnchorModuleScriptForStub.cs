// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.SpatialAnchors
{
    /// <summary>
    ///     Azure Spatial AnchorsのUnity Edtor上で実行するためのスタブクラス
    /// </summary>
    public class AnchorModuleScriptForStub : MonoBehaviour, IAnchorModuleScript
    {
        private readonly Dictionary<string, Vector3> anchorsPosition =
            new Dictionary<string, Vector3>();

        private readonly Queue<Action> dispatchQueue = new Queue<Action>();

        private readonly Dictionary<string, IDictionary<string, string>> locatedAnchors =
            new Dictionary<string, IDictionary<string, string>>();

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
        }

    #endregion

    #region Public Methods

#pragma warning disable 1998
        public async Task StartAzureSession()
#pragma warning restore 1998
        {
            Debug.Log("\nAnchorModuleScript.StartAzureSession()");

            OutputLog("Starting Azure session... please wait...");

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

#pragma warning disable 1998
        public async Task StopAzureSession()
#pragma warning restore 1998
        {
            Debug.Log("\nAnchorModuleScript.StopAzureSession()");

            OutputLog("Stopping Azure session... please wait...");

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
        }

        public void SetMaxResultCount(int maxResultCount)
        {
        }

        public void SetExpiration(int expiration)
        {
        }

#pragma warning disable 1998
        public async Task UpdateProperties(IDictionary<string, string> currentCloudAnchor, string key, string val,
#pragma warning restore 1998
            bool replace = true)
        {
            var identifier = locatedAnchors.Where(x => x.Value.Equals(currentCloudAnchor)).Select(x => x.Key)
                .FirstOrDefault();
            OutputLog($"anchor properties.id:{identifier} -- key:{key},val:{val}....");
            if (currentCloudAnchor != null)
            {
                if (currentCloudAnchor.ContainsKey(key))
                {
                    if (replace)
                    {
                        currentCloudAnchor[key] = val;
                    }
                    else
                    {
                        currentCloudAnchor[key] = currentCloudAnchor[key] + "," + val;
                    }
                }
                else
                {
                    currentCloudAnchor.Add(key, val);
                }


                OutputLog($"anchor properties.id:{identifier} -- key:{key},val:{val}....successfully",
                    isOverWrite: true);
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

            // Save anchor to cloud
            OutputLog("Move your device to capture more environment data: 0%");

            var isReadyForCreate = false;
            var createProgress = 0f;
            while (!isReadyForCreate)
            {
                await Task.Delay(100);
                QueueOnUpdate(() => OutputLog($"Move your device to capture more environment data: {createProgress:0%}",
                    isOverWrite: true));
                createProgress += 0.25f;
                if (createProgress > 0.99)
                {
                    isReadyForCreate = true;
                }
            }

            try
            {
                OutputLog("Creating Azure anchor... please wait...");

                var success = true;

                if (success)
                {
                    OutputLog($"Azure anchor with ID '{locatedAnchors.Count}' created successfully");
                    var id = locatedAnchors.Count.ToString();
                    locatedAnchors.Add(id, appProperties);
                    anchorsPosition.Add(id, theObject.transform.position);
                    return id;
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
            Debug.Log("\nAnchorModuleScript.FindAzureAnchor()");
            OutputLog("Trying to find near by Azure anchor");

            var data = (int.Parse(anchorId) + 1).ToString();

            GameObject invoke;
            CallBackManager.OnLocatedAnchorObject(data, locatedAnchors[data], out invoke);
            invoke.transform.position = anchorsPosition[data];

            CallBackManager.OnLocatedAnchorComplete();
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

            foreach (var id in anchorsToFind)
            {
                Debug.Log($"Anchor locate criteria configured to look for Azure anchor with ID '{id}'");
                GameObject obj;
                CallBackManager.OnLocatedAnchorObject(id, locatedAnchors[id], out obj);
            }

            CallBackManager.OnLocatedAnchorComplete();
        }

#pragma warning disable 1998
        public async void DeleteAllAzureAnchor()
#pragma warning restore 1998
        {
            Debug.Log("\nAnchorModuleScript.DeleteAllAzureAnchor()");

            // Notify AnchorFeedbackScript
            OutputLog("Trying to find Azure anchor...");

            locatedAnchors.Clear();

            OutputLog("Trying to find Azure anchor...Successfully");
        }

        public void SetASACallBackManager(IASACallBackManager iasaCallBackManager)
        {
            CallBackManager = iasaCallBackManager;
        }

    #endregion
    }
}