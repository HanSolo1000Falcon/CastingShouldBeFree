using System.Collections.Generic;
using CastingShouldBeFree.Core;
using CastingShouldBeFree.Core.Mode_Handlers;
using CastingShouldBeFree.Patches;
using CastingShouldBeFree.Utils;
using UnityEngine;

namespace CastingShouldBeFree.Nametags;

public class NametagHandler : Singleton<NametagHandler>
{
    public GameObject NametagPrefab;

    private readonly Dictionary<VRRig, Nametag> nametags         = new();
    public readonly  Color                      StandaloneColour = new(0f, 0.5412027f, 0.8396226f);

    public readonly Color SteamColour = new(0f, 0.4205668f, 0.6509434f);

    private bool _nametagsEnabled = true;

    public bool NametagsEnabled
    {
        get => _nametagsEnabled;

        set
        {
            if (_nametagsEnabled != value)
            {
                foreach (KeyValuePair<VRRig, Nametag> nametag in nametags)
                {
                    nametag.Value.enabled = value;
                    nametag.Value.ShowTpTag = nametag.Key != CoreHandler.Instance.CastedRig ||
                                              CoreHandler.Instance.CurrentHandlerName !=
                                              FirstPersonModeHandler.HandlerNameStatic();
                }

                _nametagsEnabled = value;
            }
        }
    }

    private void Start()
    {
        NametagPrefab = Plugin.Instance.CastingBundle.LoadAsset<GameObject>("Nametag");

        if (SetColourPatch.SpawnedRigs.Contains(VRRig.LocalRig))
            OnRigSpawned(VRRig.LocalRig);

        RigUtils.OnRigSpawned                       += OnRigSpawned;
        RigUtils.OnRigCached                        += OnRigCached;
        CoreHandler.Instance.OnCastedRigChange      += OnCastedRigChange;
        CoreHandler.Instance.OnCurrentHandlerChange += OnCurrentHandlerChange;
    }

    private void OnRigSpawned(VRRig rig)
    {
        if (!nametags.ContainsKey(rig))
        {
            nametags[rig]         = rig.AddComponent<Nametag>();
            nametags[rig].enabled = NametagsEnabled;
            nametags[rig].ShowTpTag = rig != CoreHandler.Instance.CastedRig ||
                                      CoreHandler.Instance.CurrentHandlerName !=
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
            nametag2.enabled   = NametagsEnabled;
            nametag2.ShowTpTag = CoreHandler.Instance.CurrentHandlerName != FirstPersonModeHandler.HandlerNameStatic();
        }
    }

    private void OnCurrentHandlerChange(string handlerName)
    {
        foreach (KeyValuePair<VRRig, Nametag> nametag in nametags)
            if (nametag.Key == CoreHandler.Instance.CastedRig)
            {
                nametag.Value.enabled   = NametagsEnabled;
                nametag.Value.ShowTpTag = handlerName != FirstPersonModeHandler.HandlerNameStatic();
            }
    }
}