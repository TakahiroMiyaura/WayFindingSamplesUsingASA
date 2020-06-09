// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.ComponentModel;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.MixedReality.Toolkit.Experimental.Extensions.UX
{
    /// <summary>
    /// Input Text Field using system virtual keyboard.
    /// </summary>
    public class SystemKeyboardInputHelper : MonoBehaviour
    {
        public enum UIModeEnum
        {
            InputField,
            Label
        }

        #region private fields

#if WINDOWS_UWP
        private MixedRealityKeyboard wmrKeyboard;
#elif UNITY_IOS || UNITY_ANDROID
        private TouchScreenKeyboard touchscreenKeyboard;
#endif

        private TextMeshPro inputArea;
        private TextMeshPro placeholder;
        #endregion

        #region Unity Inspector Properties

        public UIModeEnum UIMode = UIModeEnum.InputField;

        public string text
        {
            get
            {
                if (inputArea == null)
                {
                    var componentInChildren = GetComponentsInChildren<TextMeshPro>();
                    placeholder = componentInChildren[0];
                    inputArea = componentInChildren[1];
                }
                return inputArea.text;
            }
            set
            {
                if (inputArea == null)
                {
                    var componentInChildren = GetComponentsInChildren<TextMeshPro>();
                    placeholder = componentInChildren[0];
                    inputArea = componentInChildren[1];
                }
                inputArea.text = value;
                Update();
            }
        }

        

        #endregion

        [SerializeField] private MixedRealityKeyboardPreview mixedRealityKeyboardPreview = null;

        private bool isReflectText = false;

        public void OpenSystemKeyboard()
        {
            if (UIMode == UIModeEnum.InputField)
            {
#if WINDOWS_UWP
                wmrKeyboard.ShowKeyboard(wmrKeyboard.Text, false);
#elif UNITY_IOS || UNITY_ANDROID
                touchscreenKeyboard = TouchScreenKeyboard.Open(string.Empty, TouchScreenKeyboardType.Default, false, false,
                    false, false);
#endif
            }
        }
        
        private void Start()
        {
            var componentInChildren = GetComponentsInChildren<TextMeshPro>();
            placeholder = componentInChildren[0];
            inputArea = componentInChildren[1];

            if (mixedRealityKeyboardPreview != null)
            {
                mixedRealityKeyboardPreview.gameObject.SetActive(false);
            }

#if WINDOWS_UWP
            // Windows mixed reality keyboard initialization goes here
            wmrKeyboard = gameObject.AddComponent<MixedRealityKeyboard>();
            wmrKeyboard.OnShowKeyboard?.AddListener(() =>
            {
                if (mixedRealityKeyboardPreview != null)
                {
                    mixedRealityKeyboardPreview.gameObject.SetActive(true);
                }
            });
            wmrKeyboard.OnHideKeyboard?.AddListener(() =>
            {
                if (mixedRealityKeyboardPreview != null)
                {
                    mixedRealityKeyboardPreview.gameObject.SetActive(false);
                }
            });
#endif
        }
        
        private void Update()
        {
            if (UIMode == UIModeEnum.InputField)
            {
#if WINDOWS_UWP
                if (wmrKeyboard.Visible)
                {
                    if (mixedRealityKeyboardPreview != null)
                    {
                        mixedRealityKeyboardPreview.Text = wmrKeyboard.Text;
                        mixedRealityKeyboardPreview.CaretIndex = wmrKeyboard.CaretIndex;
                    }
                    isReflectText = true;
                }
                else
                {
                    if(isReflectText)
                    {
                        inputArea.text = wmrKeyboard.Text;
                        isReflectText = false;
                    }
                    if (mixedRealityKeyboardPreview != null)
                    {
                        mixedRealityKeyboardPreview.Text = string.Empty;
                        mixedRealityKeyboardPreview.CaretIndex = 0;
                    }
                }
#elif UNITY_IOS || UNITY_ANDROID
                if (touchscreenKeyboard != null)
                {
                    inputArea.text = touchscreenKeyboard.text;
                    if (!TouchScreenKeyboard.visible)
                    {
                        touchscreenKeyboard = null;
                    }
                }
#endif
            }

            if (inputArea.text.Length == 0)
            {
                inputArea.gameObject.SetActive(false);
                placeholder.gameObject.SetActive(true);
            }
            else
            {
                inputArea.gameObject.SetActive(true);
                placeholder.gameObject.SetActive(false);
            }
        }

    }
}