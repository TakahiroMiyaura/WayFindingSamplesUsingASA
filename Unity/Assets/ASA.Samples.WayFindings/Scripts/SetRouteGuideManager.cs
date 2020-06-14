// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using Com.Reseul.ASA.Samples.WayFindings.Anchors;
using Com.Reseul.ASA.Samples.WayFindings.Factories;
using Com.Reseul.ASA.Samples.WayFindings.SpatialAnchors;
using Com.Reseul.ASA.Samples.WayFindings.UX.Dialogs;
using Com.Reseul.ASA.Samples.WayFindings.UX.Effects;
using Com.Reseul.ASA.Samples.WayFindings.UX.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Com.Reseul.ASA.Samples.WayFindings
{
    /// <summary>
    ///     経路設定を実施するためのクラス
    /// </summary>
    public class SetRouteGuideManager : MonoBehaviour
    {
        private string basePointAnchorId;
        private IDictionary<string, string> basePointAppProperties;

        [HideInInspector]
        public string CurrentAnchorId;

        private GameObject currentAnchorObject;
        private LinkLineDataProvider currentLinkLine;
        private bool isDestination;

        private SettingPointAnchor settingPointAnchor;

    #region Static Methods

        /// <summary>
        ///     Azure Spatial Anchorsの処理を実行するクラスのインスタンスを取得します。
        /// </summary>
        public static SetRouteGuideManager Instance
        {
            get
            {
                var module = FindObjectsOfType<SetRouteGuideManager>();
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

        public GameObject AnchorCollection;

        [Tooltip("Set a GameObject of Instruction.")]
        public GameObject Instruction;

        [Tooltip("Set a GameObject of Menu.")]
        public RouteGuideMenu Menu;

    #endregion

    #region Public Methods

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

                basePointAnchorId = anchorId;
                CurrentAnchorId = basePointAnchorId;
                settingPointAnchor = anchorObj;
                basePointAppProperties = appProperties;

                Dialog.OpenDialog("Set Destination.",
                    "Please set the destination.\n * Selecting 'Set Destination', set destination and title.\n * Selecting 'Create Next Point', add transit point.",
                    new[] {"Ok"}, new[]
                    {
                        new UnityAction(() =>
                        {
                            Instruction?.SetActive(enabled);
                            AnchorCollection?.SetActive(enabled);
                            Menu.ChangeStatus(BaseMenu.MODE_INITIALIZE);
                        })
                    });
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        /// <summary>
        ///     アンカーの可視化に利用するGameobjectを生成します。
        /// </summary>
        /// <param name="isDestination">中継点か目的地かを判定するフラグ</param>
        public void CreateAnchorObject(bool isDestination)
        {
            try
            {
                Menu.ChangeStatus(RouteGuideMenu.MODE_CREATE_ANCHOR);

                this.isDestination = isDestination;

                if (currentAnchorObject == null)
                {
                    currentAnchorObject = settingPointAnchor.gameObject;
                }

                var dest = AnchorGenerateFactory.GenerateSettingsPointAnchor(isDestination
                    ? SettingPointAnchor.AnchorMode.Destination
                    : SettingPointAnchor.AnchorMode.Point);

                currentLinkLine = LinkLineGenerateFactory.CreateLinkLineObject(currentAnchorObject, dest.gameObject,
                    AnchorCollection.transform);

                currentAnchorObject = dest.gameObject;
                currentAnchorObject.transform.parent = AnchorCollection.transform;
                currentAnchorObject.transform.position =
                    Camera.main.transform.position + Camera.main.transform.forward * 1f;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        /// <summary>
        ///     可視化しているすべてのアンカーとリンクオブジェクトを削除します。
        /// </summary>
        public void DeleteAnchorObject()
        {
            var prevObject = currentLinkLine.FromPoint;
            DestroyImmediate(currentLinkLine.gameObject);
            DestroyImmediate(currentAnchorObject);
            currentAnchorObject = prevObject;
            Menu.ChangeStatus(BaseMenu.MODE_INITIALIZE);
        }

        /// <summary>
        ///     次の処理ステップ（経路探索）へ進むための処理を実行します。
        /// </summary>
        public void NextStepWayFinding()
        {
            Initialize(false);
            while (transform.GetChild(2).childCount > 0)
            {
                DestroyImmediate(transform.GetChild(2).GetChild(0).gameObject);
            }

            Menu.ChangeStatus(BaseMenu.MODE_CLOSE);
            WayFindingManager.Instance.Initialize(true, basePointAnchorId, settingPointAnchor,
                basePointAppProperties);
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

        /// <summary>
        ///     Azure Spatial Anchorsサービスにアンカーを追加します。
        /// </summary>
        public async void CreateAzureAnchor()
        {
            try
            {
                // Then we create a new local cloud anchor
                var appProperties = new Dictionary<string, string>();

                appProperties.Add(RouteGuideInformation.ANCHOR_TYPE,
                    isDestination
                        ? RouteGuideInformation.ANCHOR_TYPE_DESTINATION
                        : RouteGuideInformation.ANCHOR_TYPE_POINT);

                appProperties.Add(RouteGuideInformation.PREV_ANCHOR_ID, CurrentAnchorId);

                var identifier = await AnchorModuleProxy.Instance.CreateAzureAnchor(currentAnchorObject, appProperties);


                if (identifier == null)
                {
                    return;
                }

                CurrentAnchorId = identifier;
                var point = currentAnchorObject.GetComponent<SettingPointAnchor>();
                point.DisabledEffects();

                if (isDestination)
                {
                    var destinationTitle = point.Text;
#if UNITY_EDITOR
                    destinationTitle = "TestDest";
#endif
                    AnchorModuleProxy.Instance.UpdatePropertiesAll(RouteGuideInformation.DESTINATION_TITLE,
                        destinationTitle, false);

                    Menu.ChangeStatus(BaseMenu.MODE_COMPLETE);
                }
                else
                {
                    Menu.ChangeStatus(BaseMenu.MODE_INITIALIZE);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

    #endregion
    }
}