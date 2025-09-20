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
    #region Backing Fields

    private VRRig _castedRig;
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

    public int MaxSmoothing;
    
    public Action<string> OnCurrentHandlerChange;
    public Action<VRRig, VRRig> OnCastedRigChange;
    
    public readonly Dictionary<string, ModeHandlerBase> ModeHandlers = new();

    private void Start()
    {
        GameObject modeHandlersComponents = new GameObject("Casting Should Be Free Mode Handlers");
        
        Type[] modeHandlerTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
            type.IsClass && !type.IsAbstract && typeof(ModeHandlerBase).IsAssignableFrom(type)).ToArray();
        
        foreach (Type modeHandlerType in modeHandlerTypes)
        {
            Component modeHandlerComponent = modeHandlersComponents.AddComponent(modeHandlerType);

            if (modeHandlerComponent is ModeHandlerBase modeHandler)
            {
                modeHandler.enabled = false;
                ModeHandlers[modeHandler.HandlerName] = modeHandler;
            }
            else
            {
                Debug.Log(modeHandlerType.Name + " isn't a mode handler, removing.");
                Destroy(modeHandlerComponent);
            }
        }

        gameObject.AddComponent<GUIHandler>();
        gameObject.AddComponent<WorldSpaceHandler>();
    }
}