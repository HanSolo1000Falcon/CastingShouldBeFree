using System.Collections.Generic;

namespace CastingShouldBeFree.Utils;

public class TagManager : Singleton<TagManager>
{
    public readonly HashSet<VRRig> TaggedRigs = new();
    public readonly HashSet<VRRig> UnTaggedRigs = new();

    private void Start()
    {
        RigUtils.OnMatIndexChange += OnMatIndexChange;
        RigUtils.OnRigCached += OnRigCached;
    }
    
    private void OnMatIndexChange(VRRig rig)
    {
        TaggedRigs.Remove(rig);
        UnTaggedRigs.Remove(rig);

        if (rig.IsTagged())
            TaggedRigs.Add(rig);
        else
            UnTaggedRigs.Add(rig);
    }
    
    private void OnRigCached(VRRig rig)
    {
        TaggedRigs.Remove(rig);
        UnTaggedRigs.Remove(rig);
    }
}