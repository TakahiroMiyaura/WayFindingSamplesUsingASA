// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class RouteGuideInformation : MonoBehaviour
{
    public const string ANCHOR_TYPE = "Type";
    public const string ANCHOR_TYPE_DESTINATION = "Destination";
    public const string ANCHOR_TYPE_POINT = "Point";
    public const string DESTINATION_TITLE = "DestinationTitle";
    public const string PREV_ANCHOR_ID = "PrevAnchorId";
    public const string ANCHOR_ID_NOT_INITIALIZED = "NotInitialized";

    [HideInInspector]
    public string BasePointAnchorId = ANCHOR_ID_NOT_INITIALIZED;

    [HideInInspector]
    public string CurrentAnchorId = ANCHOR_ID_NOT_INITIALIZED;

    [HideInInspector]
    public string Destination;

    public GameObject DestinationPointPrefab;
    public GameObject LinkLinePrefab;

    public Dictionary<string, AnchorInfo> LocatedAnchors = new Dictionary<string, AnchorInfo>();
    public Transform RootLinkLineObjects { get; private set; }
    public Transform RootPointObjects { get; private set; }


    // Start is called before the first frame update
    public static RouteGuideInformation Instance
    {
        get
        {
            var module = FindObjectsOfType<RouteGuideInformation>();
            if (module.Length == 1)
            {
                return module[0];
            }

            Debug.LogWarning(
                "Not found an existing AnchorModuleScript in your scene. The Anchor Module Script requires only one.");
            return null;
        }
    }

    private void Start()
    {
        RootPointObjects = transform.GetChild(0);
        RootLinkLineObjects = transform.GetChild(1);

    }

    public class AnchorInfo
    {
        public AnchorInfo([NotNull] IDictionary<string, string> cloudAnchorInfo, [NotNull] GameObject anchorObject,
            bool isLinkLineCreated = false)
        {
            if (cloudAnchorInfo == null) throw new ArgumentNullException(nameof(cloudAnchorInfo));
            if (anchorObject == null) throw new ArgumentNullException(nameof(anchorObject));
            CloudAnchorInfo = cloudAnchorInfo;
            AnchorObject = anchorObject;
        }

        public IDictionary<string, string> CloudAnchorInfo { get; private set; }
        public GameObject AnchorObject { get; }
        public bool IsLinkLineCreated { get; set; }

        public void Destory()
        {
            DestroyImmediate(AnchorObject);
            CloudAnchorInfo = null;
        }
    }
}