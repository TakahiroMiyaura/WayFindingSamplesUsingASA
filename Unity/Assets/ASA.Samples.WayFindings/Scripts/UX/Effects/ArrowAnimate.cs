// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;

namespace Com.Reseul.ASA.Samples.WayFindings.UX.Effects
{
    /// <summary>
    ///     オブジェクトが視野外の存在する際に方向を示すインジケーターオブジェクトのアニメーションを定義するクラス。
    /// </summary>
    public class ArrowAnimate : MonoBehaviour
    {
        private float currentOffset = .7f;
        private float currentTime;

    #region Inspector Properites

        [SerializeField]
        private float intervals = 0.01f;

    #endregion

        private Material material;

    #region Unity Lifecycle

        private void Start()
        {
            material = GetComponent<MeshRenderer>().material;
        }

        private void Update()
        {
            currentTime += Time.deltaTime;
            if (intervals > currentTime)
            {
                return;
            }

            currentTime = 0;
            currentOffset = currentOffset - 0.01f;
            if (currentOffset < -1f)
            {
                currentOffset = 0f;
            }

            material.mainTextureOffset = new Vector2(currentOffset, 0);
        }

    #endregion
    }
}