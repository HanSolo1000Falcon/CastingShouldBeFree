using CastingShouldBeFree.Utils;
using HarmonyLib;

namespace CastingShouldBeFree.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SetNameTagText))]
public static class SetNameTagTextPatch
{
    private static void Postfix(VRRig __instance, string name) => RigUtils.OnRigNameChange?.Invoke(__instance, name);
}