using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
/// <summary>
/// Unity's lack of jagged array serialisation requires this. Wonderful.
/// </summary>
public struct SpawnpointList
{
    public List<TeamSpawnpoints> spawnpoints;
}
public class SpawnpointManager : MonoBehaviour
{
    public static SpawnpointManager Instance;

    public List<SpawnpointList> teamSpawnpoints;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public Transform GetSpawnpoint(int teamIndex)
    {
        if (teamIndex < 0 || teamIndex >= teamSpawnpoints.Count)
            return null;

        return teamSpawnpoints[teamIndex].spawnpoints[Random.Range(0, teamSpawnpoints[teamIndex].spawnpoints.Count)].GetRandomSpawnpoint();
    }
}
