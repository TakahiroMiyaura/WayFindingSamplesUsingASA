// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;

/// <summary>
///     Azure Spatial Anchorsのサービスへのアクセスを行うプロキシクラス
/// </summary>
/// <remarks>
///     Azure Spatial AnchorsはUnity Editor上ではエラーで動作しないため、Unity Editor上はスタブで動作するようにこのクラスを提供しています。
/// </remarks>
public class AnchorModuleProxy : MonoBehaviour
{
    /// <summary>
    ///     処理の途中経過を表示するためのログ出力用デリゲート。別途、<see cref="AnchorFeedbackScript" />内で呼出します。
    /// </summary>
    /// <param name="description">メッセージ内容</param>
    /// <param name="isOverWrite">直前のメッセージを上書きするかどうか。Trueの場合は上書きする</param>
    /// <param name="isReset">直前のメッセージを削除するかどうか。Trueの場合はこれまでの出力を削除してから表示する</param>
    public delegate void FeedbackDescription(string description, bool isOverWrite = false, bool isReset = false);

    public float DistanceInMeters => distanceInMeters;

#region Static Methods

    /// <summary>
    ///     Azure Spatial Anchorsの処理を実行するクラスのインスタンスを取得します。
    /// </summary>
    public static IAnchorModuleScript Instance
    {
        get
        {
#if UNITY_EDITOR
            // Unity Editor実行時にはスタブで処理を実行する
            var module = FindObjectsOfType<AnchorModuleScriptForStub>();
#else
            var module = FindObjectsOfType<AnchorModuleScript>();
#endif
            if (module.Length == 1)
            {
                var proxy = FindObjectOfType<AnchorModuleProxy>();
                //Azure Spatial Anchors で利用するパラメータを設定します。
                module[0].SetDistanceInMeters(proxy.distanceInMeters);
                module[0].SetMaxResultCount(proxy.maxResultCount);
                module[0].SetExpiration(proxy.Expiration);
                return module[0];
            }

            Debug.LogWarning(
                "Not found an existing AnchorModuleScript in your scene. The Anchor Module Script requires only one.");
            return null;
        }
    }

#endregion

#region Unity Lifecycle

    private void Start()
    {
#if UNITY_EDITOR
        // Unity Editor実行時はAzure Spatial Anchors本体のオブジェクトを無効化します。
        transform.GetChild(0).gameObject.SetActive(false);
#endif
    }

#endregion

#region Inspector Properites

    [Header("NearbySetting")]
    [SerializeField]
    [Tooltip("Maximum distance in meters from the source anchor (defaults to 5).")]
    private float distanceInMeters = 5f;

    [SerializeField]
    [Tooltip("Maximum desired result count (defaults to 20).")]
    private int maxResultCount = 20;

    [Header("CreateAnchorParams")]
    [SerializeField]
    [Tooltip("The number of days until the anchor is automatically deleted")]
    private int Expiration = 7;

#endregion
}