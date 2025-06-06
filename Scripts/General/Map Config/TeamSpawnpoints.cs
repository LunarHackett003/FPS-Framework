using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSpawnpoints : MonoBehaviour
{
    public Transform[] spawnpoints;

    public Transform GetRandomSpawnpoint()
    {
        if (spawnpoints.Length == 0)
            return null;
        else
            return spawnpoints[Random.Range(0, spawnpoints.Length)];
    }
}
