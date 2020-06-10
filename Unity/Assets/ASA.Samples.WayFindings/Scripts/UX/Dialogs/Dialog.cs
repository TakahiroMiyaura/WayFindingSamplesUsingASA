// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// ダイアログ出力のためのユーティリティクラス
/// </summary>
public class Dialog : MonoBehaviour
{

    [SerializeField]
    [Tooltip("Set the dialog prefab to be used to display.")]
    private SimpleDialog dialog = null;

    /// <summary>
    /// このクラスのインスタンスを取得します。
    /// </summary>
    public static Dialog Instance
    {
        get
        {
            var module = FindObjectsOfType<Dialog>();
            if (module.Length == 1)
            {
                return module[0];
            }

            Debug.LogWarning(
                "Not found an existing Dialog in your scene. The Dialog requires only one.");
            return null;
        }
    }

    /// <summary>
    /// ダイアログを表示します
    /// </summary>
    /// <param name="title">タイトル</param>
    /// <param name="message">メッセージ</param>
    /// <param name="buttonLabels">ボタンのラベル。最大2要素設定します。</param>
    /// <param name="events">ボタン押下時のイベント</param>
    public static void OpenDialog(string title, string message, string[] buttonLabels, UnityAction[] events = null)
    {
        var dialogObj = Instantiate(Instance.dialog);
        dialogObj.SetDialog(title, message, buttonLabels, events);
    }
}