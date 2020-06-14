// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Com.Reseul.ASA.Samples.WayFindings.SpatialAnchors;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Effects
{
    /// <summary>
    ///     Azure Spatial Anchorsで周辺を探索可能な範囲を可視化するオブジェクトです、
    /// </summary>
    public class NextAnchorRnageMarker : MonoBehaviour
    {
    #region Inspector Properites

        [SerializeField]
        private float intervals = 0.001f;

        [SerializeField]
        private Vector3 maxRange = Vector3.one;

        private Vector3 initializeScale;

        [SerializeField]
        private float threshold = .1f;

    #endregion

    #region Unity Lifecycle

        private void Start()
        {
            var proxy = FindObjectOfType<AnchorModuleProxy>();
            if (proxy != null)
            {
                var val = proxy.DistanceInMeters;
                initializeScale = transform.localScale;
                var vec = new Vector3(1, 1, 1);
                vec.x = val * 2f / transform.parent.lossyScale.x;
                vec.y = 1f;
                vec.z = val * 2f / transform.parent.lossyScale.z;
                maxRange = vec;
            }
        }

        private void Update()
        {
            if ((transform.localScale - maxRange).magnitude < threshold)
            {
                transform.localScale = initializeScale;
            }

            transform.localScale = Vector3.Lerp(transform.localScale, maxRange, intervals);
        }

    #endregion
    }
}