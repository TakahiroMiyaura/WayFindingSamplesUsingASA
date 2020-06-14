// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Com.Reseul.ASA.Samples.WayFindings.Anchors;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Effects
{
    /// <summary>
    ///     経路上のポイント同士を視覚的につなぐためのLineRendererを制御するためのクラス
    /// </summary>
    [RequireComponent(typeof(SplineDataProvider))]
    public class LinkLineDataProvider : MonoBehaviour
    {
        private SplineDataProvider dataProvider;
        private Transform fromTransform;
        private Transform toTransform;

    #region Public Methods

        /// <summary>
        ///     アンカー同士をつなぐLineRendererを描画します。
        /// </summary>
        /// <param name="parent">生成したオブジェクトの親</param>
        /// <param name="fromPoint">始点になるアンカー</param>
        /// <param name="toPoint">終点になるアンカー</param>
        /// <returns></returns>
        public void CreateLinkLineObject(Transform parent, GameObject fromPoint, GameObject toPoint)
        {
            transform.parent = parent;
            ToPoint = toPoint;
            FromPoint = fromPoint;
            name = "Link:" + fromPoint.name + " > " + toPoint.name;
        }

    #endregion

    #region Inspector Properites

        [SerializeField]
        private float curv = 0.4f;

        public GameObject FromPoint { get; private set; }
        public GameObject ToPoint { get; private set; }

    #endregion

    #region Unity Lifecycle

        // Start is called before the first frame update
        private void Start()
        {
            dataProvider = GetComponent<SplineDataProvider>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (ToPoint == null || FromPoint == null)
            {
                return;
            }

            if (fromTransform == null || toTransform == null)
            {
                var fromDest = FromPoint.GetComponent<DestinationPointAnchor>();
                fromTransform = fromDest != null ? fromDest.InnerObjectTransform: FromPoint.transform;

                var toDest = ToPoint.GetComponent<DestinationPointAnchor>();
                toTransform = toDest != null ? toDest.InnerObjectTransform : ToPoint.transform;
            }

            transform.position = fromTransform.position;

            var pos = toTransform.position - fromTransform.position;
            var deltaX = pos.x * 1f / 3f;
            var deltaZ = pos.z * 1f / 3f;
            var posDelta1 = new Vector3(deltaX, curv, deltaZ);
            var posDelta2 = new Vector3(deltaX * 2f, curv, deltaZ * 2f);
            dataProvider.ControlPoints[1].Position = posDelta1;
            dataProvider.ControlPoints[2].Position = posDelta2;
            dataProvider.ControlPoints[3].Position = pos;
        }

    #endregion
    }
}