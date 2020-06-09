// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using UnityEngine;

public class ArrowAnimte : MonoBehaviour
{
    [SerializeField] private float intervals = 0.01f;

    private float currentOffset = .7f;
    private float currentTime = 0f;
    private Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        if (intervals > currentTime)
        {
            return;
        }

        currentTime = 0;
        currentOffset = currentOffset - 0.01f;
        if (currentOffset < -1f)
        {
            currentOffset = 0f;
        }

        material.mainTextureOffset = new Vector2(currentOffset, 0);
    }
}