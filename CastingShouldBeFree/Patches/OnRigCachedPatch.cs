using CastingShouldBeFree.Utils;
using HarmonyLib;

namespace CastingShouldBeFree.Patches;

[HarmonyPatch(typeof(VRRigCache), nameof(VRRigCache.RemoveRigFromGorillaParent))]
public static class OnRigCachedPatch
{
    private static void Postfix(NetPlayer player, VRRig vrrig)
    {
        if (SetColourPatch.SpawnedRigs.Contains(vrrig))
            SetColourPatch.SpawnedRigs.Remove(vrrig);

        RigUtils.OnRigCached?.Invoke(vrrig);
    }
}