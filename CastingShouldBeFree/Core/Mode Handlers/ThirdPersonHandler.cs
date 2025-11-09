using UnityEngine;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class ThirdPersonHandler : ModeHandlerBase
{
    public static float X = 0f;
    public static bool  BodyLocked;

    public override string HandlerName       => HandlerNameStatic();
    public override bool   IsPlayerDependent => true;

    private void LateUpdate()
    {
        if (CoreHandler.Instance.CastedRig == null)
            return;

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

        HandleGenericSmoothing(Time.deltaTime);
        SetCameraPositionAndRotation();
    }

    public static string HandlerNameStatic() => "Third Person";
}