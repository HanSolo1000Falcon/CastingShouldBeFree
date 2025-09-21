using CastingShouldBeFree.Core.Interface;
using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public abstract class ModeHandlerBase : MonoBehaviour
{
    public static bool RollLock = true;
    
    public abstract string HandlerName { get; }
    
    protected int GetSmoothingFactor() => -(CameraHandler.Instance.SmoothingFactor - CoreHandler.Instance.MaxSmoothing);
}