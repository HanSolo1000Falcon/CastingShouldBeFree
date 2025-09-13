using System;

namespace CastingShouldBeFree.Utils;

public static class RigUtils
{
    public static Action<VRRig> OnRigSpawned;
    public static Action<VRRig> OnRigCached;

    public static Action<VRRig, string> OnRigNameChange;
    public static Action<VRRig> OnMatIndexChange;
}