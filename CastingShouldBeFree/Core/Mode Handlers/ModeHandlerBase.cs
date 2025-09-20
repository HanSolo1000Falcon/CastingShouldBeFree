using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public abstract class ModeHandlerBase : MonoBehaviour
{
    public static bool RollLock;
    
    public abstract string HandlerName { get; }
}