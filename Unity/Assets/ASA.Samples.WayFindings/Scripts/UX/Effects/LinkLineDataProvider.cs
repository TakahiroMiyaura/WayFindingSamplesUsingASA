// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

[RequireComponent(typeof(SplineDataProvider))]
public class LinkLineDataProvider : MonoBehaviour
{
    [SerializeField] private readonly float curv = 0.4f;

    public GameObject FromPoint;
    public GameObject ToPoint;

    private SplineDataProvider dataProvider;
    private Transform fromTransform;
    private Transform toTransform;


    // Start is called before the first frame update
    private void Start()
    {
        dataProvider = GetComponent<SplineDataProvider>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (ToPoint == null || FromPoint == null)
        {
            return;
        }

        if (fromTransform == null || toTransform == null)
        {
            var fromDest = FromPoint.GetComponent<DestinationPoint>();
            fromTransform = fromDest != null ? fromDest.InnerObjectTransform.transform : FromPoint.transform;

            var toDest = ToPoint.GetComponent<DestinationPoint>();
            toTransform = toDest != null ? toDest.InnerObjectTransform.transform : ToPoint.transform;
        }

        transform.position = fromTransform.position;

        var pos = toTransform.position - fromTransform.position;
        var deltaX = pos.x * 1f / 3f;
        var deltaZ = pos.z * 1f / 3f;
        var posDelta1 = new Vector3(deltaX, curv, deltaZ);
        var posDelta2 = new Vector3(deltaX * 2f, curv, deltaZ * 2f);
        dataProvider.ControlPoints[1].Position = posDelta1;
        dataProvider.ControlPoints[2].Position = posDelta2;
        dataProvider.ControlPoints[3].Position = pos;
    }
}