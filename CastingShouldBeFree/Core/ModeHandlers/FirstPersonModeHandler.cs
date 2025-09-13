using System.Linq;
using CastingShouldBeFree.Core.Interface;
using GorillaNetworking;
using UnityEngine;

namespace CastingShouldBeFree.Core.ModeHandlers;

public class FirstPersonModeHandler : ModeHandlerBase
{
    public override string HandlerName => "First Person";
    
    private void OnEnable()
    {
        OnCastedRigChange(GUIHandler.Instance.CastedRig, null);
        GUIHandler.Instance.OnCastedRigChange += OnCastedRigChange;
    }

    private void OnCastedRigChange(VRRig currentRig, VRRig lastRig)
    {
        if (currentRig == null)
            return;

        CameraHandler.Instance.Parent = currentRig.headMesh.transform;
        CameraHandler.Instance.LocalPosition = new Vector3(0f, 0.15f, 0f);
        CameraHandler.Instance.LocalRotation = Quaternion.identity;

        ToggleFaceCosmetics(currentRig, false);

        if (lastRig != null)
            ToggleFaceCosmetics(lastRig, true);
    }

    private void OnDisable()
    {
        GUIHandler.Instance.OnCastedRigChange -= OnCastedRigChange;
        ToggleFaceCosmetics(GUIHandler.Instance.CastedRig, true);
        CameraHandler.Instance.ToggleVisibility(true);
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