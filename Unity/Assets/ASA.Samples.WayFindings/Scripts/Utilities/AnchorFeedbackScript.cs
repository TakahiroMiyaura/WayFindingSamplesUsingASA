// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Com.Reseul.ASA.Samples.WayFindings.SpatialAnchors;
using TMPro;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.Utilities
{
    /// <summary>
    ///     指定のラベル欄に処理中ログを出力します。
    /// </summary>
    public class AnchorFeedbackScript : MonoBehaviour
    {
        private static object lockObj = new object();

        // ラベル出力を行う処理クラス
        private IAnchorModuleScript anchorModuleScript;
        private string prevDescription = "";

    #region Unity Lifecycle

        /// <summary>
        ///     起動時の初期処理を実施します。
        /// </summary>
        private void Awake()
        {
            anchorModuleScript = AnchorModuleProxy.Instance;

            anchorModuleScript.OnFeedbackDescription += AnchorModuleScriptOnFeedbackDescription;
        }

    #endregion

    #region Private Methods

        /// <summary>
        ///     ラベル領域に処理内容を出力する
        /// </summary>
        /// <param name="description">記載内容</param>
        /// <param name="isOverWrite">直前のメッセージを上書きする</param>
        /// <param name="isReset">メッセージをクリアする</param>
        private void AnchorModuleScriptOnFeedbackDescription(string description, bool isOverWrite = false,
            bool isReset = false)
        {
            lock (lockObj)
            {
                if (isReset)
                {
                    feedbackText.text = "";
                }

                if (!isOverWrite || string.IsNullOrEmpty(prevDescription))
                {
                    var writeRowCount = (int) Mathf.Ceil((float) description.Length / cloumnCount);
                    var lines = feedbackText.text.Split('\n');
                    if (lines.Length + writeRowCount > rowCount)
                    {
                        feedbackText.text = "";
                        for (var i = writeRowCount; i < lines.Length; i++)
                        {
                            feedbackText.text += lines[i] + "\n";
                        }
                    }

                    feedbackText.text += description + "\n";
                }
                else
                {
                    feedbackText.text = feedbackText.text.Replace(prevDescription, description);
                }


                prevDescription = description;
            }
        }

    #endregion

    #region Inspector Properites

        [SerializeField]
        [Tooltip("Number of characters per line.")]
        private int cloumnCount = 65;

        [SerializeField]
        [Tooltip("Reference to the Text Mesh Pro component on this object.")]
        private TextMeshPro feedbackText = default;

        [SerializeField]
        [Tooltip("number of lines.")]
        private int rowCount = 7;

    #endregion
    }
}