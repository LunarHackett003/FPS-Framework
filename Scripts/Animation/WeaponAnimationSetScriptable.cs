using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Animation Set")]
public class WeaponAnimationSetScriptable : ScriptableObject
{
    public AnimationClipPair[] clips = new AnimationClipPair[0];
}
[System.Serializable]
public struct AnimationClipPair
{
    public AnimationClip targetClip;
    public AnimationClip characterClip, weaponClip;
}
