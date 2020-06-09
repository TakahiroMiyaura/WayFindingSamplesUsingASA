// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using TMPro;
using UnityEngine;

public class DestinationPoint : MonoBehaviour
{
    private float coefficient = -1f;

    [SerializeField]
    public string DestinationTitle;

    [SerializeField]
    public string Identifier;

    private Transform labelObjectTransform;
    // Start is called before the first frame update

    private Transform outerObjectTransform;
    private TextMeshPro textMeshPro;

    public Transform InnerObjectTransform { get; private set; }

    public Transform NearRangeIndicatorObjectTransform { get; private set; }

    public Transform NearAutoSearchObjectTransform { get; private set; }

    public Transform DirectionIndicatorObjectTransform { get; private set; }

    // Update is called once per frame
    private void Start()
    {
        outerObjectTransform = transform.GetChild(0);
        InnerObjectTransform = transform.GetChild(1);
        labelObjectTransform = transform.GetChild(2);
        NearRangeIndicatorObjectTransform = transform.GetChild(3);
        NearAutoSearchObjectTransform = transform.GetChild(4);
        DirectionIndicatorObjectTransform = transform.GetChild(5);
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        outerObjectTransform.GetComponent<MeshRenderer>().enabled = true;
        InnerObjectTransform.localPosition = new Vector3(InnerObjectTransform.localPosition.x, Random.Range(-.3f, .3f),
            InnerObjectTransform.localPosition.z);
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position,
            new Vector3(transform.position.x, Camera.main.transform.position.y, transform.position.z), 0.015f);
        var enabled = !string.IsNullOrEmpty(DestinationTitle);
        InnerObjectAnimate();
        OuterObjectAnimate(enabled);
        LabelObjectAnimate(enabled);
        SetNearbyAutoSearch(enabled);
    }

    private void SetNearbyAutoSearch(bool enabled)
    {
        NearAutoSearchObjectTransform.gameObject.SetActive(!enabled);
    }

    private void LabelObjectAnimate(bool enabled)
    {
        labelObjectTransform.gameObject.SetActive(enabled);
        textMeshPro.text = DestinationTitle;
    }

    private void OuterObjectAnimate(bool enabled)
    {
        outerObjectTransform.gameObject.SetActive(enabled);
    }


    private void InnerObjectAnimate()
    {
        if (InnerObjectTransform == null)
        {
            Debug.LogWarning("innerObjectTransform is null.");
            return;
        }

        InnerObjectTransform.Rotate(new Vector3(.2f, 0.05f, 0.1f) * coefficient);

        InnerObjectTransform.localPosition =
            Vector3.Lerp(InnerObjectTransform.localPosition, Vector3.up * .15f * coefficient, .005f);
        var threshold = (InnerObjectTransform.localPosition - Vector3.up * .15f * coefficient).magnitude;
        if (threshold < 1e-2)
        {
            coefficient = coefficient * -1f;
        }
    }
}