using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

public struct ExplosionHitData
{
    public float damageAccumulated, forceAccumulated;
    public Vector3 originPoint;
    public ulong sourceID;
}


public class Explosion : ProjectileHitEffect
{
    public bool explodeOnSpawn;
    public VisualEffect[] visualEffects;

    public float blastRadius;
    public AnimationCurve blastFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float damagePointBlank = 50, damageAtEdge = 0, forcePointBlank = 50, forceAtEdge = 0;

    public LayerMask blastMask;
    public bool doExplosion = true;

    public bool explodeOnlyOnce;
    public bool exploded;

    public DebuffStats[] debuffsToApply;

    public bool useLimitedAngle;
    public Vector3 blastBaseDirection = Vector3.up;
    [Range(0, 180)]
    public float blastAngle;

    public bool canDamageFriendlies;

    public DamageSourceType damageSourceType;

    public Dictionary<Collider, ExplosionHitData> hitData = new();

    Vector3 RandomBlastDirection => Quaternion.Euler(Random.Range(-blastAngle, blastAngle), Random.Range(-blastAngle, blastAngle), 0) * blastBaseDirection;

    public bool despawnAfterExplosion;
    public float explodeDespawnTime;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer && explodeOnSpawn)
        {
            Explode_RPC();
        }
    }

    [Rpc(SendTo.Everyone)]
    public virtual void Explode_RPC()
    {
        visualEffects.PlayVFX(true);

        if(explodeOnlyOnce)
            exploded = true;

        if (!IsServer || !doExplosion || exploded)
            return;

        Collider[] array = new Collider[30];
        int hits = Physics.OverlapSphereNonAlloc(transform.position, blastRadius, array, blastMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < array.Length; i++)
        {
            Collider item = array[i];
            //If a collider is null, we've probably hit the end, right?
            if (item == null)
                return;
            //if we do NOT use a limited blast angle OR the target is between the acceptable blast angle
            if (!useLimitedAngle || Vector3.Angle(item.ClosestPoint(transform.position) - transform.position, transform.rotation * blastBaseDirection) < blastAngle)
            {
                Vector3[] directions = new Vector3[]
                {
                    (item.bounds.center - transform.position).normalized,
                    ((item.bounds.center + (0.5f * item.bounds.extents.y * item.transform.up)) - transform.position).normalized,
                    ((item.bounds.center - (0.5f * item.bounds.extents.y * item.transform.up)) - transform.position).normalized,
                    ((item.bounds.center + (0.5f * item.bounds.extents.x * item.transform.right)) - transform.position).normalized,
                    ((item.bounds.center - (0.5f * item.bounds.extents.x * item.transform.right)) - transform.position).normalized,
                };

                ExplosionData data = new()
                {
                    canDamageFriendlies = canDamageFriendlies,
                    colliders = array,
                    damageAtEdge = damageAtEdge,
                    damageAtZero = damagePointBlank,
                    damageOverRangeCurve = blastFalloff,
                    damageSourceType = damageSourceType,
                    explosionDirections = directions,
                    explosionOrigin = transform.position,
                    explosionRange = blastRadius,
                    forceAtEdge = forceAtEdge,
                    forceAtZero = forcePointBlank,
                    ownerID = OwnerClientId,
                    debuffsToApply = debuffsToApply,
                };

                ExplosionManager.AddExplosion(data);
            }
            else
            {
                continue;
            }
        }
        if (despawnAfterExplosion)
        {
            StartCoroutine(DespawnAfterExplosion());
        }
    }

    IEnumerator DespawnAfterExplosion()
    {
        yield return new WaitForSeconds(explodeDespawnTime);
        NetworkObject.Despawn();
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        if (useLimitedAngle)
        {
            for (int i = 0; i < 6; i++)
            {
                Gizmos.DrawRay(Vector3.zero, Quaternion.Euler(blastAngle, 60 * i, 0) * (blastBaseDirection * blastRadius));
            }
        }
        else
        {
            Gizmos.DrawWireSphere(Vector3.zero, blastRadius);
        }


        Gizmos.matrix = Matrix4x4.identity;
    }
}
