// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEngine;
using UnityEngine.XR.WSA;

public class AnchorModuleScript : MonoBehaviour, IAnchorModuleScript
{
    /// <summary>
    /// 排他制御用オブジェクト
    /// </summary>
    private static object lockObj = new object();

    /// <summary>
    /// Unityのメインスレッド上で実行したい処理を格納するキュー
    /// </summary>
    private readonly Queue<Action> dispatchQueue = new Queue<Action>();

    /// <summary>
    /// Azure Spatial Anchorsから取得したAnchorの情報を格納するDictionary
    /// </summary>
    private readonly Dictionary<string, CloudSpatialAnchor> locatedAnchors =
        new Dictionary<string, CloudSpatialAnchor>();

    /// <summary>
    /// Azure Spatial Anchorsの検索時に設定する<see cref="AnchorLocateCriteria"/>
    /// </summary>
    private AnchorLocateCriteria anchorLocateCriteria;

    /// <summary>
    /// Azure Spatial Anchorsの管理クラス
    /// </summary>
    private SpatialAnchorManager cloudManager;

    /// <summary>
    /// Azure Spatial Anchors検索時に利用する監視クラス
    /// </summary>
    private CloudSpatialAnchorWatcher currentWatcher;

    /// <summary>
    /// Azure Spatial Anchorsのパラメータ：アンカー周辺を検索する際の探索範囲（単位:m）
    /// </summary>
    private float distanceInMeters;

    /// <summary>
    /// Azure Spatial Anchorsのパラメータ：アンカー周辺の検索時に取得するアンカーの上限数
    /// </summary>
    private int maxResultCount;

    /// <summary>
    /// Azure Spatial Anchorsのパラメータ：Spatial Anchor登録時のアンカーの寿命（単位:日）
    /// </summary>
    private int expiration;

    /// <summary>
    /// 特定のアンカーを中心に検索した際に見つかったアンカー一覧
    /// </summary>
    private List<string> findNearByAnchorIds = new List<string>();

    /// <summary>
    /// アンカー取得後に実行する個別処理を持つコントローラクラス
    /// </summary>
    public IASACallBackManager CallBackManager { get; set; }

#region Public Events

    /// <summary>
    /// 処理状況を出力するイベント
    /// </summary>
    public event AnchorModuleProxy.FeedbackDescription OnFeedbackDescription;

#endregion

#region Internal Methods and Coroutines

    /// <summary>
    /// Unityのメインスレッド上で実行したい処理をキューに投入します。
    /// </summary>
    /// <param name="updateAction"></param>
    private void QueueOnUpdate(Action updateAction)
    {
        lock (dispatchQueue)
        {
            dispatchQueue.Enqueue(updateAction);
        }
    }

#endregion

#region Unity Lifecycle

