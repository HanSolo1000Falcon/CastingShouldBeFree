using CastingShouldBeFree.Utils;
using TMPro;

namespace CastingShouldBeFree.Core.Interface.Panel_Handlers;

public class MoreInfoHandler : Singleton<MoreInfoHandler>
{
    private TextMeshProUGUI modsText;

    protected override void Awake()
    {
        modsText = transform.Find("ModsInstalled/InstalledMods").GetComponent<TextMeshProUGUI>();
        modsText.text = "<color=red>No</color> Player Selected";

        if (CoreHandler.Instance.CastedRig != null)
            OnCastedRigChange(CoreHandler.Instance.CastedRig, null);
        
        CoreHandler.Instance.OnCastedRigChange += OnCastedRigChange;
    }

    private void OnCastedRigChange(VRRig currentRig, VRRig lastRig)
    {
        if (currentRig == null)
            return;

        if (currentRig.OwningNetPlayer == null)
        {
            modsText.text =
                "Casted Player <color=red>Doesn't</color> Have An OwningNetPlayer, Are You Connected To A Room?";
            return;
        }
        
        var customProps = currentRig.OwningNetPlayer.GetPlayerRef().CustomProperties;
        string text = "";
        
        foreach (string prop in customProps.Keys)
        {
            if (Plugin.Instance.KnownMods.TryGetValue(prop, out string mod))
                text += "<color=green>[" + mod + "]</color> ";
            else if (Plugin.Instance.KnownCheats.TryGetValue(prop, out string cheat))
                text += "<color=red>[" + cheat + "]</color> ";
        }

        if (text == "")
            text = "<color=red>Couldn't</color> Detect Any Mods";
        
        modsText.text = text.Trim();
    }
}