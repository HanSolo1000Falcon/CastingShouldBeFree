using CastingShouldBeFree.Core.Interface;
using GorillaLocomotion;
using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class SelfieModeHandler : ModeHandlerBase
{
    public override string HandlerName => "Selfie";

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    private void OnEnable()
    {
        targetPosition = CameraHandler.Instance.transform.position;
        targetRotation = CameraHandler.Instance.transform.rotation;
    }

    private void LateUpdate()
    {
        if (Vector3.Distance(GTPlayer.Instance.leftControllerTransform.position, targetPosition) < 0.3f && ControllerInputPoller.instance.leftGrab)
        {
            if (ControllerInputPoller.instance.leftGrabMomentary)
            {
                positionOffset = GTPlayer.Instance.leftControllerTransform.InverseTransformPoint(targetPosition);
                rotationOffset = Quaternion.Inverse(GTPlayer.Instance.leftControllerTransform.rotation) * targetRotation;
            }
            else if (ControllerInputPoller.instance.leftGrab)
            {
                targetPosition = GTPlayer.Instance.leftControllerTransform.TransformPoint(positionOffset);
                targetRotation = GTPlayer.Instance.leftControllerTransform.rotation * rotationOffset;
            }
        }
        else if (Vector3.Distance(GTPlayer.Instance.rightControllerTransform.position, targetPosition) < 0.3f && ControllerInputPoller.instance.rightGrab)
        {
            if (ControllerInputPoller.instance.rightGrabMomentary)
            {
                positionOffset = GTPlayer.Instance.rightControllerTransform.InverseTransformPoint(targetPosition);
                rotationOffset = Quaternion.Inverse(GTPlayer.Instance.rightControllerTransform.rotation) * targetRotation;
            }
            else if (ControllerInputPoller.instance.rightGrab)
            {
                targetPosition = GTPlayer.Instance.rightControllerTransform.TransformPoint(positionOffset);
                targetRotation = GTPlayer.Instance.rightControllerTransform.rotation * rotationOffset;
            }
        }

        Vector3 targetPositionReal = targetPosition;
        Quaternion targetRotationReal = targetRotation;

        if (CameraHandler.Instance.SmoothingFactor > 0)
        {
            int realSmoothingFactor = GetSmoothingFactor();
            targetPositionReal = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition, Time.deltaTime * realSmoothingFactor);
            targetRotationReal = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation, Time.deltaTime * realSmoothingFactor);
        }

        CameraHandler.Instance.transform.position = targetPositionReal;
        CameraHandler.Instance.transform.rotation = targetRotationReal;
    }
}