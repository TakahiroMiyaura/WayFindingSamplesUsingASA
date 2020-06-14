// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Menus
{
    /// <summary>
    /// メニュー用の抽象クラスです。
    /// </summary>
    public abstract class BaseMenu : MonoBehaviour
    {
        /// <summary>
        ///     初期化モードを表す定数
        /// </summary>
        public static string MODE_INITIALIZE = "Initialize";

        /// <summary>
        ///     作業完了モードを表す定数
        /// </summary>
        public static string MODE_COMPLETE = "Complete";

        /// <summary>
        ///     閉じるモードを表す定数
        /// </summary>
        public static string MODE_CLOSE = "Close";

        private bool isInitialize;

        protected void OnEnable()
        {
            if (isInitialize)
            {
                return;
            }

            isInitialize = true;

            Initialize();
        }

        /// <summary>
        ///     メニューのステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        public abstract void ChangeStatus(string status);

        /// <summary>
        ///     メニューに必要な初期化処理を実行します。
        /// </summary>
        protected abstract void Initialize();
    }
}