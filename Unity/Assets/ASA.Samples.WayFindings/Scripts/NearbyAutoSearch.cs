// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using Com.Reseul.ASA.Samples.WayFindings.Anchors;
using Com.Reseul.ASA.Samples.WayFindings.SpatialAnchors;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings
{
    /// <summary>
    ///     アンカーに近づくと1度だけAzure Spatial Anchorsに対して検索を実行するオブジェクトです。
    ///     このオブジェクトは親オブジェクトに<see cref="DestinationPointAnchor" />を実装していることを前提にしています。
    /// </summary>
    public class NearbyAutoSearch : MonoBehaviour
    {
        private CapsuleCollider colidar;
        private GameObject headPosition;
        private bool isProcessingTrigger;

        private DestinationPointAnchor parent;

    #region Inspector Properites

        [SerializeField]
        private float radius = .5f;

    #endregion


    #region Private Methods

        /// <summary>
        ///     アンカーに近づいたときに実行する処理
        /// </summary>
        /// <param name="colidar"></param>
        private void OnTriggerEnter(Collider colidar)
        {
            try
            {
                if (isProcessingTrigger)
                {
                    return;
                }

                if (colidar.gameObject.name.Equals("HeadPosition"))
                {
                    isProcessingTrigger = true;
                    Debug.Log($"Call Nearby Anchor. id:{parent.Identifier}");
                    // アンカーを中心に周辺のSpatial Anchorの検索を実施します。
                    AnchorModuleProxy.Instance.FindNearByAnchor(parent.Identifier);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

    #endregion


    #region Unity Lifecycle

        /// <summary>
        ///     初期化処理を実施します
        /// </summary>
        private void Start()
        {
            parent = GetComponentInParent<DestinationPointAnchor>();
            colidar = GetComponent<CapsuleCollider>();
            colidar.radius = radius;

            headPosition = transform.GetChild(0).gameObject;
        }

        /// <summary>
        ///     フレーム毎に実行する処理を実施します。
        /// </summary>
        private void Update()
        {
            headPosition.transform.position = Camera.main.transform.position;
            colidar.isTrigger = true;
        }

    #endregion
    }
}