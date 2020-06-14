// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings
{
    /// <summary>
    ///     経路探索、道案内先に必要なアンカー情報を管理するクラスです。
    /// </summary>
    public class RouteGuideInformation : MonoBehaviour
    {
        /// <summary>
        ///     AppPropertiesで利用するキー：アンカーの種別（Point/Destination）を格納するキー名を表す定数
        /// </summary>
        public const string ANCHOR_TYPE = "Type";

        /// <summary>
        ///     アンカーの種別（Destination）を表す定数
        /// </summary>
        public const string ANCHOR_TYPE_DESTINATION = "Destination";

        /// <summary>
        ///     アンカーの種別（Point）を表す定数
        /// </summary>
        public const string ANCHOR_TYPE_POINT = "Point";

        /// <summary>
        ///     AppPropertiesで利用するキー：目的地名を格納するキー名を表す定数
        /// </summary>
        public const string DESTINATION_TITLE = "DestinationTitle";

        /// <summary>
        ///     AppPropertiesで利用するキー：リンクする直前のAnchorIdを格納するキー名を表す定数
        /// </summary>
        public const string PREV_ANCHOR_ID = "PrevAnchorId";

        /// <summary>
        ///     リンクする直前のAnchorIdが未設定であることを表す定数
        /// </summary>
        public const string ANCHOR_ID_NOT_INITIALIZED = "NotInitialized";

        /// <summary>
        ///     現在のアンカーIdを取得/設定します。
        /// </summary>
        [HideInInspector]
        public string CurrentAnchorId = ANCHOR_ID_NOT_INITIALIZED;

        /// <summary>
        ///     目的地名を取得/設定します。
        /// </summary>
        [HideInInspector]
        public string Destination;

        /// <summary>
        ///     取得済みSpatial Anchorの情報を管理するプロパティです。
        /// </summary>
        public Dictionary<string, AnchorInfo> LocatedAnchors = new Dictionary<string, AnchorInfo>();

        /// <summary>
        ///     アンカー同士の接続を可視化するGameObjectを格納するルート要素を取得します。
        /// </summary>
        public Transform RootLinkLineObjects { get; private set; }

        /// <summary>
        ///     アンカーを可視化するGameObjectを格納するルート要素を取得します。
        /// </summary>
        public Transform RootPointObjects { get; private set; }

    #region Static Methods

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

    #endregion

    #region Unity Lifecycle

        /// <summary>
        ///     初期化処理を実施します
        /// </summary>
        private void Start()
        {
            RootPointObjects = transform.GetChild(0);
            RootLinkLineObjects = transform.GetChild(1);
        }

    #endregion

    #region Inner Class

        /// <summary>
        ///     内部で管理するアンカー情報を保持するキャリアクラス
        /// </summary>
        public class AnchorInfo
        {
            /// <summary>
            ///     コンストラクタです。
            /// </summary>
            /// <param name="appProperties">アンカーに設定されたAppProperties</param>
            /// <param name="anchorObject">アンカーのGemeObject</param>
            /// <param name="isLinkLineCreated">アンカー同士のリンクの可視化が完了しているか</param>
            public AnchorInfo([NotNull] IDictionary<string, string> appProperties, [NotNull] GameObject anchorObject,
                bool isLinkLineCreated = false)
            {
                if (appProperties == null) throw new ArgumentNullException(nameof(appProperties));
                if (anchorObject == null) throw new ArgumentNullException(nameof(anchorObject));
                AppProperties = appProperties;
                AnchorObject = anchorObject;
            }

            /// <summary>
            ///     アンカーに設定されたAppPropertiesを取得します。
            /// </summary>
            public IDictionary<string, string> AppProperties { get; private set; }

            /// <summary>
            ///     アンカーのGemeObjectを取得します。
            /// </summary>
            public GameObject AnchorObject { get; }

            /// <summary>
            ///     アンカー同士のリンクの可視化状況を取得/設定します。
            /// </summary>
            public bool IsLinkLineCreated { get; set; }

            /// <summary>
            ///     オブジェクトの破棄を実施します。
            /// </summary>
            public void Destroy()
            {
                DestroyImmediate(AnchorObject);
                AppProperties = null;
            }
        }

    #endregion
    }
}