using System.Collections.Generic;
using CastingShouldBeFree.Core.Interface;
using CastingShouldBeFree.Utils;
using UnityEngine;

namespace CastingShouldBeFree.Core;

public class AutoCaster : Singleton<AutoCaster>
{
    public bool IsEnabled; // doing it like this means that 'Instance' will be assigned

    private float lastTime;
    
    private void Update()
    {
        if (!IsEnabled || Time.time - lastTime < 5f)
            return;

        lastTime = Time.time;

        List<VRRig> farAwayPeople = new();
        
        VRRig chosenRig = VRRig.LocalRig;
        float fastestSpeed = 0f;

        foreach (VRRig untaggedRig in TagManager.Instance.UnTaggedRigs)
        {
            if (untaggedRig == VRRig.LocalRig)
                continue;
            
            float distance = GetTagDistance(untaggedRig);

            if (distance > 20f)
            {
                farAwayPeople.Add(untaggedRig);
                continue;
            }

            Vector3 velocity = untaggedRig.LatestVelocity();
            velocity.y = 0f;
            float actualVelocity = velocity.magnitude;

            if (actualVelocity > fastestSpeed)
            {
                fastestSpeed = actualVelocity;
                chosenRig = untaggedRig;
            }
        }

        if (farAwayPeople.Count > 0 && chosenRig == VRRig.LocalRig)
        {
            foreach (VRRig farAwayRig in farAwayPeople)
            {
                Vector3 velocity = farAwayRig.LatestVelocity();
                velocity.y = 0f;
                float actualVelocity = velocity.magnitude;

                if (actualVelocity > fastestSpeed)
                {
                    fastestSpeed = actualVelocity;
                    chosenRig = farAwayRig;
                }
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