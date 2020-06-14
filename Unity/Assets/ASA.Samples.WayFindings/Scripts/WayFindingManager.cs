// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using Com.Reseul.ASA.Samples.WayFindings.Anchors;
using Com.Reseul.ASA.Samples.WayFindings.Factories;
using Com.Reseul.ASA.Samples.WayFindings.SpatialAnchors;
using Com.Reseul.ASA.Samples.WayFindings.UX.Dialogs;
using Com.Reseul.ASA.Samples.WayFindings.UX.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Com.Reseul.ASA.Samples.WayFindings
{
    /// <summary>
    ///     経路探索を実施するためのクラス
    /// </summary>
    public class WayFindingManager : MonoBehaviour, IASACallBackManager
    {
        private static readonly object lockObj = new object();

        private string basePointAnchorId;
        private IDictionary<string, string> basePointAppProperties;
        private string currentAnchorId = RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED;
        private SettingPointAnchor settingPointAnchor;

    #region Static Methods

        public static WayFindingManager Instance
        {
            get
            {
                var module = FindObjectsOfType<WayFindingManager>();
                if (module.Length == 1)
                {
                    return module[0];
                }

                Debug.LogWarning(
                    "Not found an existing SetRouteGuideManager in your scene. The SetRouteGuideManager requires only one.");
                return null;
            }
        }

    #endregion

    #region Inspector Properites

        [Tooltip("Set the destination title.")]
        public string Destination;

        [Tooltip("Set a GameObject of Instruction.")]
        public GameObject Instruction;

        [Tooltip("Set a GameObject of Menu.")]
        public WayFindingMenu Menu;

        [Tooltip("Set a GameObject of RouteGuideInformation.")]
        public RouteGuideInformation RouteInfo;

        [Tooltip("Set a GameObject of Select Destination Menu.")]
        public SelectDestinationMenu SelectDestinationMenu;

    #endregion

    #region Public Methods

        /// <summary>
        ///     Spatial Anchorの設置完了時に実行する処理
        ///     Spatial Anchorの設置がすべて完了した場合に実行されます。
        /// </summary>
        public void OnLocatedAnchorComplete()
        {
            try
            {
                foreach (var current in RouteInfo.LocatedAnchors.Keys)
                {
                    Debug.Log(
                        $"Current:{current} IsLinkLineCreated:{RouteInfo.LocatedAnchors[current].IsLinkLineCreated}");
                    var appProperties = RouteInfo.LocatedAnchors[current].AppProperties;
                    if (!RouteInfo.LocatedAnchors[current].IsLinkLineCreated)
                    {
                        RouteInfo.LocatedAnchors[current].IsLinkLineCreated = true;
                        var dest = RouteInfo.LocatedAnchors[current].AnchorObject;
                        if (appProperties.ContainsKey(RouteGuideInformation.PREV_ANCHOR_ID))
                        {
                            var key = appProperties[RouteGuideInformation.PREV_ANCHOR_ID];
                            if (RouteInfo.LocatedAnchors.TryGetValue(key, out var anchorInfo))
                            {
                                var pointObj = anchorInfo.AnchorObject.GetComponent<DestinationPointAnchor>();
                                pointObj?.DisabledEffects();
                                var basePointObj = anchorInfo.AnchorObject.GetComponent<SettingPointAnchor>();
                                basePointObj?.DisabledEffects();

                                Debug.Log($"LinkLine:{anchorInfo.AnchorObject.name} to {dest.name}");
                                LinkLineGenerateFactory.CreateLinkLineObject(anchorInfo.AnchorObject, dest,
                                    RouteInfo.RootLinkLineObjects.transform);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        /// <summary>
        ///     Spatial Anchorの可視化に必要なオブジェクトの生成を行います。
        ///     Spatial Anchorの設置が完了した場合に発生するイベント内で実行されます。
        /// </summary>
        /// <param name="identifier">AnchorId</param>
        /// <param name="appProperties">Spatial Anchorに含まれるAppProperties</param>
        /// <param name="gameObject">可視化に利用するオブジェクト</param>
        /// <returns>アンカーの設置対象か。trueの場合設置対象</returns>
        public bool OnLocatedAnchorObject(string identifier,
            IDictionary<string, string> appProperties, out GameObject gameObject)
        {
            try
            {
                appProperties.TryGetValue(RouteGuideInformation.ANCHOR_TYPE, out var anchorType);

                var destinations = new string[0];
                if (appProperties.TryGetValue(RouteGuideInformation.DESTINATION_TITLE, out var destination))
                {
                    destinations = destination.Split(',');
                }

                // 指定ルート以外のアンカー情報に対しては可視化を行わない。
                if (!destinations.Any(x => x.Equals(Destination)))
                {
                    gameObject = null;
                    return false;
                }

                DestinationPointAnchor point;

                if (!RouteInfo.LocatedAnchors.ContainsKey(identifier))
                {
                    //アンカーの可視化が完了していない場合はアンカーを生成する。
                    point = AnchorGenerateFactory.GenerateDestinationPointAnchor(RouteInfo.RootPointObjects);
                    lock (lockObj)
                    {
                        RouteInfo.LocatedAnchors.Add(identifier,
                            new RouteGuideInformation.AnchorInfo(appProperties, point.gameObject,
                                currentAnchorId.Equals(RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED)));
                    }
                }
                else
                {
                    //すでにアンカーが可視化されている場合はそのオブジェクトを取得する。
                    point = RouteInfo.LocatedAnchors[identifier].AnchorObject.GetComponent<DestinationPointAnchor>();

                    RouteInfo.LocatedAnchors[identifier] =
                        new RouteGuideInformation.AnchorInfo(appProperties, point.gameObject,
                            currentAnchorId.Equals(RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED));
                }

                currentAnchorId = identifier;

                point.Identifier = identifier;
                if (RouteGuideInformation.ANCHOR_TYPE_DESTINATION.Equals(anchorType))
                {
                    point.DestinationTitle = destination;
                }

                gameObject = point.gameObject;
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        /// <summary>
        ///     オブジェクトの有効無効を設定します。
        /// </summary>
        /// <param name="enabled"></param>
        public void Initialize(bool enabled, string anchorId = null,
            SettingPointAnchor anchorObj = null, IDictionary<string, string> appProperties = null)
        {
            try
            {
                if (anchorId == null || anchorObj == null || appProperties == null)
                {
                    Debug.LogWarning("null reference the base point anchor information.");
                    return;
                }

                AnchorModuleProxy.Instance.SetASACallBackManager(this);

                Dialog.OpenDialog("Way Destination.",
                    "Please select the destination.\nWhen you select a destination, the application searches the anchors and places Spatial Anchor. When you approach this Spatial Anchor, it will automatically search for the next anchor . Finally, you reach the destination, you're done.",
                    new[] {"Ok"}, new[]
                    {
                        new UnityAction(() =>
                        {
                            Instruction?.SetActive(enabled);
                            RouteInfo?.gameObject.SetActive(enabled);
                            if (basePointAppProperties.TryGetValue(RouteGuideInformation.DESTINATION_TITLE,
                                out var rowData))
                            {
                                var destinations = rowData.Split(',');
                                SelectDestinationMenu.GenerateDestination(destinations, OnSelectDestination);
                                SelectDestinationMenu.SetActive(enabled);
                            }

                            Menu.ChangeStatus(BaseMenu.MODE_INITIALIZE);
                        })
                    });

                basePointAnchorId = anchorId;
                settingPointAnchor = anchorObj;
                var locatedAnchors = RouteInfo.LocatedAnchors;
                if (!locatedAnchors.ContainsKey(basePointAnchorId))
                {
                    locatedAnchors.Add(basePointAnchorId,
                        new RouteGuideInformation.AnchorInfo(appProperties, settingPointAnchor.gameObject));
                }

                basePointAppProperties = appProperties;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }


        /// <summary>
        ///     アプリケーションを終了します。
        /// </summary>
        public void Exit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Windows.ApplicationModel.Core.CoreApplication.Exit();
#endif
        }

    #endregion

    #region Private Methods

        /// <summary>
        ///     選択された目的地名を設定します。
        /// </summary>
        /// <param name="destination"></param>
        private void OnSelectDestination(string destination)
        {
            SelectDestinationMenu.SetActive(false);
            Destination = destination;
            FindNearbyAnchors(basePointAnchorId);
        }

        /// <summary>
        ///     指定されたAnchorId周辺に存在するSpatial Anchorの探索を行います。
        /// </summary>
        /// <param name="identifier"></param>
        private void FindNearbyAnchors(string identifier)
        {
            AnchorModuleProxy.Instance.FindNearByAnchor(identifier);
        }

    #endregion
    }
}