// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using TMPro;
using UnityEngine;

/// <summary>
/// デバッグ用の情報を出力するクラス
/// </summary>
public class DebugWindowMessaging : MonoBehaviour
{
    private static DebugWindowMessaging debugWindow;
    private int lineCount;
    private static object lockObj = new object();

#region Inspector Properites

    [Tooltip("Enables or disables the output of the log.")]
    public bool DebugWindowEnabled = false;

    [Tooltip("The text object to be output to the log.")]
    public TextMeshPro DebugText;

#endregion

#region Unity Lifecycle
    /// <summary>
    /// 起動時の初期処理を実施します。
    /// </summary>
    private void Awake()
    {
        if (debugWindow == null)
        {
            debugWindow = this;
        }

        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

#endregion

#region Private Methods

    /// <summary>
    /// メッセージをテキストフィールドに出力します。
    /// </summary>
    /// <param name="message">メッセージ</param>
    private void Write(string message)
    {
        if (!DebugWindowEnabled)
        {
            return;
        }

        if (lineCount >= 50)
        {
            DebugText.text = "";
            lineCount = 0;
        }

        //        lock (lockObj)
        //        {
        //            var filename = "Logdata.txt";
        //            var path = Application.persistentDataPath;

        //#if WINDOWS_UWP
        //        Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //        path = storageFolder.Path.Replace('\\', '/') + "/";
        //#endif

        //            var filePath = System.IO.Path.Combine(path, filename);
        //            System.IO.File.AppendAllText(filePath, message + " \n");
        //        }

        DebugText.text += message + " \n";

        lineCount++;
    }

    /// <summary>
    /// ログを出力します。
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="stackTrace">スタックトレース</param>
    /// <param name="type">ログ種別</param>
    private void HandleLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            debugWindow.DebugText.GetComponent<Renderer>().material.color = Color.red;
        }

        debugWindow.Write(message);
        debugWindow.DebugText.GetComponent<Renderer>().material.color = Color.white;
    }

#endregion
}