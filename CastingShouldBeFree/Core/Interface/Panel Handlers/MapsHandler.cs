using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GorillaLocomotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface.Panel_Handlers;

public class MapsHandler : PanelHandlerBase
{
    private Transform mapsButtonHolder;

    protected override void Start()
    {
        mapsButtonHolder = transform.Find("MapsGrid/Viewport/Content");
        GameObject mapButtonPrefab = mapsButtonHolder.GetChild(0).gameObject;
        
        foreach (GTZone zone in Enum.GetValues(typeof(GTZone)))
        {
            string parsedZoneName = ParseZoneName(zone);
            if (string.IsNullOrEmpty(parsedZoneName)) continue;
            GameObject mapButton = Instantiate(mapButtonPrefab, mapsButtonHolder);
            mapButton.GetComponentInChildren<TextMeshProUGUI>().text = parsedZoneName;
            mapButton.GetComponent<Button>().onClick.AddListener(() => ZoneManagement.SetActiveZone(zone));
        }
        
        Destroy(mapButtonPrefab);

        transform.Find("ZeroGravity").GetComponent<Button>();
        
        base.Start();
    }

    private string ParseZoneName(GTZone zone)
    {
        string zoneName = zone.ToString();
        if (string.IsNullOrEmpty(zoneName)) return zoneName;
        string spacedZoneName = Regex.Replace(zoneName, "(?<!^)([A-Z])", " $1");
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(spacedZoneName);
    }
}