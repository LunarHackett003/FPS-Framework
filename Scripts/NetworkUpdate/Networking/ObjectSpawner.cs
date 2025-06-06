using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// Ideally a scene-placed object. Spawns any props that might be needed.
/// </summary>
public class ObjectSpawner : NetworkBehaviour
{
    [System.Serializable] public struct PrefabLocation
    {
        public NetworkObject[] prefabs;
        public Transform[] locations;

        public NetworkObject GetRandomObject()
        {
            return prefabs[Random.Range(0, prefabs.Length)];
        }
    }

    public PrefabLocation[] prefabLocations;
    public bool spawnObjectsOnSpawn;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


        if (!IsServer)
            return;

        if (spawnObjectsOnSpawn)
        {
            SpawnObjects();
        }

    }
    public void SpawnObjects()
    {

        for (int i = 0; i < prefabLocations.Length; i++)
        {
            PrefabLocation pl = prefabLocations[i];
            for (int y = 0; y < pl.locations.Length; y++)
            {
                NetworkManager.SpawnManager.InstantiateAndSpawn(pl.GetRandomObject(), 0, false, false, false, pl.locations[y].position, pl.locations[y].rotation);
            }
        }
    }
}
