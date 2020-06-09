// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;

public class NextAnchorRnageMarker : MonoBehaviour
{
    [SerializeField] private float intervals = 0.001f;

    [SerializeField] private Vector3 maxRange = Vector3.one;

    private Vector3 initializeScale;

    [SerializeField] private  float threshold = .1f;

    private void Start()
    {
        initializeScale = transform.localScale;
    }

    private void Update()
    {
        if ((transform.localScale - maxRange).magnitude < threshold)
        {
            transform.localScale = initializeScale;
        }

        transform.localScale = Vector3.Lerp(transform.localScale, maxRange, intervals);
    }
}