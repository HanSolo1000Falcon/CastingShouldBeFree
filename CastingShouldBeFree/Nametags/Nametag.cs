using CastingShouldBeFree.Utils;
using GorillaLocomotion;
using TMPro;
using UnityEngine;

namespace CastingShouldBeFree.Nametags;

public class Nametag : MonoBehaviour
{
    private bool _showTpTag = true;

    public bool ShowTpTag
    {
        get => _showTpTag;
        set
        {
            if (value != _showTpTag)
            {
                _showTpTag = value;
                thirdPersonNametag.Nametag.gameObject.SetActive(value);
            }
        }
    }
    
    private bool hasInit;

    private Transform nametagParent;

    private VRRig associatedRig;

    private string platform = "[STANDALONE]";

    private struct NametagComponents
    {
        public Transform Nametag;

        public TextMeshProUGUI NameText;
        public TextMeshProUGUI PlatformText;
        public TextMeshProUGUI FPSText;
    }

    private NametagComponents firstPersonNametag;
    private NametagComponents thirdPersonNametag;

    private void OnEnable()
    {
        if (!hasInit)
            return;

        nametagParent.gameObject.SetActive(true);
        RigUtils.OnRigNameChange += OnNameUpdate;
    }

    private void OnDisable()
    {
        if (!hasInit)
            return;
        
        nametagParent.gameObject.SetActive(false);
        RigUtils.OnRigNameChange -= OnNameUpdate;
    }

    private void Awake()
    {
        nametagParent = new GameObject("NametagParent").transform;
        nametagParent.SetParent(transform);
        nametagParent.localPosition = new Vector3(0f, 0.5f, 0f);

        associatedRig = GetComponent<VRRig>();

        if (associatedRig.isLocal)
        {
            platform = "[PC]";
        }
        else
        {
            platform = GetPlayerPlatform();
            RigUtils.OnRigCosmeticsLoad += (rig) =>
            {
                if (rig == associatedRig)
                    platform = GetPlayerPlatform();
            };
        }

        if (!associatedRig.isLocal)
            firstPersonNametag = SetUpNametagComponents(true);

        thirdPersonNametag = SetUpNametagComponents(false);

        hasInit = true;
    }

    private NametagComponents SetUpNametagComponents(bool firstPerson)
    {
        NametagComponents nametagComponents = new();
        nametagComponents.Nametag = Instantiate(NametagHandler.Instance.NametagPrefab, nametagParent).transform;
        nametagComponents.Nametag.gameObject.name = firstPerson ? "FirstPersonNametag" : "ThirdPersonNametag";
        nametagComponents.Nametag.transform.localPosition = Vector3.zero;

        nametagComponents.NameText = nametagComponents.Nametag.Find("Name").GetComponent<TextMeshProUGUI>();
        nametagComponents.PlatformText = nametagComponents.Nametag.Find("Platform").GetComponent<TextMeshProUGUI>();
        nametagComponents.FPSText = nametagComponents.Nametag.Find("FPS").GetComponent<TextMeshProUGUI>();
        
        nametagComponents.PlatformText.text = platform;
        nametagComponents.PlatformText.color = platform == "[STANDALONE]"
            ? NametagHandler.Instance.StandaloneColour
            : NametagHandler.Instance.SteamColour;

        nametagComponents.NameText.text = associatedRig.OwningNetPlayer != null
            ? associatedRig.OwningNetPlayer.NickName
            : associatedRig.playerText1.text;

        SetLayer(firstPerson, nametagComponents.Nametag);

        return nametagComponents;
    }

    private void OnNameUpdate(VRRig rig, string name)
    {
        if (rig != associatedRig)
            return;
        
        if (!associatedRig.isLocal)
            firstPersonNametag.NameText.text = name;
        
        thirdPersonNametag.NameText.text = name;
    }

    private void SetLayer(bool firstPerson, Transform trans)
    {
        foreach (Transform child in trans)
            SetLayer(firstPerson, child);

        trans.gameObject.layer = firstPerson ? UnityLayer.FirstPersonOnly.GetIndex() : UnityLayer.MirrorOnly.GetIndex();
    }

    private string GetPlayerPlatform()
    {
        string concat = associatedRig.concatStringOfCosmeticsAllowed.ToLower();

        if (concat.Contains("s. first login")) return "[STEAM]";
        if (concat.Contains("first login") || concat.Contains("game-purchase") || associatedRig.OwningNetPlayer.GetPlayerRef().CustomProperties.Count > 1) return "[PC]";
        if (platform == "[PC]" || platform == "[STEAM]") return platform;
        return "[STANDALONE]";
    }

    private void LateUpdate()
    {
        int fpsActual = associatedRig.isLocal ? (int)(1f / Time.unscaledDeltaTime) : associatedRig.fps;
        string colour = fpsActual > 60 ? fpsActual > 72 ? "green" : "orange" : "red";
        string fps = $"<color={colour}>{fpsActual} FPS</color>";

        if (ShowTpTag)
        {
            thirdPersonNametag.Nametag.LookAt(Plugin.Instance.PCCamera);
            thirdPersonNametag.Nametag.Rotate(0f, 180f, 0f);
            thirdPersonNametag.FPSText.text = fps;
        }
        
        if (!associatedRig.isLocal)
        {
            firstPersonNametag.Nametag.LookAt(GTPlayer.Instance.headCollider.transform);
            firstPersonNametag.Nametag.Rotate(0f, 180f, 0f);
            firstPersonNametag.FPSText.text = fps;
        }
    }
}