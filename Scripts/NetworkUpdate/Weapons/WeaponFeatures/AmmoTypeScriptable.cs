using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DebuffStats
{
    [Tooltip("If true, this ammo type can apply this debuff")]
    public bool canInflict;
    [Tooltip("The chance to apply this debuff, from 0 (never) to 1 (always)"), Range(0, 1)]
    public float chanceToApply;
    [Tooltip("How long to apply this debuff for, in seconds.")]
    public float duration;
    [Tooltip("If true, this ammo will ADD the duration onto the existing debuff time, if a debuff of the same type exists on the target.")]
    public bool addDuration;
    [Tooltip("The debuff type to be applied by this ammo")]
    public DebuffType debuffType;

}


[CreateAssetMenu(menuName = "Scriptable Objects/Ammo Type")]
public class AmmoTypeScriptable : ScriptableObject
{
    [Tooltip("the multiplier to damage when charge is 0, if the weapon uses charge.")] public float minChargeDamageMultiplier = 0.2f;
    [Tooltip("the multiplier to damage when charge is 1, if the weapon uses charge.")] public float maxChargeDamageMultiplier = 0.2f;

    [Tooltip("How many projectiles fired by this weapon.")]
    public int fireIterations = 1;

    [Tooltip("Which debuffs to apply when this ammo type hits something")]
    public DebuffStats[] debuffsToApply;

    [Tooltip("The prefab to be used for the projectile")]
    public GameObject ProjectilePrefab;

}
