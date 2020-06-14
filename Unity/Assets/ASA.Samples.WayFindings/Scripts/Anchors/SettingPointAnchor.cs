// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Com.Reseul.ASA.Samples.WayFindings.UX.Effects;
using Com.Reseul.ASA.Samples.WayFindings.UX.KeyboardHelpers;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.Anchors
{
    /// <summary>
    ///     Spatial Anchor可視化に利用する経路設定時のアンカー用のコンポーネントです。
    /// </summary>
    public class SettingPointAnchor : MonoBehaviour
    {
        /// <summary>
        ///     アンカーのモード
        /// </summary>
        public enum AnchorMode
        {
            /// <summary>
            ///     基準点
            /// </summary>
            BasePoint,

            /// <summary>
            ///     中継点
            /// </summary>
            Point,

            /// <summary>
            ///     目的地
            /// </summary>
            Destination
        }

        private ArrowAnimate arrowAnimate;
        private HandInteractionHint handInteractionHint;
        private NearInteractionGrabbable interactionGrabbable;
        private bool isInitialized;
        private ManipulationHandler manipulationHandler;
        private Vector3 pos1;
        private Vector3 pos2;
        private SystemKeyboardInputHelper systemKeyboardInputHelper;
        private ToolTip toolTip;

        /// <summary>
        ///     テキストフィールドに設定した目的地の値を設定します。
        /// </summary>
        public string Text { get; private set; }

    #region Unity Lifecycle

        /// <summary>
        ///     初期化処理を実施します
        /// </summary>
        private void OnEnable()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            manipulationHandler = GetComponent<ManipulationHandler>();
            interactionGrabbable = GetComponent<NearInteractionGrabbable>();
            handInteractionHint = GetComponentInChildren<HandInteractionHint>();
            arrowAnimate = GetComponentInChildren<ArrowAnimate>();
            toolTip = GetComponentInChildren<ToolTip>();
            systemKeyboardInputHelper = GetComponentInChildren<SystemKeyboardInputHelper>();
            pos1 = handInteractionHint.transform.GetChild(1).transform.position;
            pos2 = handInteractionHint.transform.GetChild(2).transform.position;
        }


        /// <summary>
        ///     フレーム毎に実行する処理を実施します。
        /// </summary>
        private void Update()
        {
            ///回転系を何とかしたい
            transform.rotation =
                Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
            //handInteractionHint.transform.GetChild(0).rotation = transform.rotation;
            handInteractionHint.transform.GetChild(1).transform.position = transform.rotation * pos1;
            handInteractionHint.transform.GetChild(2).transform.position = transform.rotation * pos2;

            Text = systemKeyboardInputHelper?.text;
        }

    #endregion

    #region Public Methods

        /// <summary>
        ///     アンカーの可視化時の各種エフェクトを停止します。
        /// </summary>
        public void DisabledEffects()
        {
            manipulationHandler.enabled = false;
            interactionGrabbable.enabled = false;
            handInteractionHint?.gameObject.SetActive(false);
            arrowAnimate?.transform.parent.gameObject.SetActive(false);
            toolTip?.transform.parent.gameObject.SetActive(false);
        }

        /// <summary>
        ///     オブジェクトの有効無効を設定します。
        /// </summary>
        /// <param name="state"></param>
        public void SetActiveState(bool state)
        {
            gameObject.SetActive(state);
        }

        /// <summary>
        ///     アンカーのモードを設定します
        /// </summary>
        /// <param name="mode"></param>
        public void SetAnchorMode(AnchorMode mode)
        {
            OnEnable();
            switch (mode)
            {
                case AnchorMode.BasePoint:
                    systemKeyboardInputHelper.gameObject.SetActive(false);
                    break;
                case AnchorMode.Destination:
                    systemKeyboardInputHelper.gameObject.SetActive(true);
                    break;
                case AnchorMode.Point:
                    systemKeyboardInputHelper.gameObject.SetActive(false);
                    break;
            }
        }

    #endregion
    }
}