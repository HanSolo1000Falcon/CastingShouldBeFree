namespace CastingShouldBeFree.Utils;

public static class Extensions
{
    public static bool IsTagged(this VRRig rig)
    {
        bool isInfectionTagged = rig.setMatIndex == 2 || rig.setMatIndex == 11;
        bool isRockTagged      = rig.setMatIndex == 1;

        return isInfectionTagged || isRockTagged;
    }
}