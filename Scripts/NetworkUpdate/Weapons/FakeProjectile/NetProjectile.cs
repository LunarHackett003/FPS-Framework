using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public class NetProjectile : LunarNetScript
{
    public float damageMultiplier = 1;
    public float velocityMultiplier = 1;
    public ParticleSystem projectileEffect;
    public VisualEffect[] projectileVFX;
    public float maxAliveTime = 10;
    public float maxDistance = 100;

    internal float distanceTravelled;
    [SerializeField] internal float timeAlive;

    public bool projectileAlive;

    internal RangedNetWeapon weapon;

    [SerializeField] internal bool terminated;

    public float returnTimeAfterTerminate = 1;
    [SerializeField] protected float terminateTime;
    [SerializeField] protected bool waitingToFire = true;
    [SerializeField] internal float radius;
    [SerializeField] internal Vector3 direction;
    [SerializeField] internal float velocity = 10;
    [SerializeField] internal float gravityMultiplier = 1;
    public HashSet<Collider> ignoredColliders;

    public Renderer[] renderersToHideOnHit;

    public NetworkObject terminatePrefab;

    public bool spawnEffectOnExpire, spawnEffectOnHit;

    bool firstSpawn;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(projectileEffect)
            projectileEffect.Play();
        if (projectileVFX.Length > 0)
            projectileVFX.PlayVFX(true);

        if (IsServer)
        {
            if (!firstSpawn)
            {
                ProjectileSimulator.allProjectiles.Add(this);
                firstSpawn = true;
            }
        }
    }



    public void InitialiseProjectile(RangedNetWeapon weapon, Vector3 direction, float charge)
    {

        if (NetworkManager.Singleton.IsServer)
        {
            ignoredColliders = new(weapon.ignoredColliders);

            this.weapon = weapon;
            damageMultiplier = charge != 0 ? Mathf.Lerp(weapon.minChargeDamageMultiplier, weapon.maxChargeDamageMultiplier, charge) : 1;
            transform.position = weapon.fireOrigin.position;
            transform.forward = direction;
            this.direction = direction;
            terminated = false;
            timeAlive = 0;
            distanceTravelled = 0;
            terminateTime = 0;
            waitingToFire = false;

            projectileAlive = true;


            GetComponent<NetworkObject>().SpawnWithOwnership(weapon.OwnerClientId);

            SetAliveState_RPC(true);
        }
    }
    public void TerminateProjectile(bool reasonIsHit)
    {
        terminated = true;
        projectileAlive = false;
        timeAlive = maxAliveTime;

        if((reasonIsHit && spawnEffectOnHit) || (!reasonIsHit && spawnEffectOnExpire))
        {
            SpawnHitEffect();
        }

        SetAliveState_RPC(false);
    }
    void SpawnHitEffect()
    {
        if (NetworkManager.SpawnManager.InstantiateAndSpawn(terminatePrefab, OwnerClientId, position: transform.position, rotation: transform.rotation).TryGetComponent(out ProjectileHitEffect phe))
        {
            phe.source = weapon;
        }
    }
    public override void LTimestep()
    {
        if (IsServer)
        {
            if (terminated && !waitingToFire)
            {
                TerminatedTick();
            }
        }
    }

    public virtual void TickProjectile()
    {
        if (!terminated)
        {
            distanceTravelled += Time.fixedDeltaTime * velocity;
            timeAlive += Time.fixedDeltaTime;
            transform.position += Time.fixedDeltaTime * velocity * direction;
            direction += gravityMultiplier * Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
        }
    }
    public virtual void TerminatedTick()
    {
        terminateTime += Time.fixedDeltaTime;
        if(terminateTime > returnTimeAfterTerminate)
        {
            RemoveProjectile();
        }
    }
    public void RemoveProjectile()
    {
        if (terminated)
        {
            weapon.ProjectilePool.Release(this);
            NetworkObject.Despawn(false);
            waitingToFire = true;
        }
    }
    [Rpc(SendTo.Everyone)]
    public void SetAliveState_RPC(bool state)
    {
        SetTerminateStateOnClients(state);
    }
    public virtual void SetTerminateStateOnClients(bool state)
    {
        for (int i = 0; i < renderersToHideOnHit.Length; i++)
        {
            renderersToHideOnHit[i].enabled = state;
        }
        projectileVFX.PlayVFX(state);
        if (projectileEffect)
        {
            if (state)
                projectileEffect.Play();
            else
                projectileEffect.Stop();
        }
    }
}
