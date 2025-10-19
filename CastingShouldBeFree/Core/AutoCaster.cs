using CastingShouldBeFree.Utils;
using UnityEngine;

namespace CastingShouldBeFree.Core;

public class AutoCaster : Singleton<AutoCaster>
{
    public bool IsEnabled; // doing it like this means that 'Instance' will be assigned

    private float lastTime;

    private void Update()
    {
        if (!IsEnabled || CoreHandler.Instance.CastedRig == null)
            return;

        if (CoreHandler.Instance.CastedRig.IsTagged())
            lastTime = 0f;

        if (Time.time - lastTime < 5f)
            return;

        lastTime = Time.time;

        VRRig chosenRig                  = VRRig.LocalRig;
        float fastestAndClosestRatioBest = float.MaxValue;

        foreach (VRRig untaggedRig in TagManager.Instance.UnTaggedRigs)
        {
            float   distance = GetTagDistance(untaggedRig);
            Vector3 velocity = untaggedRig.LatestVelocity();
            velocity.y = 0f;
            float actualVelocity                                        = velocity.magnitude;
            if (Mathf.Approximately(actualVelocity, 0f)) actualVelocity = 0.0001f;
            float ratio                                                 = distance / actualVelocity;

            if (ratio < fastestAndClosestRatioBest)
            {
                fastestAndClosestRatioBest = ratio;
                chosenRig                  = untaggedRig;
            }
        }

        CoreHandler.Instance.CastedRig = chosenRig;
    }

    private float GetTagDistance(VRRig rig)
    {
        float closestDistance = float.MaxValue;

        foreach (VRRig taggedRig in TagManager.Instance.TaggedRigs)
        {
            if (taggedRig == rig)
                continue;

            float distance = Vector3.Distance(taggedRig.transform.position, rig.transform.position);
            if (distance < closestDistance) closestDistance = distance;
        }

        return closestDistance;
    }
}