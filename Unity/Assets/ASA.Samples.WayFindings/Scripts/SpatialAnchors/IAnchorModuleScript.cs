// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Azure Spatial Anchorsの処理を実行するための管理クラスに実装すインターフェース
/// </summary>
public interface IAnchorModuleScript
{
    /// <summary>
    /// 初期化処理を実施します
    /// </summary>
    void Start();

    /// <summary>
    /// フレーム毎に実行する処理を実施します。
    /// </summary>
    void Update();

    /// <summary>
    /// オブジェクトの後処理（廃棄）を実施します。
    /// </summary>
    void OnDestroy();

    /// <summary>
    /// Azure Spatial Anchorsサービスとの接続を行い、セションを開始します。
    /// </summary>
    /// <returns></returns>
    Task StartAzureSession();

    /// <summary>
    /// Azure Spatial Anchorsサービスとの接続を停止します。
    /// </summary>
    /// <returns></returns>
    Task StopAzureSession();

    /// <summary>
    /// Azure Spatial Anchorsから取得済みのSpatial AnchorのAppPropertiesを一括で変更します。
    /// キーがすでに存在する場合はreplaceパラメータの値に応じて置換え、追記を切り替えて処理を実施します。
    /// </summary>
    /// <param name="key">AppPropertiesのキー</param>
    /// <param name="val">キーに対応する値</param>
    /// <param name="replace">true:上書き、false:カンマ区切りで追記</param>

    void UpdatePropertiesAll(string key, string val, bool replace = true);

    /// <summary>
    /// Spatial Anchorの検索範囲を設定します。
    /// </summary>
    /// <param name="distanceInMeters">検索範囲（単位:m）</param>
    void SetDistanceInMeters(float distanceInMeters);

    /// <summary>
    /// Spatial Anchorの同時検索数を設定します。
    /// </summary>
    /// <param name="distanceInMeters">検索数</param>
    void SetMaxResultCount(int maxResultCount);

    /// <summary>
    /// Spatial Anchorの寿命を設定します
    /// </summary>
    /// <param name="expiration">Anchorの登録期間（単位:日）</param>
    void SetExpiration(int expiration);

    /// <summary>
    /// Azure Spatial Anchorsサービスにアンカーを追加します。
    /// </summary>
    /// <param name="theObject">Spatial Anchorの情報として登録する現実空間に設置したオブジェクト</param>
    /// <param name="appProperties">Spatial Anchorに含める情報</param>
    /// <returns>登録時のAnchorId</returns>
    Task<string> CreateAzureAnchor(GameObject theObject, IDictionary<string, string> appProperties);
    
    /// <summary>
    /// 指定されたAnchorIdで登録されたAnchorを中心に他のアンカーが存在するか検索を実施します。
    /// </summary>
    /// <param name="anchorId">基準になるAnchorId</param>
    void FindNearByAnchor(string anchorId);

    /// <summary>
    /// 指定されたAnchorIdで登録されたAnchorを中心に他のアンカーが存在するか検索を実施します。
    /// </summary>
    /// <param name="anchorId">基準になるAnchorId</param>
    void FindAzureAnchorById(params string[] azureAnchorIds);

    /// <summary>
    /// Azure Spatial Anchorsサービスから取得済みのすべてのアンカーを削除します。
    /// </summary>
    void DeleteAllAzureAnchor();

    /// <summary>
    /// 処理状況を出力するイベント
    /// </summary>
    event AnchorModuleProxy.FeedbackDescription OnFeedbackDescription;

    /// <summary>
    /// アンカー取得後に実行する個別処理を持つコントローラクラス
    /// </summary>
    IASACallBackManager CallBackManager { set; get; }

    /// <summary>
    /// Anchor生成処理を実行するためのコントローラを設定します。
    /// </summary>
    /// <param name="iasaCallBackManager"></param>
    void SetASACallBackManager(IASACallBackManager iasaCallBackManager);
}