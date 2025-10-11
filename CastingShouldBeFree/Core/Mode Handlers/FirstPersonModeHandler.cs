using System.Linq;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Utils;
using GorillaNetworking;
using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class FirstPersonModeHandler : ModeHandlerBase
{
    public override string HandlerName => HandlerNameStatic();

    private void LateUpdate()
    {
        if (CoreHandler.Instance.CastedRig == null)
            return;

        Quaternion targetRotation = CoreHandler.Instance.CastedRig.headMesh.transform.rotation;

        if (RollLock)
        {
            Vector3 forward = targetRotation * Vector3.forward;
            targetRotation = Quaternion.LookRotation(forward, Vector3.up);
            Vector3 euler = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);
        }

        if (CameraHandler.Instance.SmoothingFactor > 0)
            targetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                    Time.deltaTime * GetSmoothingFactor());

        CameraHandler.Instance.transform.rotation = targetRotation;
        CameraHandler.Instance.transform.position =
                CoreHandler.Instance.CastedRig.headMesh.transform.TransformPoint(new Vector3(0f, 0.15f, 0f));
    }

    private void OnEnable()
    {
        OnCastedRigChange(CoreHandler.Instance.CastedRig, null);
        CameraHandler.Instance.ToggleVisibility(false);
        CoreHandler.Instance.OnCastedRigChange += OnCastedRigChange;
        RigUtils.OnRigCosmeticsChange          += OnRigCosmeticsUpdate;
    }

    private void OnDisable()
    {
        CoreHandler.Instance.OnCastedRigChange -= OnCastedRigChange;
        RigUtils.OnRigCosmeticsChange          -= OnRigCosmeticsUpdate;
        ToggleFaceCosmetics(CoreHandler.Instance.CastedRig, true);
        CameraHandler.Instance.ToggleVisibility(true);
    }

    public static string HandlerNameStatic() => "First Person";

    private void OnCastedRigChange(VRRig currentRig, VRRig lastRig)
    {
        if (currentRig == null)
            return;

        ToggleFaceCosmetics(currentRig, false);

        if (lastRig != null)
            ToggleFaceCosmetics(lastRig, true);
    }

    private void OnRigCosmeticsUpdate(VRRig rig)
    {
        if (rig != CoreHandler.Instance.CastedRig)
            return;

        ToggleFaceCosmetics(rig, false);
    }

    private void ToggleFaceCosmetics(VRRig rig, bool toggled)
    {
        CosmeticsController.CosmeticItem[] headItems = rig.cosmeticSet.items.Where(item =>
                    item.itemCategory == CosmeticsController.CosmeticCategory.Face ||
                    item.itemCategory == CosmeticsController.CosmeticCategory.Hat).ToArray();

        foreach (CosmeticsController.CosmeticItem cosmeticItem in headItems)
        {
            CosmeticItemInstance cosmeticObject = rig.cosmeticsObjectRegistry.Cosmetic(cosmeticItem.displayName);

            CosmeticsController.CosmeticSlots slot =
                    cosmeticItem.itemCategory == CosmeticsController.CosmeticCategory.Face
                            ? CosmeticsController.CosmeticSlots.Face
                            : CosmeticsController.CosmeticSlots.Hat;

            if (toggled)
                cosmeticObject.EnableItem(slot, rig);
            else
                cosmeticObject.DisableItem(slot);
        }
    }
}