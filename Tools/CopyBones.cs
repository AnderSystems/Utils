using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CopyBones : MonoBehaviour
{
    public SkinnedMeshRenderer Source = new SkinnedMeshRenderer();
    public List<SkinnedMeshRenderer> Targets = new List<SkinnedMeshRenderer>();
    public bool getAllChildTargets;
    public bool copy;

    public static void Copy(SkinnedMeshRenderer from, SkinnedMeshRenderer to)
    {
        if (to != null && from != null)
        {
            to.rootBone = from.rootBone;
            to.bones = from.bones;
        }
    }
    void OnValidate()
    {
        if (getAllChildTargets)
        {
            Targets = GetComponentsInChildren<SkinnedMeshRenderer>(true).ToList();
            getAllChildTargets = false;
        }

        if (copy)
        {
            foreach (var target in Targets)
            {
                Copy(Source, target);
            }

            copy = false;
        }
    }
}