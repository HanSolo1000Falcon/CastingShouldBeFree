using CastingShouldBeFree.Utils;
using GorillaExtensions;
using UnityEngine;

namespace CastingShouldBeFree.Core.Interface;

public class CameraHandler : Singleton<CameraHandler>
{
    public int SmoothingFactor;
    
    private void Start()
    {
        FixEverything(gameObject);
        
        Plugin.Instance.PCCamera.transform.SetParent(transform);
        Plugin.Instance.PCCamera.transform.localPosition = Vector3.zero;
        Plugin.Instance.PCCamera.transform.localRotation = Quaternion.identity;
    }

    private void FixEverything(GameObject obj)
    {
        foreach (Transform child in obj.transform)
            FixEverything(child.gameObject);
        
        if (obj.TryGetComponent(out Renderer renderer))
        {
            foreach (Material material in renderer.materials)
            {
                material.shader = Shader.Find("GorillaTag/UberShader");
                
                if (material.mainTexture != null)
                    material.EnableKeyword("_USE_TEXTURE");
            }
        }
        
        obj.layer = UnityLayer.FirstPersonOnly.GetIndex();
    }
    
    public void ToggleVisibility(bool toggled) => ToggleVisibilityInternal(gameObject, toggled);

    private void ToggleVisibilityInternal(GameObject obj, bool toggled)
    {
        foreach (Transform child in obj.transform)
            ToggleVisibilityInternal(child.gameObject, toggled);

        if (obj.TryGetComponent(out Renderer renderer))
            renderer.enabled = toggled;
    }
}