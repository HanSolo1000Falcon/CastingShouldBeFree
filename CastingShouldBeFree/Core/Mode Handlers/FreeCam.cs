using BepInEx;
using CastingShouldBeFree.Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CastingShouldBeFree.Core.Mode_Handlers;

public class FreeCam : ModeHandlerBase
{
    private float pitch;

    private Vector3    targetPosition;
    private Quaternion targetRotation;

    private         float  yaw;
    public override string HandlerName => "Free Cam";

    private void LateUpdate()
    {
        float speed = UnityInput.Current.GetKey(KeyCode.LeftShift) ? 30f : 10f;
        speed *= Time.deltaTime;

        Vector3 movementDir = Vector3.zero;

        if (UnityInput.Current.GetKey(KeyCode.W)) movementDir += CameraHandler.Instance.transform.forward * speed;
        if (UnityInput.Current.GetKey(KeyCode.S)) movementDir -= CameraHandler.Instance.transform.forward * speed;
        if (UnityInput.Current.GetKey(KeyCode.A)) movementDir -= CameraHandler.Instance.transform.right * speed;
        if (UnityInput.Current.GetKey(KeyCode.D)) movementDir += CameraHandler.Instance.transform.right * speed;
        if (UnityInput.Current.GetKey(KeyCode.Space)) movementDir += Vector3.up * speed;
        if (UnityInput.Current.GetKey(KeyCode.LeftControl)) movementDir -= Vector3.up * speed;

        targetPosition += movementDir;
        Vector3 realTargetPosition = targetPosition;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta  = Mouse.current.delta.ReadValue();
            float   sensitivity = 6f;

            yaw   += mouseDelta.x * sensitivity * Time.deltaTime;
            pitch -= mouseDelta.y * sensitivity * Time.deltaTime;
            pitch =  Mathf.Clamp(pitch, -89f, 89f);

            targetRotation = Quaternion.Euler(pitch, yaw, 0f);

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        Quaternion realTargetRotation = targetRotation;

        if (CameraHandler.Instance.SmoothingFactor > 0)
        {
            int realSmoothingFactor = GetSmoothingFactor();
            realTargetPosition = Vector3.Lerp(CameraHandler.Instance.transform.position, targetPosition,
                    Time.deltaTime * realSmoothingFactor);

            realTargetRotation = Quaternion.Slerp(CameraHandler.Instance.transform.rotation, targetRotation,
                    Time.deltaTime * realSmoothingFactor);
        }

        CameraHandler.Instance.transform.position = realTargetPosition;
        CameraHandler.Instance.transform.rotation = realTargetRotation;
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