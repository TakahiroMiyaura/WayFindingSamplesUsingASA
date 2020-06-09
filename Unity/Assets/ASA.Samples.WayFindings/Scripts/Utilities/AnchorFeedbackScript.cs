// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using TMPro;
using UnityEngine;

public class AnchorFeedbackScript : MonoBehaviour
{
    private IAnchorModuleScript anchorModuleScript;

    [SerializeField]
    private int cloumnCount = 65;

    [SerializeField]
    [Tooltip("Reference to the Text Mesh Pro component on this object.")]
    private TextMeshPro feedbackText = default;

    private string prevDescription = "";

    [SerializeField]
    private int rowCount = 7;

    private void Awake()
    {
        anchorModuleScript = AnchorModuleProxy.Instance;

        anchorModuleScript.OnFeedbackDescription += AnchorModuleScriptOnFeedbackDescription;
    }

    public static object obj = new object();

    private void AnchorModuleScriptOnFeedbackDescription(string description, bool isOverWrite = false,
        bool isReset = false)
    {
        lock (obj)
        {
            if (isReset)
            {
                feedbackText.text = "";
            }

            if (!isOverWrite || string.IsNullOrEmpty(prevDescription))
            {
                var writeRowCount = (int) Mathf.Ceil((float) description.Length / cloumnCount);
                var lines = feedbackText.text.Split('\n');
                if (lines.Length + writeRowCount > rowCount)
                {
                    feedbackText.text = "";
                    for (var i = writeRowCount; i < rowCount; i++)
                    {
                        feedbackText.text += lines[i] + "\n";
                    }
                }

                feedbackText.text += description + "\n";
            }
            else
            {
                feedbackText.text = feedbackText.text.Replace(prevDescription, description);
            }


            prevDescription = description;
        }
    }
}