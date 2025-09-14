using System;
using UnityEngine;

namespace CastingShouldBeFree.Utils;

public static class RigUtils
{
    public static Action<VRRig> OnRigSpawned;
    public static Action<VRRig> OnRigCached;

    public static Action<VRRig, string> OnRigNameChange;
    public static Action<VRRig> OnMatIndexChange;
    public static Action<VRRig, Color> OnRigColourChange;
    public static Action<VRRig> OnRigCosmeticsChange;
}