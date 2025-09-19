using System.Collections.Generic;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Core.ModeHandlers;
using CastingShouldBeFree.Patches;
using CastingShouldBeFree.Utils;
using TMPro;
using UnityEngine;

namespace CastingShouldBeFree.Nametags;

public class NametagHandler : Singleton<NametagHandler>
{
    public GameObject NametagPrefab;

    private bool _nametagsEnabled = true;

    public bool NametagsEnabled
    {
        get => _nametagsEnabled;
        set
        {
            if (_nametagsEnabled != value)
            {
                foreach (var nametag in nametags)
                {
                    nametag.Value.enabled = value;
                    nametag.Value.ShowTpTag = nametag.Key != GUIHandler.Instance.CastedRig ||
                                              GUIHandler.Instance.CurrentHandlerName !=
                                              FirstPersonModeHandler.HandlerNameStatic();
                }

                _nametagsEnabled = value;
            }
        }
    }

    public readonly Color SteamColour = new(0f, 0.4205668f, 0.6509434f);
    public readonly Color StandaloneColour = new(0f, 0.5412027f, 0.8396226f);

    private Dictionary<VRRig, Nametag> nametags = new();

    private void Start()
    {
        NametagPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("Nametag");

        if (SetColourPatch.SpawnedRigs.Contains(VRRig.LocalRig))
            OnRigSpawned(VRRig.LocalRig);

        RigUtils.OnRigSpawned += OnRigSpawned;
        RigUtils.OnRigCached += OnRigCached;
        GUIHandler.Instance.OnCastedRigChange += OnCastedRigChange;
        GUIHandler.Instance.OnCurrentHandlerChange += OnCurrentHandlerChange;
    }

    private void OnRigSpawned(VRRig rig)
    {
        if (!nametags.ContainsKey(rig))
        {
            nametags[rig] = rig.AddComponent<Nametag>();
            nametags[rig].enabled = NametagsEnabled;
            nametags[rig].ShowTpTag = rig != GUIHandler.Instance.CastedRig ||
                                      GUIHandler.Instance.CurrentHandlerName !=
                                      FirstPersonModeHandler.HandlerNameStatic();
        }
    }

    private void OnRigCached(VRRig rig)
    {
        if (nametags.ContainsKey(rig))
        {
            Destroy(nametags[rig]);
            nametags.Remove(rig);
            
            GorillaTagger.Instance.rigidbody.AddForce(-Physics.gravity * GorillaTagger.Instance.rigidbody.mass);
        }
    }

    private void OnCastedRigChange(VRRig currentRig, VRRig lastRig)
    {
        if (lastRig != null && nametags.TryGetValue(lastRig, out Nametag nametag))
            nametag.enabled = NametagsEnabled;

        if (nametags.TryGetValue(currentRig, out Nametag nametag2))
        {
            nametag2.enabled = NametagsEnabled;
            nametag2.ShowTpTag = GUIHandler.Instance.CurrentHandlerName != FirstPersonModeHandler.HandlerNameStatic();
        }
    }

    private void OnCurrentHandlerChange(string handlerName)
    {
        foreach (var nametag in nametags)
        {
            if (nametag.Key == GUIHandler.Instance.CastedRig)
            {
                nametag.Value.enabled = NametagsEnabled;
                nametag.Value.ShowTpTag = handlerName != FirstPersonModeHandler.HandlerNameStatic();
            }
        }
    }
}