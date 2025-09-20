using System.Globalization;
using CastingShouldBeFree.Utils;
using TMPro;
using UnityEngine;

namespace CastingShouldBeFree.Core.Interface;

public class ClosestTaggedHandler : Singleton<ClosestTaggedHandler>
{
    private TextMeshProUGUI closestTaggedText;

    private void Start() => closestTaggedText = GetComponentInChildren<TextMeshProUGUI>();

    private void LateUpdate()
    {
        if (TagManager.Instance.TaggedRigs.Contains(CoreHandler.Instance.CastedRig))
        {
            closestTaggedText.text = "Casted Player is <color=orange>Tagged!</color>";
            return;
        }

        if (TagManager.Instance.TaggedRigs.Count < 1)
        {
            closestTaggedText.text = "No <color=orange>Tagged</color> Players!";
            return;
        }

        float closestTaggedDistance = float.MaxValue;

        foreach (VRRig rig in TagManager.Instance.TaggedRigs)
        {
            if (rig == CoreHandler.Instance.CastedRig)
                continue;

            float distance = Vector3.Distance(rig.transform.position, CoreHandler.Instance.CastedRig.transform.position);

            if (distance < closestTaggedDistance)
                closestTaggedDistance = distance;
        }

        string colour = closestTaggedDistance > 5f ? closestTaggedDistance > 10f ? "green" : "orange" : "red";
        closestTaggedText.text =
            $"<color=orange>Lava</color> Distance: <color={colour}>{closestTaggedDistance.ToString("F1", CultureInfo.InvariantCulture)}m</color>";
    }
}