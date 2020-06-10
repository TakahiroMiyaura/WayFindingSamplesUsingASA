// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEditor;
using UnityEngine;

/// <summary>
///     経路設定を実施するためのクラス
/// </summary>
public class SetRouteGuideManager : MonoBehaviour
{
    private Transform back;
    private GameObject basePointAnchor;
    private string basePointAnchorId;
    private IDictionary<string, string> basePointAppProperties;
    private Transform createAnchor;
    private GameObject currentAnchorObject;
    private bool isDestination;
    private GameObject linkLine;
    private Transform nextStepButtons;
    private Transform setDestination;
    private Transform setNextPoint;

#region Static Methods

    /// <summary>
    ///     Azure Spatial Anchorsの処理を実行するクラスのインスタンスを取得します。
    /// </summary>
    public static SetRouteGuideManager Instance
    {
        get
        {
            var module = FindObjectsOfType<SetRouteGuideManager>();
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

    public GameObject AnchorCollection;

    [Tooltip("Set a GameObject to visualize Spatial Anchor.")]
    public GameObject AnchorObject;

    [Tooltip("Set the current anchor id.")]
    public string CurrentAnchorId;

    [Tooltip("Set a GameObject of Dialog")]
    public SimpleDialog Dialog;

    [Tooltip("Set a GameObject of Instruction.")]
    public GameObject Instruction;

    [Tooltip("Set a GameObject of Menu.")]
    public GameObject Operations;

    [Tooltip("Set a GameObject of LinkLine.")]
    public GameObject LinkLinePrefab;

#endregion

#region Public Methods

    /// <summary>
    ///     オブジェクトの有効無効を設定します。
    /// </summary>
    /// <param name="enabled"></param>
    public void IsContentsStart(bool enabled, string anchorId = null,
        GameObject anchorObj = null, IDictionary<string, string> appProperties = null)
    {
        try
        {
            Instruction?.SetActive(enabled);
            AnchorCollection?.SetActive(enabled);
            Operations?.SetActive(enabled);
            if (anchorId == null || anchorObj == null || appProperties == null)
            {
                Debug.LogWarning("null reference the base point anchor information.");
                return;
            }

            basePointAnchorId = anchorId;
            CurrentAnchorId = basePointAnchorId;
            basePointAnchor = anchorObj;
            basePointAppProperties = appProperties;

            var buttons = Operations.transform.GetChild(0);
            setNextPoint = buttons.GetChild(0);
            setDestination = buttons.GetChild(1);
            createAnchor = buttons.GetChild(2);
            back = buttons.GetChild(3);
            createAnchor.gameObject.SetActive(false);
            back.gameObject.SetActive(false);
            nextStepButtons = Operations.transform.GetChild(1);
            nextStepButtons.gameObject.SetActive(false);

            var dialog = Instantiate(Dialog);
            dialog.SetDialog("Set Destination.",
                "Please set the destination.\n * Selecting 'Set Destination', set destination and title.\n * Selecting 'Create Next Point', add transit point.",
                new[] {"Ok"});
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    ///     アンカーの可視化に利用するGameobjectを生成します。
    /// </summary>
    /// <param name="isDestination">中継点か目的地かを判定するフラグ</param>
    public void CreateAnchorObject(bool isDestination)
    {
        try
        {
            this.isDestination = isDestination;

            if (currentAnchorObject == null)
            {
                currentAnchorObject = basePointAnchor;
            }

            var dest = Instantiate(AnchorObject);

            linkLine = Instantiate(LinkLinePrefab);
            linkLine.name = "Link:" + currentAnchorObject.name + " > " + dest.name;
            linkLine.transform.parent = AnchorCollection.transform;
            var linkLineComponent = linkLine.GetComponent<LinkLineDataProvider>();
            linkLineComponent.FromPoint = currentAnchorObject;
            linkLineComponent.ToPoint = dest;
            currentAnchorObject.GetComponent<NextAnchorRnageMarker>()?.gameObject.SetActive(false);

            currentAnchorObject = dest;
            currentAnchorObject.transform.parent = AnchorCollection.transform;
            currentAnchorObject.transform.position =
                Camera.main.transform.position + Camera.main.transform.forward * 1f;
            var systemKeyboardInputHelper = currentAnchorObject.GetComponentInChildren<SystemKeyboardInputHelper>();
            if (systemKeyboardInputHelper != null)
            {
                systemKeyboardInputHelper.gameObject.SetActive(isDestination);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

    /// <summary>
    ///     可視化しているすべてのアンカーとリンクオブジェクトを削除します。
    /// </summary>
    public void DeleteAnchorObject()
    {
        DestroyImmediate(linkLine);
        DestroyImmediate(currentAnchorObject);
    }

    /// <summary>
    ///     次の処理ステップ（経路探索）へ進むための処理を実行します。
    /// </summary>
    public void NextStepWayFinding()
    {
        IsContentsStart(false);
        for (var i = 0; i < transform.GetChild(2).childCount; i++)
        {
            DestroyImmediate(transform.GetChild(2).GetChild(i).gameObject);
        }

        WayFindingManager.Instance.IsContentsStart(true, basePointAnchorId, basePointAnchor, basePointAppProperties);
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

    /// <summary>
    ///     Azure Spatial Anchorsサービスにアンカーを追加します。
    /// </summary>
    public async void CreateAzureAnchor()
    {
        try
        {
            // Then we create a new local cloud anchor
            var appProperties = new Dictionary<string, string>();

            appProperties.Add(RouteGuideInformation.ANCHOR_TYPE,
                isDestination
                    ? RouteGuideInformation.ANCHOR_TYPE_DESTINATION
                    : RouteGuideInformation.ANCHOR_TYPE_POINT);

            appProperties.Add(RouteGuideInformation.PREV_ANCHOR_ID, CurrentAnchorId);

            var identifier = await AnchorModuleProxy.Instance.CreateAzureAnchor(currentAnchorObject, appProperties);


            if (identifier == null)
            {
                return;
            }

            CurrentAnchorId = identifier;
            currentAnchorObject.GetComponent<ManipulationHandler>().enabled = false;
            currentAnchorObject.GetComponent<NearInteractionGrabbable>().enabled = false;
            currentAnchorObject.GetComponentInChildren<SolverHandler>().UpdateSolvers = false;

            if (isDestination)
            {
                var destinationTitle = currentAnchorObject.GetComponentInChildren<SystemKeyboardInputHelper>().text;
#if UNITY_EDITOR
                destinationTitle = "TestDest";
#endif
                AnchorModuleProxy.Instance.UpdatePropertiesAll(RouteGuideInformation.DESTINATION_TITLE,
                    destinationTitle, false);

                setNextPoint.gameObject.SetActive(false);
                setDestination.gameObject.SetActive(false);
                createAnchor.gameObject.SetActive(false);
                back.gameObject.SetActive(false);
                nextStepButtons.gameObject.SetActive(true);
            }
            else
            {
                setNextPoint.gameObject.SetActive(true);
                setDestination.gameObject.SetActive(true);
                createAnchor.gameObject.SetActive(false);
                back.gameObject.SetActive(false);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }

#endregion
}