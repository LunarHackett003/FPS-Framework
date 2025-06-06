using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerSpawnButton : MonoBehaviour
{
    public void SpawnPlayer()
    {
        if (NetworkPlayer.LocalNetworkPlayer)
        {
            NetworkPlayer.LocalNetworkPlayer.SpawnPlayer_RPC(TestLoadoutMenu.Instance.selectedWeapons.ToArray());
        }
    }
}
