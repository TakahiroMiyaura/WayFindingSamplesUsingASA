// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Dialogs
{
    /// <summary>
    ///     シンプルなダイアログ処理を実現するクラス。
    ///     MRTK V2.4.0に追加されたダイアログの外観をまねて作成しています。
    /// </summary>
    public class SimpleDialog : MonoBehaviour
    {
        private GameObject button1;
        private GameObject button2;
        private GameObject buttonSingle;
        private TextMeshPro messageObj;

        private TextMeshPro titleObj;

    #region Unity Lifecycle

        /// <summary>
        ///     起動時の初期処理を実施します。
        /// </summary>
        private void Awake()
        {
            titleObj = transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
            messageObj = transform.GetChild(1).GetComponent<TextMeshPro>();
            var buttons = transform.GetChild(2);
            buttonSingle = buttons.GetChild(0).gameObject;
            button1 = buttons.GetChild(1).gameObject;
            button2 = buttons.GetChild(2).gameObject;
        }

    #endregion

    #region Public Methods

        /// <summary>
        ///     表示するダイアログに情報を設定します。
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="buttonLabels">ボタンのラベル。最大2要素設定します。</param>
        /// <param name="events">ボタン押下時のイベント</param>
        public void SetDialog(string title, string message, string[] buttonLabels, UnityAction[] events = null)
        {
            titleObj.text = title;
            messageObj.text = message;
            if (buttonLabels.Length == 1)
            {
                button1.SetActive(false);
                button2.SetActive(false);
                buttonSingle.SetActive(true);
                SetButtonInformation(buttonSingle, buttonLabels[0], events?[0]);
            }
            else if (buttonLabels.Length == 2)
            {
                button1.SetActive(true);
                button2.SetActive(true);
                buttonSingle.SetActive(false);
                SetButtonInformation(button1, buttonLabels[0], events?[0]);
                SetButtonInformation(button2, buttonLabels[1], events?[1]);
            }
            else
            {
                Debug.LogError("button ");
            }
        }

    #endregion

    #region Private Methods

        /// <summary>
        ///     ボタンの情報を設定します。
        /// </summary>
        /// <param name="buttonObj">ダイアログ内のボタンオブジェクト</param>
        /// <param name="buttonLabel">ボタンのラベル情報</param>
        /// <param name="unityAction">割り当てるイベント処理</param>
        private void SetButtonInformation(GameObject buttonObj, string buttonLabel, UnityAction unityAction = null)
        {
            var pressableButtonHoloLens2 = buttonObj.GetComponent<PressableButtonHoloLens2>();
            var interactable = buttonObj.GetComponent<Interactable>();
            if (unityAction != null)
            {
                pressableButtonHoloLens2.ButtonPressed.AddListener(unityAction);
                interactable.OnClick.AddListener(unityAction);
            }

            pressableButtonHoloLens2.ButtonPressed.AddListener(OwnClosed);
            interactable.OnClick.AddListener(OwnClosed);

            buttonObj.GetComponentInChildren<TextMeshPro>().text = buttonLabel;
        }

        /// <summary>
        ///     自身を閉じる処理
        /// </summary>
        private void OwnClosed()
        {
            Destroy(gameObject);
        }

    #endregion
    }
}