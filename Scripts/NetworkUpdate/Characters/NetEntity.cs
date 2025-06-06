using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class NetEntity : NetDamageable
{
    

    public NetworkVariable<bool> isDead = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] protected bool canRegenerateHealth = false;
    [SerializeField] protected float regenerationDelay = 5;
    [SerializeField] protected float currentRegenTime = 0;
    [SerializeField] protected float regenerationRate = 5;
    [SerializeField] protected bool immuneToDamage = false;

    [SerializeField] protected bool receivesDebuffs;
    [SerializeField] protected List<Debuff> currentDebuffs;

    [SerializeField] protected float movementModifier = 0;
    [SerializeField] protected float rotationModifier = 0;

    [SerializeField] protected float damageMultiplier = 1;
    /// <summary>
    /// Invokes an event on all subscribers, passing the new health and the source entity's ID.
    /// </summary>
    public UnityEvent<float, NetworkBehaviourReference> HealthModified;

    public bool despawnAfterDeath;
    public float despawnAfterDeathTime;

    float lastHealth;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        if(rb == null)
        {
            TryGetComponent(out rb);
        }
    }

    public virtual void ClearDebuff(Debuff debuff)
    {
        if (currentDebuffs.Contains(debuff))
        {
            currentDebuffs.Remove(debuff);
        }
    }


    [Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
    public void HealthChanged_RPC(float deltaHealth, NetworkBehaviourReference source, DamageSourceType damageSourceType = DamageSourceType.world, bool isCrit = false)
    {
        HealthModified?.Invoke(deltaHealth, source);
        if(deltaHealth < 0)
            currentRegenTime = 0;
        ModifyHealth(deltaHealth, source, damageSourceType, isCrit);
    }

    public override void ModifyHealth(float delta, NetworkBehaviourReference source = default, DamageSourceType damageSourceType = DamageSourceType.world, bool isCrit = false)
    {
        base.ModifyHealth(delta, source, damageSourceType, isCrit);

        if(delta < 0)
        {
            currentRegenTime = 0;
        }
    }

    public override void LTimestep()
    {
        base.LTimestep();
        lastHealth = currentHealth.Value;

        if(!isDead.Value && transform.position.y < -50 && currentHealth.Value > 0)
        {
            ModifyHealth(-999, null, DamageSourceType.world, false);
        }

        if (canRegenerateHealth && !isDead.Value)
        {
            if(currentHealth.Value < maxHealth)
            {
                if (currentRegenTime >= regenerationDelay)
                {
                    if (IsServer)
                    {
                        currentHealth.Value = Mathf.Clamp(currentHealth.Value + Time.fixedDeltaTime * regenerationRate, 0, maxHealth);
                    }
                }
                else
                {
                    currentRegenTime += Time.fixedDeltaTime;
                }
            }
        }

        if (IsServer)
        {
            if(currentHealth.Value > maxHealth || currentHealth.Value < 0)
            {
                currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
            }
        }

        if (IsServer && lastHealth != currentHealth.Value)
        {
            float delta = currentHealth.Value - lastHealth;
            lastHealth = currentHealth.Value;
            Debug.Log($"Updating health - {delta} hp changed");
            //We check if we've regenerated any health, and then we tell the clients that the owner of this object modified its health.
            HealthChanged_RPC(delta, this);
        }
    }

    public override void DamageableDied(NetworkBehaviourReference sourceObj, bool isCrit)
    {
        base.DamageableDied(sourceObj, isCrit);

        if (IsServer)
        {
            isDead.Value = true;
        }
        if (despawnAfterDeath)
        {
            StartCoroutine(DespawnAfterDeath());
        }
    }
    IEnumerator DespawnAfterDeath()
    {
        yield return new WaitForSeconds(despawnAfterDeathTime);
        NetworkObject.Despawn();
    }
}


