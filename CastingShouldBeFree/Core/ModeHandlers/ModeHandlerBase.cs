using UnityEngine;

namespace CastingShouldBeFree.Core.ModeHandlers;

public abstract class ModeHandlerBase : MonoBehaviour
{
    public abstract string HandlerName { get; }
}