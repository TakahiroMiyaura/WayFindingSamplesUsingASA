// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

public class WayFindingManager : MonoBehaviour, IASACallBackManager
{
    private static object lockObj = new object();
    private GameObject basePointAnchor;

    private string basePointAnchorId;
    private IDictionary<string, string> basePointAppProperties;
    private string currentAnchorId = RouteGuideInformation.ANCHOR_ID_NOT_INITIALIZED;
    public string Destination;
    public GameObject DestinationPointPrefab;

    public SimpleDialog Dialog;
    public GameObject Instruction;
    public GameObject LinkLinePrefab;
    public GameObject Operations;
    public RouteGuideInformation RouteInfo;
    public GameObject SelectDestinationMenu;

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

    /// <summary>
    ///     Visualize the Anchor link line renderer.
    /// </summary>
    public void OnLocatedAnchorComplete()
    {
        foreach (var current in RouteInfo.LocatedAnchors.Keys)
        {
            var appProperties = RouteInfo.LocatedAnchors[current].CloudAnchorInfo;
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

                        var linkLine = Instantiate(LinkLinePrefab);
                        linkLine.name = "Link:" + key + " > " + current;
                        linkLine.transform.parent = RouteInfo.RootLinkLineObjects;
                        var linkLineComponent = linkLine.GetComponent<LinkLineDataProvider>();
                        linkLineComponent.FromPoint = prevObj;
                        linkLineComponent.ToPoint = dest;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Locate the Anchor that set  Destination property.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="appProperties"></param>
    /// <returns></returns>
    public GameObject OnLocatedAnchorObject(string identifier,
        IDictionary<string, string> appProperties)
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
            return null;
        }

        GameObject point;
        if (!RouteInfo.LocatedAnchors.ContainsKey(identifier))
        {
            point = Instantiate(DestinationPointPrefab);
            point.GetComponentInChildren<NearbyAutoSearch>()?.OnTiggerFindNearByAnchor.AddListener(FindNearbyAnchors);
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


        return point;
    }

    public void IsContentsStart(bool enabled, string anchorId = null,
        GameObject anchorObj = null, IDictionary<string, string> appProperties = null)
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
            locatedAnchors.Add(basePointAnchorId, new RouteGuideInformation.AnchorInfo(appProperties, basePointAnchor));
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

    private void SetSelectButton(GameObject button, string destinaion)
    {
        button.name = destinaion;
        button.GetComponentInChildren<TextMeshPro>().text = destinaion;
        button.GetComponent<Interactable>().OnClick.AddListener(() => OnSelectDestination(destinaion));
        button.GetComponent<PressableButtonHoloLens2>().ButtonPressed
            .AddListener(() => OnSelectDestination(destinaion));
    }

    private void OnSelectDestination(string d)
    {
        SelectDestinationMenu.SetActive(false);
        Destination = d;
        FindNearbyAnchors(basePointAnchorId);
    }

    private void FindNearbyAnchors(string identifier)
    {
        AnchorModuleProxy.Instance.FindNearByAnchor(identifier);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
    }
}