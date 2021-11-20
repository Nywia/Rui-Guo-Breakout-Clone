using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ScrEventManager : ScrSingletonBase<ScrEventManager>
{
    #region BlockDestroyed
    /// <summary>
    ///     Subscribe to this event to know when a
    ///     a block is destroyed
    /// </summary>
    public event Action<float> onBlockDestroyed;

    /// <summary>
    ///     Broadcasts the block being destroyed to
    ///     subscribers
    /// </summary>
    public void BlockDestroyed(float points)
    {
        onBlockDestroyed?.Invoke(points);
    }
#endregion
}
