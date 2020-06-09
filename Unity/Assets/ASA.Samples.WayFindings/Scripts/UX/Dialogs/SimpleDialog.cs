// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SimpleDialog : MonoBehaviour
{
    private GameObject button1;
    private GameObject button2;
    private GameObject buttonSingle;
    private TextMeshPro messageObj;

    private TextMeshPro titleObj;

    // Start is called before the first frame update
    private void Awake()
    {
        titleObj = transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
        messageObj = transform.GetChild(1).GetComponent<TextMeshPro>();
        var buttons = transform.GetChild(2);
        buttonSingle = buttons.GetChild(0).gameObject;
        button1 = buttons.GetChild(1).gameObject;
        button2 = buttons.GetChild(2).gameObject;
    }

    public void SetDialog(string title, string message, string[] buttonLabels, UnityAction[] events = null)
    {
        titleObj.text = title;
        messageObj.text = message;
        if (buttonLabels.Length == 1)
        {
            button1.SetActive(false);
            button2.SetActive(false);
            buttonSingle.SetActive(true);
            SetButtonInformation(buttonSingle, buttonLabels[0], events?[0]);
        }
        else if (buttonLabels.Length == 2)
        {
            button1.SetActive(true);
            button2.SetActive(true);
            buttonSingle.SetActive(false);
            SetButtonInformation(button1, buttonLabels[0], events?[0]);
            SetButtonInformation(button2, buttonLabels[1], events?[1]);
        }
        else
        {
            Debug.LogError("button ");
        }
    }

    private void SetButtonInformation(GameObject buttonObj, string buttonLabel, UnityAction unityAction = null)
    {
        var pressableButtonHoloLens2 = buttonObj.GetComponent<PressableButtonHoloLens2>();
        var interactable = buttonObj.GetComponent<Interactable>();
        if (unityAction != null)
        {
            pressableButtonHoloLens2.ButtonPressed.AddListener(unityAction);
            interactable.OnClick.AddListener(unityAction);
        }

        pressableButtonHoloLens2.ButtonPressed.AddListener(OwnClosed);
        interactable.OnClick.AddListener(OwnClosed);

        buttonObj.GetComponentInChildren<TextMeshPro>().text = buttonLabel;
    }

    private void OwnClosed()
    {
        Destroy(gameObject);
    }
}