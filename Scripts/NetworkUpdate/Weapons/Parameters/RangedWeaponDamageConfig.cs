using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon Config/Ranged Damage")]
public class RangedWeaponDamageConfig : ScriptableObject
{
    [Tooltip("The distance at which the projectile deals maximum damage, in metres/units")]
    public float damageFalloffStart = 10;
    [Tooltip("The distance at which the projectile deals minimum damage, in metres/units")]
    public float damageFalloffEnd = 50;
    [Tooltip("The damage dealt up to min range")]
    public float maxDamage = 10;
    [Tooltip("The damage dealt past max range")]
    public float minDamage = 2;

    [Tooltip("The rate of damage falloff between the start and end ranges")]
    public AnimationCurve damageFalloffCurve = AnimationCurve.Linear(0, 1, 1, 0);
}
