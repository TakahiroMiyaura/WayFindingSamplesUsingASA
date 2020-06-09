// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Experimental.Extensions.UX;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class SetRouteGuideManager : MonoBehaviour
{
    public GameObject AnchorCollection;
    public GameObject AnchorObject;
    public string CurrentAnchorId;
    public SimpleDialog Dialog;
    public GameObject Instruction;
    public GameObject Operations;
    public GameObject LinkLinePrefab;

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

    public void IsContentsStart(bool enabled, string anchorId = null,
        GameObject anchorObj = null, IDictionary<string, string> appProperties = null)
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
            "Please set the destination.\n * Selecting 'Set Destination', set destination and title.\n * Selecting 'Create Next Point', add transit point.", new[] { "Ok" });
    }

    public void CreateAnchorObject(bool isDestination)
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
        currentAnchorObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1f;
        var systemKeyboardInputHelper = currentAnchorObject.GetComponentInChildren<SystemKeyboardInputHelper>();
        if (systemKeyboardInputHelper != null)
        {
            systemKeyboardInputHelper.gameObject.SetActive(isDestination);
        }
    }

    public void DeleteAnchorObject()
    {
        DestroyImmediate(linkLine);
        DestroyImmediate(currentAnchorObject);

    }


    public void NextStepWayFinding()
    {
        IsContentsStart(false);
        for (var i = 0; i < transform.GetChild(2).childCount; i++)
        {
            DestroyImmediate(transform.GetChild(2).GetChild(i).gameObject);
        }

        WayFindingManager.Instance.IsContentsStart(true, basePointAnchorId, basePointAnchor, basePointAppProperties);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
        UnityEngine.Application.Quit();
#endif
    }

    public async void CreateAzureAnchor()
    {
        // Then we create a new local cloud anchor
        var appProperties = new Dictionary<string, string>();

        appProperties.Add(RouteGuideInformation.ANCHOR_TYPE,
            isDestination ? RouteGuideInformation.ANCHOR_TYPE_DESTINATION : RouteGuideInformation.ANCHOR_TYPE_POINT);

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
            AnchorModuleProxy.Instance.UpdatePropertiesAll(RouteGuideInformation.DESTINATION_TITLE, destinationTitle,false);

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

}