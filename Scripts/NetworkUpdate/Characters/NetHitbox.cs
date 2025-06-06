using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetHitbox : NetDamageable
{
    [SerializeField] NetDamageable rootDamageable;

    [SerializeField] internal float damageMultiplier = 1;
    [SerializeField] internal bool isCritBox = false;

    [SerializeField] Collider col;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (col == null)
        {
            col = GetComponent<Collider>();
        }

        if(rb == null && rootDamageable.rb != null)
        {
            rb = rootDamageable.rb;
        }

        col.enabled = true;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        if(col != null)
        {
            col.enabled = false;
        }
    }

    public override void ModifyHealth(float delta, NetworkBehaviourReference source = default, DamageSourceType damageSourceType = 0, bool isCrit = false)
    {
        if(rootDamageable != null)
        {
            rootDamageable.ModifyHealth(delta, source, damageSourceType, isCritBox || isCrit);
            Debug.Log("Passed damage from hitbox to root damageable");
        }
        else
        {
            Debug.Log("Hitbox blocked all damage.");
        }
    }

}
