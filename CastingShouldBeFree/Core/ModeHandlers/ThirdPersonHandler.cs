using CastingShouldBeFree.Core.Interface;
using UnityEngine;

namespace CastingShouldBeFree.Core.ModeHandlers;

public class ThirdPersonHandler : ModeHandlerBase
{
    public override string HandlerName => "Third Person";

    private void OnEnable()
    {
        ChangeCastedRig(GUIHandler.Instance.CastedRig, null);
        GUIHandler.Instance.OnCastedRigChange += ChangeCastedRig;
    }
    
    private void OnDisable() => GUIHandler.Instance.OnCastedRigChange -= ChangeCastedRig;
    
    private void ChangeCastedRig(VRRig currentRig, VRRig lastRig)
    {
        CameraHandler.Instance.Parent = currentRig.bodyRenderer.transform;
        CameraHandler.Instance.LocalPosition = new Vector3(0f, 0.5f, -1f);
        CameraHandler.Instance.LocalRotation = Quaternion.identity;
    }
}