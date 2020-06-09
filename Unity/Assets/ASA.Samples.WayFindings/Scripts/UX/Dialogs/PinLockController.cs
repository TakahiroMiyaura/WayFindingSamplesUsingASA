// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

public class PinLockController : MonoBehaviour
{
    public SolverHandler HostSolverHander;

    public void TogglePin()
    {
        HostSolverHander.enabled = !HostSolverHander.enabled;
    }
}