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


        if (!IsServer)
            return;


        Collider[] array = new Collider[maxHits];

        int hits = Physics.OverlapSphereNonAlloc(transform.position, blastRadius, array, blastMask, QueryTriggerInteraction.Ignore);
        if (hits == 0)
            return;

        NativeArray<RaycastCommand> explodeRays = new(hits, Allocator.TempJob);
        QueryParameters qp = new()
        {
            hitBackfaces = false,
            hitMultipleFaces = false,
            hitTriggers = QueryTriggerInteraction.Ignore,
            layerMask = blastMask
        };
        for (int i = 0; i < array.Length; i++)
        {
            Collider item = array[i];
            //If a collider is null, we've probably hit the end, right?
            if (item == null)
                return;
            //if we do NOT use a limited blast angle OR 
            if (!useLimitedAngle || Vector3.Angle(transform.position - item.ClosestPoint(transform.position), transform.rotation * blastBaseDirection) < blastAngle)
            {
                for (int j = 0; j < 5; j++)
                {
                    Vector3[] directions = new Vector3[]
                    {
                        item.bounds.center - transform.position,
                        (item.bounds.center + (0.5f * item.bounds.extents.y * item.transform.up)) - transform.position,
                        (item.bounds.center - (0.5f * item.bounds.extents.y * item.transform.up)) - transform.position,
                        (item.bounds.center + (0.5f * item.bounds.extents.x * item.transform.right)) - transform.position,
                        (item.bounds.center + (0.5f * item.bounds.extents.x * item.transform.right)) - transform.position,
                    };
                    explodeRays[i + j] = new()
                    {
                        direction = directions[j].normalized,
                        distance = blastRadius * 1.1f,
                        from = transform.position - (explodeRays[i + j].direction * 0.1f),
                        queryParameters = qp,
                    };
                }
            }
        }
        NativeArray<RaycastHit> explodeHits = new(explodeRays.Length, Allocator.TempJob);
        JobHandle job = RaycastCommand.ScheduleBatch(explodeRays, explodeHits, 5);
        job.Complete();
        for (int i = 0; i < hits; i++)
        {
            int offset = i * hits;
            for (int j = 0; j < 5; j++)
            {
                if (explodeHits[offset + j].collider == null || explodeHits[offset + j].rigidbody == null)
                {
                    continue;
                }
                RaycastHit hit = explodeHits[offset + j];

                float rangeLerp = blastFalloff.Evaluate(Mathf.InverseLerp(0, blastRadius, hit.distance));
                float damage = Mathf.Lerp(damagePointBlank, damageAtEdge, rangeLerp);
                float force = Mathf.Lerp(forcePointBlank, forceAtEdge, rangeLerp);
                if (hitData.ContainsKey(hit.collider))
                {
                    ExplosionHitData data = hitData[hit.collider];
                    data.damageAccumulated += damage;
                    data.forceAccumulated += force;
                    hitData[hit.collider] = data;
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

            if(hitData.Count > 0)
            {
                foreach (KeyValuePair<Collider, ExplosionHitData> item in hitData)
                {
                    if (item.Key.TryGetComponent(out NetDamageable d))
                    {
                        d.ModifyHealth(item.Value.damageAccumulated);
                        d.ApplyForceToOwner_RPC(transform.position - item.Key.ClosestPoint(transform.position) * item.Value.forceAccumulated, transform.position);
                    }
                }
            }
            hitData.Clear();
            explodeHits.Dispose();
            explodeRays.Dispose();
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
