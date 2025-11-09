using BepInEx;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class FreeCam : ModeHandlerBase
{
    private float pitch;

    private         float  yaw;
    public override string HandlerName       => "Free Cam";
    public override bool   IsPlayerDependent => false;

    private void LateUpdate()
    {
        float speed = UnityInput.Current.GetKey(KeyCode.LeftShift) ? 30f : 10f;
        speed *= Time.deltaTime;

        Vector3 movementDir = Vector3.zero;

        if (UnityInput.Current.GetKey(KeyCode.W)) movementDir           += CameraHandler.Instance.transform.forward;
        if (UnityInput.Current.GetKey(KeyCode.S)) movementDir           -= CameraHandler.Instance.transform.forward;
        if (UnityInput.Current.GetKey(KeyCode.A)) movementDir           -= CameraHandler.Instance.transform.right;
        if (UnityInput.Current.GetKey(KeyCode.D)) movementDir           += CameraHandler.Instance.transform.right;
        if (UnityInput.Current.GetKey(KeyCode.Space)) movementDir       += Vector3.up;
        if (UnityInput.Current.GetKey(KeyCode.LeftControl)) movementDir -= Vector3.up;

        movementDir.Normalize();
        movementDir    *= speed;
        targetPosition += movementDir;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2     mouseDelta  = Mouse.current.delta.ReadValue();
            const float Sensitivity = 6f;

            yaw   += mouseDelta.x * Sensitivity * Time.deltaTime;
            pitch -= mouseDelta.y * Sensitivity * Time.deltaTime;
            pitch =  Mathf.Clamp(pitch, -89f, 89f);

            targetRotation = Quaternion.Euler(pitch, yaw, 0f);

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        Vector3    otherTargetPos = targetPosition;
        Quaternion otherTargetRot = targetRotation;

        if (CameraHandler.Instance.SmoothingFactor > 0)
        {
            int realSmoothingFactor = GetSmoothingFactor();

            if (SnappySmoothing)
            {
                Vector3 velocity        = (targetPosition - lastPosition) / Time.deltaTime;
                Vector3 angularVelocity = targetRotation.GetAngularVelocity(lastRotation, Time.deltaTime);

                otherTargetPos = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition,
                        Time.deltaTime * realSmoothingFactor * velocity.magnitude);

                otherTargetRot = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                        Time.deltaTime * realSmoothingFactor * ParseAngularVelocity(angularVelocity));
            }
            else
            {
                otherTargetPos = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition,
                        Time.deltaTime * realSmoothingFactor);

                otherTargetRot = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                        Time.deltaTime * realSmoothingFactor);
            }
        }

        lastRotation = CameraHandler.Instance.transform.rotation;
        lastPosition = CameraHandler.Instance.transform.position;

        CameraHandler.Instance.transform.position = otherTargetPos;
        CameraHandler.Instance.transform.rotation = otherTargetRot;
    }

    private void OnEnable()
    {
        targetPosition = CameraHandler.Instance.transform.position;
        targetRotation = CameraHandler.Instance.transform.rotation;

        Vector3 euler = CameraHandler.Instance.transform.rotation.eulerAngles;
        yaw   = euler.y;
        pitch = euler.x;
    }

    private void OnDisable() => Cursor.lockState = CursorLockMode.None;
}