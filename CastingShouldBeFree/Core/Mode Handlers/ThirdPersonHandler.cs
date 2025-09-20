using CastingShouldBeFree.Core.Interface;
using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class ThirdPersonHandler : ModeHandlerBase
{
    public override string HandlerName => HandlerNameStatic();
    public static string HandlerNameStatic() => "Third Person";

    public static float X = 0f;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void LateUpdate()
    {
        if (GUIHandler.Instance.CastedRig == null)
            return;
        
        targetPosition = GUIHandler.Instance.CastedRig.bodyRenderer.transform.TransformPoint(new Vector3(X, 0.3f, -1f));
        targetRotation = GUIHandler.Instance.CastedRig.headMesh.transform.rotation;
        
        Vector3 forward = targetRotation * Vector3.forward;
        targetRotation = Quaternion.LookRotation(forward, Vector3.up);
        Vector3 euler = targetRotation.eulerAngles;
        targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);

        if (CameraHandler.Instance.SmoothingFactor > 0)
        {
            int realSmoothingFactor = CameraHandler.Instance.GetRealSmoothingFactor();
            targetPosition = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition, Time.deltaTime * realSmoothingFactor);
            targetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation, Time.deltaTime * realSmoothingFactor);
        }
        
        CameraHandler.Instance.transform.position = targetPosition;
        CameraHandler.Instance.transform.rotation = targetRotation;
    }
}