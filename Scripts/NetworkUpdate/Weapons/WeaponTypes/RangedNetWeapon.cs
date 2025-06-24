using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// The latest iteration in the weapon system, combining hitscan AND projectile weapons.<br></br>
/// 
/// </summary>
public class RangedNetWeapon : BaseNetWeapon
{

    IObjectPool<NetProjectile> projectilePool;
    public int poolStartCapacity = 50, poolMaxCapacity = 500;

    public IObjectPool<NetProjectile> ProjectilePool
    {
        get
        {
            projectilePool ??= new ObjectPool<NetProjectile>(CreatePooledItem, TakeFromPool, ReturnToPool, DestroyPoolObject, true, poolStartCapacity, poolMaxCapacity);
            return projectilePool;
        }
    }

    NetProjectile CreatePooledItem()
    {
        NetProjectile np = Instantiate(ammoType.ProjectilePrefab, fireOrigin.position, Quaternion.identity, null).GetComponent<NetProjectile>();
        np.gameObject.hideFlags = HideFlags.HideInHierarchy;
        return np;
    }
    void ReturnToPool(NetProjectile trace)
    {
        trace.gameObject.hideFlags = HideFlags.HideAndDontSave;
        if(trace.projectileEffect != null)
        {
            trace.projectileEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        trace.projectileVFX?.PlayVFX(false);
        trace.gameObject.SetActive(false);
    }
    void TakeFromPool(NetProjectile trace)
    {
        trace.gameObject.SetActive(true);
        trace.gameObject.hideFlags = HideFlags.None;
        trace.terminated = false;
    }
    void DestroyPoolObject(NetProjectile trace)
    {
        if (trace.NetworkObject != null && trace.NetworkObject.IsSpawned)
        {
            trace.NetworkObject.Despawn(true);
        }
        Destroy(trace.gameObject);
    
    }







    public enum FireMode : int
    {
        single = 1,
        burst = 2,
        automatic = 4,
        animated = 8
    }
    [SerializeField] internal RangedWeaponDamageConfig damageConfig;

    [Tooltip("the blastRadius of the base spread per unit distance covered by the shot.")]
    public float baseSpreadPerUnit = 0.1f;
    [Tooltip("the blastRadius of the max spread influenced by movement.")]
    public float maxInfluencedSpreadPerUnit = 0.1f;
    [Tooltip("the current influence of the owner's movement.")]
    public float currentMovementInfluence = 0;


    public virtual Vector3 SpreadVector => (((Vector3)Random.insideUnitCircle *
        (baseSpreadPerUnit + (maxInfluencedSpreadPerUnit * controller.Spread(baseAttackSpread + attackSpreadAmount))))
        + Vector3.forward).normalized;

    public Quaternion FireRotation => controller.fireOrigin != null ? controller.fireOrigin.rotation : fireOrigin.rotation;
    public Vector3 FirePosition => controller.fireOrigin != null ? controller.fireOrigin.position : fireOrigin.position;

    public FireMode[] allowedFireModes = new FireMode[] { FireMode.automatic };
    public int fireModeIndex = 0;
    public float fireModeSwitchTime;
    public FireMode CurrentFireMode => allowedFireModes[fireModeIndex];
    public int roundsPerMinute;
    public int roundsInBurst;
    protected int burstRoundsFired;
    public float timeBetweenRounds;
    public float TrueTimeBetweenRounds => timeBetweenRounds /
        (chargeAffectsFireRate ? Mathf.Lerp(minChargeFireRateMultiplier, maxChargeFireRateMultiplier, chargeAmount) : 1);

    public float debugdisplay_truetime;
    public float burstCooldown;
    public bool autoBurst;

    [Tooltip("Does the weapon's charge affect its fire rate?")] public bool chargeAffectsFireRate = false;
    [Tooltip("")] public float minChargeFireRateMultiplier = 0.4f;
    [Tooltip("")] public float maxChargeFireRateMultiplier = 1f;


    public AmmoTypeScriptable ammoType;

    [SerializeField] internal float currentFireCooldown;
    protected bool burstFiring = false;


    public Transform fireOrigin;



    public override float GetDamage(float distance = 0)
    {
        float damageLerp = Mathf.Clamp01(Mathf.InverseLerp(damageConfig.damageFalloffStart, damageConfig.damageFalloffEnd, distance));
        return Mathf.Lerp(damageConfig.maxDamage, damageConfig.minDamage, damageConfig.damageFalloffCurve.Evaluate(damageLerp));
    }

    public override void LTimestep()
    {
        base.LTimestep();

        if (fired)
        {
            currentFireCooldown += Time.fixedDeltaTime;
        }
        if (currentFireCooldown >= TrueTimeBetweenRounds)
        {
            fired = false;
            currentFireCooldown = 0;
        }

        debugdisplay_truetime = TrueTimeBetweenRounds;
    }

    [Rpc(SendTo.NotServer, DeferLocal = true)]
    protected void FireWeapon_RPC(bool primary = true)
    {

        PostAttackBehaviour();
    }
    public virtual void ServerFire(Quaternion rotation, Vector3 origin)
    {
        for (int i = 0; i < ammoType.fireIterations; i++)
        {
            ProjectilePool.Get(out NetProjectile v);
            Debug.Log("Fired weapon");
            v.InitialiseProjectile(this, rotation * SpreadVector, chargeAmount);
        }
    }
    public void FireWeaponBeforeSend(Quaternion rotation, Vector3 origin, bool primary = true)
    {
        if (IsServer)
        {
            ServerFire(rotation, origin);
            PostAttackBehaviour();
        }

        if(IsClient)
        {
            if (IsOwner)
            {
                TriggerAnimation(primary ? PRIMARYATTACK : SECONDARYATTACK, TRIGGERTIMETINY);
                FireWeapon_RPC(primary);
            }
        }
    }
    protected override void PrimaryBehaviour()
    {
        base.PrimaryBehaviour();


        bool chargeMet = (chargeRate <= 0 || chargeAmount >= (chargeOnlyUntilMinimum ? minimumChargeToFire : 1));
        if (!fireOnRelease && (primaryInput || chargeHoldFrame) && chargeMet || (fireOnRelease && primaryPressed && !primaryInput))
        {
            if (!primaryUsesCharge || (chargeAmount > minimumChargeToFire))
            {
                TryFireRanged();
            }
        }
        primaryPressed = primaryInput && chargeMet;

    }
    protected virtual void TryFireRanged()
    {
        switch (CurrentFireMode)
        {
            case FireMode.single:
                if (!primaryPressed || fireOnRelease)
                {
                    FireWeaponBeforeSend(FireRotation, FirePosition);
                    fired = true;
                }
                break;
            case FireMode.automatic:
                FireWeaponBeforeSend(FireRotation, FirePosition);
                fired = true;
                break;
            case FireMode.animated:
                if (!animatedFirePending)
                {
                    animatedFirePending = true;
                    FireWeaponBeforeSend(FireRotation, FirePosition);
                }
                break;
            case FireMode.burst:
                if (roundsInBurst > 0 && !burstFiring)
                {
                    StartCoroutine(BurstFire());
                }
                break;
            default:
                break;
        }
    }
    protected override void SecondaryBehaviour()
    {

    }
    protected virtual void OnValidate()
    {
        timeBetweenRounds = 1 / ((float)roundsPerMinute / 60);
    }
    protected virtual IEnumerator BurstFire()
    {
        burstFiring = true;
        while (burstRoundsFired < roundsInBurst && (!useAmmunition || CurrentAmmo.Value > 0))
        {
            burstRoundsFired++;
            FireWeaponBeforeSend(FireRotation, FirePosition);
            yield return new WaitForSeconds(timeBetweenRounds);
        }
        yield return new WaitForSeconds(burstCooldown);
        if (!autoBurst)
        {
            yield return new WaitUntil(() => { return primaryInput == false; });
        }
        burstRoundsFired = 0;
        burstFiring = false;
        yield break;
    }
    public virtual void IncrementFireMode()
    {
        fireModeIndex++;
        fireModeIndex %= allowedFireModes.Length;
    }

    public override void OnNetworkDespawn()
    {

        ProjectilePool.Clear();

        base.OnNetworkDespawn();

    }
}
