using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class ItemSpineChanger : MonoBehaviour
{
    public void SpineSet(string spineName)
    {
        GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, spineName, false);
    }
}
