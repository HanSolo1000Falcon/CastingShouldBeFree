using System.Collections.Generic;
using CastingShouldBeFree.Core.Mode_Handlers;
using CastingShouldBeFree.Utils;
using GorillaLocomotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface;

public class WorldSpaceHandler : Singleton<WorldSpaceHandler>
{
    public Camera RenderTextureCamera;

    public TextMeshProUGUI FOVText;
    public TextMeshProUGUI NearClipText;
    public TextMeshProUGUI SmoothingText;

    private GameObject canvas;

    private float initTime;

    private bool wasPressed;

    private void Start()
    {
        GameObject canvasPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("InGameCanvas");
        canvas = Instantiate(canvasPrefab);
        Destroy(canvasPrefab);
        canvas.name = "InGameCanvas";

        SetUpRenderTexture();
        SetUpCameraModes();
        SetUpCameraSettings();

        canvas.SetActive(false);
        initTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - initTime < 5f)
            return;

        bool isPressed = ControllerInputPoller.instance.leftControllerPrimaryButton;

        if (isPressed && !wasPressed)
        {
            canvas.SetActive(!canvas.activeSelf);

            canvas.transform.position = GTPlayer.Instance.bodyCollider.transform.position +
                                        GTPlayer.Instance.bodyCollider.transform.forward * 0.5f;

            canvas.transform.LookAt(GTPlayer.Instance.headCollider.transform);
            canvas.transform.Rotate(0f, 180f, 0f);

            if (!GUIHandler.Instance.HasInitEventSystem)
            {
                GUIHandler.Instance.InitEventSystem();
                GUIHandler.Instance.Canvas.transform.Find("MainPanel").gameObject.SetActive(false);
            }
        }

        wasPressed = isPressed;
    }

    private void SetUpRenderTexture()
    {
        RenderTexture renderTexture = new(1920, 1080, 24, RenderTextureFormat.ARGB32);
        renderTexture.name = "VR Render Texture";
        renderTexture.Create();
        // ^^ doing it like this because the stupid fucking assetbundle wouldnt load my render texture
        // assetbundles are so cool but for some fucking reason that bitch wouldnt load!!!!!!

        canvas.transform.Find("MainPanel/Image").GetComponent<RawImage>().texture = renderTexture;

        RenderTextureCamera             = new GameObject("Render Texture Camera").AddComponent<Camera>();
        RenderTextureCamera.cullingMask = Plugin.Instance.PCCamera.GetComponent<Camera>().cullingMask;

        RenderTextureCamera.transform.SetParent(Plugin.Instance.PCCamera.parent, false);
        RenderTextureCamera.transform.localPosition = Vector3.zero;
        RenderTextureCamera.transform.localRotation = Quaternion.identity;

        RenderTextureCamera.targetTexture = renderTexture;
    }

    private void SetUpCameraModes()
    {
        GameObject buttonPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("ModeButtonTemplate");
        Transform  modeContent  = canvas.transform.Find("MainPanel/Chin/Content");

        foreach (KeyValuePair<string, ModeHandlerBase> modeHandlerPair in CoreHandler.Instance.ModeHandlers)
        {
            GameObject modeButton = Instantiate(buttonPrefab, modeContent);
            modeButton.GetComponentInChildren<TextMeshProUGUI>().text = modeHandlerPair.Value.HandlerName;
            modeButton.transform.Find("Collider").AddComponent<PressableButton>().OnPress +=
                    () => CoreHandler.Instance.SetCurrentHandler(modeHandlerPair.Value.HandlerName);
        }
    }

    private void SetUpCameraSettings()
    {
        Transform tunablesContent = canvas.transform.Find("MainPanel/Tunables/Content");

        Transform fovPanel = tunablesContent.Find("FOVPanel");
        FOVText = fovPanel.Find("FOVText").GetComponent<TextMeshProUGUI>();
        fovPanel.Find("MoreFOV/Collider").AddComponent<PressableButton>().OnPress += () =>
                    CoreHandler.Instance.SetFOV((int)GUIHandler.Instance.FOVSlider.value + 5);

        fovPanel.Find("LessFOV/Collider").AddComponent<PressableButton>().OnPress += () =>
                    CoreHandler.Instance.SetFOV((int)GUIHandler.Instance.FOVSlider.value - 5);

        Transform nearClipPanel = tunablesContent.Find("NearClipPanel");
        NearClipText = nearClipPanel.Find("NearClipText").GetComponent<TextMeshProUGUI>();
        nearClipPanel.Find("MoreNearClip/Collider").AddComponent<PressableButton>().OnPress += () =>
                    CoreHandler.Instance.SetNearClip((int)GUIHandler.Instance.NearClipSlider.value + 1);

        nearClipPanel.Find("LessNearClip/Collider").AddComponent<PressableButton>().OnPress += () =>
                    CoreHandler.Instance.SetNearClip((int)GUIHandler.Instance.NearClipSlider.value - 1);

        Transform smoothingPanel = tunablesContent.Find("SmoothingPanel");
        SmoothingText = smoothingPanel.Find("SmoothingText").GetComponent<TextMeshProUGUI>();
        smoothingPanel.Find("MoreSmoothing/Collider").AddComponent<PressableButton>().OnPress += () =>
                    CoreHandler.Instance.SetSmoothing(CameraHandler.Instance.SmoothingFactor + 1);

        smoothingPanel.Find("LessSmoothing/Collider").AddComponent<PressableButton>().OnPress += () =>
                    CoreHandler.Instance.SetSmoothing(CameraHandler.Instance.SmoothingFactor - 1);
    }
}