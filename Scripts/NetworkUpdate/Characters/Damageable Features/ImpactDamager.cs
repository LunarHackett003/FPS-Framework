using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactDamager : LunarNetScript
{
    public NetDamageable self;
   
    public bool damageSelf, damageOther;

    public float selfDamageImpulseThreshold;
    public float otherDamageImpulseThreshold;

    public bool selfDealFixedDamage, otherDealFixedDamage;
    public float selfDamage, otherDamage;

    public float selfDamageImpulseMultiplier;
    public float otherDamageImpulseMultiplier;

    public DamageSourceType damageSourceType;

    private void OnCollisionEnter(Collision collision)
    {
        float impulse = Mathf.Abs(collision.impulse.magnitude);
        if (damageSelf && self != null)
        {
            if(impulse > selfDamageImpulseThreshold)
            {
                self.ModifyHealth(selfDealFixedDamage ? -selfDamage : (-(impulse - selfDamageImpulseThreshold) * selfDamageImpulseMultiplier));
            }
        }
        if (damageOther && collision.collider.TryGetComponent(out NetDamageable d))
        {
            if(impulse > otherDamageImpulseThreshold)
            {
                
                d.ModifyHealth(otherDealFixedDamage ? -otherDamage : (-(impulse - selfDamageImpulseThreshold) * selfDamageImpulseMultiplier));
            }
        }
    }
}
