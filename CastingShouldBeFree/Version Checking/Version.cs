using System;

namespace CastingShouldBeFree.Version_Checking;

// idk why i make but is cool
public class Version
{
    public readonly int Major;
    public readonly int Minor;
    public readonly int Patch;

    public Version(string versionString)
    {
        string[] parts = versionString.Split('.');

        if (parts.Length != 3 || !int.TryParse(parts[0], out Major) || !int.TryParse(parts[1], out Minor) ||
            !int.TryParse(parts[2],                      out Patch))
            throw new ArgumentException("Invalid version string. Use format 'x.y.z'");
    }

    private static int MakeInt(Version version) =>
            version.Major * 10000 + version.Minor * 100 + version.Patch;

    public static bool operator >(Version a, Version b) => MakeInt(a) > MakeInt(b);
    public static bool operator <(Version a, Version b) => MakeInt(a) < MakeInt(b);
}