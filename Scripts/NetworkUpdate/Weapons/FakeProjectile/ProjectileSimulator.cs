using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ProjectileSimulator : LunarNetScript
{

    public static List<NetProjectile> allProjectiles;
    public struct HitData
    {
        public RangedNetWeapon weapon;
        public float damageAccumulated;
        public int hits;
        public Vector3 forceAccumulated;
        public Vector3 hitPointAccumulated;

        public ulong sourceClientID;
    }
    public Dictionary<Collider, HitData> colliderHitData;
    public float raycastDebugTime = 0.1f;
    public int maxHits = 8;

    public LayerMask layermask;

    NativeArray<SpherecastCommand> castCommands;
    NativeArray<RaycastHit> hits;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            colliderHitData = new();
            allProjectiles = new();
        }
        
    }

    public override void LTimestep()
    {
        base.LTimestep();

        if (!IsServer)
            return;

        /*  Old simulation code
        *
        *   if (allProjectiles.Count == 0)
        *       return;
        *   SimulateProjectiles();
        *   
        *   for (int i = allProjectiles.Count - 1; i >= 0; i--)
        *   {
        *       NetProjectile projectile = allProjectiles[i];
        *       if (projectile.terminated) continue;
        *   
        *       projectile.TickProjectile();
        *   }
            CheckAndTerminateProjectiles();
        */

        //New simulation code
        New_ProjectileSimulate();
    }

    void SimulateProjectiles()
    {
        QueryParameters qp = new()
        {
            layerMask = layermask,
            hitTriggers = QueryTriggerInteraction.Collide,
            hitMultipleFaces = true
        };
        castCommands = new(allProjectiles.Count, Allocator.TempJob);
        for (int i = 0; i < castCommands.Length; i++)
        {
            NetProjectile proj = allProjectiles[i];
            castCommands[i] = new(proj.transform.position, proj.radius, proj.direction, qp, proj.velocity * Time.fixedDeltaTime);
        }

        //Multiply by 16 to allow us to hit UP TO 16 targets.
        hits = new NativeArray<RaycastHit>(allProjectiles.Count * 16, Allocator.TempJob);
        JobHandle job = SpherecastCommand.ScheduleBatch(castCommands, hits, 1, 16);
        job.Complete();

        int rayCount = 0;
        float distance = 0;
        int indexOfClosestCollider = 0;
        if(hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i += 16)
            {
                SpherecastCommand command = castCommands[rayCount];
                Debug.DrawRay(command.origin, command.distance * command.direction, Color.red, raycastDebugTime);
                distance = float.MaxValue;
                RaycastHit hit = hits[i];
                NetProjectile proj = allProjectiles[rayCount];
                rayCount++;
                int offset = rayCount * 16;
                for (int j = 0; j < 16; j++)
                {
                    if (hits[offset + j].collider == null || !proj.ignoredColliders.Contains(hits[i + j].collider))
                        continue;
                    float compareDistance = Vector3.Distance(command.origin, hit.point);
                    if (compareDistance <= distance)
                    {
                        indexOfClosestCollider = i + j;
                        distance = compareDistance;
                    }
                }

                //Now that we've found the closest collider, we can replace the hit used to query the above bit
                hit = hits[indexOfClosestCollider];
                //Cache the collider, and now everything SHOULD function as it did before, right?
                Collider c = hit.collider;
                if (c == null)
                    continue;
                //If we progress past here, we hit something.
                float damageDealt = proj.weapon.GetDamage(proj.distanceTravelled + hit.distance);
                if(colliderHitData.TryGetValue(c, out HitData chd))
                {
                    chd.damageAccumulated += damageDealt;
                    chd.forceAccumulated += -hit.normal * damageDealt;
                    chd.hitPointAccumulated += hit.point;
                    chd.hits++;
                    colliderHitData[c] = chd;
                }
                else
                {
                    colliderHitData.TryAdd(c, new()
                    {
                        weapon = proj.weapon,
                        damageAccumulated = damageDealt,
                        forceAccumulated = -hit.normal * damageDealt,
                        hitPointAccumulated = hit.point,
                        hits = 1,
                        sourceClientID = proj.OwnerClientId
                    });
                }
                //Implement Ricochet and Penetration later on


                //Ricochet + Penetration
                proj.transform.position = hit.point;
                proj.TerminateProjectile(true);

                Debug.DrawRay(proj.transform.position, hits[i].point, Color.green, raycastDebugTime);

            }
            if(colliderHitData.Count > 0)
            {
                foreach (var item in colliderHitData)
                {
                    if(item.Key.attachedRigidbody != null)
                    {
                        item.Key.attachedRigidbody.AddForceAtPosition(item.Value.forceAccumulated, item.Value.hitPointAccumulated / item.Value.hits);
                    }
                    if (item.Key.TryGetComponent(out NetDamageable d))
                    {
                        d.ModifyHealth(item.Value.damageAccumulated, item.Value.weapon, DamageSourceType.weapon, false);
                    }
                }
            }
        }
        castCommands.Dispose();
        hits.Dispose();
        colliderHitData.Clear();
    }

    void New_ProjectileSimulate()
    {
        //Something about the previous one is broken. We're going to do EVERYTHING just in this new method, and hopefully it works alright.
        //If we have no projectiles, we'll exit.
        if (allProjectiles.Count == 0)
            return;

        colliderHitData = new();
        /*
            //Find all the projectiles that need to be terminated.
            projectilesToTerminate.AddRange(allProjectiles.FindAll(x => x.timeAlive >= x.maxAliveTime));
            if(projectilesToTerminate.Count > 0)
            {
                //...And then take them out back and tell them to think of the rabbits.
                foreach (var item in projectilesToTerminate)
                {
                    item.TerminateProjectile(false);
                    allProjectiles.RemoveAll(x => x == item);
                }
                allProjectiles.RemoveAll(x => x == null);
                projectilesToTerminate.Clear();
            }
            //You killed them all! How could you?!
            if (allProjectiles.Count == 0)
                return;
        */

        NetProjectile[] activeProjectileArray = allProjectiles.FindAll(x => x.projectileAlive).ToArray();
        if (activeProjectileArray.Length == 0)
            return;

        castCommands = new NativeArray<SpherecastCommand>(activeProjectileArray.Length, Allocator.TempJob);
        QueryParameters qp = new()
        {
            layerMask = layermask,
            hitMultipleFaces = true,
            hitTriggers = QueryTriggerInteraction.Collide,
            hitBackfaces = false,
        };
        for (int i = 0; i < activeProjectileArray.Length; i++)
        {
            NetProjectile item = activeProjectileArray[i];
            Vector3 dirNorm = item.direction.normalized;
            castCommands[i] = new(item.transform.position - (dirNorm * 0.02f), item.radius, dirNorm, qp, item.velocity * Time.fixedDeltaTime * 1.02f);
            Debug.DrawRay(castCommands[i].origin, castCommands[i].direction * castCommands[i].distance, Color.red, raycastDebugTime);
        }
        //compiled our cast command array, now we create our hits array.
        hits = new(castCommands.Length * maxHits, Allocator.TempJob);

        //Now we complete the job
        JobHandle job = SpherecastCommand.ScheduleBatch(castCommands, hits, 1, maxHits);
        job.Complete();
        for (int x = 0; x < castCommands.Length; x++)
        {
            float distance = float.MaxValue;
            bool validHit = false;
            NetProjectile np = activeProjectileArray[x];
            //Big up @peturdarri on the Unity Discord Server <3
            //Thanks for letting me know about this, no clue how I mucked that up lol. Why was i just ADDING x and y??
            int offset = x * maxHits;
            int indexOfClosestHit = -1;
            for (int y = 0; y < maxHits; y++)
            {
                RaycastHit hit = hits[offset + y];
                if (hit.collider == null || np.ignoredColliders.Contains(hit.collider))
                {
                    continue;
                }
                if(hit.distance != 0 && hit.distance < distance)
                {
                    //Debug.Log($"Found new closest collider at {hit.distance} metres from origin", hit.collider);
                    //Debug.Log(hit.point);
                    distance = hit.distance;
                    validHit = true;
                    indexOfClosestHit = offset + y;
                }
            }
            if (validHit)
            {
                ProcessHit(ref np, hits[indexOfClosestHit]);
                continue;
            }
            np.TickProjectile();
            //If our projectile has gone too far/been alive too long, terminate it on the next frame
            if(np.timeAlive > np.maxAliveTime || np.distanceTravelled > np.maxDistance)
            {
                np.TerminateProjectile(false);
            }
        }

        DamageColliders();
        castCommands.Dispose();
        hits.Dispose();
        colliderHitData.Clear();

        activeProjectileArray = null;
    }
    void ProcessHit(ref NetProjectile np, RaycastHit hit)
    {

        //If we've hit something valid, then we want to do the maths on that bit
        QueryColliderAndAddStats(np, hit, hit.collider);
        //move the projectile to the hit point
        np.transform.position = hit.point;
        //Terminate it. Think of the rabbits, Projectile...
        np.timeAlive = np.maxAliveTime;
        np.TerminateProjectile(true);
    }
    void QueryColliderAndAddStats(NetProjectile proj, RaycastHit hit, Collider c)
    {
        float damageDealt = proj.weapon.GetDamage(proj.distanceTravelled + hit.distance);
        if (colliderHitData.TryGetValue(c, out HitData chd))
        {
            chd.damageAccumulated += damageDealt;
            chd.forceAccumulated += -hit.normal * damageDealt;
            chd.hitPointAccumulated += hit.point;
            chd.hits++;
            colliderHitData[c] = chd;
        }
        else
        {
            colliderHitData.TryAdd(c, new()
            {
                weapon = proj.weapon,
                damageAccumulated = damageDealt,
                forceAccumulated = -hit.normal * damageDealt,
                hitPointAccumulated = hit.point,
                hits = 1,
                sourceClientID = proj.OwnerClientId,
            });
        }
    }

    void DamageColliders()
    {
        if (colliderHitData.Count > 0)
        {
            foreach (var item in colliderHitData)
            {
                if (item.Key.attachedRigidbody != null)
                {
                    item.Key.attachedRigidbody.AddForceAtPosition(item.Value.forceAccumulated, item.Value.hitPointAccumulated / item.Value.hits);
                }
                if (item.Key.TryGetComponent(out NetDamageable d))
                {
                    bool canDamage = d.receiveDamageFromTeamOrOwner || !NetworkPlayer.IsPlayerOnMyTeam(item.Value.sourceClientID, d.OwnerClientId);
                    if (canDamage)
                    {    
                        d.ModifyHealth(item.Value.damageAccumulated, item.Value.weapon, DamageSourceType.weapon, false);
                        if(item.Key.attachedRigidbody == null && d.rb != null)
                        {
                            //If we haven't already applied force to a rigidbody AND this damageable has one, then we'll use this.
                            d.ApplyForceToOwner_RPC(item.Value.forceAccumulated, item.Value.hitPointAccumulated / item.Value.hits);
                        }
                    }
                }
            }
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if(castCommands.IsCreated)
            castCommands.Dispose();
        if(hits.IsCreated)
            hits.Dispose();
    }
}
