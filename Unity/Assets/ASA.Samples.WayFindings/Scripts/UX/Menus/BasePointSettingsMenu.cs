// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Menus
{
    /// <summary>
    ///     <see cref="BasePointSettingsManager" />で利用するメニュー管理クラス
    /// </summary>
    public class BasePointSettingsMenu : BaseMenu
    {
        /// <summary>
        ///     Anchor作成モードを表す定数
        /// </summary>
        public static string MODE_CREATE_ANCHOR = "CreateAnchor";

        /// <summary>
        ///     Anchor探索モードを表す定数
        /// </summary>
        public static string MODE_FIND_ANCHOR = "FindAnchor";

        private GameObject createAnchorButton;
        private GameObject findByAnchorId;
        private GameObject nextStepButtons;
        private GameObject startSessionButton;

        /// <summary>
        ///     メニューのステータスを変更します。
        /// </summary>
        /// <param name="status">変更するステータス</param>
        protected override void Initialize()
        {
            nextStepButtons = transform.GetChild(1).gameObject;
            startSessionButton = transform.GetChild(0).GetChild(0).gameObject;
            createAnchorButton = transform.GetChild(0).GetChild(1).gameObject;
            findByAnchorId = transform.GetChild(0).GetChild(2).gameObject;
        }


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
                nextStepButtons.SetActive(false);
            }
            else if (status.Equals(MODE_CREATE_ANCHOR))
            {
                startSessionButton.SetActive(true);
                createAnchorButton.SetActive(true);
                findByAnchorId.SetActive(false);
                nextStepButtons.SetActive(false);
            }
            else if (status.Equals(MODE_FIND_ANCHOR))
            {
                startSessionButton.SetActive(true);
                createAnchorButton.SetActive(false);
                findByAnchorId.SetActive(true);
                nextStepButtons.SetActive(false);
            }
            else if (status.Equals(MODE_COMPLETE))
            {
                startSessionButton.SetActive(false);
                createAnchorButton.SetActive(false);
                findByAnchorId.SetActive(false);
                nextStepButtons.SetActive(true);
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
    }
}