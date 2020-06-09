// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System;
using UnityEngine;
using UnityEngine.Events;

public class NearbyAutoSearch : MonoBehaviour
{
    private CapsuleCollider colidar;
    private GameObject headPosition;
    private bool isProcessingTrigger = false;
    [System.Serializable]
    public class TiggerFindNearByAnchorCallback : UnityEvent<string>
    {

    }

    public UnityEvent<string> OnTiggerFindNearByAnchor = new TiggerFindNearByAnchorCallback();

    [SerializeField]
    private float radius;

    private DestinationPoint parent;

    // Start is called before the first frame update
    private void Start()
    {
        parent = GetComponentInParent<DestinationPoint>();
        colidar = GetComponent<CapsuleCollider>();
        colidar.radius = radius;

        headPosition = transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    private void Update()
    {
        headPosition.transform.position = Camera.main.transform.position;
        colidar.isTrigger = true;
    }

    private void OnTriggerEnter(Collider colidar)
    {
        if (isProcessingTrigger) {return;}

        if (colidar.gameObject.name.Equals("HeadPosition"))
        {
            isProcessingTrigger = true;
            OnTiggerFindNearByAnchor?.Invoke(parent.Identifier);
        }
    }
}