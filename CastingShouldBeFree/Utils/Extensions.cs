using UnityEngine;

namespace CastingShouldBeFree.Utils;

public static class Extensions
{
    public static bool IsTagged(this VRRig rig)
    {
        bool isInfectionTagged = rig.setMatIndex == 2 || rig.setMatIndex == 11;
        bool isRockTagged      = rig.setMatIndex == 1;

        return isInfectionTagged || isRockTagged;
    }

    public static Vector3 GetAngularVelocity(this Quaternion currentRotation, Quaternion lastRotation, float t)
    {
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
        if (angleInDegrees > 180) angleInDegrees -= 360;
        float   angleInRadians                   = angleInDegrees          * Mathf.Deg2Rad;
        Vector3 angularVelocity                  = rotationAxis.normalized * (angleInRadians / t);

        return angularVelocity;
    }
}