using CastingShouldBeFree.Utils;
using HarmonyLib;

namespace CastingShouldBeFree.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.ChangeMaterialLocal))]
public class ChangeMaterialLocalPatch
{
    private static void Postfix(VRRig __instance, int materialIndex) => RigUtils.OnMatIndexChange?.Invoke(__instance);
}