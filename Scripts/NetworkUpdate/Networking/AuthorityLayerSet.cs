using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AuthorityLayerSet : NetworkBehaviour
{
    public int ownerLayerIndex, remoteLayerIndex;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        gameObject.layer = IsOwner ? ownerLayerIndex : remoteLayerIndex;
    }
}
