using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Utils;
using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class ThirdPersonHandler : ModeHandlerBase
{
    public static float X = 0f;
    public static bool  BodyLocked;

    private Vector3    lastPosition;
    private Quaternion lastRotation; // for the thangy langs idfk gng

    public override string HandlerName => HandlerNameStatic();

    private void LateUpdate()
    {
        if (CoreHandler.Instance.CastedRig == null)
            return;

        Vector3    targetPosition;
        Quaternion targetRotation;

        if (BodyLocked)
        {
            targetPosition =
                    CoreHandler.Instance.CastedRig.bodyRenderer.transform.TransformPoint(new Vector3(X, 0.3f, -1f));

            Vector3 euler = CoreHandler.Instance.CastedRig.bodyRenderer.transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(0f, euler.y, 0f);
        }
        else
        {
            targetPosition =
                    CoreHandler.Instance.CastedRig.headMesh.transform.TransformPoint(new Vector3(X, 0.3f, -1f));

            targetRotation = CoreHandler.Instance.CastedRig.headMesh.transform.rotation;
            Vector3 forward = targetRotation * Vector3.forward;
            targetRotation = Quaternion.LookRotation(forward, Vector3.up);
            Vector3 euler = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);
        }

        if (CameraHandler.Instance.SmoothingFactor > 0)
        {
            int realSmoothingFactor = GetSmoothingFactor();

            if (SnappySmoothing)
            {
                Vector3    currentPosition = targetPosition;
                Quaternion currentRotation = targetRotation;

                Vector3 velocity =
                        (currentPosition - lastPosition) /
                        Time.deltaTime; // so much easier than the angular velocity (type shit)

                Vector3 angularVelocity = currentRotation.GetAngularVelocity(lastRotation);

                targetPosition = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition,
                        Time.deltaTime * realSmoothingFactor *
                        Mathf.Clamp(velocity.magnitude, 1f,
                                float.MaxValue)); // please tell me this isnt complete shit

                targetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                        Time.deltaTime * realSmoothingFactor *
                        Mathf.Clamp(angularVelocity.magnitude, 1f, float.MaxValue)); // pls look good T-T

                lastPosition = currentPosition;
                lastRotation = currentRotation;
            }
            else
            {
                targetPosition = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition,
                        Time.deltaTime * realSmoothingFactor);

                targetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                        Time.deltaTime * realSmoothingFactor);
            }
        }

        CameraHandler.Instance.transform.position = targetPosition;
        CameraHandler.Instance.transform.rotation = targetRotation;
    }

    public static string HandlerNameStatic() => "Third Person";
}