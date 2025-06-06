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
    public int maxHits = 50;
    public int rayCount = 100;

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



        //Only the server should run explosion logic.
        if (!IsServer)
            return;


        if (maxHits <= 0 || rayCount <= 0 || !doExplosion)
            return;


        QueryParameters qp = new QueryParameters()
        {
            hitTriggers = QueryTriggerInteraction.Ignore,
            layerMask = blastMask,
            hitBackfaces = false,
            hitMultipleFaces = false
        };
        NativeArray<RaycastCommand> commands = new(rayCount, Allocator.TempJob);
        for (int i = 0; i < rayCount; i++)
        {
            if (useLimitedAngle && blastAngle != 180)
            {
                Vector3 direction = transform.rotation * RandomBlastDirection;
                commands[i] = new(transform.position - direction * 0.02f, direction, qp, blastRadius);
            }
            else
            {
                Vector3 direction = Random.onUnitSphere;
                commands[i] = new(transform.position - direction * 0.02f, direction, qp, blastRadius);
            }
        }
        NativeArray<RaycastHit> hits = new(rayCount, Allocator.TempJob);
        JobHandle job = RaycastCommand.ScheduleBatch(commands, hits, 1);
        job.Complete();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            //if we do not hit a collider, move on
            if (hit.collider == null)
            {
                continue;
            }
            if (hit.collider.attachedRigidbody != null)
            {
                float rangeLerp = blastFalloff.Evaluate(Mathf.InverseLerp(0, blastRadius, hit.distance));
                float damage = Mathf.Lerp(damagePointBlank, damageAtEdge, rangeLerp);
                float force = Mathf.Lerp(forcePointBlank, forceAtEdge, rangeLerp);
                if (hitData.ContainsKey(hit.collider))
                {

                }
                else
                {
                    hitData.TryAdd(hit.collider, new()
                    {
                        damageAccumulated = damage,
                        forceAccumulated = force,
                    });
                }
            }

        }

        if(hitData.Count > 0)
        {
            foreach (KeyValuePair<Collider, ExplosionHitData> item in hitData)
            {
                if (item.Key.attachedRigidbody)
                {
                    item.Key.attachedRigidbody.AddExplosionForce(item.Value.forceAccumulated, transform.position, blastRadius, 0.5f, ForceMode.Impulse);
                    
                    if(item.Key.attachedRigidbody.TryGetComponent(out NetDamageable nd))
                    {
                        if(nd.receiveDamageFromTeamOrOwner || canDamageFriendlies || !NetworkPlayer.IsPlayerOnMyTeam(OwnerClientId, nd.OwnerClientId) || OwnerClientId == nd.OwnerClientId)
                        {
                            nd.ModifyHealth(item.Value.damageAccumulated, source, damageSourceType, false);
                            if (!nd.IsOwnedByServer)
                            {

                            }

                        }
                    }

                }
            }
        }

        hits.Dispose();
        commands.Dispose();


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
