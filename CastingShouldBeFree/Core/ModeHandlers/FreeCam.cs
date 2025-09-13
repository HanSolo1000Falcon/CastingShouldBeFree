using BepInEx;
using CastingShouldBeFree.Core.Interface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CastingShouldBeFree.Core.ModeHandlers;

public class FreeCam : ModeHandlerBase
{
    public override string HandlerName => "Free Cam";

    private float yaw;
    private float pitch;

    private void OnEnable()
    {
        CameraHandler.Instance.Parent = null;
        CameraHandler.Instance.TargetPosition = CameraHandler.Instance.transform.position;
        CameraHandler.Instance.TargetRotation = CameraHandler.Instance.transform.rotation;

        Vector3 euler = CameraHandler.Instance.transform.rotation.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    private void OnDisable() => Cursor.lockState = CursorLockMode.None;

    private void Update()
    {
        float speed = UnityInput.Current.GetKey(KeyCode.LeftShift) ? 30f : 15f;
        speed *= Time.deltaTime;

        Vector3 movementDir = Vector3.zero;

        if (UnityInput.Current.GetKey(KeyCode.W)) movementDir += CameraHandler.Instance.transform.forward * speed;
        if (UnityInput.Current.GetKey(KeyCode.S)) movementDir -= CameraHandler.Instance.transform.forward * speed;
        if (UnityInput.Current.GetKey(KeyCode.A)) movementDir -= CameraHandler.Instance.transform.right * speed;
        if (UnityInput.Current.GetKey(KeyCode.D)) movementDir += CameraHandler.Instance.transform.right * speed;
        if (UnityInput.Current.GetKey(KeyCode.Space)) movementDir += Vector3.up * speed;
        if (UnityInput.Current.GetKey(KeyCode.LeftControl)) movementDir -= Vector3.up * speed;

        CameraHandler.Instance.TargetPosition += movementDir;

        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float sensitivity = 6f;

            yaw += mouseDelta.x * sensitivity * Time.deltaTime;
            pitch -= mouseDelta.y * sensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            CameraHandler.Instance.TargetRotation = Quaternion.Euler(pitch, yaw, 0f);

            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}