using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Core.Mode_Handlers;
using CastingShouldBeFree.Utils;
using UnityEngine;

namespace CastingShouldBeFree.Core;

public class CoreHandler : Singleton<CoreHandler>
{
    public int MaxFOV;
    public int MinFOV;

    public int MaxNearClip;
    public int MinNearClip;

    public int MaxSmoothing;
    public int MinSmoothing;

    public readonly Dictionary<string, ModeHandlerBase> ModeHandlers = new();
    public          Action<VRRig, VRRig>                OnCastedRigChange;

    public Action<string> OnCurrentHandlerChange;

    private void Start()
    {
        GameObject modeHandlersComponents = new("Casting Should Be Free Mode Handlers");

        Type[] modeHandlerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
                    type.IsClass && !type.IsAbstract && typeof(ModeHandlerBase).IsAssignableFrom(type)).ToArray();

        foreach (Type modeHandlerType in modeHandlerTypes)
        {
            Component modeHandlerComponent = modeHandlersComponents.AddComponent(modeHandlerType);

            if (modeHandlerComponent is ModeHandlerBase modeHandler)
            {
                modeHandler.enabled                   = false;
                ModeHandlers[modeHandler.HandlerName] = modeHandler;
            }
            else
            {
                Debug.Log(modeHandlerType.Name + " isn't a mode handler, removing...");
                Destroy(modeHandlerComponent);
            }
        }

        gameObject.AddComponent<GUIHandler>();
        gameObject.AddComponent<WorldSpaceHandler>();
    }

    public void SetFOV(int fov)
    {
        fov                                                         = Mathf.Clamp(fov, MinFOV, MaxFOV);
        Plugin.Instance.PCCamera.GetComponent<Camera>().fieldOfView = fov;
        WorldSpaceHandler.Instance.RenderTextureCamera.fieldOfView  = fov;
        GUIHandler.Instance.FOVText.text                            = $"FOV: {fov}";
        WorldSpaceHandler.Instance.FOVText.text                     = $"FOV: {fov}";

        if (!Mathf.Approximately(GUIHandler.Instance.FOVSlider.value, fov))
            GUIHandler.Instance.FOVSlider.value = fov;
    }

    public void SetNearClip(int nearClip)
    {
        nearClip                                                      = Mathf.Clamp(nearClip, MinNearClip, MaxNearClip);
        GUIHandler.Instance.NearClipSlider.value                      = nearClip;
        Plugin.Instance.PCCamera.GetComponent<Camera>().nearClipPlane = nearClip / 100f;
        WorldSpaceHandler.Instance.RenderTextureCamera.nearClipPlane  = nearClip / 100f;
        GUIHandler.Instance.NearClipText.text                         = $"Near Clip: {nearClip}";
        WorldSpaceHandler.Instance.NearClipText.text                  = $"Near Clip: {nearClip}";

        if (!Mathf.Approximately(GUIHandler.Instance.NearClipSlider.value, nearClip))
            GUIHandler.Instance.NearClipSlider.value = nearClip;
    }

    public void SetSmoothing(int smoothing)
    {
        smoothing                                     = Mathf.Clamp(smoothing, MinSmoothing, MaxSmoothing);
        GUIHandler.Instance.SmoothingSlider.value     = smoothing;
        CameraHandler.Instance.SmoothingFactor        = smoothing;
        GUIHandler.Instance.SmoothingText.text        = $"Smoothing: {smoothing}";
        WorldSpaceHandler.Instance.SmoothingText.text = $"Smoothing: {smoothing}";

        if (!Mathf.Approximately(GUIHandler.Instance.SmoothingSlider.value, smoothing))
            GUIHandler.Instance.SmoothingSlider.value = smoothing;
    }

#region Backing Fields

    private VRRig  _castedRig;
    private string _currentHandlerName;

#endregion

#region Getters and Setters

    public VRRig CastedRig
    {
        get => _castedRig;

        set
        {
            if (_castedRig != value)
            {
                OnCastedRigChange?.Invoke(value, _castedRig);
                _castedRig = value;
            }
        }
    }

    public string CurrentHandlerName
    {
        get => _currentHandlerName;

        set
        {
            if (_currentHandlerName != value)
            {
                _currentHandlerName = value;

                foreach (string modeHandlerName in ModeHandlers.Keys)
                    ModeHandlers[modeHandlerName].enabled = modeHandlerName == value;

                OnCurrentHandlerChange?.Invoke(value);
            }
        }
    }

#endregion
}