using CastingShouldBeFree.Utils;
using HarmonyLib;

namespace CastingShouldBeFree.Patches;

[HarmonyPatch(typeof(VRRig), "IUserCosmeticsCallback.OnGetUserCosmetics")]
public static class OnRigCosmeticsLoadPatch
{
    private static void Postfix(VRRig __instance) => RigUtils.OnRigCosmeticsLoad?.Invoke(__instance);
}