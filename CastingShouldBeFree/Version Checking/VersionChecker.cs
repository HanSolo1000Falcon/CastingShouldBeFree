using System.IO;
using System.Net.Http;
using CastingShouldBeFree.Core;
using UnityEngine;

namespace CastingShouldBeFree.Version_Checking;

public class VersionChecker : MonoBehaviour
{
    private void Start()
    {
        using HttpClient client = new();
        HttpResponseMessage response = client
                                      .GetAsync(
                                               "https://raw.githubusercontent.com/HanSolo1000Falcon/CastingShouldBeFree/master/version.txt")
                                      .Result;

        response.EnsureSuccessStatusCode();
        using Stream       stream   = response.Content.ReadAsStreamAsync().Result;
        using StreamReader reader   = new(stream);
        string             contents = reader.ReadToEnd().Trim();

        string[] parts = contents.Split(";");

        Version mostUpToDateVersion = new(parts[0]);
        Version currentVersion      = new(Constants.PluginVersion);

        if (currentVersion < mostUpToDateVersion)
            CoreHandler.Instance.OnDeprecatedVersionDetected(currentVersion, mostUpToDateVersion, parts[1]);
    }
}