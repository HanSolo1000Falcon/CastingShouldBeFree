using CastingShouldBeFree.Core.Interface;
using GorillaLocomotion;
using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class SelfieModeHandler : ModeHandlerBase
{
    private bool       isHolding;
    private bool       leftHandActive;
    private Vector3    positionOffset;
    private Quaternion rotationOffset;

    public override string HandlerName => "Selfie";

    private void LateUpdate()
    {
        if (!isHolding)
        {
            TryStartHolding(GTPlayer.Instance.leftHand.controllerTransform, ControllerInputPoller.instance.leftGrab,
                    ControllerInputPoller.instance.leftGrabMomentary,  true);

            TryStartHolding(GTPlayer.Instance.rightHand.controllerTransform, ControllerInputPoller.instance.rightGrab,
                    ControllerInputPoller.instance.rightGrabMomentary,  false);
        }
        else
        {
            Transform activeController = leftHandActive
                                                 ? GTPlayer.Instance.leftHand.controllerTransform
                                                 : GTPlayer.Instance.rightHand.controllerTransform;

            bool grabHeld = leftHandActive
                                    ? ControllerInputPoller.instance.leftGrab
                                    : ControllerInputPoller.instance.rightGrab;

            if (grabHeld)
            {
                targetPosition = activeController.TransformPoint(positionOffset);
                targetRotation = activeController.rotation * rotationOffset;
            }
            else
            {
                isHolding = false;
            }
        }

        HandleGenericSmoothing(Time.deltaTime);
        SetCameraPositionAndRotation();
    }

    private void OnEnable()
    {
        targetPosition = CameraHandler.Instance.transform.position;
        targetRotation = CameraHandler.Instance.transform.rotation;
    }

    private void TryStartHolding(Transform controller, bool grabHeld, bool grabPressed, bool isLeft)
    {
        if (!grabHeld || !grabPressed)
            return;

        if (Vector3.Distance(controller.position, targetPosition) < 0.3f)
        {
            positionOffset = controller.InverseTransformPoint(targetPosition);
            rotationOffset = Quaternion.Inverse(controller.rotation) * targetRotation;
            isHolding      = true;
            leftHandActive = isLeft;
        }
    }
}