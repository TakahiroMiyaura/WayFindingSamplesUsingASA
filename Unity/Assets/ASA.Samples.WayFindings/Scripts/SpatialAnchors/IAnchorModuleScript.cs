// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors;
using UnityEngine;

public interface IAnchorModuleScript
{
    void Start();
    void Update();
    void OnDestroy();
    Task StartAzureSession();
    void OutputLog(string startSessionLog, LogType logType=LogType.Log, bool isOverWrite=false,bool isReset = false);
    Task StopAzureSession();
    void UpdatePropertiesAll(string key, string val, bool replace = true);

    void SetDistanceInMeters(float distanceInMeters);
    void SetMaxResultCount(int maxResultCount);
    void SetExpiration(int expiration);

    Task<string> CreateAzureAnchor(GameObject theObject, IDictionary<string, string> appProperties);
    void FindNearByAnchor(string anchorId);
    void FindAzureAnchorById(params string[] azureAnchorIds);
    void DeleteAllAzureAnchor();

    event AnchorModuleProxy.FeedbackDescription OnFeedbackDescription;

    IASACallBackManager CallBackManager { set; get; }

    void SetASACallBackManager(IASACallBackManager iasaCallBackManager);
}