    /// <summary>
    /// 初期化処理を実施します
    /// </summary>
    public void Start()
    {
        try
        {
            // Azure Spatial Anchors管理用のコンポーネントを取得します。
            cloudManager = GetComponent<SpatialAnchorManager>();

            // Azure Spatial Anchorsサービスを呼出したときに発生するイベントを割り当てます。
            // Azure Spatial Anchorsから取得したアンカー情報をもとにアンカーの設置が完了した際に発生するイベント
            cloudManager.AnchorLocated += CloudManager_AnchorLocated;

            // Azure Spatial Anchorsから取得したアンカー設置処理がすべて完了すると呼ばれるイベント
            cloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;

            // Azure Spatial Anchorsへの検索条件を設定するクラスのインスタンス化
            anchorLocateCriteria = new AnchorLocateCriteria();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// フレーム毎に実行する処理を実施します。
    /// </summary>
    public void Update()
    {
        try
        {
            // Unityのメインスレッド上で実行したい処理をキューから取り出し処理を開始する。
            lock (dispatchQueue)
            {
                if (dispatchQueue.Count > 0)
                {
                    dispatchQueue.Dequeue()();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// オブジェクトの後処理（廃棄）を実施します。
    /// </summary>
    public void OnDestroy()
    {
        try
        {
            if (cloudManager != null && cloudManager.Session != null)
            {
                cloudManager.DestroySession();
            }

            if (currentWatcher != null)
            {
                currentWatcher.Stop();
                currentWatcher = null;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

#endregion

#region Public Methods

    /// <summary>
    /// Azure Spatial Anchorsサービスとの接続を行い、セションを開始します。
    /// </summary>
    /// <returns></returns>
    public async Task StartAzureSession()
    {
        try
        {
            Debug.Log("\nAnchorModuleScript.StartAzureSession()");

            OutputLog("Starting Azure session... please wait...");

            if (cloudManager.Session == null)
            {
                // Creates a new session if one does not exist
                await cloudManager.CreateSessionAsync();
            }

            // Starts the session if not already started
            await cloudManager.StartSessionAsync();

            OutputLog("Azure session started successfully");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// Azure Spatial Anchorsサービスとの接続を停止します。
    /// </summary>
    /// <returns></returns>
    public async Task StopAzureSession()
    {
        try
        {
            Debug.Log("\nAnchorModuleScript.StopAzureSession()");

            OutputLog("Stopping Azure session... please wait...");

            // Stops any existing session
            cloudManager.StopSession();

            // Resets the current session if there is one, and waits for any active queries to be stopped
            await cloudManager.ResetSessionAsync();

            OutputLog("Azure session stopped successfully", isOverWrite: true);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// Azure Spatial Anchorsから取得済みのSpatial AnchorのAppPropertiesを一括で変更します。
    /// キーがすでに存在する場合はreplaceパラメータの値に応じて置換え、追記を切り替えて処理を実施します。
    /// </summary>
    /// <param name="key">AppPropertiesのキー</param>
    /// <param name="val">キーに対応する値</param>
    /// <param name="replace">true:上書き、false:カンマ区切りで追記</param>
    public async void UpdatePropertiesAll(string key, string val, bool replace = true)
    {
        try
        {
            OutputLog("Trying to update AppProperties of Azure anchors");
            foreach (var info in locatedAnchors.Values)
            {
                await UpdateProperties(info, key, val, replace);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// Azure Spatial Anchorsサービスにアンカーを追加します。
    /// </summary>
    /// <param name="theObject">Spatial Anchorの情報として登録する現実空間に設置したオブジェクト</param>
    /// <param name="appProperties">Spatial Anchorに含める情報</param>
    /// <returns>登録時のAnchorId</returns>
    public async Task<string> CreateAzureAnchor(GameObject theObject, IDictionary<string, string> appProperties)
    {
        try
        {
            Debug.Log("\nAnchorModuleScript.CreateAzureAnchor()");

            OutputLog("Creating Azure anchor");

            // Azure Spatial Anchorsサービスの登録に必要なNative Anchorの設定を行います。
            theObject.CreateNativeAnchor();

            OutputLog("Creating local anchor");

            // Azure Spatial Anchorsサービスに登録するSpatial Anchorの情報を準備します。
            var localCloudAnchor = new CloudSpatialAnchor();

            // Spatial Anchorにふくめる情報を格納します。
            foreach (var key in appProperties.Keys)
            {
                localCloudAnchor.AppProperties.Add(key, appProperties[key]);
            }


            // Native Anchorのポインタを渡します。
            localCloudAnchor.LocalAnchor = theObject.FindNativeAnchor().GetPointer();

            // Native Anchorが正常に生成されているかを確認します。生成に失敗している時は終了します。
            if (localCloudAnchor.LocalAnchor == IntPtr.Zero)
            {
                OutputLog("Didn't get the local anchor...", LogType.Error);
                return null;
            }

            Debug.Log("Local anchor created");

            // Spatial Anchorの寿命を設定します。この日数分Azure Spatial Anchorsサービス上にAnchorが残ります。
            localCloudAnchor.Expiration = DateTimeOffset.Now.AddDays(expiration);

            OutputLog("Move your device to capture more environment data: 0%");

            // Spatial Anchorの登録に必要な空間の特徴点が必要十分になっているかを確認します。
            // RecommendedForCreateProgressが100%で必要な情報が収集できています（HoloLensの場合ほぼ一瞬でおわります）
            do
            {
                await Task.Delay(330);
                var createProgress = cloudManager.SessionStatus.RecommendedForCreateProgress;
                QueueOnUpdate(() => OutputLog($"Move your device to capture more environment data: {createProgress:0%}",
                    isOverWrite: true));
            } while (!cloudManager.IsReadyForCreate);

            try
            {
                OutputLog("Creating Azure anchor... please wait...");

                // Azure Spatial Anchorsに登録を試みます。
                await cloudManager.CreateAnchorAsync(localCloudAnchor);

                // 正常に登録できた場合は登録結果（AnchorId）が格納されたオブジェクトが返却されます。
                var success = localCloudAnchor != null;
                if (success)
                {
                    OutputLog($"Azure anchor with ID '{localCloudAnchor.Identifier}' created successfully");
                    locatedAnchors.Add(localCloudAnchor.Identifier, localCloudAnchor);
                    return localCloudAnchor.Identifier;
                }

                OutputLog("Failed to save cloud anchor to Azure", LogType.Error);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

            return null;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// 指定されたAnchorIdで登録されたAnchorを中心に他のアンカーが存在するか検索を実施します。
    /// </summary>
    /// <param name="anchorId">基準になるAnchorId</param>
    public void FindNearByAnchor(string anchorId)
    {
        try
        {
            anchorLocateCriteria.Identifiers = new string[0];
            Debug.Log("\nAnchorModuleScript.FindAzureAnchor()");
            OutputLog("Trying to find near by Azure anchor");

            // このクラスで管理している取得済みSpatial Anchorの一覧の中に指定のAnchorが存在するか確認します。
            if (!locatedAnchors.ContainsKey(anchorId))
            {
                OutputLog($"Not found anchor.id:{anchorId}.", LogType.Error);
                return;
            }

            // Azure Spatial Anchorsを検索する条件を設定します。
            // アンカー周辺を検索するためにはCriteriaにNearAnchorCriteriaのインスタンスを割り当てます。
            anchorLocateCriteria.NearAnchor = new NearAnchorCriteria();

            // 基点になるAnchorの情報を設定します。
            anchorLocateCriteria.NearAnchor.SourceAnchor = locatedAnchors[anchorId];

            // 探索範囲と、同時検出数を設定します。
            anchorLocateCriteria.NearAnchor.DistanceInMeters = distanceInMeters;
            anchorLocateCriteria.NearAnchor.MaxResultCount = maxResultCount;

            // 探索時のルールを設定します。周辺探索にはAnyStrategyを設定します。
            anchorLocateCriteria.Strategy = LocateStrategy.AnyStrategy;

            Debug.Log(
                $"Anchor locate criteria configured to Search Near by Azure anchor ID '{anchorLocateCriteria.NearAnchor.SourceAnchor.Identifier}'");

            // アンカーの探索を開始します。この処理は時間がかかるためAzure Spatial Anchorsでは
            // Watcherを生成し別スレッド上で非同期処理が実施されます。
            // Anchorの探索と配置が完了した情報から順次AnchorLocatedイベントが発生します。
            // 取得したSpatial Anchorの設置がすべて完了するとLocatedAnchorsCompleteイベントが発生します。
            if (cloudManager != null && cloudManager.Session != null)
            {
                currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);
                Debug.Log("Watcher created");
                OutputLog("Looking for Azure anchor... please wait...");
            }
            else
            {
                OutputLog("Attempt to create watcher failed, no session exists", LogType.Error);
                currentWatcher = null;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// 指定されたAnchorIdに対応するSpatial AnchorをAzure Spatial Anchorsサービスから取得します。
    /// </summary>
    /// <param name="azureAnchorIds"></param>
    public void FindAzureAnchorById(params string[] azureAnchorIds)
    {
        try
        {
            Debug.Log("\nAnchorModuleScript.FindAzureAnchor()");

            OutputLog("Trying to find Azure anchor");

            var anchorsToFind = new List<string>();

            if (azureAnchorIds != null && azureAnchorIds.Length > 0)
            {
                anchorsToFind.AddRange(azureAnchorIds);
            }
            else
            {
                OutputLog("Current Azure anchor ID is empty", LogType.Error);
                return;
            }

            // Azure Spatial Anchorsを検索する条件を設定します。
            anchorLocateCriteria = new AnchorLocateCriteria();

            // 検索するAnchorIdのリストを設定します。
            anchorLocateCriteria.Identifiers = anchorsToFind.ToArray();

            // 一度取得したアンカー情報に対して再取得した場合にローカルの情報をキャッシュとして利用するかを設定します。
            // 今回はキャッシュをバイパスするため、毎回Azure Spatial Anchorsへ問い合わせが発生します。
            anchorLocateCriteria.BypassCache = true;

            // アンカーの探索を開始します。この処理は時間がかかるためAzure Spatial Anchorsでは
            // Watcherを生成し別スレッド上で非同期処理が実施されます。
            // Anchorの探索と配置が完了した情報から順次AnchorLocatedイベントが発生します。
            // 取得したSpatial Anchorの設置がすべて完了するとLocatedAnchorsCompleteイベントが発生します。
            if (cloudManager != null && cloudManager.Session != null)
            {
                currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);

                Debug.Log("Watcher created");
                OutputLog("Looking for Azure anchor... please wait...");
            }
            else
            {
                OutputLog("Attempt to create watcher failed, no session exists", LogType.Error);

                currentWatcher = null;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// Azure Spatial Anchorsサービスから取得済みのすべてのアンカーを削除します。
    /// </summary>
    public async void DeleteAllAzureAnchor()
    {
        try
        {
            Debug.Log("\nAnchorModuleScript.DeleteAllAzureAnchor()");

            // Notify AnchorFeedbackScript
            OutputLog("Trying to find Azure anchor...");

            foreach (var AnchorInfo in locatedAnchors.Values)
            {
                // Delete the Azure anchor with the ID specified off the server and locally
                await cloudManager.DeleteAnchorAsync(AnchorInfo);
            }

            locatedAnchors.Clear();

            OutputLog("Trying to find Azure anchor...Successfully");
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// Anchor生成処理を実行するためのコントローラを設定します。
    /// </summary>
    /// <param name="iasaCallBackManager"></param>
    public void SetASACallBackManager(IASACallBackManager iasaCallBackManager)
    {
        CallBackManager = iasaCallBackManager;
    }

    /// <summary>
    /// Spatial Anchorの検索範囲を設定します。
    /// </summary>
    /// <param name="distanceInMeters">検索範囲（単位:m）</param>
    public void SetDistanceInMeters(float distanceInMeters)
    {
        this.distanceInMeters = distanceInMeters;
    }

    /// <summary>
    /// Spatial Anchorの寿命を設定します
    /// </summary>
    /// <param name="expiration">Anchorの登録期間（単位:日）</param>
    public void SetExpiration(int expiration)
    {
        this.expiration = expiration;
    }

    /// <summary>
    /// Spatial Anchorの同時検索数を設定します。
    /// </summary>
    /// <param name="distanceInMeters">検索数</param>
    public void SetMaxResultCount(int maxResultCount)
    {
        this.maxResultCount = maxResultCount;
    }
#endregion

#region Private Methods
    /// <summary>
    /// 指定されたSpatial AnchorのAppPropertiesを変更します。
    /// キーがすでに存在する場合はreplaceパラメータの値に応じて置換え、追記を切り替えて処理を実施します。
    /// </summary>
    /// <param name="currentCloudAnchor">変更対象のSpatial Anchorの情報</param>
    /// <param name="key">AppPropertiesのキー</param>
    /// <param name="val">キーに対応する値</param>
    /// <param name="replace">true:上書き、false:カンマ区切りで追記</param>
    /// <returns></returns>
    private async Task UpdateProperties(CloudSpatialAnchor currentCloudAnchor, string key, string val,
        bool replace = true)
    {
        try
        {
            OutputLog($"anchor properties.id:{currentCloudAnchor.Identifier} -- key:{key},val:{val}....");
            if (currentCloudAnchor != null)
            {
                if (currentCloudAnchor.AppProperties.ContainsKey(key))
                {
                    if (replace || currentCloudAnchor.AppProperties[key].Length == 0)
                    {
                        currentCloudAnchor.AppProperties[key] = val;
                    }
                    else
                    {
                        currentCloudAnchor.AppProperties[key] = currentCloudAnchor.AppProperties[key] + "," + val;
                    }
                }
                else
                {
                    currentCloudAnchor.AppProperties.Add(key, val);
                }

                // Start watching for Anchors
                if (cloudManager != null && cloudManager.Session != null)
                {
                    await cloudManager.Session.UpdateAnchorPropertiesAsync(currentCloudAnchor);
                    var result = await cloudManager.Session.GetAnchorPropertiesAsync(currentCloudAnchor.Identifier);

                    OutputLog(
                        $"anchor properties.id:{currentCloudAnchor.Identifier} -- key:{key},val:{val}....successfully",
                        isOverWrite: true);
                }
                else
                {
                    OutputLog("Attempt to create watcher failed, no session exists", LogType.Error);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// 処理経過を出力します。
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="logType">出力ログの種別</param>
    /// <param name="isOverWrite">直前のメッセージを上書きする</param>
    /// <param name="isReset">メッセージをクリアする</param>
    private void OutputLog(string message, LogType logType = LogType.Log, bool isOverWrite = false,
        bool isReset = false)
    {
        try
        {
            OnFeedbackDescription?.Invoke(message, isOverWrite, isReset);
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(message);
                    break;
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                case LogType.Warning:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

#endregion

#region Event Handlers

    /// <summary>
    /// Spatial Anchorの設置が完了した場合に発生するイベントで実行する処理
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="args">args</param>
    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        try
        {
            // 設置したアンカーの状態に応じて処理を実施します。
            if (args.Status == LocateAnchorStatus.Located || args.Status == LocateAnchorStatus.AlreadyTracked)
            {
                //FindNearbyAnchorsでAnchorを検索した場合AppPropertiesが空になります（バグ？）
                //このため、FindNearbyAnchorsの検索で見つかったアンカーはIDのみ集約しすべての配置が完了後
                //FindAzureAnchorByIdで再取得をかける。この処理はCloudManager_LocateAnchorsCompleted内で実施します。
                if (IsNearbyMode())
                {
                    var id = args.Anchor.Identifier;
                    QueueOnUpdate(() => Debug.Log($"Find near by Anchor id:{id}"));
                    lock (lockObj)
                    {
                        findNearByAnchorIds.Add(id);
                    }
                }
                else
                {
                    QueueOnUpdate(() => Debug.Log("Anchor recognized as a possible Azure anchor"));
                    // 取得したSpatial Anchorの情報をリストに格納します。
                    lock (lockObj)
                    {
                        if (!locatedAnchors.ContainsKey(args.Anchor.Identifier))
                        {
                            locatedAnchors.Add(args.Anchor.Identifier, args.Anchor);
                        }
                    }

                    // 取得したSpatial Anchorの情報からUnityのオブジェクトを生成し、現実空間の正しい位置に配置します。
                    QueueOnUpdate( () =>
                    {
                        var currentCloudAnchor = args.Anchor;

                        Debug.Log("Azure anchor located successfully");

                        GameObject point = null;

                        // Spatial Anchorに対応するUnityオブジェクトを生成する処理を呼出します。
                        if(CallBackManager != null && !CallBackManager.OnLocatedAnchorObject(currentCloudAnchor.Identifier,
                            locatedAnchors[currentCloudAnchor.Identifier].AppProperties, out point))
                        {
                            return;
                        }

                        if (point == null)
                        {
                            OutputLog("Not Anchor Object", LogType.Error);
                            return;
                        }

                        point.SetActive(true);
                        
                        // Notify AnchorFeedbackScript
                        OutputLog("Azure anchor located");

#if UNITY_ANDROID || UNITY_IOS
                    Pose anchorPose = Pose.identity;
                    anchorPose = currentCloudAnchor.GetPose();
#endif

#if WINDOWS_UWP || UNITY_WSA
                        // Native Anchorを生成します。
                        point.CreateNativeAnchor();

                        OutputLog("Creating local anchor");

                        // Unityオブジェクトを現実空間の正しい位置に配置します。
                        if (currentCloudAnchor != null)
                        {
                            Debug.Log("Local anchor position successfully set to Azure anchor position");

                            point.GetComponent<WorldAnchor>().SetNativeSpatialAnchorPtr(currentCloudAnchor.LocalAnchor);
                        }
#else
                    Debug.Log($"Setting object to anchor pose with position '{anchorPose.position}' and rotation '{anchorPose.rotation}'");
                    point.transform.position = anchorPose.position;
                    point.transform.rotation = anchorPose.rotation;

                    // Create a native anchor at the location of the object in question
                    point.CreateNativeAnchor();
#endif
                    });
                }
            }
            else
            {
                QueueOnUpdate(() =>
                    OutputLog(
                        $"Attempt to locate Anchor with ID '{args.Identifier}' failed, locate anchor status was not 'Located' but '{args.Status}'",
                        LogType.Error));
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// 検索したすべてのSpatial Anchorの設置が完了した後実行する処理。
    /// </summary>
    /// <param name="sender">sender</param>
    /// <param name="args">args</param>
    private void CloudManager_LocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
    {
        try
        {
            
            if (IsNearbyMode())
            {
                // NearbyAnchorで取得した場合、AppPropertiesの情報が取得できないため
                // 一度配置できたSpatial AnchorのAnchorIdを保持しておき再度ID指定でアンカーの取得を実施します。
                QueueOnUpdate(() => OutputLog("Get the spatial anchors with Anchor App Properties."));
                QueueOnUpdate(() => FindAzureAnchorById(findNearByAnchorIds.ToArray()));
            }
            else
            {
                findNearByAnchorIds.Clear();
                QueueOnUpdate(() => OutputLog("Locate Azure anchors Complete."));

                if (!args.Cancelled)
                {
                    // 検索したすべてのSpatial Anchorの設置が完了した後実行します。
                    QueueOnUpdate(() => CallBackManager?.OnLocatedAnchorComplete());
                }
                else
                {
                    QueueOnUpdate(() => OutputLog("Attempt to locate Anchor Complete failed.", LogType.Error));
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    /// NearbyAnchorでの検索かどうかをチェックします。
    /// </summary>
    /// <returns>NearbyAnchorでの検索はtrue</returns>
    private bool IsNearbyMode()
    {
        return anchorLocateCriteria?.NearAnchor != null;
    }

#endregion
}