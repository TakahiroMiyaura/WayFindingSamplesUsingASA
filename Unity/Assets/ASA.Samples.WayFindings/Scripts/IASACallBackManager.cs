// Copyright (c) 2020 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;
using UnityEngine;

public interface IASACallBackManager
{
    /// <summary>
    /// Locate the Anchor that set  Destination property.
    /// </summary>
    /// <param name="identifier"></param>
    /// <param name="appProperties"></param>
    /// <returns></returns>
    GameObject OnLocatedAnchorObject(string identifier,
        IDictionary<string, string> appProperties);
    
    void OnLocatedAnchorComplete();
}