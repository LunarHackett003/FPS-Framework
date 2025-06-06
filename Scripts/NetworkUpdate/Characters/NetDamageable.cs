using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetDamageable : LunarNetScript
{
    public NetworkVariable<float> currentHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] internal int maxHealth;
    public int IntHealth => Mathf.RoundToInt(currentHealth.Value);

    public bool receiveDamageFromTeamOrOwner = false;

    public UnityEvent damageableDiedEvent;

    public Rigidbody rb;
    
    public virtual void ModifyHealth(float delta, NetworkBehaviourReference source = default, DamageSourceType damageSourceType = 0, bool isCrit = false)
    {
        float startHealth = currentHealth.Value;

        if (delta > 0)
            return;

        if (IsServer)
        {
            HealthUpdated_RPC(delta, source, damageSourceType, isCrit);
        }
        switch (damageSourceType)
        {
            case DamageSourceType.world:
                WorldDamage(delta);
                break;
            case DamageSourceType.weapon:
                WeaponDamage(delta, isCrit, source);
                break;
            case DamageSourceType.hazard:
                MeleeDamage(delta, source);
                break;
            case DamageSourceType.melee:
                MeleeDamage(delta, source);
                break;
            default:
                break;
        }
        if ((currentHealth.Value <= 0))
        {
            DamageableDied(source, isCrit);
        }
    }
    [Rpc(SendTo.NotServer)]
    public void HealthUpdated_RPC(float delta, NetworkBehaviourReference source = default, DamageSourceType damageSourceType = 0, bool isCrit = false)
    {
        ModifyHealth(delta, source, damageSourceType, isCrit);
    }
    public virtual void WorldDamage(float deltaHealth)
    {
        if (IsServer)
        {
            currentHealth.Value += deltaHealth;
        }
    }
    public virtual void WeaponDamage(float deltaHealth, bool isCrit, NetworkBehaviourReference reference)
    {
        if(reference.TryGet(out BaseNetWeapon weapon))
        {
            if (isCrit && weapon.canCrit)
            {
                deltaHealth *= weapon.critMultiplier;
            }
            if (IsServer)
            {
                currentHealth.Value += deltaHealth;
            }
        }
    }
    public virtual void MeleeDamage(float deltaHealth, NetworkBehaviourReference reference)
    {
        if (IsServer)
        {
            currentHealth.Value += deltaHealth;
        }
    }
    public virtual void HazardDamage(float deltaHealth, NetworkBehaviourReference reference)
    {
        if (IsServer)
        {
            currentHealth.Value += deltaHealth;
        }
    }

    public virtual void DamageableDied(NetworkBehaviourReference sourceObj, bool isCrit)
    {
        damageableDiedEvent?.Invoke();
    }
    [Rpc(SendTo.Owner)]
    public void ApplyForceToOwner_RPC(Vector3 force, Vector3 point = default)
    {
        if(rb != null)
        {
            rb.AddForceAtPosition(force, point, ForceMode.Impulse);
        }
    }
}


public enum DamageSourceType : int
{
    world = 0,
    weapon = 1,
    hazard = 2,
    melee = 3
}
