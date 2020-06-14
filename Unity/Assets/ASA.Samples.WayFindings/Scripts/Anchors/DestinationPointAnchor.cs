// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using TMPro;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.Anchors
{
    /// <summary>
    ///     Spatial Anchor可視化に利用する経路案内時のアンカー用のコンポーネントです。
    /// </summary>
    public class DestinationPointAnchor : MonoBehaviour
    {
        private float coefficient = -1f;
        private Transform directionIndicatorObjectTransform;
        private string identifier;
        private Transform labelObjectTransform;
        private Transform nearAutoSearchObjectTransform;
        private Transform nearRangeIndicatorObjectTransform;
        private Transform outerObjectTransform;
        private TextMeshPro textMeshPro;


    #region Public Methods

        /// <summary>
        ///     アンカーの可視化時の各種エフェクトを停止します。
        /// </summary>
        public void DisabledEffects()
        {
            nearAutoSearchObjectTransform?.gameObject.SetActive(false);
            directionIndicatorObjectTransform?.gameObject.SetActive(false);
            nearRangeIndicatorObjectTransform?.gameObject.SetActive(false);
        }

    #endregion


    #region Public Properites

        /// <summary>
        ///     目的地名を設定取得します。
        /// </summary>
        [HideInInspector]
        public string DestinationTitle;

        /// <summary>
        ///     Anchor IDを設定取得します。
        /// </summary>
        [HideInInspector]
        public string Identifier
        {
            get => identifier;
            set
            {
                identifier = value;
                name = "Anchor:" + value;
            }
        }
        /// <summary>
        ///   子のオブジェクトのTrasformを取得します。
        /// </summary>
        [HideInInspector]
        public Transform InnerObjectTransform { get; private set; }

    #endregion


    #region Unity Lifecycle

        /// <summary>
        ///     初期化処理を実施します
        /// </summary>
        private void Start()
        {
            outerObjectTransform = transform.GetChild(0);
            InnerObjectTransform = transform.GetChild(1);
            labelObjectTransform = transform.GetChild(2);
            nearRangeIndicatorObjectTransform = transform.GetChild(3);
            nearAutoSearchObjectTransform = transform.GetChild(4);
            directionIndicatorObjectTransform = transform.GetChild(5);
            textMeshPro = GetComponentInChildren<TextMeshPro>();
            outerObjectTransform.GetComponent<MeshRenderer>().enabled = true;
            InnerObjectTransform.localPosition = new Vector3(InnerObjectTransform.localPosition.x,
                Random.Range(-.3f, .3f),
                InnerObjectTransform.localPosition.z);
        }

        /// <summary>
        ///     フレーム毎に実行する処理を実施します。
        /// </summary>
        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position,
                new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z), 0.015f);
            var enabled = !string.IsNullOrEmpty(DestinationTitle);
            InnerObjectAnimate();
            OuterObjectAnimate(enabled);
            LabelObjectAnimate(enabled);
            SetNearbyAutoSearch(enabled);
        }

    #endregion


    #region Private Methods

        /// <summary>
        ///     アンカーに近づいた場合、自動的にこのアンカーを中心にSpatial Anchorの検索を行うオブジェクトの有効/無効を設定します。
        /// </summary>
        /// <param name="enabled">enabled</param>
        private void SetNearbyAutoSearch(bool enabled)
        {
            nearAutoSearchObjectTransform.gameObject.SetActive(!enabled);
        }

        /// <summary>
        ///     目的地名を表示するラベルオブジェクトの有効/無効を設定します。
        /// </summary>
        /// <param name="enabled">enabled</param>
        private void LabelObjectAnimate(bool enabled)
        {
            labelObjectTransform.gameObject.SetActive(enabled);
            textMeshPro.text = DestinationTitle;
        }

        /// <summary>
        ///     目的地となるオブジェクトのエフェクトの有効/無効を設定します。
        /// </summary>
        /// <param name="enabled">enabled</param>
        private void OuterObjectAnimate(bool enabled)
        {
            outerObjectTransform.gameObject.SetActive(enabled);
        }

        /// <summary>
        ///     オブジェクトのアニメーションを行います。
        /// </summary>
        private void InnerObjectAnimate()
        {
            if (InnerObjectTransform == null)
            {
                return;
            }

            InnerObjectTransform.Rotate(new Vector3(.2f, 0.05f, 0.1f) * coefficient);

            InnerObjectTransform.localPosition =
                Vector3.Lerp(InnerObjectTransform.localPosition, Vector3.up * .15f * coefficient, .005f);
            var threshold = (InnerObjectTransform.localPosition - Vector3.up * .15f * coefficient).magnitude;
            if (threshold < 1e-2)
            {
                coefficient = coefficient * -1f;
            }
        }

    #endregion
    }
}