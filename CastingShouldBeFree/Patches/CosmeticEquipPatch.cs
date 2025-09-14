using CastingShouldBeFree.Utils;
using HarmonyLib;

namespace CastingShouldBeFree.Patches;

[HarmonyPatch(typeof(VRRig), nameof(VRRig.SetCosmeticsActive))]
public class CosmeticEquipPatch
{
    private static void Postfix(VRRig __instance) => RigUtils.OnRigCosmeticsChange?.Invoke(__instance);
}