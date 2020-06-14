// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Menus
{
    /// <summary>
    ///     <see cref="WayFindingManager" />で利用するメニュー管理クラス
    /// </summary>
    public class WayFindingMenu : BaseMenu
    {
        /// <summary>
        ///     メニューのステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        public override void ChangeStatus(string status)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (status.Equals(MODE_INITIALIZE))
            {
            }
            else if (status.Equals(MODE_CLOSE))
            {
                gameObject.SetActive(false);
            }
            else
            {
                throw new InvalidOperationException($"Not exits status.Status Code:{status}");
            }
        }

        /// <summary>
        ///     メニューに必要な初期化処理を実行します。
        /// </summary>
        protected override void Initialize()
        {
        }
    }
}