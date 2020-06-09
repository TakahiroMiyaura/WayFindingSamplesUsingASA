// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using UnityEngine;


public class AnchorModuleProxy : MonoBehaviour
{

    public delegate void FeedbackDescription(string description, bool isOverWrite = false, bool isReset = false);

    [Header("NearbySetting")]
    [SerializeField]
    [Tooltip("Maximum distance in meters from the source anchor (defaults to 5).")]
    private float distanceInMeters = 5f;

    [SerializeField]
    [Tooltip("Maximum desired result count (defaults to 20).")]
    private int maxResultCount = 20;

    [Header("CreateAnchorParams")]
    [SerializeField]
    [Tooltip("The number of days until the anchor is automatically deleted")]
    private int Expiration = 7;

    public float DistanceInMeters => distanceInMeters;

#region Static Methods

    public static IAnchorModuleScript Instance
    {
        get
        {
#if UNITY_EDITOR
            var module = FindObjectsOfType<AnchorModuleScriptForStub>();
#else
            var module = FindObjectsOfType<AnchorModuleScript>();
#endif
            if (module.Length == 1)
            {
                var proxy = FindObjectOfType<AnchorModuleProxy>();
                module[0].SetDistanceInMeters(proxy.distanceInMeters);
                module[0].SetMaxResultCount(proxy.maxResultCount);
                module[0].SetExpiration(proxy.Expiration);
                return module[0];
            }

            Debug.LogWarning(
                "Not found an existing AnchorModuleScript in your scene. The Anchor Module Script requires only one.");
            return null;
        }
    }

#endregion

    private void Start()
    {
#if UNITY_EDITOR
        transform.GetChild(0).gameObject.SetActive(false);
#endif
    }
}