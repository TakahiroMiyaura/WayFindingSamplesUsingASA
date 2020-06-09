// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;
using UnityEngine.Events;

public class Dialog : MonoBehaviour
{
    [SerializeField]
    private SimpleDialog dialog;

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

    public static void OpenDialog(string title, string message, string[] buttonLabels, UnityAction[] events = null)
    {
        var dialogObj = Instantiate(Instance.dialog);
        dialogObj.SetDialog(title, message, buttonLabels, events);
    }
}