// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Menus
{
    /// <summary>
    /// 登録済みの目的地一覧を表示するためのメニュークラス
    /// </summary>
    public class SelectDestinationMenu : MonoBehaviour
    {
        public delegate void SelectDestination(string destination);


        public void GenerateDestination(string[] destinations, SelectDestination onSelected)
        {
            if (destinations.Length > 0)
            {
                var button = transform.GetChild(1).GetChild(0).gameObject;
                SetSelectButton(button, destinations[0], onSelected);

                for (var i = 1; i < destinations.Length; i++)
                {
                    button = Instantiate(button);
                    button.transform.parent = transform.GetChild(1);
                    SetSelectButton(button, destinations[i], onSelected);
                }
            }
        }

        /// <summary>
        ///     目的地選択ボタンにに目的地名を設定します。
        /// </summary>
        /// <param name="button">ボタンオブジェクト</param>
        /// <param name="destinaion">目的値名</param>
        private void SetSelectButton(GameObject button, string destinaion, SelectDestination onSelected)
        {
            button.name = destinaion;
            button.GetComponentInChildren<TextMeshPro>().text = destinaion;
            button.GetComponent<Interactable>().OnClick.AddListener(() => onSelected?.Invoke(destinaion));
            button.GetComponent<PressableButtonHoloLens2>().ButtonPressed
                .AddListener(() => onSelected?.Invoke(destinaion));
        }

        public void SetActive(bool enabled)
        {
            gameObject.SetActive(enabled);
            transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        }
    }
}