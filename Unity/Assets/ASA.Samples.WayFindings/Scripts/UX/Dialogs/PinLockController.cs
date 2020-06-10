// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

/// <summary>
///     メニュー表示時の固定状態を管理するクラス
/// </summary>
public class PinLockController : MonoBehaviour
{
#region Inspector Properites

    public SolverHandler HostSolverHander;

#endregion

#region Public Methods

    /// <summary>
    ///     メニューの固定状態を切り替えます。
    /// </summary>
    public void TogglePin()
    {
        HostSolverHander.enabled = !HostSolverHander.enabled;
    }

#endregion
}