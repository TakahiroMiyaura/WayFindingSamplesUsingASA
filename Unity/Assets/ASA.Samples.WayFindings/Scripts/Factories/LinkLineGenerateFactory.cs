// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Com.Reseul.ASA.Samples.WayFindings.UX.Effects;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.Factories
{
    /// <summary>
    ///     各種アンカーを生成するクラス
    /// </summary>
    public class LinkLineGenerateFactory : MonoBehaviour
    {
        private static LinkLineGenerateFactory instanceGenerate;

    #region Inspector Properites

        [SerializeField]
        [Tooltip("Set LinkLineDataProvider prefab for create instance.")]
        private LinkLineDataProvider linkLine = null;

    #endregion

    #region Public Static Methods

        /// <summary>
        ///     このクラスのインスタンスを取得します。
        /// </summary>
        private static LinkLineGenerateFactory Instance
        {
            get
            {
                if (instanceGenerate == null)
                {
                    var module = FindObjectsOfType<LinkLineGenerateFactory>();
                    if (module.Length == 1)
                    {
                        instanceGenerate = module[0];
                    }
                    else
                    {
                        Debug.LogWarning(
                            "Not found an existing Dialog in your scene. The Dialog requires only one.");
                    }
                }

                return instanceGenerate;
            }
        }

    #endregion

    #region Public Static Methods

        /// <summary>
        ///     アンカー同士をつなぐLineRendererを描画します。
        /// </summary>
        /// <param name="parent">生成したオブジェクトの親</param>
        /// <param name="fromPoint">始点になるアンカー</param>
        /// <param name="toPoint">終点になるアンカー</param>
        /// <returns></returns>
        public static LinkLineDataProvider CreateLinkLineObject(GameObject fromPoint, GameObject toPoint,
            Transform parent = null)
        {
            var linkLineObj = Instantiate(Instance.linkLine);
            linkLineObj.CreateLinkLineObject(parent, fromPoint, toPoint);
            return linkLineObj;
        }

    #endregion
    }
}