using System.Linq;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Utils;
using GorillaNetworking;
using UnityEngine;

namespace CastingShouldBeFree.Core.ModeHandlers;

public class FirstPersonModeHandler : ModeHandlerBase
{
    public override string HandlerName => HandlerNameStatic();
    public static string HandlerNameStatic() => "First Person";

    private void OnEnable()
    {
        OnCastedRigChange(GUIHandler.Instance.CastedRig, null);
        CameraHandler.Instance.ToggleVisibility(false);
        GUIHandler.Instance.OnCastedRigChange += OnCastedRigChange;
        RigUtils.OnRigCosmeticsChange += OnRigCosmeticsUpdate;
    }

    private void OnDisable()
    {
        GUIHandler.Instance.OnCastedRigChange -= OnCastedRigChange;
        RigUtils.OnRigCosmeticsChange -= OnRigCosmeticsUpdate;
        ToggleFaceCosmetics(GUIHandler.Instance.CastedRig, true);
        CameraHandler.Instance.ToggleVisibility(true);
    }

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
        if (rig != GUIHandler.Instance.CastedRig)
            return;
        
        ToggleFaceCosmetics(rig, false);
    }

    private void LateUpdate()
    {
        if (GUIHandler.Instance.CastedRig == null)
            return;
        
        Quaternion targetRotation = GUIHandler.Instance.CastedRig.headMesh.transform.rotation;

        if (CameraHandler.Instance.SmoothingFactor > 0)
            targetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                Time.deltaTime * CameraHandler.Instance.GetRealSmoothingFactor());

        CameraHandler.Instance.transform.rotation = targetRotation;
        CameraHandler.Instance.transform.position =
            GUIHandler.Instance.CastedRig.headMesh.transform.TransformPoint(new Vector3(0f, 0.15f, 0f));
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