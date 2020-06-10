// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
///     経路探索を実施するためのクラス
/// </summary>
public class WayFindingManager : MonoBehaviour, IASACallBackManager
{
    private static object lockObj = new object();
    private GameObject basePointAnchor;

    private string basePointAnchorId;
    private IDictionary<string, string> basePointAppProperties;
    private string currentAnchorId = RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED;

#region Static Methods

    public static WayFindingManager Instance
    {
        get
        {
            var module = FindObjectsOfType<WayFindingManager>();
            if (module.Length == 1)
            {
                return module[0];
            }

            Debug.LogWarning(
                "Not found an existing SetRouteGuideManager in your scene. The SetRouteGuideManager requires only one.");
            return null;
        }
    }

#endregion

#region Inspector Properites

    [Tooltip("Set the destination title.")]
    public string Destination;

    [Tooltip("Set a prefab to visualize Spatial Anchor.")]
    public GameObject DestinationPointPrefab;


    [Tooltip("Set a GameObject of Dialog")]
    public SimpleDialog Dialog;

    [Tooltip("Set a GameObject of Instruction.")]
    public GameObject Instruction;

    [Tooltip("Set a GameObject of LinkLine.")]
    public GameObject LinkLinePrefab;

    [Tooltip("Set a GameObject of Menu.")]
    public GameObject Operations;

    [Tooltip("Set a GameObject of RouteGuideInformation.")]
    public RouteGuideInformation RouteInfo;

    [Tooltip("Set a GameObject of Select Destination Menu.")]
    public GameObject SelectDestinationMenu;

#endregion

#region Public Methods

