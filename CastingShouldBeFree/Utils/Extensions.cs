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

    public static Vector3 GetAngularVelocity(this Quaternion currentRotation, Quaternion lastRotation)
    {
        Quaternion deltaRotation = currentRotation * Quaternion.Inverse(lastRotation);
        deltaRotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);

        if (angleInDegrees > 180f)
            angleInDegrees -= 360f;
        
        if (angleInDegrees < 0.01f)
            return Vector3.one;

        float angularSpeed = angleInDegrees * Mathf.Deg2Rad / Mathf.Max(Time.deltaTime, 0.0001f);
        float maxPossible  = Mathf.PI                       / Time.deltaTime;
        float mappedSpeed  = Mathf.InverseLerp(0, maxPossible, angularSpeed) * 10f + 1f;
        return rotationAxis * mappedSpeed;
    }
}