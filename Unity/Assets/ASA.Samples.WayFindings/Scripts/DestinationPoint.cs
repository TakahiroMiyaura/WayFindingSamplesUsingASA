// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using TMPro;
using UnityEngine;

/// <summary>
///     Spatial Anchor可視化に利用するアンカー用のコンポーネントです。
/// </summary>
public class DestinationPoint : MonoBehaviour
{
    private float coefficient = -1f;

    private Transform labelObjectTransform;
    // Start is called before the first frame update

    private Transform outerObjectTransform;
    private TextMeshPro textMeshPro;

#region Inspector Properites

    [SerializeField]
    [Tooltip("Set title of Destination.")]
    public string DestinationTitle;

    [SerializeField]
    [Tooltip("Set the anchor id of this object.")]
    public string Identifier;

#endregion


#region Public Properties

    public Transform InnerObjectTransform { get; private set; }

    public Transform NearRangeIndicatorObjectTransform { get; private set; }

    public Transform NearAutoSearchObjectTransform { get; private set; }

    public Transform DirectionIndicatorObjectTransform { get; private set; }

#endregion


#region Unity Lifecycle

    /// <summary>
    ///     初期化処理を実施します
    /// </summary>
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

    /// <summary>
    ///     フレーム毎に実行する処理を実施します。
    /// </summary>
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

#endregion


#region Private Methods

    /// <summary>
    ///     アンカーに近づいた場合、自動的にこのアンカーを中心にSpatial Anchorの検索を行うオブジェクトの有効/無効を設定します。
    /// </summary>
    /// <param name="enabled">enabled</param>
    private void SetNearbyAutoSearch(bool enabled)
    {
        NearAutoSearchObjectTransform.gameObject.SetActive(!enabled);
    }

    /// <summary>
    ///     目的地名を表示するラベルオブジェクトの有効/無効を設定します。
    /// </summary>
    /// <param name="enabled">enabled</param>
    private void LabelObjectAnimate(bool enabled)
    {
        labelObjectTransform.gameObject.SetActive(enabled);
        textMeshPro.text = DestinationTitle;
    }

    /// <summary>
    ///     目的地となるオブジェクトのエフェクトの有効/無効を設定します。
    /// </summary>
    /// <param name="enabled">enabled</param>
    private void OuterObjectAnimate(bool enabled)
    {
        outerObjectTransform.gameObject.SetActive(enabled);
    }

    /// <summary>
    ///     オブジェクトのアニメーションを行います。
    /// </summary>
    private void InnerObjectAnimate()
    {
        if (InnerObjectTransform == null)
        {
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

#endregion
}