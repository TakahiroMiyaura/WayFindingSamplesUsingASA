// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.IO;
using TMPro;
using UnityEngine;

//#if WINDOWS_UWP
//using Windows.Storage;
//#endif

public class DebugWindowMessaging : MonoBehaviour
{
    private static DebugWindowMessaging debugWindow;

    public bool _debugWindowEnabled = false;

    public TextMeshPro debugText;
    private int lineCount;

    private bool parentWindow;

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

    private static object lockObj = new object();
    private void Write(string message)
    {
        if (!_debugWindowEnabled)
        {
            return;
        }

        if (lineCount >= 50)
        {
            debugText.text = "";
            lineCount = 0;
        }

//        lock (lockObj)
//        {
//            var filename = "Logdata.txt";
//            var path = Application.persistentDataPath;

//#if WINDOWS_UWP
//        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
//        path = storageFolder.Path.Replace('\\', '/') + "/";
//#endif

//            var filePath = Path.Combine(path, filename);
//            File.AppendAllText(filePath, message + " \n");
//        }

        debugText.text += message + " \n";

        lineCount++;
    }


    private void HandleLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Error)
        {
            debugWindow.debugText.GetComponent<Renderer>().material.color = Color.red;
        }

        debugWindow.Write(message);
        debugWindow.debugText.GetComponent<Renderer>().material.color = Color.white;
    }

    public static void Clear()
    {
        debugWindow.debugText.text = "";
    }
}