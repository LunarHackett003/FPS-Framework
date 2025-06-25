using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Struct that contains data relating to an explosion. Passed to 
/// </summary>
public struct ExplosionData
{
    public Vector3 explosionOrigin;
    public Vector3[] explosionDirections;
    public float explosionRange;
    public float damageAtZero, damageAtEdge, forceAtZero, forceAtEdge;
    public AnimationCurve damageOverRangeCurve;

    public Collider[] colliders;

    public DamageSourceType damageSourceType;

    public bool canDamageFriendlies;
    public ulong ownerID;

    public DebuffStats[] debuffsToApply;
}


public class ExplosionManager : LunarNetScript
{
    public static List<ExplosionData> explosions = new();
    public static List<ExplosionData> explosionsForNextFrame = new();
    public static int explosionsWaitingForNextFrame = 0;
    public static int explosionsPending = 0;

    public int maxHits = 50;

    static bool executingExplosions = false;

    public LayerMask damageableMask, obstructionMask;

    public Dictionary<Collider, ExplosionHitData> explosionHits = new();

    public float explosionInsetFactor = 0.01f;

    public static void AddExplosion(ExplosionData explosion)
    {
        if (!executingExplosions)
        {
            if(explosionsPending >= explosions.Count)
            {
                explosions.Add(explosion);
            }
            else
            {
                explosions[explosionsPending] = explosion;
            }
            Debug.Log("Queued explosion for this frame");
            explosionsPending++;
        }
        else
        {
            if(explosionsWaitingForNextFrame >= explosionsForNextFrame.Count)
            {
                explosionsForNextFrame.Add(explosion);
            }
            else
            {
                explosionsForNextFrame[explosionsWaitingForNextFrame] = explosion;
            }
            Debug.Log("Queued explosion for next frame");
            explosionsWaitingForNextFrame++;
        }
    }

    public override void LTimestep()
    {
        base.LTimestep();

        if(explosionsPending > 0)
        {
            executingExplosions = true;
            Explode();
        }
    }

    public void Explode()
    {
        QueryParameters qp = new()
        {
            hitBackfaces = false,
            hitMultipleFaces = false,
            hitTriggers = QueryTriggerInteraction.Ignore,
            layerMask = obstructionMask
        };
        int allColliderCount = 0;
        for (int i = 0; i < explosionsPending; i++)
        {
            allColliderCount += explosions[i].colliders.Length;
        }
        Debug.Log($"Performing explosions on {allColliderCount} colliders.");
        NativeArray<RaycastCommand> explodeRays = new(allColliderCount * 5, Allocator.TempJob);
        for (int i = 0; i < explosionsPending; i++)
        {
            ExplosionData dat = explosions[i];
            for (int x = 0; x < dat.colliders.Length; x++)
            {
                int offset = i * x;
                for (int j = 0; j < 5; j++)
                {
                    if(offset + j > explodeRays.Length)
                    {
                        Debug.LogWarning($"Index {offset + j} was larger than the allocated length {explodeRays.Length} of explode rays");
                    }
                    else
                    {
                        //when assigning the explosion direction and origins, slightly inset the origin by the direction, to prevent explosions starting inside something and not hitting it
                        explodeRays[offset + j] = new(dat.explosionOrigin - (dat.explosionDirections[j] * explosionInsetFactor), dat.explosionDirections[j], qp, dat.explosionRange + explosionInsetFactor);
                    }
                }
            }
        }
        NativeArray<RaycastHit> hits = new(explodeRays.Length, Allocator.TempJob);
        JobHandle job = RaycastCommand.ScheduleBatch(explodeRays, hits, 5, 1);
        job.Complete();

        for (int x = 0; x < explosionsPending; x++)
        {
            ExplosionData dat = explosions[x];
            int xOffset = x * dat.colliders.Length;
            for (int y = 0; y < dat.colliders.Length; y++)
            {
                if (dat.colliders[y] == null)
                    continue;
                if (!explosionHits.ContainsKey(dat.colliders[y]))
                {
                    explosionHits.TryAdd(dat.colliders[y], new()
                    {
                        sourceID = dat.ownerID,
                        damageAccumulated = 0,
                        forceAccumulated = 0
                    });
                }
                int yOffset = xOffset * 5;
                for (int z = 0; z < 5; z++)
                {
                    if (hits[yOffset + z].collider == null || hits[yOffset + z].collider != dat.colliders[y])
                    {
                        Debug.Log("Collider was null or was not the collider we were checking for.");
                        continue;
                    }
                    RaycastHit hit = hits[yOffset + z];
                    Debug.DrawLine(dat.explosionOrigin - (dat.explosionDirections[z] * explosionInsetFactor), hit.point, Color.green, 10);
                    ExplosionHitData ehd = explosionHits[dat.colliders[y]];
                    float rangeLerp = dat.damageOverRangeCurve.Evaluate(Mathf.InverseLerp(0, dat.explosionRange, hit.distance));
                    ehd.forceAccumulated += Mathf.Lerp(dat.forceAtZero, dat.forceAtEdge, rangeLerp);
                    ehd.damageAccumulated += Mathf.Lerp(dat.damageAtZero, dat.damageAtEdge, rangeLerp);
                    explosionHits[dat.colliders[y]] = ehd;
                }
            }
        }

        if(explosionHits.Count > 0)
        {
            foreach (var item in explosionHits)
            {
                if(item.Key.TryGetComponent(out NetDamageable d))
                {
                    //If the damageable hit can receive "friendly fire", is the player that launched the projectile
                    if(d.receiveDamageFromTeamOrOwner || (NetPlayerEntity.playersByID.TryGetValue(d.OwnerClientId, out NetPlayerEntity npe) && d == npe) || !NetworkPlayer.IsPlayerOnMyTeam(item.Value.sourceID, d.OwnerClientId))
                    {
                        d.ModifyHealth(item.Value.damageAccumulated);
                        d.ApplyForceToOwner_RPC(item.Value.forceAccumulated * (item.Key.ClosestPoint(item.Value.originPoint) - item.Value.originPoint), item.Value.originPoint);

                        Debug.Log("applied damage to an object", d.gameObject);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Nothing was hit by this explosion.");
        }


        explosionsPending = 0;
        executingExplosions = false;
        if (explosionsWaitingForNextFrame > 0)
        {
            explosions = explosionsForNextFrame;
            
            explosionsPending = explosionsWaitingForNextFrame;
            explosionsWaitingForNextFrame = 0;
        }
        explosionHits.Clear();
    }

}
