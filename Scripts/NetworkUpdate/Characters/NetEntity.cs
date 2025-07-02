using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

[System.Serializable]
public struct EntityDebuffData
{
    public DebuffType debuffType;
    public VisualEffect debuffVFX;
    public bool active;
}

public class NetEntity : NetDamageable
{
    

    public NetworkVariable<bool> isDead = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] protected bool canRegenerateHealth = false;
    [SerializeField] protected float regenerationDelay = 5;
    [SerializeField] protected float currentRegenTime = 0;
    [SerializeField] protected float regenerationRate = 5;
    [SerializeField] protected bool immuneToDamage = false;

    [SerializeField] protected bool receivesDebuffs;
    public List<Debuff> debuffs = new();

    public float movementModifier = 0;
    public float rotationModifier = 0;
    public bool canJump, canSprint;
    public bool[] slotsAllowed = new bool[]
    {
        true, true, true, true
    };
    [SerializeField] protected float damageMultiplier = 1;
    /// <summary>
    /// Invokes an event on all subscribers, passing the new health and the source entity's ID.
    /// </summary>
    public UnityEvent<float, NetworkBehaviourReference> HealthModified;

    public bool despawnAfterDeath;
    public float despawnAfterDeathTime;

    public EntityDebuffData[] debuffData;
    public Dictionary<DebuffType, EntityDebuffData> allowedDebuffs = new();

    int ticksBetweenDebuffSync = 5;
    int debuffSyncTick;
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
        //If DebuffData.Length is 0, then this entity is immune to debuffs.
        if(debuffData.Length > 0)
        {
            debuffs = new (debuffData.Length);
            //Convert the array of Debuff Data into a dictionary to check if we're allowed to use this debuff on this entity.
            for (int i = 0; i < debuffData.Length; i++)
            {
                EntityDebuffData item = debuffData[i];
                allowedDebuffs.Add(item.debuffType, item);
                //Clone the debuff info of that type into this debuff
                debuffs[i] = DebuffManager.Instance.debuffDictionary[item.debuffType];
                debuffs[i].entity = this;
            }
        }
    }

    /// <summary>
    /// "Cures" this debuff, setting the time to zero and setting it inactive.
    /// </summary>
    /// <param name="debuff"></param>
    public virtual void RemoveDebuff(Debuff debuff)
    {
        debuff.timeRemaining = 0;
        debuff.tickDownTime = false;
        allowedDebuffs[debuff.type].debuffVFX.Stop();
        SyncDebuffState_RPC((int)debuff.type, 0, false);
    }
    public virtual void InflictDebuff(DebuffStats statsIn, DebuffType targetDebuff)
    {
        int index = debuffs.FindIndex(x => x.type == targetDebuff);
        debuffs[index].InitialiseDebuff(statsIn);
        SyncDebuffState_RPC(index, debuffs[index].timeRemaining, debuffs[index].tickDownTime);
        allowedDebuffs[targetDebuff].debuffVFX.Play();
    }
    [Rpc(SendTo.Everyone)]
    public void SyncDebuffState_RPC(int debuffIndex, float timeRemaining, bool state)
    {
        debuffs[debuffIndex].timeRemaining = timeRemaining;
        debuffs[debuffIndex].tickDownTime = state;
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
        if (receivesDebuffs)
        {
            UpdateDebuffs();
        }
        
    }
    public void UpdateDebuffs()
    {
        for (int i = 0; i < slotsAllowed.Length; i++)
        {
            slotsAllowed[i] = true;
        }
        movementModifier = rotationModifier = 0;
        canJump = canSprint = true;
        for (int i = 0; i < debuffs.Count; i++)
        {
            debuffs[i].UpdateDebuff(IsServer);
        }

        debuffSyncTick++;
        if (debuffSyncTick >= ticksBetweenDebuffSync)
        {
            debuffSyncTick %= ticksBetweenDebuffSync;
            SyncDebuffs();
        }
    }
    public void SyncDebuffs()
    {
        for (int i = 0; i < debuffs.Count; i++)
        {
            SyncDebuffState_RPC(i, debuffs[i].timeRemaining, debuffs[i].tickDownTime);
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


