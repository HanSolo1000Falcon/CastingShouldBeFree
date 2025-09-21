using BepInEx;
using CastingShouldBeFree.Utils;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface;

public class WorldSpaceHandler : Singleton<WorldSpaceHandler>
{
    public Camera RenderTextureCamera;

    private GameObject canvas;

    private bool wasPressed;

    private float initTime;

    private void Start()
    {
        GameObject canvasPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("InGameCanvas");
        canvas = Instantiate(canvasPrefab);
        Destroy(canvasPrefab);
        canvas.name = "InGameCanvas";

        SetUpRenderTexture();

        canvas.SetActive(false);
        initTime = Time.time;
    }

    private void SetUpRenderTexture()
    {
        RenderTexture renderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        renderTexture.name = "VR Render Texture";
        renderTexture.Create();
        // ^^ doing it like this because the stupid fucking assetbundle wouldnt load my render texture
        // assetbundles are so cool but for some fucking reason that bitch wouldnt load!!!!!!

        canvas.transform.Find("MainPanel/Image").GetComponent<RawImage>().texture = renderTexture;

        RenderTextureCamera = new GameObject("Render Texture Camera").AddComponent<Camera>();
        RenderTextureCamera.cullingMask = Plugin.Instance.PCCamera.GetComponent<Camera>().cullingMask;

        RenderTextureCamera.transform.SetParent(Plugin.Instance.PCCamera.parent, false);
        RenderTextureCamera.transform.localPosition = Vector3.zero;
        RenderTextureCamera.transform.localRotation = Quaternion.identity;

        RenderTextureCamera.targetTexture = renderTexture;
    }

    private void Update()
    {
        if (Time.time - initTime < 5f)
            return;

        bool isPressed = ControllerInputPoller.instance.leftControllerPrimaryButton ||
                         UnityInput.Current.GetKey(KeyCode.F);

        if (isPressed && !wasPressed)
        {
            canvas.SetActive(!canvas.activeSelf);

            canvas.transform.position = GTPlayer.Instance.bodyCollider.transform.position +
                                        GTPlayer.Instance.bodyCollider.transform.forward;
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
}