    /// <summary>
    ///     Spatial Anchorの設置完了時に実行する処理
    ///     Spatial Anchorの設置がすべて完了した場合に実行されます。
    /// </summary>
    public void OnLocatedAnchorComplete()
    {
        try
        {
            foreach (var current in RouteInfo.LocatedAnchors.Keys)
            {
                Debug.Log(
                    $"@Current:{current} IsLinkLineCreated:{RouteInfo.LocatedAnchors[current].IsLinkLineCreated}");
                var appProperties = RouteInfo.LocatedAnchors[current].AppProperties;
                if (!RouteInfo.LocatedAnchors[current].IsLinkLineCreated)
                {
                    RouteInfo.LocatedAnchors[current].IsLinkLineCreated = true;
                    var dest = RouteInfo.LocatedAnchors[current].AnchorObject;
                    if (appProperties.ContainsKey(RouteGuideInformation.PREV_ANCHOR_ID))
                    {
                        var key = appProperties[RouteGuideInformation.PREV_ANCHOR_ID];
                        if (RouteInfo.LocatedAnchors.TryGetValue(key, out var anchorInfo))
                        {
                            var prevObj = anchorInfo.AnchorObject;
                            Debug.Log($"@LinkLine:{prevObj.name} to {dest.name}");

                            var animation = prevObj.GetComponentInChildren<DestinationPoint>();
                            var indicator = prevObj.GetComponentInChildren<NextAnchorRnageMarker>();
                            if (animation != null)
                            {
                                animation.NearAutoSearchObjectTransform.gameObject.SetActive(false);
                                animation.DirectionIndicatorObjectTransform.gameObject.SetActive(false);
                            }
                            else if (indicator != null)
                            {
                                indicator.gameObject.SetActive(false);
                            }

                            Debug.Log("@@ Create LinkLine Object.");
                            var linkLine = Instantiate(LinkLinePrefab);
                            linkLine.name = "Link:" + key + " > " + current;
                            linkLine.transform.parent = RouteInfo.RootLinkLineObjects;
                            var linkLineComponent = linkLine.GetComponent<LinkLineDataProvider>();
                            linkLineComponent.FromPoint = prevObj;
                            linkLineComponent.ToPoint = dest;
                            Debug.Log("@@ Create LinkLine Object....successfully");
                        }
                    }
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
    ///     Spatial Anchorの可視化に必要なオブジェクトの生成を行います。
    ///     Spatial Anchorの設置が完了した場合に発生するイベント内で実行されます。
    /// </summary>
    /// <param name="identifier">AnchorId</param>
    /// <param name="appProperties">Spatial Anchorに含まれるAppProperties</param>
    /// <param name="gameObject">可視化に利用するオブジェクト</param>
    /// <returns>アンカーの設置対象か。trueの場合設置対象</returns>
    public bool OnLocatedAnchorObject(string identifier,
        IDictionary<string, string> appProperties, out GameObject gameObject)
    {
        try
        {
            appProperties.TryGetValue(RouteGuideInformation.ANCHOR_TYPE, out var anchorType);

            var destinations = new string[0];
            if (appProperties.TryGetValue(RouteGuideInformation.DESTINATION_TITLE, out var destination))
            {
                destinations = destination.Split(',');
            }

            // 指定ルート以外のアンカー情報に対しては可視化を行わない。
            if (!destinations.Any(x => x.Equals(Destination)))
            {
                gameObject = null;
                return false;
            }

            GameObject point;
            if (!RouteInfo.LocatedAnchors.ContainsKey(identifier))
            {
                point = Instantiate(DestinationPointPrefab);
                lock (lockObj)
                {
                    RouteInfo.LocatedAnchors.Add(identifier,
                        new RouteGuideInformation.AnchorInfo(appProperties, point,
                            currentAnchorId.Equals(RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED)));
                }
            }
            else
            {
                point = RouteInfo.LocatedAnchors[identifier].AnchorObject;

                RouteInfo.LocatedAnchors[identifier] =
                    new RouteGuideInformation.AnchorInfo(appProperties, point,
                        currentAnchorId.Equals(RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED));
            }

            point.name = "Anchor:" + identifier;

            point.transform.parent = RouteInfo.RootPointObjects;

            currentAnchorId = identifier;

            var dest = point.GetComponent<DestinationPoint>();

            if (dest != null)
            {
                dest.Identifier = identifier;
                if (RouteGuideInformation.ANCHOR_TYPE_DESTINATION.Equals(anchorType))
                {
                    dest.DestinationTitle = destination;
                }
            }

            gameObject = point;
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    ///     オブジェクトの有効無効を設定します。
    /// </summary>
    /// <param name="enabled"></param>
    public void IsContentsStart(bool enabled, string anchorId = null,
        GameObject anchorObj = null, IDictionary<string, string> appProperties = null)
    {
        try
        {
            AnchorModuleProxy.Instance.SetASACallBackManager(this);

            Instruction?.SetActive(enabled);
            RouteInfo?.gameObject.SetActive(enabled);
            Operations?.SetActive(enabled);
            SelectDestinationMenu?.SetActive(enabled);
            if (anchorId == null || anchorObj == null || appProperties == null)
            {
                Debug.LogWarning("null reference the base point anchor information.");
                return;
            }

            basePointAnchorId = anchorId;
            basePointAnchor = anchorObj;
            var locatedAnchors = RouteInfo.LocatedAnchors;
            if (!locatedAnchors.ContainsKey(basePointAnchorId))
            {
                locatedAnchors.Add(basePointAnchorId,
                    new RouteGuideInformation.AnchorInfo(appProperties, basePointAnchor));
            }

            basePointAppProperties = appProperties;
            if (basePointAppProperties.TryGetValue(RouteGuideInformation.DESTINATION_TITLE, out var rowData))
            {
                var destinations = rowData.Split(',');
                if (destinations.Length > 0)
                {
                    var button = SelectDestinationMenu.transform.GetChild(1).GetChild(0).gameObject;
                    SetSelectButton(button, destinations[0]);

                    for (var i = 1; i < destinations.Length; i++)
                    {
                        button = Instantiate(button);
                        button.transform.parent = SelectDestinationMenu.transform.GetChild(1);
                        SetSelectButton(button, destinations[i]);
                    }
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
    ///     アプリケーションを終了します。
    /// </summary>
    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        Windows.ApplicationModel.Core.CoreApplication.Exit();
#endif
    }

#endregion

#region Private Methods

    /// <summary>
    ///     目的地選択ボタンにに目的地名を設定します。
    /// </summary>
    /// <param name="button">ボタンオブジェクト</param>
    /// <param name="destinaion">目的値名</param>
    private void SetSelectButton(GameObject button, string destinaion)
    {
        button.name = destinaion;
        button.GetComponentInChildren<TextMeshPro>().text = destinaion;
        button.GetComponent<Interactable>().OnClick.AddListener(() => OnSelectDestination(destinaion));
        button.GetComponent<PressableButtonHoloLens2>().ButtonPressed
            .AddListener(() => OnSelectDestination(destinaion));
    }

    /// <summary>
    ///     選択された目的地名を設定します。
    /// </summary>
    /// <param name="d"></param>
    private void OnSelectDestination(string d)
    {
        SelectDestinationMenu.SetActive(false);
        Destination = d;
        FindNearbyAnchors(basePointAnchorId);
    }

    /// <summary>
    ///     指定されたAnchorId周辺に存在するSpatial Anchorの探索を行います。
    /// </summary>
    /// <param name="identifier"></param>
    private void FindNearbyAnchors(string identifier)
    {
        AnchorModuleProxy.Instance.FindNearByAnchor(identifier);
    }

#endregion
}