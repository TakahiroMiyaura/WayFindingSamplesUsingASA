// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.IO;
using Com.Reseul.ASA.Samples.WayFindings.Anchors;
using Com.Reseul.ASA.Samples.WayFindings.SpatialAnchors;
using Com.Reseul.ASA.Samples.WayFindings.UX.Dialogs;
using Com.Reseul.ASA.Samples.WayFindings.UX.Menus;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#if WINDOWS_UWP
using Windows.Storage;
#endif


namespace Com.Reseul.ASA.Samples.WayFindings
{
    /// <summary>
    ///     基点となるSpatial Anchorの設置/取得を実施するためのクラス
    /// </summary>
    public class BasePointSettingsManager : MonoBehaviour, IASACallBackManager
    {
        private IDictionary<string, string> appProperties;

        private bool isLocateAnchors;
        private GameObject nextStepButtons;
        public string BasePointAnchorId { get; private set; }

    #region Inspector Properites

        [Tooltip("Set a GemeObject to visualize Spatial Anchor.")]
        public SettingPointAnchor AnchorObject;

        [Tooltip("Set a GemeObject of Menu.")]
        public BasePointSettingsMenu Menu;

    #endregion

    #region Unity Lifecycle

        /// <summary>
        ///     初期化処理を実施します
        /// </summary>
        private void Start()
        {
            try
            {
                AnchorModuleProxy.Instance.SetASACallBackManager(this);
                if (!GetAzureAnchorIdFromDisk())
                {
                    Dialog.OpenDialog("Not load Anchor Id on this device.",
                        "Please set object and create base point.First, open session.", new[] {"Ok"},
                        new[]
                        {
                            new UnityAction(() =>
                            {
                                transform.GetChild(0).gameObject.SetActive(true);
                                AnchorObject.SetActiveState(true);
                                Menu.ChangeStatus(BasePointSettingsMenu.MODE_CREATE_ANCHOR);
                            })
                        });
                }
                else
                {
                    Dialog.OpenDialog("Anchor Id is loaded.", "Please open session and find Azure Anchor.",
                        new[] {"Ok"},
                        new[]
                        {
                            new UnityAction(() =>
                            {
                                transform.GetChild(0).gameObject.SetActive(true);
                                AnchorObject.SetActiveState(false);
                                Menu.ChangeStatus(BasePointSettingsMenu.MODE_FIND_ANCHOR);
                            })
                        });
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        /// <summary>
        ///     フレーム毎に実行する処理を実施します。
        /// </summary>
        private void Update()
        {
            if (isLocateAnchors)
            {
                Menu.ChangeStatus(BaseMenu.MODE_COMPLETE);
            }
        }

    #endregion


    #region Public Methods

        /// <summary>
        ///     Azure Spatial Anchorsサービスとの接続を行い、セションを開始します。
        /// </summary>
        public async void StartAzureSession()
        {
            await AnchorModuleProxy.Instance.StartAzureSession();
        }

        /// <summary>
        ///     Azure Spatial Anchorsサービスとの接続を停止します。
        /// </summary>
        public async void StopAzureSession()
        {
            await AnchorModuleProxy.Instance.StopAzureSession();
        }

        /// <summary>
        ///     指定されたAnchorIdに対応するSpatial AnchorをAzure Spatial Anchorsサービスから取得します。
        /// </summary>
        public void FindAzureAnchor()
        {
            AnchorModuleProxy.Instance.FindAzureAnchorById(BasePointAnchorId.Split(','));
        }

        /// <summary>
        ///     Azure Spatial Anchorsサービスにアンカーを追加します。
        /// </summary>
        public async void CreateLocalCloudAnchor()
        {
            try
            {
                // アンカーに登録する情報を格納します。
                appProperties = new Dictionary<string, string>();

                // アンカーの種別を中継点に設定します。
                appProperties.Add(RouteGuideInformation.ANCHOR_TYPE, RouteGuideInformation.ANCHOR_TYPE_POINT);
                // 基点のアンカーとなるため接続するアンカーをANCHOR_ID_NOT_INITIALIZEDで設定します。
                appProperties.Add(RouteGuideInformation.PREV_ANCHOR_ID,
                    RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED);

                BasePointAnchorId =
                    await AnchorModuleProxy.Instance.CreateAzureAnchor(AnchorObject.gameObject, appProperties);
                SaveAzureAnchorIdToDisk();

                AnchorObject.DisabledEffects();

                isLocateAnchors = true;
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
                this.appProperties = appProperties;
                BasePointAnchorId = identifier;

                AnchorObject.SetActiveState(true);
                AnchorObject.DisabledEffects();
                AnchorObject.name = "Anchor:" + identifier;

                isLocateAnchors = true;

                gameObject = AnchorObject.gameObject;
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        /// <summary>
        ///     Spatial Anchorの設置完了時に実行する処理
        ///     Spatial Anchorの設置がすべて完了した場合に実行されます。
        /// </summary>
        public void OnLocatedAnchorComplete()
        {
            //実装無し
        }

        /// <summary>
        ///     アプリケーションを終了します。
        /// </summary>
        public void Exit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#elif WINDOWS_UWP
            Windows.ApplicationModel.Core.CoreApplication.Exit();
#endif
        }

        /// <summary>
        ///     次の処理ステップ（経路設定）へ進むための処理を実行します。
        /// </summary>
        public void NextStepRouteCreate()
        {
            CloseContents();
            SetRouteGuideManager.Instance.Initialize(true, BasePointAnchorId, AnchorObject, appProperties);
        }

        /// <summary>
        ///     次の処理ステップ（経路探索）へ進むための処理を実行します。
        /// </summary>
        public void NextStepWayFinding()
        {
            CloseContents();
            WayFindingManager.Instance.Initialize(true, BasePointAnchorId, AnchorObject, appProperties);
        }

    #endregion


    #region Private Methods

        /// <summary>
        ///     取得済みアンカーのIDをローカルストレージに保存します。
        /// </summary>
        private void SaveAzureAnchorIdToDisk()
        {
            try
            {
                Debug.Log("\nAnchorModuleScript.SaveAzureAnchorIDToDisk()");

                var filename = "SavedAzureAnchorID.txt";
                var path = Application.persistentDataPath;

#if WINDOWS_UWP
            var storageFolder = ApplicationData.Current.LocalFolder;
            path = storageFolder.Path.Replace('\\', '/') + "/";
#endif

                var filePath = Path.Combine(path, filename);

#if !UNITY_EDITOR
            File.WriteAllText(filePath, BasePointAnchorId);
#endif

                Debug.Log($"Current Azure anchor ID '{BasePointAnchorId}' successfully saved to path '{filePath}'");
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
        private void CloseContents()
        {
            isLocateAnchors = false;
            Menu.ChangeStatus(BaseMenu.MODE_CLOSE);
            transform.GetChild(0).gameObject.SetActive(false);
        }

        /// <summary>
        ///     ローカルストレージに保存されているアンカーのIDを取得します
        /// </summary>
        private bool GetAzureAnchorIdFromDisk()
        {
            try
            {
                Debug.Log("\nAnchorModuleScript.LoadAzureAnchorIDFromDisk()");

                var filename = "SavedAzureAnchorID.txt";
                var path = Application.persistentDataPath;

#if WINDOWS_UWP
            var storageFolder = ApplicationData.Current.LocalFolder;
            path = storageFolder.Path.Replace('\\', '/') + "/";
#endif

                var filePath = Path.Combine(path, filename);
                if (File.Exists(filePath))
                {
                    BasePointAnchorId = File.ReadAllText(filePath);
                    Debug.Log(
                        $"Current Azure anchor ID successfully updated with saved Azure anchor ID '{BasePointAnchorId}' from path '{path}'");
                    return true;
                }

                Debug.Log($"File Not Founded.'{filePath}'");
                return false;
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